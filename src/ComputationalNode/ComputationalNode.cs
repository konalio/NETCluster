using System;
using System.Collections.Generic;
using ClusterMessages;
using ClusterUtils;
using ClusterUtils.Communication;

namespace ComputationalNode
{
    class ComputationalNode : RegisteredComponent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentConfig">Server info from App.config and arguments.</param>
        public ComputationalNode(ComponentConfig componentConfig) : base(componentConfig, "ComputationalNode") {}

        /// <summary>
        /// Starts the Node:
        /// - Prints info about the Server,
        /// - Attempts to register to the Server,
        /// - Starts sending Status messages to the Server.
        /// </summary>
        public void Start()
        {
            LogRuntimeInfo();
            Register();
            StartSendingStatus();
        }

        /// <summary>
        /// Processes messages received from Server.
        /// </summary>
        /// <param name="responses">All received messages.</param>
        protected override void ProcessMessages(IEnumerable<MessagePackage> responses)
        {
            foreach (var message in responses)
            {
                switch (MessageTypeResolver.GetMessageType(message.XmlMessage))
                {
                    case MessageTypeResolver.MessageType.NoOperation:
                        ProcessNoOperationMessage(message);
                        break;
                    case MessageTypeResolver.MessageType.PartialProblems:
                        ProcessPartialProblemsMessage(message);
                        break;
                }
            }
        }

        /// <summary>
        /// Support for handling Partial Problems message.
        /// Node mocks working on partial problem and sends partial solution to server.
        /// </summary>
        /// <param name="package"></param>
        private void ProcessPartialProblemsMessage(MessagePackage package)
        {
            var message = Serializers.ByteArrayObject<SolvePartialProblems>(package.MessageBytes);
            var problemInstanceId = message.Id;

            var partialProblem = message.PartialProblems[0];

            var taskId = partialProblem.TaskId;

            Console.WriteLine("Received partial problem {0} from problem instance {1}.", taskId, problemInstanceId);
            
            CreateAndSendPartialSolution(taskId, problemInstanceId);
        }

        private void CreateAndSendPartialSolution(ulong taskId, ulong problemInstanceId)
        {
            var solution = new Solutions
            {
                Solutions1 = new[] {new SolutionsSolution
                {
                    TaskId = taskId, 
                    TaskIdSpecified = true,
                    Type = SolutionsSolutionType.Partial, 
                    TimeoutOccured = false, 
                    ComputationsTime = 1,
                    Data = new byte[0]
                }},
                Id = problemInstanceId,
                ProblemType = "DVRP",
                CommonData = new byte[0]
            };

            SendMessageNoResponse(solution);
        }
    }
}
