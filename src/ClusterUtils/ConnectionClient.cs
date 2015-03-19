using ClusterMessages;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace ClusterUtils
{
    class ConnectionClient
    {
        private static readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);
        private static readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);
        private static readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);

        private List<ClusterMessage> _responses;

        private IPAddress _serverAddress;
        private int _serverPort;

        private Socket _client;

        public ConnectionClient(string serverAddress, string serverPort)
        {
            _serverAddress = IPAddress.Parse(serverAddress);
            _serverPort = int.Parse(serverPort);
        }

        public void Connect()
        {
            var remoteEP = new IPEndPoint(_serverAddress, _serverPort);

            var client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            client.BeginConnect(remoteEP,
                ConnectCallback, client);
            ConnectDone.WaitOne();
        }

        public List<ClusterMessage> SendAndWaitForResponse(ClusterMessage message)
        {
            Send(_client, Serializers.ObjectToByteArray(message));
            SendDone.WaitOne();

            Receive(_client);
            ReceiveDone.WaitOne();
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

                var bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.ByteBuffer.AddRange(state.Buffer);

                    client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                        ReceiveCallback, state);
                }
                else
                {
                    if (state.ByteBuffer.Count > 1)
                    {
                        _response = Serializers.ByteArrayObject<RegisterResponse>(state.ByteBuffer.ToArray());
                    }
                    ReceiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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
                    Console.WriteLine("Register message sent...");

                SendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
