using System;
using System.Collections.Generic;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">Component config.</param>
        /// <param name="type">Type of component.</param>
        protected ComputingComponent(ComponentConfig config, string type) : base(config, type)
        { }

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
            Console.WriteLine(string.Format("Loaded library {0}.", commands[1]));
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
