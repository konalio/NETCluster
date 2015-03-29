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
        private readonly ManualResetEvent _connectDone =
            new ManualResetEvent(false);
        private readonly ManualResetEvent _sendDone =
            new ManualResetEvent(false);
        private readonly ManualResetEvent _receiveDone =
            new ManualResetEvent(false);

        private readonly List<XmlDocument> _responses = new List<XmlDocument>();

        private readonly IPAddress _serverAddress;
        private readonly int _serverPort;

        private Socket _client;

        public ConnectionClient(ServerInfo serverInfo)
        {
            _serverAddress = IPAddress.Parse(serverInfo.Address);
            _serverPort = int.Parse(serverInfo.Port);
        }

        public void Connect()
        {
            var remoteEP = new IPEndPoint(_serverAddress, _serverPort);

            _client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            _client.BeginConnect(remoteEP,
                ConnectCallback, _client);
            _connectDone.WaitOne();
        }

        public List<XmlDocument> SendAndWaitForResponses(IClusterMessage message)
        {
            var byteMessage = Serializers.ObjectToByteArray(message);

            Send(_client, byteMessage);
            _sendDone.WaitOne();

            Receive(_client);
            _receiveDone.WaitOne();

            return _responses;
        }

        public void Close()
        {
            _client.Shutdown(SocketShutdown.Receive);
            _client.Close();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;

                client.EndConnect(ar);
                
                _connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client)
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

        private void ReceiveCallback(IAsyncResult ar)
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
                            ExtractSingleResponse(state);
                        }
                        _receiveDone.Set();
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

        private void ExtractSingleResponse(StateObject state)
        {
            var response = Serializers.ByteArrayObject<XmlDocument>(state.ByteBuffer.ToArray());
            _responses.Add(response);
        }

        private void Send(Socket client, byte[] byteData)
        {
            client.BeginSend(byteData, 0, byteData.Length, 0,
                SendCallback, client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket)ar.AsyncState;

                var bytesSent = client.EndSend(ar);
                
                if (bytesSent == 0)
                    _client.Shutdown(SocketShutdown.Send);

                _sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
