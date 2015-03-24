using System;
using System.Collections.Generic;
using System.Xml;
using ClusterUtils;
using ClusterUtils.Communication;
using System.Threading;

namespace ComputationalNode
{
    class ComputationalNode
    {
        public ulong AssignedId { get; set; }

        public string ServerPort { get; set; }

        public string ServerAddress { get; set; }

        public List<StatusThread> StatusThreads { get; set; }


        public ComputationalNode(ComponentConfig componentConfig)
        {
            ServerPort = componentConfig.ServerPort;
            ServerAddress = componentConfig.ServerAddress;
        }

        public void Start()
        {
            LogNodeInfo();
            Register();

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }


        public void StartSendingStatus()
        {
            // defining how often we want to send the KeepAlive message,
            // before deciding on global solution, I'm setting it to 5s for testing purposes
            int msStatusCycleTime = 5000; 
            
            var tcpClient = new ConnectionClient(ServerAddress, ServerPort);

            tcpClient.Connect();
            Status statusMessage = new Status();
            statusMessage.Id = AssignedId;
            statusMessage.Threads = StatusThreads.ToArray();

            Thread keepSendingStatusThread = new Thread(() => 
                    tcpClient.KeepSendingStatus(statusMessage, msStatusCycleTime));
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


        private void LogNodeInfo()
        {
            Console.WriteLine("Node is running...");
            Console.WriteLine("Server address: {0}", ServerAddress);
            Console.WriteLine("Server port: {0}", ServerPort);
        }
    }
}
