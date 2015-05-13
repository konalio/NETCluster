using System;
using System.Collections.Generic;
using System.Linq;
using DVRPTaskSolver.Wrappers;
using DVRPTaskSolver.Wrappers.DVRP;

namespace DVRPTaskSolver
{
    /// <summary>
    /// Class for solving TSP-like problem for subset of clients. Restrictions described in DVRP definition are applied.
    /// </summary>
    public class TSPSolver
    {
        private readonly double[,] _distances;
        private readonly DVRPData _data;
        private readonly LocationObject[] _locationsArray;

        private Dictionary<int[], int> _nextVertices;

        /// <summary>
        /// Constructor for TSPSolver.
        /// </summary>
        /// <param name="locations">Locations of all depots and related verticies.</param>
        /// <param name="distances">Distances between all locations (indexed by depot/visit Id).</param>
        /// <param name="commonData">General data related to problem (vehicle sleep, capacity, etc).</param>
        public TSPSolver(LocationObject[] locations, double[,] distances, DVRPData commonData)
        {
            _distances = distances;
            _data = commonData;
            _locationsArray = locations;
        }

        /// <summary>
        /// TSP Solving using Held–Karp algorithm.
        /// </summary>
        /// <param name="path">(Output parameter) Found minimum-cost path.</param>
        /// <param name="clients">Clients that we have to visit.</param>
        /// <param name="startingVertice">Vertex from which we start travelling.</param>
        /// <returns>Distance travelled in returned path.</returns>
        public double Solve(out int[] path, int[] clients, int startingVertice)
        {
            _nextVertices = new Dictionary<int[], int>(new ArrayComparer());
            path = new int[clients.Length];

            var minCost = StartSolving(startingVertice, clients.ToList(), _data.VehicleCapacity, 0);

            RecreatePath(path, clients);

            return minCost;

        }

        private void RecreatePath(int[] path, int[] clients)
        {
            var clientsList = new List<int>(clients);

            for (var i = 0; i < clients.Length; i++)
            {
                path[i] = _nextVertices[clientsList.ToArray()];
                clientsList.Remove(path[i]);
            }
        }

        private double EuclideanDistance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        /// <summary>
        /// Finds depot id that is closest to given location and is open at given time.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private int FindClosestOpenedDepot(double time, Point location)
        {
            var optimalDist = double.MaxValue;
            var index = 0;
            foreach (var depot in _data.Depots)
            {
                if (!IsInTimeWindow(time, depot.TimeWindow)) continue;

                var dist = EuclideanDistance(location, depot.Location);

                if (!(dist < optimalDist)) continue;

                index = depot.Id;
                optimalDist = dist;
            }
            return index;
        }

        private static bool IsInTimeWindow(double time, TimeWindow timeWindow)
        {
            return (timeWindow.Start <= time) && (time >= timeWindow.End);
        }

        private static double CalculateTravelingTime(double distance, double velocity)
        {
            return distance / velocity;
        }

        /// <summary>
        /// Main function in TSPSolver. Recursively tries to add all of left verticies. If it finds path with lower cost - replaces old one.
        /// </summary>
        /// <param name="startingVertice">Vertex from which we start looking for next vertex.</param>
        /// <param name="vertices">Verticies that we are considering as possible next in path.</param>
        /// <param name="currentCapacity">Current load of vehicle.</param>
        /// <param name="currentTime">Current moment in time.</param>
        /// <returns></returns>
        private double StartSolving(int startingVertice, List<int> vertices, int currentCapacity, double currentTime)
        {
            if (vertices.Count == 0)
            {
                var depoIndex = FindClosestOpenedDepot(currentTime, _locationsArray[startingVertice].Location);
                return depoIndex == -1 ? int.MaxValue : _distances[startingVertice, depoIndex];
            }

            double additionalCost = 0;
            var temporaryTime = currentTime;
            double min = int.MaxValue;
            int depotIndex;
            var copy = new List<int>(vertices);

            if (currentCapacity == 0 || currentCapacity == _data.VehicleCapacity)
            {
                if (currentCapacity == 0)
                    currentCapacity += _data.VehicleCapacity;
                else
                    currentCapacity = 0;

                depotIndex = FindClosestOpenedDepot(currentTime, _locationsArray[startingVertice].Location);
                additionalCost += _distances[startingVertice, depotIndex];
            }

            var temporaryCapacity = currentCapacity;

            foreach (var vertex in vertices)
            {
                if (((Request)_locationsArray[vertex]).AvailableTime > temporaryTime)
                {
                    temporaryTime = ((Request)_locationsArray[vertex]).AvailableTime;
                }

                var load = ((Request)_locationsArray[vertex]).Quantity;
                var timeLoad = ((Request)_locationsArray[vertex]).UnloadDuration;

                if (currentCapacity - load < 0 || currentCapacity - load > _data.VehicleCapacity)
                {
                    temporaryCapacity = currentCapacity - load < 0 ? _data.VehicleCapacity : 0;

                    depotIndex = FindClosestOpenedDepot(currentTime, _locationsArray[vertex].Location);
                    additionalCost += _distances[depotIndex, vertex];
                }

                copy.Remove(vertex);

                var dist = _distances[startingVertice, vertex];
                var timeDistance = CalculateTravelingTime(dist, _data.VehicleSpeed);
                dist += StartSolving(vertex, copy, temporaryCapacity - load, temporaryTime + timeDistance + timeLoad);

                min = UpdateBestIfFoundBetterPath(vertices, dist, min, vertex);

                copy.Add(vertex);
                temporaryCapacity = currentCapacity;
                temporaryTime = currentTime;
            }

            return min + additionalCost;
        }

        private double UpdateBestIfFoundBetterPath(List<int> vertices, double dist, double min, int vertice)
        {
            if (!(dist < min)) return min;

            min = dist;

            try
            {
                _nextVertices.Add(vertices.ToArray(), vertice);
            }
            catch (Exception)
            {
                _nextVertices[vertices.ToArray()] = vertice;
            }
            return min;
        }
    }
}
