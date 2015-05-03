using System;
using System.Collections.Generic;
using System.Text;

namespace DVRPTaskSolver
{
    public class DVRPTaskSolver : UCCTaskSolver.TaskSolver
    {
        public DVRPTaskSolver(byte[] problemData) : base(problemData) { }

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
            DVRPData data = DVRPData.GetFromBytes(base._problemData);
            DVRPLocationsSubset subset = DVRPLocationsSubset.GetFromByteArray(partialData);
            int[] clients = subset.Locations;

            throw new NotImplementedException();
        }     
        
    }
}
