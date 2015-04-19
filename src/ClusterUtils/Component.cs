using System;
using System.Collections.Generic;
using ClusterMessages;
using ClusterUtils.Communication;

namespace ClusterUtils
{
    /// <summary>
    /// General class for components:
    ///     - Node
    ///     - Manager
    ///     - Client
    ///     - Server as backup
    /// 
    /// Contains type and server info, and common methods for sending IClusterMessage to Server.
    /// 
    /// </summary>
    public abstract class Component
    {
        /// <summary>
        /// Address and port of server that this component is bound to.
        /// </summary>
        protected ServerInfo ServerInfo;

        /// <summary>
        /// Type of component.
        /// </summary>
        protected string Type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">Config instance containing server info.</param>
        /// <param name="type">Type of component</param>
        protected Component(ComponentConfig config, string type) 
        {
            ServerInfo = new ServerInfo(config.ServerPort, config.ServerAddress);
            Type = type;
        }

        /// <summary>
        /// Method logging components type and server info.
        /// </summary>
        protected void LogRuntimeInfo()
        {
            Console.WriteLine("{0} is running...", Type);
            Console.WriteLine("Server address: {0}", ServerInfo.Address);
            Console.WriteLine("Server port: {0}", ServerInfo.Port);
        }

        /// <summary>
        /// Sends single message to server and waits for single response.
        /// </summary>
        /// <param name="message">XmlMessage to be sent.</param>
        /// <returns>Response from server.</returns>
        protected MessagePackage SendMessageSingleResponse(IClusterMessage message)
        {
            var responses = SendMessage(message);
            return responses[0];
        }

        /// <summary>
        /// General method for sending messages. Sends single message to server and waits for any responses.
        /// </summary>
        /// <param name="message">XmlMessage to be sent.</param>
        /// <returns>All received messages as XMLDocuments.</returns>
        protected List<MessagePackage> SendMessage(IClusterMessage message)
        {
            var tcpClient = new ConnectionClient(ServerInfo);

            tcpClient.Connect();

            var responses = tcpClient.SendAndWaitForResponses(message);

            tcpClient.Close();

            return responses;
        }
    }
}
