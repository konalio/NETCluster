using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using ClusterMessages;

namespace ClusterUtils.Communication
{
    public class ConnectionClient
    {
        private static readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);
        private static readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);
        private static readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);

        private static readonly List<XmlDocument> Responses = new List<XmlDocument>();

        private readonly IPAddress _serverAddress;
        private readonly int _serverPort;

        private Socket _client;

        public ConnectionClient(string serverAddress, string serverPort)
        {
            _serverAddress = IPAddress.Parse(serverAddress);
            _serverPort = int.Parse(serverPort);
        }

        public void Connect()
        {
            var remoteEP = new IPEndPoint(_serverAddress, _serverPort);

            _client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            _client.BeginConnect(remoteEP,
                ConnectCallback, _client);
            ConnectDone.WaitOne();
        }

        public List<XmlDocument> SendAndWaitForResponses(IClusterMessage message)
        {
            var byteMessage = Serializers.ObjectToByteArray(message);

            Send(_client, byteMessage);
            SendDone.WaitOne();

            Receive(_client);
            ReceiveDone.WaitOne();

            return Responses;
        }

        public void Close()
        {
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint);

                ConnectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                var state = new StateObject { WorkSocket = client };

                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    ReceiveCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;

                try
                {
                    var bytesRead = client.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        foreach (var receivedByte in state.Buffer.TakeWhile(receivedByte => receivedByte != 0))
                        {
                            if (receivedByte == 23)
                            {
                                ExtractSingleResponse(state);
                                state.ByteBuffer.Clear();
                                continue;
                            }
                            state.ByteBuffer.Add(receivedByte);
                        }

                        client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                            ReceiveCallback, state);
                    }
                    else
                    {
                        if (state.ByteBuffer.Count > 1)
                        {
                            var response = Serializers.ByteArrayObject<XmlDocument>(state.ByteBuffer.ToArray());
                            Responses.Add(response);
                        }
                        ReceiveDone.Set();
                    }
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ExtractSingleResponse(StateObject state)
        {
            var response = Serializers.ByteArrayObject<XmlDocument>(state.ByteBuffer.ToArray());
            Responses.Add(response);
        }

        private static void Send(Socket client, byte[] byteData)
        {
            client.BeginSend(byteData, 0, byteData.Length, 0,
                SendCallback, client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;

                var bytesSent = client.EndSend(ar);

                if (bytesSent > 0)
                {
                    Console.WriteLine("Message sent...");
                }

                SendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
