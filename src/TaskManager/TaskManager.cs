using System;
using System.Collections.Generic;
using System.Xml;
using ClusterMessages;
using ClusterUtils;
using ClusterUtils.Communication;

namespace TaskManager
{
    public class TaskManager
    {
        private readonly ServerInfo _serverInfo;

        private int _id;

        public TaskManager(ComponentConfig cc)
        {   
            _serverInfo = new ServerInfo(cc.ServerAddress, cc.ServerPort);          
        }

        public void Start()
        {
            LogManagerInfo();
            Register();

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private void Register()
        {
            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var responses = tcpClient.SendAndWaitForResponses (
                new Register
                {
                    Type = "TaskManager"
                }
            );

            tcpClient.Close();
            ProcessMessages(responses);
        }

        private void ProcessMessages(List<XmlDocument> responses)
        {
            foreach (var xmlMessage in responses)
            {
                switch (MessageTypeResolver.GetMessageType(xmlMessage))
                {
                    case MessageTypeResolver.MessageType.NoOperation:
                        ProcessNoOperationMessage(xmlMessage);
                        break;
                    case MessageTypeResolver.MessageType.RegisterResponse:
                        ProcessRegisterResponse(xmlMessage);
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
            //Pretend to choose solution
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

            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var response = tcpClient.SendAndWaitForResponses(solution);

            tcpClient.Close();

            if (response.Count != 1)
                throw new Exception();
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

            var tcpClient = new ConnectionClient(_serverInfo);

            tcpClient.Connect();

            var response = tcpClient.SendAndWaitForResponses(partialProblems);

            tcpClient.Close();

            if (response.Count != 1)
                throw new Exception();
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

        private void ProcessRegisterResponse(XmlDocument response)
        {
            _id = int.Parse(response.GetElementsByTagName("Id")[0].InnerText);
            Console.WriteLine("Registered at server with Id: {0}.", _id);
        }
        private void ProcessNoOperationMessage(XmlDocument xmlMessage)
        {
            //TODO update backup servers info
            Console.WriteLine("Received NoOperation message.");
        }

        private void LogManagerInfo()
        {
            Console.WriteLine("Manager is running...");
            Console.WriteLine("Server address: {0}", _serverInfo.Address);
            Console.WriteLine("Server port: {0}", _serverInfo.Port);
        }
    }
}
