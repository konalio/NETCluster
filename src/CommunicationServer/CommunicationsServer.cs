using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using ClusterUtils;
using ClusterUtils.Communication;
using ClusterMessages;

namespace CommunicationServer
{
    class CommunicationsServer
    {
        public static ManualResetEvent AllDone = new ManualResetEvent(false);

        private static int _componentCount;

        private readonly string _listeningPort;
        private bool _backupMode;
        private readonly ServerInfo _serverInfo;
        private int _id;
        private readonly int _componentTimeout;
        
        public CommunicationsServer(ServerConfig configuration)
        {
            _listeningPort = configuration.ServerPort;
            _backupMode = configuration.IsBackup;
            _serverInfo = new ServerInfo(configuration.ServerPort, configuration.ServerAddress);
            _componentTimeout = configuration.ComponentTimeout;
        }

        private void LogServerInfo()
        {
            Console.WriteLine("Server is running in {0} mode.", _backupMode ? "backup" : "primary");
            Console.WriteLine("Listening on port " + _listeningPort);
            Console.WriteLine("Componenet timeout = {0} [s]", _componentTimeout);
        }

        private void LogBackupInfo()
        {
            Console.WriteLine("Server is running in {0} mode.", _backupMode ? "backup" : "primary");
            Console.WriteLine("Primary Server address: {0}", _serverInfo.Address);
            Console.WriteLine("Primary Server port: {0}", _serverInfo.Port);
        }

        public void Start()
        {
            if (_backupMode)
            {
                LogBackupInfo();
                Register();

                Console.WriteLine("\nPress ENTER to continue...");
                Console.Read();
                return;
            }

            LogServerInfo();

            var localEndPoint = new IPEndPoint(IPAddress.Any, int.Parse(_listeningPort));
            Console.WriteLine("Address: {0}", localEndPoint.Address.ToString());
            var listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    AllDone.Reset();

                    Console.WriteLine("Waiting for a connection...");

                    //TODO Consider moving BeginAccept to AcceptCallback

                    listener.BeginAccept(
                        AcceptCallback,
                        listener);

                    AllDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

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
                    Type = "BackupServer"
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

        public static void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();

            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);

            var state = new StateObject {WorkSocket = handler};

            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var handler = state.WorkSocket;

            try
            {
                var bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.ByteBuffer.AddRange(state.Buffer);

                    var message = Serializers.ByteArrayObject<XmlDocument>(state.ByteBuffer.ToArray());

                    var elemList = message.GetElementsByTagName("Type");
                    var componentType = elemList[0].InnerText;

                    ++_componentCount;
                    Console.WriteLine("Registered component of type {0} with id {1}", componentType, _componentCount);

                    var response = new RegisterResponse
                    {
                        Id = _componentCount.ToString()
                    };

                    var responseBuffer = new List<byte>(Serializers.ObjectToByteArray(response));
                    Send(handler, responseBuffer.ToArray());
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        private static void Send(Socket handler, byte[] byteData)
        {
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                SendCallback, handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var handler = (Socket)ar.AsyncState;

                var bytesSent = handler.EndSend(ar);

                if (bytesSent > 0) 
                    Console.WriteLine("Response sent.");

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
