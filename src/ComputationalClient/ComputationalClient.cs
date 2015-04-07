using System;
using System.Xml;
using ClusterUtils;

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
            var problemId = RequestForSolvingProblem();
            WaitForSolution(problemId);
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
                var status = response.GetElementsByTagName("Type")[0];

                Console.WriteLine("Problem status: {0}.", status.InnerText);

                if (status.InnerText == "Final")
                {
                    break;
                }
            }
        }

        private XmlDocument AskForSolution(ulong problemId)
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
            return ulong.Parse(response.GetElementsByTagName("Id")[0].InnerText);
        }
    }
}
