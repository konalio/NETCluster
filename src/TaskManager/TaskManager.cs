using System;
using System.Collections.Generic;
using System.Xml;
using ClusterMessages;
using ClusterUtils;

namespace TaskManager
{
    /// <summary>
    /// Implementation of TaskManager.
    /// Manager registeres to server and awaits for problems to divide or partial solutions to choose final solution.
    /// </summary>
    public class TaskManager : RegisteredComponent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">Server info from App.config and arguments.</param>
        public TaskManager(ComponentConfig config) : base(config, "TaskManager") { } 

        /// <summary>
        /// Tries to register to server and starts sending status message.
        /// </summary>
        public void Start()
        {
            LogRuntimeInfo();
            Register();
            StartSendingStatus();
            Console.ReadLine();
        }
        
        protected override void ProcessMessages(IEnumerable<XmlDocument> responses)
        {
            foreach (var xmlMessage in responses)
            {
                switch (MessageTypeResolver.GetMessageType(xmlMessage))
                {
                    case MessageTypeResolver.MessageType.NoOperation:
                        ProcessNoOperationMessage(xmlMessage);
                        break;
                    case MessageTypeResolver.MessageType.DivideProblem:
                        ProcessDivideProblem(xmlMessage);
                        break;
                    case MessageTypeResolver.MessageType.Solution:
                        ProcessSolutions(xmlMessage);
                        break;
                }
            }
        }

        /// <summary>
        /// Support for processing solutions messages. 
        /// After receiving Solutions, final solution is chosen from all solutions and is sent back to server.
        /// Currently choosing final solution is random.
        /// </summary>
        /// <param name="xmlMessage">Solutions message to be processed.</param>
        private void ProcessSolutions(XmlDocument xmlMessage)
        {
            var problemInstanceId = ulong.Parse(xmlMessage.GetElementsByTagName("Id")[0].InnerText);

            Console.WriteLine("Received partial solutions for problem {0}.", problemInstanceId);

            ChooseAndSendFinalSolution(problemInstanceId);

            Console.WriteLine("Sent final solution for problem {0}", problemInstanceId);
        }

        private void ChooseAndSendFinalSolution(ulong problemInstanceId)
        {
            var taskId = ChooseFinalSolution();

            SendFinalSolution(taskId, problemInstanceId);
        }

        private ulong ChooseFinalSolution()
        {
            var r = new Random();
            return (ulong) r.Next(0, 4);
        }

        private void SendFinalSolution(ulong taskId, ulong problemInstanceId)
        {
            var solution = new Solutions
            {
                Solutions1 = new[] { new SolutionsSolution
                {
                    TaskId = taskId, 
                    TaskIdSpecified = true,
                    ComputationsTime = 1,
                    TimeoutOccured = false,
                    Type = SolutionsSolutionType.Final,
                    Data = new byte[0]
                } },
                Id = problemInstanceId,
                ProblemType = "DVRP",
                CommonData = new byte[0]
            };

            SendMessageNoResponse(solution);
        }

        /// <summary>
        /// Support for processing DivideProblem message.
        /// Currently, method creates 5 partial problems for each problem instance and sends them to server.
        /// </summary>
        /// <param name="xmlMessage">Divide problem message to be processed.</param>
        private void ProcessDivideProblem(XmlDocument xmlMessage)
        {
            var problemInstanceId = ulong.Parse(xmlMessage.GetElementsByTagName("Id")[0].InnerText);

            Console.WriteLine("Received problem {0} to divide.", problemInstanceId);

            DivideAndSendPartialProblems(problemInstanceId);

            Console.WriteLine("Sent partial problems for problem {0}", problemInstanceId);
        }

        private void DivideAndSendPartialProblems(ulong problemInstanceId)
        {
            var partialProblems = CreatePartialProblems(problemInstanceId);

            SendMessageNoResponse(partialProblems);
        }

        private SolvePartialProblems CreatePartialProblems(ulong problemInstanceId)
        {
            var partialProblems = new SolvePartialProblems
            {
                Id = problemInstanceId,
                ProblemType = "DVRP",
                CommonData = new byte[0],
                PartialProblems = new[]
                {
                    new SolvePartialProblemsPartialProblem {TaskId = 0, Data = new byte[0], NodeID = Id},
                    new SolvePartialProblemsPartialProblem {TaskId = 1, Data = new byte[0], NodeID = Id},
                    new SolvePartialProblemsPartialProblem {TaskId = 2, Data = new byte[0], NodeID = Id},
                    new SolvePartialProblemsPartialProblem {TaskId = 3, Data = new byte[0], NodeID = Id},
                    new SolvePartialProblemsPartialProblem {TaskId = 4, Data = new byte[0], NodeID = Id},
                }
            };
            return partialProblems;
        }
    }
}
