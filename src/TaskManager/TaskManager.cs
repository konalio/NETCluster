using System;
using System.Collections.Generic;
using System.Xml;
using ClusterUtils;
using ClusterUtils.Communication;

namespace TaskManager
{
    public class TaskManager
    {
        public string ServerAddress { get; set; }
        public string ServerPort { get; set; }

        public TaskManager(ComponentConfig cc)
        {   
            ServerAddress = cc.ServerAddress;
            ServerPort = cc.ServerPort;            
        }

        public void Start()
        {
            LogManagerInfo();
            Register();

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private void Register()
        {
            var tcpClient = new ConnectionClient(ServerAddress, ServerPort);

            tcpClient.Connect();

            var responses = tcpClient.SendAndWaitForResponses (
                new Register
                {
                    Type = "TaskManager"
                }
            );

            tcpClient.Close();

            ProcessResponses(responses);
        }

        private static void ProcessResponses(IReadOnlyList<XmlDocument> responses)
        {
            var response = responses[0];

            var id = response.GetElementsByTagName("Id")[0].InnerText;

            Console.WriteLine("Registered at server with Id: {0}.", id);
        }

        private void LogManagerInfo()
        {
            Console.WriteLine("Manager is running...");
            Console.WriteLine("Server address: {0}", ServerAddress);
            Console.WriteLine("Server port: {0}", ServerPort);
        }
    }
}
