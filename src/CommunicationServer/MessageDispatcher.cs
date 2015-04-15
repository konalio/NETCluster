using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using ClusterMessages;
using ClusterUtils;
using ClusterUtils.Communication;

namespace CommunicationServer
{

    class MessageDispatcher
    {
        public static ManualResetEvent AllDone = new ManualResetEvent(false);
        private ulong _problemsCount;
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
        
        /// <summary>
        /// Starts listening thread
        /// </summary>
        public void BeginDispatching()
        {
            _messageList = new List<IClusterMessage>();
            _components = new List<ComponentStatus>();
            _partialSolutions = new List<List<SolutionsSolution>>();

            var th1 = new Thread(ListeningThread);
            th1.Start(null);
        }

        /// <summary>
        /// Listens for messages from other components
        /// </summary>
        /// <param name="o"></param>
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
        /// <summary>
        /// Analyzes different messages types
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void AnalyzeMessage(ThreadPackage tp)
        {
            var messageType = MessageTypeResolver.GetMessageType(tp.Message);
            WriteMessageHandling(messageType);
            switch (messageType)
            {
                case MessageTypeResolver.MessageType.Status:
                    HandleStateMessages(tp);
                    break;
                case MessageTypeResolver.MessageType.Register:
                    HandleRegisterMessages(tp);
                    break;
                case MessageTypeResolver.MessageType.SolveRequest:
                    HandleSolveRequestMessages(tp);
                    break;
                case MessageTypeResolver.MessageType.SolutionRequest:
                    HandleSolutionRequestMessages(tp);
                    break;
                case MessageTypeResolver.MessageType.PartialProblems:
                    HandlePartialProblemsMessages(tp);
                    break;
                case MessageTypeResolver.MessageType.Solution:
                    HandleSolutionMessages(tp);
                    break;
            }
        }

        public void WriteMessageHandling(MessageTypeResolver.MessageType message)
        {
            Console.WriteLine("Handling message type: " + message.ToString());
        }
        /// <summary>
        /// Handles State Messages from components
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void HandleStateMessages(ThreadPackage tp)
        {
            var id = GetXmlElementInnerUlong("Id", tp.Message);
            var state = GetXmlElementInnerText("State", tp.Message);
            var no = new NoOperation();

            //  The components do not inform server if they are busy or idle yet, that's why this part is
            //  commented at the moment
            //if (state == "Idle")
            //{
                switch (_components[(int)id].type)
                {
                    case "TaskManager":
                    {
                        var cm = SearchTaskManagerMessages(id, tp.Handler);

                        if (cm!=null && cm.GetType() == typeof(DivideProblem))
                        {
                            ConvertAndSendTwoMessages(cm as DivideProblem, no, tp.Handler);
                       
                        }
                        else if (cm != null && cm.GetType() == typeof(Solutions))
                        {
                            ConvertAndSendTwoMessages(cm as Solutions, no, tp.Handler);

                        }
                        else
                        {
                            ConvertAndSendMessage(no, tp.Handler);
                        }
                    
                    }
                        break;
                    case "ComputationalNode":
                    {
                        var cm = SearchComputationalNodeMessages(tp.Handler);
                        if (cm != null && cm.GetType() == typeof(SolvePartialProblems))
                        {
                            ConvertAndSendTwoMessages(cm as SolvePartialProblems, no, tp.Handler);
                        }
                   
                        else
                        {
                            ConvertAndSendMessage(no, tp.Handler);
                        }
                    }
                        break;
                }
            //}

            /*
            else
            {
                ConvertAndSendMessage<NoOperation>(no, tp.Handler);
            }
            */


        }

        /// <summary>
        /// Handles Register Messages from components
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void HandleRegisterMessages(ThreadPackage tp)
        {
            var type = GetXmlElementInnerText("Type", tp.Message);
            var solvableProblems = tp.Message.GetElementsByTagName("ProblemName");
            var problems = CreateArrayFromXml(solvableProblems);

            var cs = new ComponentStatus(_componentCount, type, problems);
            _components.Add(cs);
            _componentCount++;

            var rr = new RegisterResponse
            {
                Id = cs.id.ToString(), 
                Timeout = _componentTimeout.ToString()
            };

            ConvertAndSendMessage(rr, tp.Handler);
        }

        /// <summary>
        /// Handles SolveRequest Messages from Client
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void HandleSolveRequestMessages(ThreadPackage tp)
        {
            var problemType = GetXmlElementInnerText("ProblemType", tp.Message);
            var timeout = GetXmlElementInnerUlong("SolvingTimeout", tp.Message);
            var data = GetXmlElementInnerByte("Data", tp.Message);
            var id = _problemsCount++;
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
            ConvertAndSendMessage(srr, tp.Handler);

        }

