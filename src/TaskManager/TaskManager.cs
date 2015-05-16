using System;
using System.Collections.Generic;
using System.Linq;
using ClusterMessages;
using ClusterUtils;
using ClusterUtils.Communication;

namespace TaskManager
{
    /// <summary>
    /// Implementation of TaskManager.
    /// Manager registeres to server and awaits for problems to divide or partial solutions to choose final solution.
    /// </summary>
    public class TaskManager : ComputingComponent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">Server info from App.config and arguments.</param>
        public TaskManager(ComponentConfig config) : base(config, "TaskManager") { }
        
        protected override void ProcessMessages(IEnumerable<MessagePackage> responses)
        {
            foreach (var message in responses)
            {
                switch (MessageTypeResolver.GetMessageType(message.XmlMessage))
                {
                    case MessageTypeResolver.MessageType.Error:
                        HandleErrorMessage(message);
                        break;
                    case MessageTypeResolver.MessageType.NoOperation:
                        ProcessNoOperationMessage(message);
                        break;
                    case MessageTypeResolver.MessageType.DivideProblem:
                        ProcessDivideProblem(message);
                        break;
                    case MessageTypeResolver.MessageType.Solution:
                        ProcessSolutions(message);
                        break;
                }
            }
        }

        /// <summary>
        /// Support for processing solutions messages. 
        /// After receiving Solutions, final solution is chosen from all solutions and is sent back to server.
        /// Currently choosing final solution is random.
        /// </summary>
        /// <param name="package">Solutions message to be processed.</param>
        private void ProcessSolutions(MessagePackage package)
        {
            var message = (Solutions)package.ClusterMessage;
            var problemInstanceId = message.Id;

            Console.WriteLine("Received partial solutions for problem {0}.", problemInstanceId);

            ChooseAndSendFinalSolution(message);

            Console.WriteLine("Sent final solution for problem {0}", problemInstanceId);
        }

        private void ChooseAndSendFinalSolution(Solutions message)
        {
            var solution = ChooseFinalSolution(message);
            if (solution == null) return;

            SendMessageNoResponse(solution);
        }

        private Solutions ChooseFinalSolution(Solutions solutions)
        {
            var taskSolver = CreateSolverOrSendError(solutions.ProblemType, solutions.CommonData);
            if (taskSolver == null) return null;

            var partialSolutions = solutions.Solutions1;

            var partialSolutionsData = new byte[partialSolutions.Length][];

            for (var i = 0; i < partialSolutions.Length; i++)
            {
                partialSolutionsData[i] = partialSolutions[i].Data;
            }

            var resultData = taskSolver.MergeSolution(partialSolutionsData);

            var solution = new Solutions
            {
                ProblemType = "DVRP",
                Id = solutions.Id,
                Solutions1 = new[] 
                {
                    new SolutionsSolution
                    {
                        Type = SolutionsSolutionType.Final,
                        TimeoutOccured = false,
                        ComputationsTime = 0,
                        Data = resultData       
                    }
                }
            };

            return solution;
        }

        /// <summary>
        /// Support for processing DivideProblem message.
        /// Currently, method creates 5 partial problems for each problem instance and sends them to server.
        /// </summary>
        /// <param name="package">Divide problem message to be processed.</param>
        private void ProcessDivideProblem(MessagePackage package)
        {
            var message = (DivideProblem)package.ClusterMessage;
            var problemInstanceId = message.Id;

            Console.WriteLine("Received problem {0} to divide.", problemInstanceId);

            DivideAndSendPartialProblems(message);

            Console.WriteLine("Sent partial problems for problem {0}", problemInstanceId);
        }

        private void DivideAndSendPartialProblems(DivideProblem message)
        {
            var partialProblems = CreatePartialProblems(message);
            if (partialProblems == null) return;

            SendMessageNoResponse(partialProblems);
        }

        private SolvePartialProblems CreatePartialProblems(DivideProblem message)
        {
            var taskSolver = CreateSolverOrSendError(message.ProblemType, message.Data);
            if (taskSolver == null) return null;

            var problemsData = taskSolver.DivideProblem(0);

            var partialProblemsMessage = new SolvePartialProblems
            {
                Id = message.Id,
                ProblemType = "DVRP",
                CommonData = message.Data,
                PartialProblems = problemsData.Select((t, i) => new SolvePartialProblemsPartialProblem
                {
                    TaskId = (ulong) i, NodeID = Id, Data = t
                }).ToArray()
            };

            return partialProblemsMessage;
        }
    }
}
