using System;
using System.Collections.Generic;
using System.Linq;

namespace DVRPTaskSolver
{

    public class ArrayComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            var count = x.Where((t1, i) => x.Any(t => t == y[i])).Count();
            return count == x.Length;
        }
        public int GetHashCode(int[] obj)
        {
            var result = 17;
            foreach (var t in obj)
            {
                unchecked
                {
                    result = result + t;
                }
            }
            return result;
        }

    }
    public class TSPSolver
    {
        private readonly double[,] _distances;
        private readonly DVRPData _data;
        private readonly LocationObject[] _locationsArray;

        private Dictionary<int[], int> _nextVertices;

        public TSPSolver(double[,] distances, DVRPData commonData, LocationObject[] locations)
        {
            _distances = distances;
            _data = commonData;
            _locationsArray = locations;
        }

        /// <summary>
        /// TSP Solving using Held–Karp algorithm
        /// </summary>
        /// <param name="path"></param>
        /// <param name="clients"></param>
        /// <param name="startingVertice"></param>
        /// <returns></returns>
        public double Solve(out int[] path, int[] clients, int startingVertice)
        {
            _nextVertices = new Dictionary<int[], int>(new ArrayComparer());
            path = new int[clients.Length];

            var minCost = StartSolving(startingVertice, clients.ToList(), _data.VehicleCapacity, 0, new List<int>());

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

        private double StartSolving(int startingVertice, List<int> vertices, int currentCapacity, double currentTime, List<int> currentPath)
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

            foreach (var vertice in vertices)
            {
                if (((Request)_locationsArray[vertice]).AvailableTime > temporaryTime)
                {
                    temporaryTime = ((Request)_locationsArray[vertice]).AvailableTime;
                }

                var load = ((Request)_locationsArray[vertice]).Quantity;
                var timeLoad = ((Request)_locationsArray[vertice]).UnloadDuration;

                if (currentCapacity - load < 0 || currentCapacity - load > _data.VehicleCapacity)
                {
                    temporaryCapacity = currentCapacity - load < 0 ? _data.VehicleCapacity : 0;

                    depotIndex = FindClosestOpenedDepot(currentTime, _locationsArray[vertice].Location);
                    additionalCost += _distances[depotIndex, vertice];
                }

                copy.Remove(vertice);
                currentPath.Add(vertice);

                var dist = _distances[startingVertice, vertice];
                var timeDistance = CalculateTravelingTime(dist, _data.VehicleSpeed);
                dist += StartSolving(vertice, copy, temporaryCapacity - load, temporaryTime + timeDistance + timeLoad, currentPath);

                min = UpdateBestIfFoundBetterPath(vertices, dist, min, vertice);

                currentPath.Remove(vertice);
                copy.Add(vertice);
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