        /// <summary>
        /// Handles SolutionRequest Messages from Client
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void HandleSolutionRequestMessages(ThreadPackage tp)
        {
            var id = GetXmlElementInnerUlong("Id", tp.Message);
            var solutions = new SolutionsSolution[1];
            var s = new Solutions 
            { 
                Id = id,                
                
            };

            if (_partialSolutions!=null && _partialSolutions.Count>(int)id && _partialSolutions[(int) id][0].Type== SolutionsSolutionType.Final )
            {
                solutions[0] = _partialSolutions[(int)id][0];
            }
            else
            {
                solutions[0] = new SolutionsSolution
                {
                    Type = SolutionsSolutionType.Ongoing
                };
            }

            s.Solutions1 = solutions;
            ConvertAndSendMessage(s, tp.Handler);
        }

        /// <summary>
        /// Handles SolvePartialProblem Messages from components and sends NoOperation response
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void HandlePartialProblemsMessages(ThreadPackage tp)
        {            
            var message = tp.Message;           
            var problemType = GetXmlElementInnerText("ProblemType", message);
            var commonData = GetXmlElementInnerByte("CommonData", message);
            var timeout = message.GetElementsByTagName("SolvingTimeout");
            var id = GetXmlElementInnerUlong("Id", message);

            var partialProblemTaskIds = message.GetElementsByTagName("TaskId");
            var partialProblemDatas = message.GetElementsByTagName("Data");
            var partialProblemNodeIds = message.GetElementsByTagName("NodeID");

            var count = partialProblemTaskIds.Count;

            for (var i = 0; i < count; i++)
            {
                var partialproblems = new SolvePartialProblemsPartialProblem[1];    
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

                var data = Encoding.UTF8.GetBytes(partialProblemDatas[i].InnerText);
                var tId = UInt64.Parse(partialProblemTaskIds[i].InnerText);
                var nId = UInt64.Parse(partialProblemNodeIds[i].InnerText);
                
                var problem = new SolvePartialProblemsPartialProblem
                {
                    Data = data,
                    TaskId = tId,
                    NodeID = nId
                };
               
                partialproblems[0] = problem;
                spp.PartialProblems = partialproblems;
                _messageList.Add(spp);
            }

            SendNoOperationMessage(tp);
        }

        /// <summary>
        /// Handles Solution Messages from components and sends NoOperation response. If Solution is Final, prepares it to send it to Client. 
        /// If Solution is Partial, adds it to the _partialSolutions list
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void HandleSolutionMessages(ThreadPackage tp)
        {
            var message = tp.Message;
            var types = message.GetElementsByTagName("Type");          

            switch (types[0].InnerText)
            {
                case "Final":
                    HandleFinalSolutionMessages(tp);
                    break;
                case "Partial":
                    HandlePartialSolutionMessages(tp);
                    break;
            }
            SendNoOperationMessage(tp);
        }
        /// <summary>
        /// Handles Partial Solutions Messages from components
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void HandleFinalSolutionMessages(ThreadPackage tp)
        {
            var message = tp.Message;
            var taskIds = message.GetElementsByTagName("TaskId");
            var datas = message.GetElementsByTagName("Data");
            var computationsTimes = message.GetElementsByTagName("ComputationsTime");
            var id = GetXmlElementInnerUlong("Id", message);

            var ss = new SolutionsSolution
            {
                ComputationsTime = UInt64.Parse(computationsTimes[0].InnerText),
                Data = Encoding.UTF8.GetBytes(datas[0].InnerText),
                Type = SolutionsSolutionType.Final,
                TaskId = UInt64.Parse(taskIds[0].InnerText),
                TaskIdSpecified = true
            };
            _partialSolutions[(int)id].Clear();
            _partialSolutions[(int)id].Add(ss);
        }

        /// <summary>
        /// Handles Final Solution from components
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void HandlePartialSolutionMessages(ThreadPackage tp)
        {

            var message = tp.Message;
            var timeoutOccureds = message.GetElementsByTagName("TimeoutOccured");
            var taskIds = message.GetElementsByTagName("TaskId");
            var datas = message.GetElementsByTagName("Data");
            var computationsTimes = message.GetElementsByTagName("ComputationsTime");
            var id = GetXmlElementInnerUlong("Id", message);
            var ss = new SolutionsSolution
            {
                ComputationsTime = UInt64.Parse(computationsTimes[0].InnerText),
                Data = Encoding.UTF8.GetBytes(datas[0].InnerText),
                Type = SolutionsSolutionType.Partial,
                TaskId = UInt64.Parse(taskIds[0].InnerText),
                TaskIdSpecified = true
            };

            if (timeoutOccureds.Count > 0 && timeoutOccureds[0].InnerText == "true")
            {
                ss.TimeoutOccured = true;
            }
            else
            {
                ss.TimeoutOccured = false;
            }
            AddPartialSolution(ss, id);
        }

        /// <summary>
        /// Converts two Messages of different types to the binary array data and sends them to component
        /// </summary>
        /// <typeparam name="T">Type of the first message</typeparam>
        /// <typeparam name="TS">Type of the second message</typeparam>
        /// <param name="message1">First message</param>
        /// <param name="message2">Second message</param>
        /// <param name="handler">Socket handler of the component, that messages will be sent to</param>
        public void ConvertAndSendTwoMessages<T,TS>( T message1, TS message2, Socket handler )
        {
            var messageData1 = Serializers.ObjectToByteArray(message1);
            var messageData2 = Serializers.ObjectToByteArray(message2);
            var messageData = new byte[messageData1.Length + messageData2.Length + 1];
            messageData1.CopyTo(messageData, 0);
            messageData[messageData1.Length] = 23;            
            messageData2.CopyTo(messageData, messageData1.Length + 1);
            SendMessage(handler, messageData);
        
        }

