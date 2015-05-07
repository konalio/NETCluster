using System.Collections.Generic;
using System.Linq;

namespace DVRPTaskSolver
{
    public static class SolutionExtractor
    {
        public static FinalSolution Extract(int[] requestsIds, List<DVRPPartialSolution> partialSolutions)
        {
            var partitionsSolutions = new List<FinalSolution>();

            var requestsPartitions = GetRequestsSetPartitions(requestsIds);
            var sortedPartialSolutions = SortPartialSolutions(requestsIds, partialSolutions);

            foreach (var partition in requestsPartitions)
            {
                var solution = new FinalSolution();
                var solutionVisits = new List<int[]>();
                
                ChooseMatchingPartialSolutionsForPartitionSubsets(partition, sortedPartialSolutions, solution, solutionVisits);

                solution.Visits = solutionVisits.ToArray();
                partitionsSolutions.Add(solution);
            }

            return ChooseLowestCostFinalSolution(partitionsSolutions);
        }

        private static void ChooseMatchingPartialSolutionsForPartitionSubsets(int[][] partition, List<DVRPPartialSolution>[] sortedPartialSolutions,
            FinalSolution solution, List<int[]> solutionVisits)
        {
            foreach (var partitionSubset in partition)
            {
                var partitionSubsetPartialSolution = FindMatchingFinalSolutionForSubset(sortedPartialSolutions, partitionSubset.Length, partitionSubset);

                solution.OptimalCost += partitionSubsetPartialSolution.OptimalCost;
                solutionVisits.Add(partitionSubsetPartialSolution.Visits);
            }
        }

        private static DVRPPartialSolution FindMatchingFinalSolutionForSubset(List<DVRPPartialSolution>[] sortedPartialSolutions, int subsetCount,
            int[] partitionSubset)
        {
            var foundMatchingIndex = -1;

            for (var i = 0; i < sortedPartialSolutions[subsetCount].Count; i++)
            {
                var partialSolutionRequestSubset = sortedPartialSolutions[subsetCount][i].RequestsSubset;
                var diff = 0;

                for (var j = 0; j < subsetCount; j++)
                {
                    if (partialSolutionRequestSubset[j] == partitionSubset[j]) continue;

                    diff++;
                    break;
                }
                if (diff != 0) continue;
                foundMatchingIndex = i;
                break;
            }

            return sortedPartialSolutions[subsetCount][foundMatchingIndex];
        }

        private static FinalSolution ChooseLowestCostFinalSolution(List<FinalSolution> partitionsSolutions)
        {
            FinalSolution finalSolution = null;
            double[] minCost = {double.MaxValue};

            foreach (
                var partitionsSolution in
                    partitionsSolutions.Where(partitionsSolution => partitionsSolution.OptimalCost < minCost[0]))
            {
                minCost[0] = partitionsSolution.OptimalCost;
                finalSolution = partitionsSolution;
            }
            return finalSolution;
        }

        private static List<int[][]> GetRequestsSetPartitions(int[] requestsIds)
        {
            return new List<int[][]>(Partitioning.GetAllPartitions(requestsIds));
        }

        private static List<DVRPPartialSolution>[] SortPartialSolutions(int[] requestsIds, List<DVRPPartialSolution> partialSolutions)
        {
            var sortedPartialSolutions = new List<DVRPPartialSolution>[requestsIds.Length + 1];

            for (var i = 0; i < sortedPartialSolutions.Length; i++)
                sortedPartialSolutions[i] = new List<DVRPPartialSolution>();

            foreach (var dvrpPartialSolution in partialSolutions)
            {
                var count = dvrpPartialSolution.RequestsSubset.Length;
                sortedPartialSolutions[count].Add(dvrpPartialSolution);
            }
            return sortedPartialSolutions;
        }
    }
}
