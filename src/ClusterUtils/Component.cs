using System;
using System.Collections.Generic;
using System.Xml;
using ClusterMessages;
using ClusterUtils.Communication;

namespace ClusterUtils
{
    public abstract class Component
    {
        protected ServerInfo ServerInfo;
        protected string Type;

        protected Component(ComponentConfig config, string type) 
        {
            ServerInfo = new ServerInfo(config.ServerPort, config.ServerAddress);
            Type = type;
        }

        protected void LogRuntimeInfo()
        {
            Console.WriteLine("{0} is running...", Type);
            Console.WriteLine("Server address: {0}", ServerInfo.Address);
            Console.WriteLine("Server port: {0}", ServerInfo.Port);
        }

        protected XmlDocument SendMessageSingleResponse(IClusterMessage message)
        {
            var responses = SendMessage(message);
            return responses[0];
        }

        protected List<XmlDocument> SendMessage(IClusterMessage message)
        {
            var tcpClient = new ConnectionClient(ServerInfo);

            tcpClient.Connect();

            var responses = tcpClient.SendAndWaitForResponses(message);

            tcpClient.Close();

            return responses;
        }
    }
}
