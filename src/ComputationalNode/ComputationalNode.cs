using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClusterUtils;

namespace ComputationalNode
{
    class ComputationalNode
    {
        public string ServerPort { get; set; }

        public string ServerAddress { get; set; }

        // ManualResetEvent instances signal completion.
        private static readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);
        private static readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);
        private static readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);

        private static RegisterResponse _response;

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
            try
            {
                var remoteEP = new IPEndPoint(IPAddress.Parse(ServerAddress), int.Parse(ServerPort));

                var client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEP,
                    ConnectCallback, client);
                ConnectDone.WaitOne();

                var message = new Register
                {
                    Type = "ComputationalNode"
                };

                Send(client, Serializers.ObjectToByteArray(message));
                SendDone.WaitOne();

                Receive(client);
                ReceiveDone.WaitOne();

                Console.WriteLine("Registered at server with Id: {0}.", _response.Id);

                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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
                } else
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

        private void LogNodeInfo()
        {
            Console.WriteLine("Node is running...");
            Console.WriteLine("Server address: {0}", ServerAddress);
            Console.WriteLine("Server port: {0}", ServerPort);
        }
    }
}
