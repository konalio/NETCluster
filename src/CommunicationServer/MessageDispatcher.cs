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

            var no = new NoOperation { };
            //var no = new NoOperation { };
            //ConvertAndSendMessage<NoOperation>(no, tp.handler);

            //if (state == "Idle")
            //{
                if (_components[(int)id].type == "TaskManager")
                {
                    IClusterMessage cm = SearchTaskManagerMessages(id, tp.handler);

                    if (cm!=null && cm.GetType() == typeof(DivideProblem))
                    {
                        ConvertTwoMessages<DivideProblem, NoOperation>(cm as DivideProblem, no, tp.handler);
                       
                    }
                    else
                    {
                        ConvertAndSendMessage<NoOperation>(no, tp.handler);
                    }
                    
                }

                else if (_components[(int)id].type == "ComputationalNode")
                {
                    IClusterMessage cm = SearchComputationalNodeMessages(tp.handler);
                    if (cm != null && cm.GetType() == typeof(SolvePartialProblems))
                    {
                        ConvertTwoMessages<SolvePartialProblems, NoOperation>(cm as SolvePartialProblems, no, tp.handler);
                        //ConvertAndSendMessage<SolvePartialProblems>(cm as SolvePartialProblems, tp.handler);
                    }
                    else if (cm!=null && cm.GetType() == typeof(Solutions))
                    {
                        ConvertTwoMessages<Solutions, NoOperation>(cm as Solutions, no, tp.handler);
                        //ConvertAndSendMessage<Solutions>(cm as Solutions, tp.handler);

                    }
                    else
                    {
                        ConvertAndSendMessage<NoOperation>(no, tp.handler);
                    }
                }
            //}


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

            var sr = new SolveRequest
            {
                 Id = id,
                 ProblemType = problemType,
                 SolvingTimeout = timeout,
                 Data = data
            };
            _messageList.Add(sr);

            var srr = new SolveRequestResponse
            {
                Id=id
            };
            ConvertAndSendMessage<SolveRequestResponse>(srr, tp.handler);

        }

        public void HandleSolutionRequestMessages(ThreadPackage tp)
        {
            var id = GetXmlElementInnerUlong("Id", tp.message);
            SolutionsSolution[] solutions = new SolutionsSolution[1];
            var s = new Solutions 
            { 
                Id = id,                
                
            };

            if (_partialSolutions!=null && _partialSolutions.Count<(int)id && _partialSolutions[(int)id].Count == 5)
            {
                solutions[0] = new SolutionsSolution
                {
                    Type = SolutionsSolutionType.Final
                };
            }
            else
            {
                solutions[0] = new SolutionsSolution
                {
                    Type = SolutionsSolutionType.Ongoing
                };
            }

            s.Solutions1 = solutions;
            ConvertAndSendMessage<Solutions>(s, tp.handler);
        }

        public void HandlePartialProblemsMessages(ThreadPackage tp)
        {
            XmlDocument message = tp.message;
            

            var problemType = GetXmlElementInnerText("ProblemType", message);
            var commonData = GetXmlElementInnerByte("CommonData", message);
            var timeout = message.GetElementsByTagName("SolvingTimeout");
            var id = GetXmlElementInnerUlong("Id", message);

            var partialProblemTaskIds = message.GetElementsByTagName("TaskId");
            var partialProblemDatas = message.GetElementsByTagName("Data");
            var partialProblemNodeIds = message.GetElementsByTagName("NodeID");

            int count = partialProblemTaskIds.Count;

            SolvePartialProblemsPartialProblem[] partialproblems = new SolvePartialProblemsPartialProblem[1];
            var spp = new SolvePartialProblems
            {
                 Id = id,
                 CommonData = commonData,
                 ProblemType = problemType,
                 PartialProblems = partialproblems
            };          

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
                byte[] data = System.Text.Encoding.UTF8.GetBytes(partialProblemDatas[i].InnerText);
                ulong tID = UInt64.Parse(partialProblemTaskIds[i].InnerText);
                ulong nID = UInt64.Parse(partialProblemNodeIds[i].InnerText);
                var problem = new SolvePartialProblemsPartialProblem
                {
                    Data = data,
                    TaskId = tID,
                    NodeID = nID
                };
               
                partialproblems[0] = problem;
                spp.PartialProblems = partialproblems;
                _messageList.Add(spp);
            }
            
            var no = new NoOperation { };
            ConvertAndSendMessage<NoOperation>(no, tp.handler);
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

                var ss = new SolutionsSolution
                {
                    ComputationsTime =  UInt64.Parse(computationsTimes[0].InnerText),
                    Data = System.Text.Encoding.UTF8.GetBytes(datas[0].InnerText),
                    Type = SolutionsSolutionType.Partial,
                    TaskId = UInt64.Parse(taskIds[0].InnerText),
                    TaskIdSpecified=true
                };

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
            var no = new NoOperation { };
            ConvertAndSendMessage<NoOperation>(no, tp.handler);
        }

        public void ConvertTwoMessages<T,S>( T message1, S message2, Socket handler )
        {
            byte[] messageData1 = Serializers.ObjectToByteArray<T>(message1);
            byte[] messageData2 = Serializers.ObjectToByteArray<S>(message2);
            byte[] messageData = new byte[messageData1.Length + messageData2.Length + 1];
            messageData1.CopyTo(messageData, 0);
            messageData[messageData1.Length] = 23;            
            messageData2.CopyTo(messageData, messageData1.Length + 1);
            SendMessage(handler, messageData);
        
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
            var list = doc.GetElementsByTagName(s);
            if (list.Count == 0)
            {
                return "";
            }
            else
            {
                return list[0].InnerText;
            }
        }

        public ulong GetXmlElementInnerUlong(String s, XmlDocument doc)
        {
            String number = GetXmlElementInnerText(s, doc);  
            ulong value=0;
            try
            {
                value = UInt64.Parse(number);            
            }
            catch(FormatException ex)
            {
                return 0;
            }
            return value;
        }

        public byte[] GetXmlElementInnerByte(String s, XmlDocument doc)
        {
            return System.Text.Encoding.UTF8.GetBytes(GetXmlElementInnerText(s, doc));
        }

        public void AddPartialSolution(SolutionsSolution ss, ulong listID)
        {       
            if(_partialSolutions.Count<=(int)listID)
            {
                _partialSolutions.Add(new List<SolutionsSolution>());
            }

            _partialSolutions[(int)listID].Add(ss);

            if (_partialSolutions[(int)listID].Count == 5)
            {                
                var s = new Solutions
                {
                    Id = listID,
                    Solutions1 = _partialSolutions[0].ToArray()
                };                
                _messageList.Add(s);
            }
            
        }

        public IClusterMessage SearchTaskManagerMessages(ulong id, Socket handler)
        {
            int i = 0;
            const int timeout = 2;
            int time = 0;
            ManualResetEvent ev = new ManualResetEvent(false);

            while (time <= timeout)
            {
                while (_messageList.Count == 0 && time <= timeout)
                {
                    ev.WaitOne(100);
                    time++;
                }

                lock (_messageList)
                {
                    if (i >= _messageList.Count)
                    {
                        i = 0;
                        continue;
                    }
                    if (_messageList[i] is SolveRequest)
                    {

                        SolveRequest sr = _messageList[i] as SolveRequest;
                        var dp = new DivideProblem
                        {
                            Id = sr.Id,
                            ProblemType = sr.ProblemType,
                            NodeID = id,
                            Data = sr.Data
                        };

                        _messageList.Remove(_messageList[i]);
                        return dp;


                    }
                }
                ev.WaitOne(100);
                time++;
                i++;
            }
            return null;
        }

        public IClusterMessage SearchComputationalNodeMessages(Socket handler)
        {
            int i = 0;
            const int timeout = 2;
            int time = 0;
            ManualResetEvent ev = new ManualResetEvent(false);
            while (time<=timeout)
            {
                while (_messageList.Count == 0 && time <=timeout)
                {
                    ev.WaitOne(100);
                    time++;
                }

                if (i >= _messageList.Count)
                {
                    i = 0;
                    continue;
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
                ev.WaitOne(100);
                time++;
                i++;
            }
            return null;
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
