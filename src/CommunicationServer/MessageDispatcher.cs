using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
        private object _lockingObject;
        private readonly string _listeningPort;
        private readonly int _componentTimeout;

        private List<IClusterMessage> _messageList;
        private Dictionary<int, ComponentStatus> _components;

        private readonly List<ProblemInstance> _problemInstances = new List<ProblemInstance>();
      
        private Dictionary<int, Dictionary<int, Dictionary<int, SolvePartialProblems>>> _reservedPartialProblem =
            new Dictionary<int, Dictionary<int, Dictionary<int, SolvePartialProblems>>>();

      

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
            _lockingObject = new object();
            _componentCount = 0;
            _components = new Dictionary<int, ComponentStatus>();

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
            Console.WriteLine("Address: {0}", localEndPoint.Address);
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
            var tp = obj as ThreadPackage;
            AnalyzeMessage(tp);

        }
        /// <summary>
        /// Analyzes different messages types
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void AnalyzeMessage(ThreadPackage tp)
        {
            var messageType = MessageTypeResolver.GetMessageType(tp.Message.XmlMessage);
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
                case MessageTypeResolver.MessageType.Error:
                    HandleErrorMessage(tp);
                    break;
            }
        }

        private void HandleErrorMessage(ThreadPackage package)
        {
            var errorMessage = (Error)package.Message.ClusterMessage;
            switch (errorMessage.ErrorType)
            {
                case ErrorErrorType.ExceptionOccured:
                    Console.WriteLine("Error message from server: Exception occured.");
                    break;
                case ErrorErrorType.InvalidOperation:
                    Console.WriteLine("Error message from server: Invalid operation.");
                    break;
                case ErrorErrorType.UnknownSender:
                    Console.WriteLine("Error message from server: Unknow sender.");
                    break;
            }
            Console.WriteLine(errorMessage.ErrorMessage);
            package.Handler.Shutdown(SocketShutdown.Both);
        }

        public void WriteMessageHandling(MessageTypeResolver.MessageType message)
        {
            Console.WriteLine("Handling ClusterMessage type: " + message.ToString());
        }

        /// <summary>
        /// Handles State Messages from components
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void HandleStateMessages(ThreadPackage tp)
        {
            var message = (Status)tp.Message.ClusterMessage;
            var id = message.Id;
            var threads = message.Threads;
            var noOperationResponse = new NoOperation();
            ComponentStatus cs;

            try
            {                
                cs = _components[(int)id];
            }
            catch (Exception)
            {
                Error error = new Error() { ErrorType = ErrorErrorType.UnknownSender };
                ConvertAndSendMessage<Error>(error, tp.Handler);
                return;
            }            

            cs.StatusOccured = true;

            //  The components do not inform server if they are busy or idle yet, that's why this part is
            //  commented at the moment
            //if (state == "Idle")
            //{
            switch (cs.type)
            {
                case "TaskManager":
                    {
                        var cm = SearchTaskManagerMessages(id, tp.Handler);

                        if (cm != null && cm.GetType() == typeof(DivideProblem))
                        {
                            ConvertAndSendTwoMessages(cm as DivideProblem, noOperationResponse, tp.Handler);

                        } else if (cm != null && cm.GetType() == typeof(Solutions))
                        {
                            ConvertAndSendTwoMessages(cm as Solutions, noOperationResponse, tp.Handler);

                        } else
                        {
                            ConvertAndSendMessage(noOperationResponse, tp.Handler);
                        }

                    }
                    break;
                case "ComputationalNode":
                    {
                        var cm = SearchComputationalNodeMessages((int)id,tp.Handler);
                        if (cm != null && cm.GetType() == typeof(SolvePartialProblems))
                        {                            
                            ConvertAndSendTwoMessages(cm as SolvePartialProblems, noOperationResponse, tp.Handler);
                        } else
                        {
                            ConvertAndSendMessage(noOperationResponse, tp.Handler);
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
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void HandleRegisterMessages(ThreadPackage tp)
        {
            var message = (Register)tp.Message.ClusterMessage;           
            
            lock (_lockingObject)
            {

                var registeredComponent = new ComponentStatus(
                    _componentCount,
                    message.Type,
                    message.SolvableProblems.Select(problemsWrapper => problemsWrapper.Value).ToArray()
                );
                _components.Add((int)_componentCount, registeredComponent);

                _componentCount++;

                var responseMessage = new RegisterResponse
                {
                    Id = registeredComponent.id.ToString(),
                    Timeout = _componentTimeout.ToString()
                };

                var thread = new Thread(CheckComponentTimeout);
                thread.Start((int)registeredComponent.id);
                ConvertAndSendMessage(responseMessage, tp.Handler);
            }
           
        }

        /// <summary>
        /// Handles SolveRequest Messages from Client
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void HandleSolveRequestMessages(ThreadPackage tp)
        {
            var message = (SolveRequest)tp.Message.ClusterMessage;

            var sr = new SolveRequest
            {
                Id = _problemsCount++,
                ProblemType = message.ProblemType,
                SolvingTimeout = message.SolvingTimeout,
                Data = message.Data
            };
            _messageList.Add(sr);

            _problemInstances.Add(new ProblemInstance
            {
                CommonData = message.Data,
                Id = sr.Id
            });

            var responseForClient = new SolveRequestResponse
            {
                Id = sr.Id
            };
            ConvertAndSendMessage(responseForClient, tp.Handler);
        }

        /// <summary>
        /// Handles SolutionRequest Messages from Client
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void HandleSolutionRequestMessages(ThreadPackage tp)
        {
            var message = (SolutionRequest)tp.Message.ClusterMessage;

            var id = message.Id;
            var solutions = new SolutionsSolution[1];

            var requestedSolution = new Solutions
            {
                Id = id
            };

            var problemInstance = _problemInstances.Find(pi => pi.Id == id);

            if (problemInstance == null)
                throw new Exception("Send error message - unknown problem.");

            if (problemInstance.FinalSolutionFound)
            {
                solutions[0] = problemInstance.FinalSolution;
            } else
            {
                solutions[0] = new SolutionsSolution
                {
                    Type = SolutionsSolutionType.Ongoing
                };
            }

            requestedSolution.Solutions1 = solutions;
            ConvertAndSendMessage(requestedSolution, tp.Handler);
        }

        /// <summary>
        /// Handles SolvePartialProblem Messages from components and sends NoOperation response
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void HandlePartialProblemsMessages(ThreadPackage tp)
        {
            var message = (SolvePartialProblems)tp.Message.ClusterMessage;

            var partialProblems = message.PartialProblems;

            var problemInstance = _problemInstances.Find(pi => pi.Id == message.Id);

            if (problemInstance == null)
                throw new Exception("Send error message - unknown problem.");

            problemInstance.SubproblemsCount = partialProblems.Count();

            foreach (var dividedPartialProblem in partialProblems)
            {
                var singleProblemArray = new SolvePartialProblemsPartialProblem[1];
                var singlePartialProblem = new SolvePartialProblems
                {
                    Id = message.Id,
                    CommonData = message.CommonData,
                    ProblemType = message.ProblemType,
                    SolvingTimeout = message.SolvingTimeout,
                    SolvingTimeoutSpecified = message.SolvingTimeoutSpecified
                };

                var problem = new SolvePartialProblemsPartialProblem
                {
                    Data = dividedPartialProblem.Data,
                    TaskId = dividedPartialProblem.TaskId,
                    NodeID = dividedPartialProblem.NodeID
                };

                singleProblemArray[0] = problem;
                singlePartialProblem.PartialProblems = singleProblemArray;
                _messageList.Add(singlePartialProblem);
                
            }

            SendNoOperationMessage(tp);
        }

        /// <summary>
        /// Handles Solution Messages from components and sends NoOperation response. If Solution is Final, prepares it to send it to Client. 
        /// If Solution is Partial, adds it to the _partialSolutions list
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void HandleSolutionMessages(ThreadPackage tp)
        {
            var message = (Solutions)tp.Message.ClusterMessage;
            var type = message.Solutions1[0].Type;
            
            switch (type)
            {
                case SolutionsSolutionType.Final:
                    HandleFinalSolutionMessages(tp);
                    break;
                case SolutionsSolutionType.Partial:
                    HandlePartialSolutionMessages(tp);
                    break;
            }
            SendNoOperationMessage(tp);
        }
        /// <summary>
        /// Handles Partial Solutions Messages from components
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void HandleFinalSolutionMessages(ThreadPackage tp)
        {
            var message = (Solutions)tp.Message.ClusterMessage;

            var finalSolution = message.Solutions1[0];
            var id = message.Id;

            var solution = new SolutionsSolution
            {
                ComputationsTime = finalSolution.ComputationsTime,
                Data = finalSolution.Data,
                Type = SolutionsSolutionType.Final,
                TaskId = finalSolution.TaskId,
                TaskIdSpecified = finalSolution.TaskIdSpecified
            };

            var problemInstance = _problemInstances.Find(pi => pi.Id == id);

            if (problemInstance == null)
                throw new Exception("Send error message - unknown problem.");

            problemInstance.FinalSolutionFound = true;
            problemInstance.FinalSolution = solution;
        }

        /// <summary>
        /// Handles Final Solution from components
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void HandlePartialSolutionMessages(ThreadPackage tp)
        {
            var message = (Solutions)tp.Message.ClusterMessage;
            var partialSolution = message.Solutions1[0];

            var ss = new SolutionsSolution
            {
                ComputationsTime = partialSolution.ComputationsTime,
                Data = partialSolution.Data,
                Type = SolutionsSolutionType.Partial,
                TaskId = partialSolution.TaskId,
                TaskIdSpecified = partialSolution.TaskIdSpecified,
                TimeoutOccured = partialSolution.TimeoutOccured
            };

            AddPartialSolution(ss, message.Id);

            RemovePartialProblemsBackup((int)message.Id, (int)partialSolution.TaskId);
           
        }

        public void RemovePartialProblemsBackup(int ProblemInstance, int TaskID)
        {
            var element = _reservedPartialProblem.FirstOrDefault(x => x.Value.ContainsKey(ProblemInstance));
            _reservedPartialProblem[element.Key][ProblemInstance].Remove(TaskID);

        }       

        /// <summary>
        /// Converts two Messages of different types to the binary array data and sends them to component
        /// </summary>
        /// <typeparam name="T">Type of the first ClusterMessage</typeparam>
        /// <typeparam name="TS">Type of the second ClusterMessage</typeparam>
        /// <param name="message1">First ClusterMessage</param>
        /// <param name="message2">Second ClusterMessage</param>
        /// <param name="handler">Socket handler of the component, that messages will be sent to</param>
        public void ConvertAndSendTwoMessages<T, TS>(T message1, TS message2, Socket handler)
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
        /// Converts a ClusterMessage to binary array data and sends it to component
        /// </summary>
        /// <typeparam name="T">Type of the ClusterMessage</typeparam>
        /// <param name="message">XmlMessage</param>
        /// <param name="handler">Socket handler of the component, that ClusterMessage will be sent to</param>
        public void ConvertAndSendMessage<T>(T message, Socket handler)
        {
            var messageData = Serializers.ObjectToByteArray(message);
            SendMessage(handler, messageData);
        }

        /// <summary>
        /// Send NoOperation ClusterMessage to component
        /// </summary>
        /// <param name="tp">Thread Package with Socket handler and XmlDocument ClusterMessage</param>
        public void SendNoOperationMessage(ThreadPackage tp)
        {
            var no = new NoOperation();
            ConvertAndSendMessage(no, tp.Handler);
        }

        /// <summary>
        /// Adds Parital Solution to the _partialSolutions list
        /// </summary>
        /// <param name="ss">PartialSolution that is added to the list</param>
        /// <param name="instanceId">ID of _partialSolutions list</param>
        public void AddPartialSolution(SolutionsSolution ss, ulong instanceId)
        {
            var problemInstance = _problemInstances.Find(pi => pi.Id == instanceId);

            if (problemInstance == null)
                throw new Exception("Send error message - unknown problem.");

            problemInstance.PartialSolutions.Add(ss);

            Console.WriteLine("Received {0}/{1} partial problems for problem {2}.", problemInstance.PartialSolutions.Count, problemInstance.SubproblemsCount, instanceId);

            if (problemInstance.PartialSolutions.Count != problemInstance.SubproblemsCount) return;

            var s = new Solutions
            {
                CommonData = problemInstance.CommonData,
                ProblemType = "DVRP",
                Id = instanceId,
                Solutions1 = problemInstance.PartialSolutions.ToArray()
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

        public IClusterMessage SearchComputationalNodeMessages(int id,Socket handler)
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
                    var problems = _messageList[i] as SolvePartialProblems;
                    if (problems != null)
                    {
                        var spp = problems;

                        AddPartialProblemBackup(id, spp);                                       
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

        public void AddPartialProblemBackup(int ComponentId, SolvePartialProblems spp)
        {
            if (!_reservedPartialProblem.ContainsKey(ComponentId))
            {
                _reservedPartialProblem.Add(ComponentId, new Dictionary<int, Dictionary<int, SolvePartialProblems>>());
            }
            if (!_reservedPartialProblem[ComponentId].ContainsKey((int)spp.Id))
            {
                _reservedPartialProblem[ComponentId].Add((int)spp.Id, new Dictionary<int, SolvePartialProblems>());
            }
        }
        

        /// <summary>
        /// Thread for checking if Component didn't cross the timeout
        /// </summary>
        /// <param name="componentIndex">Component Index</param>
        public void CheckComponentTimeout(object componentIndex)
        {
            var index = (int)(componentIndex);
            var ev = new ManualResetEvent(false);
            
            for (int i = 0; i < _componentTimeout; i++)
            {
                ev.WaitOne(1000);
                if (_components[index].StatusOccured)
                {
                    
                    _components[index].StatusOccured = false;
                    var thread = new Thread(CheckComponentTimeout);
                    thread.Start(componentIndex);
                    return;
                    
                }

            }
            Console.WriteLine("Removing component:: " + index.ToString());
            _components.Remove(index);

            try
            {
                foreach (var element in _reservedPartialProblem[index])
                    foreach (var elementIn in _reservedPartialProblem[index][element.Key])
                        _messageList.Add(elementIn.Value);
                _reservedPartialProblem.Remove(index);

            }
            catch (Exception)
            {
                return;
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
                    state.Buffer = new byte[StateObject.BufferSize];
                    handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
                } else if (bytesRead == 0)
                {
                    var bytes = state.ByteBuffer.ToArray();
                    var tp = new ThreadPackage()
                    {
                        Handler = handler,
                        Message = Serializers.MessageFromByteArray(bytes)
                    };
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
