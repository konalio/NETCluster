using System;
using System.Xml;
using ClusterMessages;
using ClusterUtils;
using ClusterUtils.Communication;

namespace ComputationalClient
{
    /// <summary>
    /// Implementation of computational client.
    /// Allows to send single problem instance to server and repeatedly ask for final solution.
    /// </summary>
    class ComputationalClient : Component
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentConfig">Server info from App.config and arguments.</param>
        public ComputationalClient(ComponentConfig componentConfig) : base(componentConfig, "ComputationalClient") {}

        /// <summary>
        /// Sends empty problem instance to server and starts awaiting for solution.
        /// </summary>
        public void Start()
        {
            LogRuntimeInfo();
            try
            {
                var problemId = RequestForSolvingProblem();
                WaitForSolution(problemId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Waiting loop for final solution. Allows user to repeatedly ask if solution is ready.
        /// If final solution is received as response for query, client shuts down.
        /// </summary>
        /// <param name="problemId">Problem instance id to query for.</param>
        private void WaitForSolution(ulong problemId)
        {
            while (true)
            {
                Console.WriteLine("\nPress ENTER to ask server for solution");
                Console.Read();

                var response = AskForSolution(problemId);
                SolutionsSolutionType status;

                switch (MessageTypeResolver.GetMessageType(response.XmlMessage))
                {
                    case MessageTypeResolver.MessageType.Error:
                        HandleErrorMessage(response);
                        throw new Exception("Solution request failed");
                    case MessageTypeResolver.MessageType.Solution:
                        var message = (Solutions)response.ClusterMessage;
                        status = message.Solutions1[0].Type;
                        break;
                    default:
                        throw new Exception("Solution request failed.");
                }

                if (status == SolutionsSolutionType.Final)
                {
                    Console.WriteLine("Received final solution.");
                    break;
                }
                else
                {
                    Console.WriteLine("Computations ongoing");
                }
            }
        }

        private MessagePackage AskForSolution(ulong problemId)
        {
            var request = new SolutionRequest
            {
                Id = problemId
            };

            return SendMessageSingleResponse(request);
        }

        private ulong RequestForSolvingProblem()
        {
            var request = new SolveRequest
            {
                ProblemType = "DVRP",
                Data = new byte[0]
            };
            var response = SendMessageSingleResponse(request);    
        
            switch (MessageTypeResolver.GetMessageType(response.XmlMessage))
            {
                case MessageTypeResolver.MessageType.Error:
                    HandleErrorMessage(response);
                    throw new Exception("Solve request failed");
                case MessageTypeResolver.MessageType.SolveRequestResponse:
                    var message = (SolveRequestResponse)response.ClusterMessage;
                    return message.Id;
                default:
                    throw new Exception("Solve request failed.");
            }
        }
    }
}
