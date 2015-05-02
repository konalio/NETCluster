using System;
using System.Collections.Generic;

namespace TaskSolver
{
    public class TaskSolver : UCCTaskSolver.TaskSolver
    {
        public TaskSolver() : base(new byte[0]) { }

        public override byte[][] DivideProblem(int threadCount)
        {
            throw new NotImplementedException();
        }

        public override byte[] MergeSolution(byte[][] solutions)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}
