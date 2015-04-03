using System;
using System.Collections.Generic;
using System.Xml;
using ClusterMessages;
using ClusterUtils;

namespace TaskManager
{
    public class TaskManager : RegisteredComponent
    {
        public TaskManager(ComponentConfig config) : base(config, "TaskManager") { } 

        public void Start()
        {
            LogRuntimeInfo();
            Register();
            StartSendingStatus();
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
                Solutions1 = new[] { new SolutionsSolution { TaskId = taskId, Type = SolutionsSolutionType.Final } },
                Id = problemInstanceId
            };

            SendMessageNoResponse(solution);
        }

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
                PartialProblems = new[]
                {
                    new SolvePartialProblemsPartialProblem {TaskId = 0},
                    new SolvePartialProblemsPartialProblem {TaskId = 1},
                    new SolvePartialProblemsPartialProblem {TaskId = 2},
                    new SolvePartialProblemsPartialProblem {TaskId = 3},
                    new SolvePartialProblemsPartialProblem {TaskId = 4},
                }
            };
            return partialProblems;
        }
    }
}
