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
        public ulong id;
        public String type;
        public RegisterSolvableProblemsProblemName[] solvableProblems;

        public ComponentStatus(ulong idVal, String typeVal, RegisterSolvableProblemsProblemName[] problemsVal)
        {
            id = idVal;
            type = typeVal;
            solvableProblems = problemsVal;
        }
    }

    class ThreadPackage
    {
        public Socket handler;
        public XmlDocument message;
        public ThreadPackage(Socket h, XmlDocument m)
        {
            handler = h;
            message = m;
        }
    }

    class MessageDispatcher
    {
        public static ManualResetEvent AllDone = new ManualResetEvent(false);

        private ulong _componentCount;
        private readonly string _listeningPort;
        private readonly int _componentTimeout;
        
        private List<IClusterMessage> messageList;
        private List<ComponentStatus> components;
        
        public MessageDispatcher(string listport, int timeout)
        {
            _listeningPort = listport;       
            _componentTimeout = timeout;
        }       
        

        public void BeginDispatching()
        {
            messageList = new List<IClusterMessage>();
            components = new List<ComponentStatus>();

            Thread th_1 = new Thread(new ParameterizedThreadStart(ListeningThread));
            th_1.Start(null);
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

        public void MessageReadThread(Object m)
        {
            XmlDocument message = ((ThreadPackage)m).message;
            Socket handler = ((ThreadPackage)m).handler;

            MessageTypeResolver.MessageType messageType = MessageTypeResolver.GetMessageType(message);
            MessageAnalyze(message,messageType,handler);
           
        }
        
        public void MessageAnalyze(XmlDocument message, MessageTypeResolver.MessageType messageType, Socket handler)
        {            
            if (messageType == MessageTypeResolver.MessageType.Status)
            {
                var id = UInt64.Parse(message.GetElementsByTagName("Id")[0].InnerText);
                var state = message.GetElementsByTagName("State")[0];
                if (state.InnerText == "Idle")
                {
                    while (messageList.Count == 0)
                    {
                        Thread.Sleep(100);
                    }
                    if(components[(int)id].type=="TaskManager")
                    {
                        SearchTaskManagerMessages(id,handler);
                    }
                    else if(components[(int)id].type=="ComputationalNode")
                    {
                        SearchComputationalNodeMessages(handler);
                    } 
                }

                NoOperation no = new NoOperation();
                byte[] messageData = Serializers.ObjectToByteArray<NoOperation>(no);
                SendMessage(handler, messageData);
            }

            else if (messageType == MessageTypeResolver.MessageType.Register)
            {
                var type = message.GetElementsByTagName("Type")[0];
                var solvableProblems = message.GetElementsByTagName("ProblemName");

                int count = solvableProblems.Count;
                RegisterSolvableProblemsProblemName[] problems = new RegisterSolvableProblemsProblemName[count];

                for (int i = 0; i < count;i++ )
                {
                    RegisterSolvableProblemsProblemName rp = new RegisterSolvableProblemsProblemName();
                    rp.Value = solvableProblems[i].InnerText;
                    problems[i] = rp;
                }
                ComponentStatus cs = new ComponentStatus(_componentCount, type.InnerText, problems);
                components.Add(cs);
                _componentCount++;

                RegisterResponse rr = new RegisterResponse();
                rr.Id = cs.id.ToString();
                rr.Timeout = _componentTimeout.ToString();                
               
                byte[] messageData = Serializers.ObjectToByteArray<RegisterResponse>(rr);
                SendMessage(handler,messageData);
            }
            else if (messageType == MessageTypeResolver.MessageType.SolveRequest)
            {
                SolveRequest sr = new SolveRequest();

                var problemType = message.GetElementsByTagName("ProblemType")[0];
                var timeout = message.GetElementsByTagName("SolvingTimeout")[0];
                var data = message.GetElementsByTagName("Data")[0];
                var id = UInt64.Parse(message.GetElementsByTagName("Id")[0].InnerText);

                sr.Id = id;
                sr.ProblemType = problemType.InnerText;
                sr.SolvingTimeout = UInt64.Parse(timeout.InnerText);
                sr.Data = System.Text.Encoding.ASCII.GetBytes(data.InnerText);
                messageList.Add(sr);

                SolveRequestResponse srr = new SolveRequestResponse();
                srr.Id = id;
                byte[] messageData = Serializers.ObjectToByteArray<SolveRequestResponse>(srr);
                SendMessage(handler, messageData);

            }
            else if (messageType == MessageTypeResolver.MessageType.SolutionRequest)
            {
                var id = UInt64.Parse(message.GetElementsByTagName("Id")[0].InnerText);
                Solutions s = new Solutions();
                s.Id = id;                
                
                byte[] messageData = Serializers.ObjectToByteArray<Solutions>(s);
                SendMessage(handler, messageData);
                
            }
            else if (messageType == MessageTypeResolver.MessageType.PartialProblems)
            {
                SolvePartialProblems spp = new SolvePartialProblems();
                var problemType = message.GetElementsByTagName("ProblemType")[0];
                var commonData = message.GetElementsByTagName("CommonData")[0];
                var timeout = message.GetElementsByTagName("SolvingTimeout");
                var id = UInt64.Parse(message.GetElementsByTagName("Id")[0].InnerText);

                var partialProblemTaskIds = message.GetElementsByTagName("TaskId");
                var partialProblemDatas = message.GetElementsByTagName("Data");
                var partialProblemNodeIds = message.GetElementsByTagName("NodeId");

                int count = partialProblemTaskIds.Count;

                SolvePartialProblemsPartialProblem[] partialproblems = new SolvePartialProblemsPartialProblem[count];
                for(int i=0;i<count;i++)
                {
                    SolvePartialProblemsPartialProblem problem = new SolvePartialProblemsPartialProblem();
                    problem.Data = System.Text.Encoding.ASCII.GetBytes(partialProblemDatas[i].InnerText);
                    problem.TaskId = UInt64.Parse(partialProblemTaskIds[i].InnerText);
                    problem.NodeID = UInt64.Parse(partialProblemNodeIds[i].InnerText);

                    partialproblems[i] = problem;

                }
                spp.Id = id;
                spp.CommonData = System.Text.Encoding.ASCII.GetBytes(commonData.InnerText);
                spp.ProblemType = problemType.InnerText;
                spp.PartialProblems = partialproblems;

                if (timeout.Count != 0)
                {
                    spp.SolvingTimeout = UInt64.Parse(timeout[0].InnerText);
                    spp.SolvingTimeoutSpecified = true;
                }
                else
                {
                    spp.SolvingTimeoutSpecified = false;
                }
                messageList.Add(spp);
            }
        }        

        public void SearchTaskManagerMessages(ulong id, Socket handler)
        {
            int i = 0;
            while (true)
            {
                if (i > messageList.Count)
                {
                    i = 0;
                }
                if (messageList[i] is SolveRequest)
                {

                    SolveRequest m = messageList[i] as SolveRequest;
                    DivideProblem dp = new DivideProblem();
                    dp.Id = m.Id;
                    dp.ProblemType = m.ProblemType;
                    dp.NodeID = id;
                    dp.Data = m.Data;
                    //dp.ComputationalNodes ???

                    byte[] messageData = Serializers.ObjectToByteArray<DivideProblem>(dp);
                    SendMessage(handler, messageData);
                    messageList.Remove(messageList[i]);                    
                    break;
                }
                i++;
            }
        }

        public void SearchComputationalNodeMessages(Socket handler)
        {
            int i = 0;
            while (true)
            {
                if (i > messageList.Count)
                {
                    i = 0;                    
                }
                if (messageList[i] is SolvePartialProblems)
                {
                    SolvePartialProblems m = messageList[i] as SolvePartialProblems;

                    byte[] messageData = Serializers.ObjectToByteArray<SolvePartialProblems>(m);
                    SendMessage(handler, messageData);

                    messageList.Remove(messageList[i]);
                    break;
                }
                i++;
            }
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();

            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);

            var state = new StateObject { WorkSocket = handler };

            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
        }

        public void ReadCallback(IAsyncResult ar)
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

                    ThreadPackage tp = new ThreadPackage(handler, message);
                    Thread th = new Thread(new ParameterizedThreadStart(MessageReadThread));
                    th.Start(tp);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }
        public static void SendMessage(Socket handler, byte[] byteData)
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
