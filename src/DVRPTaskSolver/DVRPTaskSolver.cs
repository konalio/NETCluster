using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DVRPTaskSolver.Wrappers;
using DVRPTaskSolver.Wrappers.DVRP;

namespace DVRPTaskSolver
{
    public class DVRPTaskSolver : UCCTaskSolver.TaskSolver
    {
        public DVRPTaskSolver(byte[] problemData) : base(problemData) { }

        public override byte[][] DivideProblem(int threadCount)
        {
            var dvrpData = DVRPData.GetFromBytes(_problemData);
            var requests = dvrpData.Requests;
            var requestsCount = dvrpData.RequestsCount;
            var requestsIds = new int[requestsCount];
            var requestsSubsets = new List<List<int>>();
            var subset = new List<int>();

            for (var i = 0; i < requestsCount; i++)
                requestsIds[i] = requests[i].Id;

            requestsSubsets.Add(subset);

            SplitIntoSubsetsRecursive(requestsIds, ref requestsSubsets, subset, requestsCount, 0);

            var returnSubsets = new byte[requestsSubsets.Count][];

            for (int i = 0; i < requestsSubsets.Count; i++)
                returnSubsets[i] = DVRPLocationsSubset.Serialize(requestsSubsets[i].ToArray());

            return returnSubsets;
        }

        /// <summary>
        /// Calculates all subsets of clients for given set of clients recursively.
        /// </summary>
        /// <param name="requestsIds">Id's of clients in this problem instance.</param>
        /// <param name="requestsSubsets">Generated subsets.</param>
        /// <param name="lastSubset">Previous subset to be extended.</param>
        /// <param name="requestsCount">Number of clients.</param>
        /// <param name="lastIndex"></param>
        private void SplitIntoSubsetsRecursive(int[] requestsIds, ref List<List<int>> requestsSubsets, List<int> lastSubset, int requestsCount, int lastIndex)
        {
            for (var i = lastIndex; i < requestsCount; i++)
            {
                var subset = lastSubset.ConvertAll(request => request);
                subset.Add(requestsIds[i]);
                requestsSubsets.Add(subset);

                SplitIntoSubsetsRecursive(requestsIds, ref requestsSubsets, subset, requestsCount, i + 1);
            }
        }

        public override byte[] MergeSolution(byte[][] solutions)
        {
            var requestsIds = GetAllRequestsIds();
            var partialSolutions = solutions.Select(DVRPPartialSolution.GetFromByteArray).ToList();

            var finalSolution = SolutionExtractor.ExtractLowestCostSolution(requestsIds, partialSolutions);

            if (finalSolution == null)
                return Encoding.UTF8.GetBytes("Error.");

            var finalSolutionString = finalSolution.ToString();
            var finalSolutionBytes = Encoding.UTF8.GetBytes(finalSolutionString);

            return finalSolutionBytes;
        }

        private int[] GetAllRequestsIds()
        {
            var dvrpData = DVRPData.GetFromBytes(_problemData);
            var requestsIds = new int[dvrpData.RequestsCount];
            for (var i = 0; i < dvrpData.RequestsCount; i++)
                requestsIds[i] = dvrpData.Requests[i].Id;
            return requestsIds;
        }

        public override string Name
        {
            get { return "DVRP"; }
        }

        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {

            var dvrpData = DVRPData.GetFromBytes(_problemData);
            var locationsData = DVRPLocationsSubset.GetFromByteArray(partialData);
            var locationsArray = locationsData.Locations;
            var locations = ConstructLocationArray(dvrpData.Depots, dvrpData.Requests);
            double min = int.MaxValue;
            int[] finalPath = null;

            var distances = CalculateDistances(locations);

            foreach (var d in dvrpData.Depots)
            {
                int[] path;
                var tspSolver = new TSPSolver(locations, distances, dvrpData);
                var cost = tspSolver.Solve(out path, locationsArray, d.Id);

                if (!(cost < min)) continue;
                finalPath = path;
                min = cost;
            }

            var partialSolutionBytes = DVRPPartialSolution.Serialize(locationsArray, finalPath, (int)min);

            return partialSolutionBytes;
        }

        private LocationObject[] ConstructLocationArray(List<Depot> depots, List<Request> requests)
        {
            if (depots == null) throw new ArgumentNullException("depots");
            if (requests == null) throw new ArgumentNullException("requests");
            var array = depots.Cast<LocationObject>().ToList();
            array.AddRange(requests);
            return array.ToArray();
        }

        private double[,] CalculateDistances(IReadOnlyList<LocationObject> locations)
        {
            if (locations == null) throw new ArgumentNullException("locations");
            var n = locations.Count;
            var array = new double[n, n];

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    if (j == i) continue;
                    var d = EuclideanDistance(locations[i].Location, locations[j].Location);
                    array[i, j] = d;
                    array[j, i] = d;
                }
            }

            return array;
        }

        private double EuclideanDistance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
    }
}
