using System;
using System.Collections.Generic;
using System.Xml;
using ClusterMessages;
using ClusterUtils;
using ClusterUtils.Communication;
using System.Threading;
using ClusterMessages;

namespace ComputationalNode
{
    class ComputationalNode
    {

        private readonly ServerInfo _serverInfo;

        private int _id;

        private List<ServerInfo> _backups = new List<ServerInfo>(); 

        public List<StatusThread> StatusThreads { get; set; }

        private int ServerTimeout { get; set; }
        
        public ComputationalNode(ComponentConfig componentConfig)
        {
            _serverInfo = new ServerInfo(componentConfig.ServerPort, componentConfig.ServerAddress);
            StatusThreads = new List<StatusThread>();
        }

        public void Start()
        {
            LogNodeInfo();
            Register();

            StartSendingStatus();
        }

        private void SendStatusMessage(object sender, System.Timers.ElapsedEventArgs e,
                                    Status message)
        {
            var tcpClient = new ConnectionClient(_serverInfo);
            tcpClient.Connect();

            Console.WriteLine("Sending status message to Server.");
            var responses = tcpClient.SendAndWaitForResponses(message);

            tcpClient.Close();
            ProcessMessages(responses);
        }

        public void KeepSendingStatus(Status message, int msCycleTime)
        {
            System.Timers.Timer sendStatus = new System.Timers.Timer(msCycleTime);
            sendStatus.Elapsed += (sender, e) => SendStatusMessage(sender, e, message);
            sendStatus.Start();
        }

        public void StartSendingStatus()
        {
            // defining how often we want to send the KeepAlive message,
            // setting it to half of the defined Server Timeout
            int msStatusCycleTime = ServerTimeout/2; 

            Status statusMessage = new Status();
            statusMessage.Id = (ulong)_id;
            statusMessage.Threads = StatusThreads.ToArray();

            Thread keepSendingStatusThread = new Thread(() => 
                    KeepSendingStatus(statusMessage, msStatusCycleTime));

            Console.WriteLine("Starting thread sending the Status messages.");
            keepSendingStatusThread.Start();
        }

        private void Register()
        {

            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var responses = tcpClient.SendAndWaitForResponses (
                new Register
                {
                    Type = "ComputationalNode"
                }
            );

            tcpClient.Close();
            ProcessMessages(responses);
        }


        //process nooperation, partialproblems
        private void ProcessMessages(IEnumerable<XmlDocument> responses)
        {
            foreach (var xmlMessage in responses)
            {
                switch (MessageTypeResolver.GetMessageType(xmlMessage))
                {
                    case MessageTypeResolver.MessageType.NoOperation:
                        ProcessNoOperationMessage(xmlMessage);
                        break;
                    case MessageTypeResolver.MessageType.PartialProblems:
                        ProcessPartialProblemsMessage(xmlMessage);
                        break;
                    case MessageTypeResolver.MessageType.RegisterResponse:
                        ProcessRegisterResponse(xmlMessage);
                        break;
                }
            }
        }

        private void ProcessRegisterResponse(XmlDocument response)
        {
            //TODO update backup servers info
            _id = int.Parse(response.GetElementsByTagName("Id")[0].InnerText);
            ServerTimeout = int.Parse(response.GetElementsByTagName("Timeout")[0].InnerText);

            Console.WriteLine("Registered at server with Id: {0}.", _id);
        }

        private void ProcessNoOperationMessage(XmlDocument xmlMessage)
        {
            //TODO update backup servers info
            Console.WriteLine("Received NoOperation message.");
        }

        private void ProcessPartialProblemsMessage(XmlDocument xmlMessage)
        {
            var problemInstanceId = ulong.Parse(xmlMessage.GetElementsByTagName("Id")[0].InnerText);

            var taskId = ulong.Parse(xmlMessage.GetElementsByTagName("TaskId")[0].InnerText);

            Console.WriteLine("Received partial problem {0} from problem instance {1}.", taskId, problemInstanceId);

            //Sleep udający liczenie?

            CreateAndSendPartialSolution(taskId, problemInstanceId);
        }

        private void CreateAndSendPartialSolution(ulong taskId, ulong problemInstanceId)
        {
            var solution = new Solutions
            {
                Solutions1 = new[] {new SolutionsSolution {TaskId = taskId, Type = SolutionsSolutionType.Partial}},
                Id = problemInstanceId
            };

            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var response = tcpClient.SendAndWaitForResponses(solution);

            tcpClient.Close();

            if (response.Count != 1)
                throw new Exception();
        }

        private void LogNodeInfo()
        {
            Console.WriteLine("Node is running...");
            Console.WriteLine("Server address: {0}", _serverInfo.Address);
            Console.WriteLine("Server port: {0}", _serverInfo.Port);
        }
    }
}
