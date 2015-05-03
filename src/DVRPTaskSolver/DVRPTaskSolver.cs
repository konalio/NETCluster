using System;
using System.Collections.Generic;

namespace DVRPTaskSolver
{
    public class DVRPTaskSolver : UCCTaskSolver.TaskSolver
    {
        public DVRPTaskSolver(byte[] problemData) : base(problemData) { }

        public override byte[][] DivideProblem(int threadCount)
        {
            DVRPData dvrpData = DVRPData.GetFromBytes(base._problemData);
            List<Request> requests = dvrpData.Requests;
            int requestsCount = dvrpData.RequestsCount;
            int[] requestsIds = new int[requestsCount];
            int[][] requestsSubsets = new int[1][];
            int[] subset = new int[0];
            byte[][] returnSubsets;

            for (int i = 0; i < requestsCount; i++)
                requestsIds[i] = requests[i].Id;

            requestsSubsets[0] = subset;

            SplitIntoSubsetsRecursive(requestsIds, ref requestsSubsets, subset, requestsCount, 0);

            returnSubsets = new byte[requestsSubsets.Length][];

            for (int i = 0; i < requestsSubsets.Length; i++)
                returnSubsets[i] = DVRPLocationsSubset.Serialize(requestsSubsets[i]);

            return returnSubsets;
        }

        private void SplitIntoSubsetsRecursive(int[] requestsIds, ref int[][] requestsSubsets, int[] lastSubset, int requestsCount, int lastIndex)
        {
            int[] subset;
            for (int i = lastIndex; i < requestsCount; i++)
            {
                subset = (int[])lastSubset.Clone();
                Array.Resize(ref requestsSubsets, requestsSubsets.Length + 1);
                Array.Resize(ref subset, subset.Length + 1);
                subset[subset.Length - 1] = requestsIds[i];
                requestsSubsets[requestsSubsets.Length - 1] = subset;

                SplitIntoSubsetsRecursive(requestsIds, ref requestsSubsets, subset, requestsCount, i + 1);
            }
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
