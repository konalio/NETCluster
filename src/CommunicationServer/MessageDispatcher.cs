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
        public String[] solvableProblems;

        public ComponentStatus(ulong idVal, String typeVal, String[] problemsVal)
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
        
        private List<IClusterMessage> _messageList;
        private List<ComponentStatus> _components;

        private List<List<SolutionsSolution>> _partialSolutions;
        
        public MessageDispatcher(string listport, int timeout)
        {
            _listeningPort = listport;                   
            _componentTimeout = timeout;
        }       
        

        public void BeginDispatching()
        {
            _messageList = new List<IClusterMessage>();
            _components = new List<ComponentStatus>();
            _partialSolutions = new List<List<SolutionsSolution>>();

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

        public void MessageReadThread(Object obj)
        {
            ThreadPackage tp = obj as ThreadPackage;
            AnalyzeMessage(tp);
           
        }

        public void AnalyzeMessage(ThreadPackage tp)
        {            
            MessageTypeResolver.MessageType messageType = MessageTypeResolver.GetMessageType(tp.message);

            if (messageType == MessageTypeResolver.MessageType.Status)
            {
                HandleStateMessages(tp);
            }

            else if (messageType == MessageTypeResolver.MessageType.Register)
            {

                HandleRegisterMessages(tp);
               
            }
            else if (messageType == MessageTypeResolver.MessageType.SolveRequest)
            {
                HandleSolveRequestMessages(tp);

            }
            else if (messageType == MessageTypeResolver.MessageType.SolutionRequest)
            {
                HandleSolutionRequestMessages(tp);
                
            }
            else if (messageType == MessageTypeResolver.MessageType.PartialProblems)
            {
                HandlePartialProblemsMessages(tp);     
            }

            else if (messageType == MessageTypeResolver.MessageType.Solution)
            {
                HandleSolutionMessages(tp);
            }
        }

        public void HandleStateMessages(ThreadPackage tp)
        {
            var id = GetXmlElementInnerUlong("Id", tp.message);
            var state = GetXmlElementInnerText("State", tp.message);

            if (state == "Idle")
            {
                if (_components[(int)id].type == "TaskManager")
                {
                    IClusterMessage cm = SearchTaskManagerMessages(id, tp.handler);

                    if (cm.GetType() == typeof(DivideProblem))
                    {
                        ConvertAndSendMessage<DivideProblem>(cm as DivideProblem, tp.handler);
                    }
                    
                }

                else if (_components[(int)id].type == "ComputationalNode")
                {
                    IClusterMessage cm = SearchComputationalNodeMessages(tp.handler);
                    if (cm.GetType() == typeof(SolvePartialProblems))
                    {
                        ConvertAndSendMessage<SolvePartialProblems>(cm as SolvePartialProblems, tp.handler);
                    }
                    else if (cm.GetType() == typeof(Solutions))
                    {
                        ConvertAndSendMessage<Solutions>(cm as Solutions, tp.handler);

                    }
                }
            }

            NoOperation no = new NoOperation();
            ConvertAndSendMessage<NoOperation>(no, tp.handler);
        }

        public void HandleRegisterMessages(ThreadPackage tp)
        {
            var type = GetXmlElementInnerText("Type", tp.message);
            var solvableProblems = tp.message.GetElementsByTagName("ProblemName");
            String[] problems = CreateArrayFromXml(solvableProblems);

            ComponentStatus cs = new ComponentStatus(_componentCount, type, problems);
            _components.Add(cs);
            _componentCount++;

            RegisterResponse rr = new RegisterResponse();
            rr.Id = cs.id.ToString();
            rr.Timeout = _componentTimeout.ToString();

            ConvertAndSendMessage<RegisterResponse>(rr, tp.handler);
        }

        public void HandleSolveRequestMessages(ThreadPackage tp)
        {
            var problemType = GetXmlElementInnerText("ProblemType", tp.message);
            var timeout = GetXmlElementInnerUlong("SolvingTimeout", tp.message);
            var data = GetXmlElementInnerByte("Data", tp.message);
            var id = GetXmlElementInnerUlong("Id", tp.message);

            SolveRequest sr = new SolveRequest();
            sr.Id = id;
            sr.ProblemType = problemType;
            sr.SolvingTimeout = timeout;
            sr.Data = data;
            _messageList.Add(sr);

            SolveRequestResponse srr = new SolveRequestResponse();
            srr.Id = id;

            ConvertAndSendMessage<SolveRequestResponse>(srr, tp.handler);

        }

        public void HandleSolutionRequestMessages(ThreadPackage tp)
        {
            var id = GetXmlElementInnerUlong("Id", tp.message);
            Solutions s = new Solutions();
            s.Id = id;

            ConvertAndSendMessage<Solutions>(s, tp.handler);
        }

        public void HandlePartialProblemsMessages(ThreadPackage tp)
        {
            XmlDocument message = tp.message;
            SolvePartialProblems spp = new SolvePartialProblems();

            var problemType = GetXmlElementInnerText("ProblemType", message);
            var commonData = GetXmlElementInnerByte("CommonData", message);
            var timeout = message.GetElementsByTagName("SolvingTimeout");
            var id = GetXmlElementInnerUlong("Id", message);

            var partialProblemTaskIds = message.GetElementsByTagName("TaskId");
            var partialProblemDatas = message.GetElementsByTagName("Data");
            var partialProblemNodeIds = message.GetElementsByTagName("NodeId");

            int count = partialProblemTaskIds.Count;

            SolvePartialProblemsPartialProblem[] partialproblems = new SolvePartialProblemsPartialProblem[1];

            spp.Id = id;
            spp.CommonData = commonData;
            spp.ProblemType = problemType;
            spp.PartialProblems = partialproblems;

            if (timeout.Count != 0)
            {
                spp.SolvingTimeout = GetXmlElementInnerUlong("SolvingTimeout", message);
                spp.SolvingTimeoutSpecified = true;
            }
            else
            {
                spp.SolvingTimeoutSpecified = false;
            }

            for (int i = 0; i < count; i++)
            {
                SolvePartialProblemsPartialProblem problem = new SolvePartialProblemsPartialProblem();
                problem.Data = System.Text.Encoding.UTF8.GetBytes(partialProblemDatas[i].InnerText);
                problem.TaskId = UInt64.Parse(partialProblemTaskIds[i].InnerText);
                problem.NodeID = UInt64.Parse(partialProblemNodeIds[i].InnerText);

                partialproblems[0] = problem;
                spp.PartialProblems = partialproblems;
                _messageList.Add(spp);
            }
        }

        public void HandleSolutionMessages(ThreadPackage tp)
        {
            XmlDocument message = tp.message;
            var taskIds = message.GetElementsByTagName("TaskId");
            var timeoutOccureds = message.GetElementsByTagName("TimeoutOccured");
            var types = message.GetElementsByTagName("Type");
            var datas = message.GetElementsByTagName("Data");
            var computationsTimes = message.GetElementsByTagName("ComputationsTime");

            var commonData = GetXmlElementInnerByte("CommonData", message);
            var id = GetXmlElementInnerUlong("Id", message);
            var problemType = GetXmlElementInnerText("ProblemType", message);

            if (types[0].InnerText == "Final")
            {
                //wyslac do CC
            }
            else if (types[0].InnerText == "Partial")
            {
                SolutionsSolution ss = new SolutionsSolution();
                ss.ComputationsTime = UInt64.Parse(computationsTimes[0].InnerText);
                ss.Data = System.Text.Encoding.UTF8.GetBytes(datas[0].InnerText);
                ss.Type = SolutionsSolutionType.Partial;
                ss.TaskId = UInt64.Parse(taskIds[0].InnerText);

                if (timeoutOccureds[0].InnerText == "true")
                {
                    ss.TimeoutOccured = true;
                }
                else
                {
                    ss.TimeoutOccured = false;
                }
                AddPartialSolution(ss, id);
            }
        }

        public void ConvertAndSendMessage<T>( T message, Socket handler )
        {
            byte[] messageData = Serializers.ObjectToByteArray<T>(message);
            SendMessage(handler, messageData);
        }

        public String[] CreateArrayFromXml(XmlNodeList xmlElementsList)
        {
            int count = xmlElementsList.Count;
            String[] array = new String[count];

            for (int i = 0; i < count; i++)
            {
                String rp = xmlElementsList[i].InnerText;
                array[i] = rp;
            }
            return array;
        }

        public String GetXmlElementInnerText(String s, XmlDocument doc)
        {
            return doc.GetElementsByTagName(s)[0].InnerText;
        }

        public ulong GetXmlElementInnerUlong(String s, XmlDocument doc)
        {
            return UInt64.Parse(doc.GetElementsByTagName(s)[0].InnerText);            
        }

        public byte[] GetXmlElementInnerByte(String s, XmlDocument doc)
        {
            return System.Text.Encoding.UTF8.GetBytes(GetXmlElementInnerText(s, doc));

        }

        public void AddPartialSolution(SolutionsSolution ss, ulong listID)
        {       
            if(_partialSolutions.Count<(int)listID)
            {
                _partialSolutions.Add(new List<SolutionsSolution>());
            }

            _partialSolutions[(int)listID].Add(ss);

            if (_partialSolutions[(int)listID].Count == 5)
            {
                Solutions s = new Solutions();                
                s.Id = listID;
                s.Solutions1 = _partialSolutions[0].ToArray();
                _messageList.Add(s);
            }
            
        }

        public IClusterMessage SearchTaskManagerMessages(ulong id, Socket handler)
        {
            int i = 0;
            while (true)
            {
                while (_messageList.Count == 0)
                {
                    Thread.Sleep(100);
                }

                if (i > _messageList.Count)
                {
                    i = 0;
                }

                if (_messageList[i] is SolveRequest)
                {

                    SolveRequest sr = _messageList[i] as SolveRequest;
                    DivideProblem dp = new DivideProblem();
                    dp.Id = sr.Id;
                    dp.ProblemType = sr.ProblemType;
                    dp.NodeID = id;
                    dp.Data = sr.Data;
                                        
                    _messageList.Remove(_messageList[i]);
                    return dp;
               
                    
                }
                i++;
            }
        }

        public IClusterMessage SearchComputationalNodeMessages(Socket handler)
        {
            int i = 0;
            while (true)
            {
                while (_messageList.Count == 0)
                {
                    Thread.Sleep(100);
                }

                if (i > _messageList.Count)
                {
                    i = 0;                    
                }
                if (_messageList[i] is SolvePartialProblems)
                {
                    SolvePartialProblems spp = _messageList[i] as SolvePartialProblems;
                    _messageList.Remove(_messageList[i]);
                    return spp;
                }
                if (_messageList[i] is Solutions)
                {
                    Solutions s = _messageList[i] as Solutions;
                    _messageList.Remove(_messageList[i]);
                    return s;
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
