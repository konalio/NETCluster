﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClusterUtils;

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

            var bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                //TODO Correct message end detection - naive approach
                state.ByteBuffer.AddRange(state.Buffer);

                var message = Serializers.ByteArrayObject<Register>(state.ByteBuffer.ToArray());

                ++_componentCount;
                Console.WriteLine("Registered component of type {0} with id {1}", message.Type, _componentCount);

                var response = new RegisterResponse
                {
                    Id = _componentCount.ToString()
                };

                Send(handler, Serializers.ObjectToByteArray(response));
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