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
            List<List<int>> requestsSubsets = new List<List<int>>();
            List<int> subset = new List<int>();
            byte[][] returnSubsets;

            for (int i = 0; i < requestsCount; i++)
                requestsIds[i] = requests[i].Id;

            requestsSubsets.Add(subset);

            SplitIntoSubsetsRecursive(requestsIds, ref requestsSubsets, subset, requestsCount, 0);

            returnSubsets = new byte[requestsSubsets.Count][];

            for (int i = 0; i < requestsSubsets.Count; i++)
                returnSubsets[i] = DVRPLocationsSubset.Serialize(requestsSubsets[i].ToArray());

            return returnSubsets;
        }

        private void SplitIntoSubsetsRecursive(int[] requestsIds, ref List<List<int>> requestsSubsets, List<int> lastSubset, int requestsCount, int lastIndex)
        {
            List<int> subset;
            for (int i = lastIndex; i < requestsCount; i++)
            {
                subset = lastSubset.ConvertAll(request => request);
                subset.Add(requestsIds[i]);
                requestsSubsets.Add(subset);

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
            
            DVRPData dvrpData = DVRPData.GetFromBytes(base._problemData);
            DVRPLocationsSubset locationsData= DVRPLocationsSubset.GetFromByteArray(partialData);
            int[] locationsArray=locationsData.Locations;
           

            throw new NotImplementedException();
        }

        private LocationObject[] ConstructLocationArray(List<Depot> depots, List<Request> requests)
        {
            List<LocationObject> array = new List<LocationObject>();
            foreach (Depot dep in depots)
                array.Add(dep);
            foreach (Request req in requests)
                array.Add(req);
            return array.ToArray();
        }
    }
}
