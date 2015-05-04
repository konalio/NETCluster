using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                CommandLineLoop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Waiting loop for final solution. 
        /// Allows user to repeatedly ask if solution is ready and insert new problem instance.
        /// </summary>
        private void CommandLineLoop()
        {
            while (true)
            {
                Console.WriteLine("\n >");
                var commandLineInput = Console.ReadLine();
                if (commandLineInput == null) continue;

                var commands = commandLineInput.Split(new [] {" "}, StringSplitOptions.RemoveEmptyEntries);

                if (commands.Length == 0) continue;

                switch (commands[0])
                {
                    case "input":
                    {
                        try
                        {
                            var problemId = SendProblemInstanceOrThrow(commands);
                            Console.WriteLine("Received problem instance id: {0}", problemId);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception.Message);
                        }
                        break;
                    }
                    case "request":
                    {
                        try
                        {
                            var problemId = GetIdOrThrow(commands);
                            RequestSolution(problemId);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                        continue;
                    }
                    case "help":
                    {
                        PrintUsageMessage();
                        continue;
                    }
                    case "quit":
                    {
                        return;
                    }
                    default:
                    {
                        Console.WriteLine("type 'help' to see commands.");
                        continue;
                    }
                }
            }
        }

        private ulong SendProblemInstanceOrThrow(IReadOnlyList<string> commands)
        {
            if (commands == null) throw new ArgumentNullException("commands");

            if (commands.Count < 3)
                throw new Exception("Incorrect number of parameters.");

            var problemType = commands[1];
            if (problemType != "DVRP")
                throw new Exception("Unsupported problem type.");

            var problemFileName = commands[2];
            var file = "";

            try
            {
                file = File.ReadAllText(problemFileName);
            }
            catch (Exception)
            {
                throw new Exception("Error while reading file.");
            }

            var data = Encoding.UTF8.GetBytes(file);

            return RequestForSolvingProblem(problemType, data);
        }

        private static void PrintUsageMessage()
        {
            Console.WriteLine("Client commands:");
            Console.WriteLine("  input problem-type input-file-path");
            Console.WriteLine("     supported problem types: DVRP");
            Console.WriteLine("  solution problem-instance-id");
            Console.WriteLine("  quit");
        }

        private static ulong GetIdOrThrow(IReadOnlyList<string> commands)
        {
            if (commands == null) throw new ArgumentNullException("commands");

            if (commands.Count < 1)
                throw new Exception("Incorrent number of arguments for request.");
            
            ulong id;

            try
            {
                id = ulong.Parse(commands[1]);
            }
            catch (Exception)
            {
                throw new Exception("Inorrect argument for request (must be unsigned integer).");
            }

            return id;
        }

        private void RequestSolution(ulong problemId)
        {
            try
            {
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

                Console.WriteLine(status == SolutionsSolutionType.Final
                    ? "Received final solution."
                    : "Computations ongoing");
            }
            catch (Exception)
            {
                throw;
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

        private ulong RequestForSolvingProblem(string type, byte[] data)
        {
            try
            {
                var request = new SolveRequest
                    {
                        ProblemType = type,
                        Data = data
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
            catch (Exception)
            {
                throw;
            }
        }
    }
}
