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
        public string ServerPort { get; set; }

        public string ServerAddress { get; set; }

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

            ProcessResponses(responses);
        }

        private static void ProcessResponses(IReadOnlyList<XmlDocument> responses)
        {
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
