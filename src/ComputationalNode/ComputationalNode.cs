using System;
using System.Collections.Generic;
using System.Xml;
using ClusterMessages;
using ClusterUtils;
using ClusterUtils.Communication;

namespace ComputationalNode
{
    class ComputationalNode
    {
        private readonly ServerInfo _serverInfo;

        private int _id;

        private List<ServerInfo> _backups = new List<ServerInfo>(); 
        
        public ComputationalNode(ComponentConfig componentConfig)
        {
            _serverInfo = new ServerInfo(componentConfig.ServerPort, componentConfig.ServerAddress);
        }

        public void Start()
        {
            LogNodeInfo();
            Register();

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
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
