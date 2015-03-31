using System;
using System.Collections.Generic;
using System.Xml;
using ClusterUtils;
using ClusterUtils.Communication;
using System.Threading;
using ClusterMessages;

namespace ComputationalNode
{
    class ComputationalNode
    {
        public ulong AssignedId { get; set; }

        public ComponentConfig ServerConfig { get; set; }
        //public string ServerPort { get; set; }

        //public string ServerAddress { get; set; }

        public List<StatusThread> StatusThreads { get; set; }

        public List<NoOperationBackupCommunicationServersBackupCommunicationServer> BackupServers { get; set; }

        public ComputationalNode(ComponentConfig componentConfig)
        {
            ServerConfig = componentConfig;
            BackupServers = new List<NoOperationBackupCommunicationServersBackupCommunicationServer>();
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
            var tcpClient = new ConnectionClient(ServerAddress, ServerPort);
            tcpClient.Connect();

            Console.WriteLine("Sending status message to Server.");
            var responses = tcpClient.SendAndWaitForResponses(message);

            tcpClient.Close();
            ProcessNoOperationMessage(responses);
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
            // before deciding on global solution, I'm setting it to 5s for testing purposes
            int msStatusCycleTime = 2000; 

            Status statusMessage = new Status();
            statusMessage.Id = AssignedId;
            statusMessage.Threads = StatusThreads.ToArray();

            Thread keepSendingStatusThread = new Thread(() => 
                    KeepSendingStatus(statusMessage, msStatusCycleTime));

            Console.WriteLine("Starting thread sending the Status messages.");
            keepSendingStatusThread.Start();
        }

        private void Register()
        {

            var tcpClient = new ConnectionClient(ServerAddress, ServerPort);

            tcpClient.Connect();

            var responses = tcpClient.SendAndWaitForResponses (
                new Register
                {
                    Type = "ComputationalNode"
                }
            );

            tcpClient.Close();

            ProcessRegisterResponse(responses);
        }

        private static void ProcessRegisterResponse(IReadOnlyList<XmlDocument> responses)
        {
            if (responses.Count == 0)
            {
                Console.WriteLine("No response from server, possible communication error.");
                return;
            }

            var response = responses[0];

            var id = response.GetElementsByTagName("Id")[0].InnerText;

            Console.WriteLine("Registered at server with Id: {0}.", id);
        }

        private void ProcessNoOperationMessage(IReadOnlyList<XmlDocument> responses)
        {
            if (responses.Count == 0)
            {
                Console.WriteLine("No response from server, possible communication error.");
                return;
            }

            var response = responses[0];
            //BackupServers = response.GetElementsByTagName("BackupCommunicationServers");
                        

            //var id = response.GetElementsByTagName("Id")[0].InnerText;
            
            Console.WriteLine("Received a NoOperation message.");
        }


        private void LogNodeInfo()
        {
            Console.WriteLine("Node is running...");
            Console.WriteLine("Server address: {0}", ServerAddress);
            Console.WriteLine("Server port: {0}", ServerPort);
        }
    }
}
