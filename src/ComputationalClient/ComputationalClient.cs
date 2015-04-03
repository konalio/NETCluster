using System;
using System.Xml;
using ClusterUtils;
using ClusterUtils.Communication;

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

            return SendSolutionRequest(request);
        }

        private XmlDocument SendSolutionRequest(SolutionRequest request)
        {
            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var response = tcpClient.SendAndWaitForResponses(request)[0];

            tcpClient.Close();

            return response;
        }

        private ulong RequestForSolvingProblem()
        {
            var request = new SolveRequest();

            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var response = tcpClient.SendAndWaitForResponses(request)[0];

            tcpClient.Close();

            return ulong.Parse(response.GetElementsByTagName("Id")[0].InnerText);
        }
    }
}
