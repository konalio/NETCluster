using System.Collections.Generic;
using System.Linq;
using DVRPTaskSolver.Wrappers.DVRP;

namespace DVRPTaskSolver
{
    public static class SolutionExtractor
    {
        public static DVRPFinalSolution ExtractLowestCostSolution(int[] requestsIds, List<DVRPPartialSolution> partialSolutions)
        {
            var partitionsSolutions = new List<DVRPFinalSolution>();

            var requestsPartitions = GetRequestsSetPartitions(requestsIds);
            var sortedPartialSolutions = SortPartialSolutions(requestsIds.Length, partialSolutions);

            foreach (var partition in requestsPartitions)
            {
                var solution = new DVRPFinalSolution();
                var solutionVisits = new List<int[]>();
                
                ChooseMatchingPartialSolutionsForPartitionSubsets(partition, sortedPartialSolutions, solution, solutionVisits);

                solution.Visits = solutionVisits.ToArray();
                partitionsSolutions.Add(solution);
            }

            return ChooseLowestCostFinalSolution(partitionsSolutions);
        }

        private static IEnumerable<int[][]> GetRequestsSetPartitions(int[] requestsIds)
        {
            return new List<int[][]>(Partitioning.GetAllPartitions(requestsIds));
        }

        private static List<DVRPPartialSolution>[] SortPartialSolutions(int requestsCount, IEnumerable<DVRPPartialSolution> partialSolutions)
        {
            var sortedPartialSolutions = new List<DVRPPartialSolution>[requestsCount + 1];

            for (var i = 0; i < sortedPartialSolutions.Length; i++)
                sortedPartialSolutions[i] = new List<DVRPPartialSolution>();

            foreach (var dvrpPartialSolution in partialSolutions)
            {
                var count = dvrpPartialSolution.RequestsSubset.Length;
                sortedPartialSolutions[count].Add(dvrpPartialSolution);
            }
            return sortedPartialSolutions;
        }

        private static void ChooseMatchingPartialSolutionsForPartitionSubsets(IEnumerable<int[]> partition, IReadOnlyList<List<DVRPPartialSolution>> sortedPartialSolutions,
            DVRPFinalSolution solution, ICollection<int[]> solutionVisits)
        {
            foreach (var partitionSubsetPartialSolution in 
                partition.Select(partitionSubset => FindMatchingFinalSolutionForSubset(partitionSubset, sortedPartialSolutions)))
            {
                solution.OptimalCost += partitionSubsetPartialSolution.OptimalCost;
                solutionVisits.Add(partitionSubsetPartialSolution.Visits);
            }
        }

        private static DVRPPartialSolution FindMatchingFinalSolutionForSubset(IReadOnlyList<int> partitionSubset, IReadOnlyList<List<DVRPPartialSolution>> sortedPartialSolutions)
        {
            var foundMatchingIndex = -1;
            var subsetCount = partitionSubset.Count;

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

        private static DVRPFinalSolution ChooseLowestCostFinalSolution(IEnumerable<DVRPFinalSolution> partitionsSolutions)
        {
            DVRPFinalSolution finalSolution = null;
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
    }
}
