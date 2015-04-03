using System;
using System.Xml;
using ClusterUtils;

namespace ComputationalClient
{
    class ComputationalClient : Component
    {
        public ComputationalClient(ComponentConfig componentConfig) : base(componentConfig, "ComputationalClient") {}

        public void Start()
        {
            LogRuntimeInfo();
            var problemId = RequestForSolvingProblem();
            WaitForSolution(problemId);
        }

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
            var request = new SolveRequest();
            var response = SendMessageSingleResponse(request);
            return ulong.Parse(response.GetElementsByTagName("Id")[0].InnerText);
        }
    }
}
