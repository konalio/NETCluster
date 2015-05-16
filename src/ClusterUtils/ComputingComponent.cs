using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClusterUtils.Communication;

namespace ClusterUtils
{
    /// <summary>
    /// Class generalizing components that need external library to perform work.
    /// General flow:
    ///     Load library/libraries that exposes classes extending TaskSolver class.
    ///     Register to server.
    ///     Do computations for cluster.
    /// </summary>
    public abstract class ComputingComponent : RegisteredComponent
    {
        protected Dictionary<string, Type> SolversCreatorTypes = new Dictionary<string, Type>();
        protected List<string> SolvableProblems = new List<string>(); 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">Component config.</param>
        /// <param name="type">Type of component.</param>
        protected ComputingComponent(ComponentConfig config, string type)
            : base(config, type)
        { }

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Starts the component:
        /// - Prints info about the Server,
        /// - Attempts to register to the Server,
        /// - Starts sending Status messages to the Server.
        /// </summary>
        public void Start()
        {
            LogRuntimeInfo();
            PrintUsageMessage();
            CommandLineLoop();
            Register();
            StartSendingStatus();
        }

        /// <summary>
        /// Register to server and process register response message.
        /// </summary>
        /// <returns>True on registration success, false otherwise.</returns>
        protected new bool Register()
        {
            var registerMessage = new Register
            {
                Type = Type,
                SolvableProblems = SolvableProblems.Select(
                    x => new RegisterSolvableProblemsProblemName
                        {
                            Value = x
                        }
                ).ToArray()
            };

            var response = SendMessageSingleResponse(registerMessage);

            return ProcessRegisterResponse(response);
        }

        /// <summary>
        /// Simple command line for loading libraries and connecting to cluster.
        /// </summary>
        protected void CommandLineLoop()
        {
            while (true)
            {
                Console.WriteLine("\n >");
                string[] commands;
                if (!GetCommands(out commands)) continue;

                if (ProcessCommands(commands)) return;
            }
        }

        private static bool GetCommands(out string[] commands)
        {
            commands = null;
            var commandLineInput = Console.ReadLine();
            if (commandLineInput == null) return false;

            commands = commandLineInput.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            return commands.Length != 0;
        }

        private bool ProcessCommands(string[] commands)
        {
            switch (commands[0])
            {
                case "load":
                    {
                        ProcessLoadCommand(commands);
                        return false;
                    }
                case "register":
                    {
                        return true;
                    }
                case "help":
                    {
                        PrintUsageMessage();
                        return false;
                    }
                default:
                    {
                        Console.WriteLine("type 'help' to see commands.");
                        return false;
                    }
            }
        }

        private void ProcessLoadCommand(string[] commands)
        {
            var dllPath = AssemblyDirectory + "\\" + commands[1];
            var dll = Assembly.LoadFile(dllPath);
            var solverTypes = dll.GetExportedTypes().Where(x => x.IsSubclassOf(typeof(UCCTaskSolver.TaskSolver)));
            var creatorTypes = dll.GetExportedTypes().Where(x => x.IsSubclassOf(typeof(UCCTaskSolver.TaskSolverCreator)));

            var solverTypesArray = solverTypes as Type[] ?? solverTypes.ToArray();
            var creatorTypesArray = creatorTypes as Type[] ?? creatorTypes.ToArray();

            if (!solverTypesArray.Any() || !creatorTypesArray.Any())
            {
                Console.WriteLine("Given library does not contain required classes.");
                return;
            }

            var creatorType = creatorTypesArray[0];

            var creator = Activator.CreateInstance(creatorType) as UCCTaskSolver.TaskSolverCreator;
            if (creator == null) return;

            var solver = creator.CreateTaskSolverInstance(new byte[0]);
            var solvableProblemName = solver.Name;

            SolversCreatorTypes.Add(solvableProblemName, creatorType);
            SolvableProblems.Add(solvableProblemName);

            Console.WriteLine(string.Format("Loaded library {0} with solver for {1} problem.", 
                                                commands[1], solvableProblemName));
        }

        private static void PrintUsageMessage()
        {
            Console.WriteLine("To load TaskSolvers:");
            Console.WriteLine("  load path/to/library.dll");
            Console.WriteLine("After loading task solvers:");
            Console.WriteLine("  register");
            Console.WriteLine("Component will connect to cluster and loading TaskSolvers won't be available.");
        }

        protected override abstract void ProcessMessages(IEnumerable<MessagePackage> responses);
    }
}
