using System;
using System.Collections.Generic;
using ClusterMessages;
using ClusterUtils;
using ClusterUtils.Communication;

namespace ComputationalNode
{
    class ComputationalNode : ComputingComponent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentConfig">Server info from App.config and arguments.</param>
        public ComputationalNode(ComponentConfig componentConfig) : base(componentConfig, "ComputationalNode") { }

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
                    case MessageTypeResolver.MessageType.Error:
                        HandleErrorMessage(message);
                        break;
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
            var message = (SolvePartialProblems)package.ClusterMessage;
            var problemInstanceId = message.Id;

            var partialProblem = message.PartialProblems[0];

            var taskId = partialProblem.TaskId;

            Console.WriteLine("Received partial problem {0} from problem instance {1}.", taskId, problemInstanceId);

            CreateAndSendPartialSolution(message, partialProblem);
        }

        private void CreateAndSendPartialSolution(SolvePartialProblems message,
                        SolvePartialProblemsPartialProblem problem)
        {
            var solverCreatorType = SolversCreatorTypes[message.ProblemType];

            var creator = Activator.CreateInstance(solverCreatorType) as UCCTaskSolver.TaskSolverCreator;
            if (creator == null)
            {
                Console.WriteLine("Cannot create solver.");
                //todo send error message?
                return;
            }

            var taskSolver = creator.CreateTaskSolverInstance(message.CommonData);
            var resultData = taskSolver.Solve(problem.Data, new TimeSpan());

            var solution = new Solutions
            {
                Solutions1 = new[] {new SolutionsSolution
                {
                    TaskId = problem.TaskId, 
                    TaskIdSpecified = true,
                    Type = SolutionsSolutionType.Partial, 
                    TimeoutOccured = false, 
                    ComputationsTime = 1,
                    Data = resultData
                }},
                Id = message.Id,
                ProblemType = "DVRP",
                CommonData = problem.Data
            };

            SendMessageNoResponse(solution);
        }
    }
}
