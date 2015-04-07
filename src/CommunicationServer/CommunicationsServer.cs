using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using ClusterUtils;
using ClusterUtils.Communication;

namespace CommunicationServer
{
    class CommunicationsServer
    {
        public static ManualResetEvent AllDone = new ManualResetEvent(false);

        private static int _componentCount;

        private readonly string _listeningPort;
        private bool _backupMode;
        private readonly int _componentTimeout;
        
        public CommunicationsServer(ServerConfig configuration)
        {
            _listeningPort = configuration.ServerPort;
            _backupMode = configuration.IsBackup;
            _componentTimeout = configuration.ComponentTimeout;
        }

        private void LogServerInfo()
        {
            Console.WriteLine("Server is running in {0} mode.", _backupMode ? "backup" : "primary");
            Console.WriteLine("Listening on port " + _listeningPort);
            Console.WriteLine("Componenet timeout = {0} [s]", _componentTimeout);
            
        }

        public void Start()
        {
            MessageDispatcher md = new MessageDispatcher(_listeningPort, _componentTimeout);
            md.BeginDispatching();
            
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

                    string elemName = message.DocumentElement.Name;

                    Object response = null;
                    switch (elemName)
                    {
                        case "Register":
                            var elemList = message.GetElementsByTagName("Type");
                            var componentType = elemList[0].InnerText;
                            ++_componentCount;
                            Console.WriteLine("Registered component of type {0} with id {1}", componentType, _componentCount);

                            response = new RegisterResponse
                            {
                                Id = _componentCount.ToString(),
                                Timeout = "5"
                            };
                            break;
                        case "Status":
                            Console.WriteLine("Responding to Status: sending a NoOperation message.");
                            NoOperationBackupCommunicationServers backupServers = new NoOperationBackupCommunicationServers();

                            //var backups = message.GetElementsByTagName("BackupCommunicationServers");

                            string componentId = message.GetElementsByTagName("Id")[0].InnerText;
                            Console.WriteLine("Received status message from component {0}", componentId);

                            // adding some example backup server data for testing
                            backupServers.BackupCommunicationServer = new NoOperationBackupCommunicationServersBackupCommunicationServer()
                            {
                                address = "192.125.24.1",
                                port = 1992,
                                portSpecified = true
                            };
                            response = new NoOperation
                            {
                                BackupCommunicationServers = backupServers
                            };
                            break;
                    }

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