        /// <summary>
        /// Converts a message to binary array data and sends it to component
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="message">Message</param>
        /// <param name="handler">Socket handler of the component, that message will be sent to</param>
        public void ConvertAndSendMessage<T>( T message, Socket handler )
        {
            var messageData = Serializers.ObjectToByteArray(message);
            SendMessage(handler, messageData);
        }

        /// <summary>
        /// Send NoOperation message to component
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument message</param>
        public void SendNoOperationMessage(ThreadPackage tp)
        {
            var no = new NoOperation();
            ConvertAndSendMessage(no, tp.Handler);
        }

        /// <summary>
        /// Creates String array from XmlNodeList element
        /// </summary>
        /// <param name="xmlElementsList">XmlNodeList element</param>
        /// <returns></returns>
        public String[] CreateArrayFromXml(XmlNodeList xmlElementsList)
        {
            var count = xmlElementsList.Count;
            var array = new String[count];

            for (var i = 0; i < count; i++)
            {
                var rp = xmlElementsList[i].InnerText;
                array[i] = rp;
            }
            return array;
        }

        /// <summary>
        /// Finds text value of the element in the XmlDocument
        /// </summary>
        /// <param name="s">Name of the element</param>
        /// <param name="doc">XmlDocument we are searching in</param>
        /// <returns></returns>
        public String GetXmlElementInnerText(String s, XmlDocument doc)
        {
            var list = doc.GetElementsByTagName(s);
            return list.Count == 0 ? "" : list[0].InnerText;
        }

        /// <summary>
        /// Finds ulong value of the element in the XmlDocument
        /// </summary>
        /// <param name="s">Name of the element</param>
        /// <param name="doc">XmlDocument we are searching in</param>
        /// <returns></returns>
        public ulong GetXmlElementInnerUlong(String s, XmlDocument doc)
        {
            var number = GetXmlElementInnerText(s, doc);  
            ulong value;
            try
            {
                value = UInt64.Parse(number);            
            }
            catch(FormatException)
            {
                return 0;
            }
            return value;
        }

        /// <summary>
        /// Finds byte[] value of the element in the XmlDocument
        /// </summary>
        /// <param name="s">Name of the element</param>
        /// <param name="doc">XmlDocument we are searching in</param>
        /// <returns></returns>
        public byte[] GetXmlElementInnerByte(String s, XmlDocument doc)
        {
            return Encoding.UTF8.GetBytes(GetXmlElementInnerText(s, doc));
        }

        /// <summary>
        /// Adds Parital Solution to the _partialSolutions list
        /// </summary>
        /// <param name="ss">PartialSolution that is added to the list</param>
        /// <param name="listId">ID of _partialSolutions list</param>
        public void AddPartialSolution(SolutionsSolution ss, ulong listId)
        {       
            if(_partialSolutions.Count<=(int)listId)
            {
                _partialSolutions.Add(new List<SolutionsSolution>());
            }

            _partialSolutions[(int)listId].Add(ss);

            if (_partialSolutions[(int) listId].Count != 5) return;

            var s = new Solutions
            {
                CommonData=new byte[1],
                ProblemType = "",
                Id = listId //, Solutions1 =_partialSolutions[(int)listID].ToArray()
            };            
            _messageList.Add(s);
        }
        /// <summary>
        /// Searches listed messages (SolveRequest and Solutions) for TaskManager
        /// </summary>
        /// <param name="id">TaskManager id</param>
        /// <param name="handler">Handler to the TaskManager</param>
        /// <returns></returns>
        public IClusterMessage SearchTaskManagerMessages(ulong id, Socket handler)
        {
            var i = 0;
            const int timeout = 2;
            var time = 0;
            var ev = new ManualResetEvent(false);

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
                    if (_messageList[i] is Solutions)
                    {
                        Solutions s = _messageList[i] as Solutions;
                        _messageList.Remove(_messageList[i]);
                        return s;
                    }
                }
                ev.WaitOne(100);
                time++;
                i++;
            }
            return null;
        }

        /// <summary>
        /// Searches listed messages (SolvePartialProblems) for ComputationalNode
        /// </summary>
        /// <param name="handler">Handler to the ComputationalNode</param>
        /// <returns></returns>

        public IClusterMessage SearchComputationalNodeMessages(Socket handler)
        {
            var i = 0;
            const int timeout = 2;
            var time = 0;
            var ev = new ManualResetEvent(false);
            while (time<=timeout)
            {
                while (_messageList.Count == 0 && time <=timeout)
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
                    var problems = _messageList[i] as SolvePartialProblems;
                    if (problems != null)
                    {
                        var spp = problems;
                        _messageList.Remove(problems);
                        return spp;
                    }
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

                    var tp = new ThreadPackage(handler, message);
                    var th = new Thread(MessageReadThread);
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
