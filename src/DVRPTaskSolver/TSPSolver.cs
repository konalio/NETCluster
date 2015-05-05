using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRPTaskSolver
{
    class TSPSolver
    {
        public static double[,] Distances;
        public static DVRPData data;
        public static LocationObject[] LocationsArray;
        /// <summary>
        /// TSP Solving using Held–Karp algorithm
        /// </summary>
        /// <param name="distances">Array of distances between locations</param>
        /// <param name="n">Number of locations</param>
        /// <param name="startingVertc">Starting location</param>
        /// <returns></returns>
        public static double Solve(int[] clients, LocationObject[] locations, int startingVertc, DVRPData dat)
        {
            int time = int.MaxValue;
            data = dat;
            LocationsArray = locations;
            Distances = CalculateDistances(locations);
            List<int> vertices = new List<int>();

            for (int i = 0; i < clients.Length; i++)
            {
                if (clients[i] == startingVertc) continue;
                vertices.Add(clients[i]);
            }

            for (int i = 0; i < data.Requests.Count; i++)
            {
                if (data.Requests[i].AvailableTime < time)
                {
                    time = data.Requests[i].AvailableTime;
                }
            }

            return StartSolving(startingVertc, vertices, data.VehicleCapacity, time);
        }

        private static double[,] CalculateDistances(LocationObject[] locations)
        {
            int n = locations.Length;
            double[,] array = new double[n, n];
            double d = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j == i) continue;
                    d = EuclideanDistance(locations[i].Location, locations[j].Location);
                    array[i, j] = d;
                    array[j, i] = d;
                }
            }

            return array;
        }

        private static double EuclideanDistance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        private static int FindOptimalAndOpenDepot(double time, Point Location)
        {
            double optimalDist = int.MaxValue;
            int index = 0;
            for (int i = 0; i < data.Depots.Count; i++)
            {
                Depot d = data.Depots[i];
                if (d.TimeWindow.Start <= time && d.TimeWindow.End >= time)
                {
                    double dist = EuclideanDistance(Location, d.Location);
                    if (dist < optimalDist)
                    {
                        index = d.Id;
                        optimalDist = dist;
                    }
                }
            }
            return index;
        }

        private static double CalculateTravelingTime(double distance, double velocity)
        {
            return (double)distance / velocity;
        }

        private static double StartSolving(int startingVertice, List<int> vertices, int currentCapacity, double currentTime)
        {
            if (vertices.Count == 0)
            {
                return Distances[startingVertice, FindOptimalAndOpenDepot(currentTime, LocationsArray[startingVertice].Location)];
            }

            double additionalCost = 0;
            bool returnToDepot = false;
            int depotIndex = 0;
            double min = int.MaxValue;
            List<int> copy = new List<int>(vertices);
            int temporaryCapacity = currentCapacity;

            if (currentCapacity <= 0)
            {
                returnToDepot = true;
                depotIndex = FindOptimalAndOpenDepot(currentTime, LocationsArray[startingVertice].Location);
                currentCapacity += data.VehicleCapacity;
                additionalCost += Distances[startingVertice, depotIndex];

            }

            foreach (int vertice in vertices)
            {
                if (((Request)LocationsArray[vertice]).AvailableTime > currentTime)
                    continue;

                int load = ((Request)LocationsArray[vertice]).Quantity;
                int timeLoad = ((Request)LocationsArray[vertice]).UnloadDuration;

                if (currentCapacity - load < 0 || currentCapacity - load > data.VehicleCapacity)
                {
                    returnToDepot = true;
                    depotIndex = FindOptimalAndOpenDepot(currentTime, LocationsArray[vertice].Location);
                    temporaryCapacity = data.VehicleCapacity;
                }


                if (returnToDepot)
                {
                    additionalCost += Distances[depotIndex, vertice];
                }


                copy.Remove(vertice);
                double dist = Distances[startingVertice, vertice];
                double timeDistance = CalculateTravelingTime(dist, data.VehicleSpeed);
                dist += StartSolving(vertice, copy, temporaryCapacity - load, currentTime + timeDistance + timeLoad);
                copy.Add(vertice);
                if (dist < min)
                {
                    min = dist;
                }
                temporaryCapacity = currentCapacity;
            }

            return min + additionalCost;

        }
    }
}
