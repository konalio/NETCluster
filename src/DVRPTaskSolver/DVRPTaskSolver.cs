using System;
using System.Collections.Generic;
using System.Linq;

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
            DVRPData dvrpData = DVRPData.GetFromBytes(base._problemData);
            int vehiclesCount = dvrpData.VehicleCount;
            List<Request> requests = dvrpData.Requests;
            int requestsCount = dvrpData.RequestsCount;
            int[] requestsIds = new int[requestsCount];
            List<DVRPPartialSolution> subset = new List<DVRPPartialSolution>();

            List<DVRPPartialSolution> partialSolutions = new List<DVRPPartialSolution>();
            List<List<DVRPPartialSolution>> selectedSolutions = new List<List<DVRPPartialSolution>>();
            int optimalTime = int.MaxValue;
            int solutionTime = 0;
            List<DVRPPartialSolution> finalSolutions = new List<DVRPPartialSolution>();
            List<int[]> finalSolutionVisits = new List<int[]>();

            for (int i = 0; i < requestsCount; i++)
                requestsIds[i] = requests[i].Id;
            
            for (int i = 0; i < solutions.Length; i++)
                partialSolutions.Add(DVRPPartialSolution.GetFromByteArray(solutions[i]));

            selectSolutionsRecursive(vehiclesCount, requestsIds, partialSolutions, ref selectedSolutions, subset, solutions.Length, 0);

            foreach(var ss in selectedSolutions)
            {
                solutionTime=0;
                foreach (var s in ss)
                    solutionTime += s.OptimalTime;

                if(solutionTime<optimalTime)
                {
                    optimalTime = solutionTime;
                    finalSolutions = ss;
                }
            }

            foreach (var fs in finalSolutions)
                finalSolutionVisits.Add(fs.Visits);

            return FinalSolution.Serialize(optimalTime, finalSolutionVisits.ToArray());
        }

        private void selectSolutionsRecursive(int length, int[] requestsIds, List<DVRPPartialSolution> partialSolutions, ref List<List<DVRPPartialSolution>> selectedSolutions,
            List<DVRPPartialSolution> lastSubset, int solutionsCount, int lastIndex)
        {
            List<DVRPPartialSolution> subset;
            bool containsElement = false;
            bool rejected = false;
            for (int i = lastIndex; i < solutionsCount; i++)
            {
                subset = lastSubset.ConvertAll(solution => solution);
                subset.Add(partialSolutions[i]);

                rejected = false;
                foreach (var rId in requestsIds)
                {
                    containsElement = false;
                    for (int j = 0; j < subset.Count; j++)
                        if (subset[j].RequestsSubset.Contains(rId))
                        {
                            if (containsElement)
                                rejected = true;
                            containsElement = true;
                        }
                    if (!containsElement)
                        rejected = true;
                }

                if (!rejected)
                {
                    while (subset.Count != length)
                        subset.Add(new DVRPPartialSolution(new int[0],new int[0],0));

                    selectedSolutions.Add(subset);
                }

                if (subset.Count == length)
                    continue;

                selectSolutionsRecursive(length, requestsIds, partialSolutions, ref selectedSolutions, subset, solutionsCount, i + 1);
            }
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
