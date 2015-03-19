using System.Collections.Generic;

namespace ClusterMessages
{
    class RegisterMessage : ClusterMessage
    {
        public Register Message { public get; private set; }

        public RegisterMessage(string type, int parallelThreads, List<string> solvableProblems)
        {
            Message = new Register
            {
                Type = type,
                ParallelThreads = parallelThreads.ToString()
            };
        }
    }
}
