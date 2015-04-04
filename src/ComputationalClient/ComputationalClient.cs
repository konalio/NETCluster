using System;
using System.Xml;
using ClusterUtils;
using ClusterUtils.Communication;

namespace ComputationalClient
{
    class ComputationalClient
    {
        private readonly ServerInfo _serverInfo;

        public ComputationalClient(ComponentConfig componentConfig)
        {
            _serverInfo = new ServerInfo(componentConfig.ServerPort, componentConfig.ServerAddress);
        }

        public void Start()
        {
            LogClientInfo();

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

            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var response = tcpClient.SendAndWaitForResponses(request)[0];

            tcpClient.Close();

            return response;
        }

        private ulong RequestForSolvingProblem()
        {
            var request = new SolveRequest
            {
                Data = new byte[0],
                ProblemType = "DVRP"
            };

            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var response = tcpClient.SendAndWaitForResponses(request)[0];

            tcpClient.Close();

            return ulong.Parse(response.GetElementsByTagName("Id")[0].InnerText);
        }
        
        private void LogClientInfo()
        {
            Console.WriteLine("Client is running...");
            Console.WriteLine("Server address: {0}", _serverInfo.Address);
            Console.WriteLine("Server port: {0}", _serverInfo.Port);
        }
    }
}
