using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using ClusterUtils;
using ClusterUtils.Communication;
using ClusterMessages;

namespace CommunicationServer
{
    class ComponentStatus
    {
        public int id;
        public String type;
        public bool busy;

        public ComponentStatus(int idVal, String typeVal, bool busyVal)
        {
            id = idVal;
            type = typeVal;
            busy = busyVal;
        }
    }

    class MessageDispatcher
    {
        public static ManualResetEvent AllDone = new ManualResetEvent(false);

        private static int _componentCount;
        private readonly string _listeningPort;
        private readonly int _componentTimeout;

        private static Queue<IClusterMessage> otherMessagesQueue;
        private static Queue<IClusterMessage> statusMessagesQueue;
        private static List<ComponentStatus> componentsStatusList;
        
        public MessageDispatcher(ServerConfig configuration)
        {
            _listeningPort = configuration.ServerPort;           
            _componentTimeout = configuration.ComponentTimeout;
        }       
        

        public void BeginDispatching()
        {
            statusMessagesQueue = new Queue<IClusterMessage>();
            otherMessagesQueue = new Queue<IClusterMessage>();
            componentsStatusList = new List<ComponentStatus>();

            Thread th_1 = new Thread(new ParameterizedThreadStart(ListeningThread));
            th_1.Start(null);

            Thread th_2 = new Thread(MessageDispatcher.StatusThread);
            th_2.Start(null);

            Thread th_3 = new Thread(MessageDispatcher.TaskThread);
            th_3.Start(null);

        }

        public void ListeningThread(Object o)
        {               
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

        public static void StatusThread(Object o)
        {
            while (true)
            {
                if (statusMessagesQueue.Count == 0)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public static void TaskThread(Object o)
        {
            while (true)
            {
                if (otherMessagesQueue.Count == 0)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public static void MessageReadThread(Object m)
        {
            XmlDocument message = (XmlDocument) m;

            var elemIdList = message.GetElementsByTagName("Id");
            var elemList = message.GetElementsByTagName("Status");

            var elemIdString = elemIdList[0].InnerText;
            var elemIdNumber = UInt64.Parse(elemIdString);

            //Wiadomosc o Statusie
            if (elemList.Count != 0)
            {
                elemList = message.GetElementsByTagName("State");
                var state = elemList[0];

                if(state.InnerText =="Idle")
                {
                    componentsStatusList[(int) elemIdNumber].busy = false;
                }
                else
                {
                    componentsStatusList[(int)elemIdNumber].busy = true;
                }
            }

            else
            {
                
                elemList = message.GetElementsByTagName("Register");

                //Wiadomosc o Zarejestrowaniu
                if (elemList.Count != 0)
                {
                    var elemTypeList = message.GetElementsByTagName("Type");
                    ComponentStatus cs = new ComponentStatus(_componentCount, elemTypeList[0].InnerText, false);

                    _componentCount++;

                }
                //Pozostale wiadomosci (SolveRequest, SolutionRequest, PartialProblems)
                else
                {
                    elemList = message.GetElementsByTagName("SolveRequest");
                    if (elemList.Count != 0)
                    {
                    }
                }
            }       
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();

            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);

            var state = new StateObject { WorkSocket = handler };

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
                
                    Thread th = new Thread(MessageDispatcher.MessageReadThread);
                    th.Start(message);                                                
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }     

    }
}
