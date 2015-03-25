using System;
using System.Collections.Generic;
using System.Xml;
using ClusterMessages;
using ClusterUtils;
using ClusterUtils.Communication;

namespace ComputationalClient
{
    class ComputationalClient
    {
        private readonly ServerInfo _serverInfo;
        private int _id;

        public ComputationalClient(ComponentConfig componentConfig)
        {
            _serverInfo = new ServerInfo(componentConfig.ServerPort, componentConfig.ServerAddress);
        }

        public void Start()
        {
            LogClientInfo();
            Register();

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private void Register()
        {
            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var responses = tcpClient.SendAndWaitForResponses(
                new Register
                {
                    Type = "ComputationalClient"
                }
            );

            tcpClient.Close(); 
            ProcessMessages(responses);
        }

        private void ProcessMessages(List<XmlDocument> responses)
        {
            foreach (var xmlMessage in responses)
            {
                switch (MessageTypeResolver.GetMessageType(xmlMessage))
                {
                    case MessageTypeResolver.MessageType.NoOperation:
                        ProcessNoOperationMessage(xmlMessage);
                        break;
                    case MessageTypeResolver.MessageType.RegisterResponse:
                        ProcessRegisterResponse(xmlMessage);
                        break;
                }
            }
        }

        private void ProcessRegisterResponse(XmlDocument response)
        {
            _id = int.Parse(response.GetElementsByTagName("Id")[0].InnerText);
            Console.WriteLine("Registered at server with Id: {0}.", _id);
        }
        private void ProcessNoOperationMessage(XmlDocument xmlMessage)
        {
            //TODO update backup servers info
            Console.WriteLine("Received NoOperation message.");
        }

        private void LogClientInfo()
        {
            Console.WriteLine("Client is running...");
            Console.WriteLine("Server address: {0}", _serverInfo.Address);
            Console.WriteLine("Server port: {0}", _serverInfo.Port);
        }
    }
}
