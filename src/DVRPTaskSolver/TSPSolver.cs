using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int count = 0;
            for (int i = 0; i < x.Length; i++)
            {
                for (int j = 0; j < x.Length; j++)
                {
                    if (x[j] == y[i])
                    {
                        count++;
                        break;
                    }
                }

            }
            if (count == x.Length)
                return true;
            return false;
        }
        public int GetHashCode(int[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result + obj[i];
                }
            }
            return result;
        }

    }
    class TSPSolver
    {
        public static double[,] Distances;
        public static DVRPData Data;
        public static LocationObject[] LocationsArray;
        public static int ClientsCount;

        public static Dictionary<int[], int> NextVertices;

        /// <summary>
        /// TSP Solving using Held–Karp algorithm
        /// </summary>
        /// <param name="distances">Array of distances between locations</param>
        /// <param name="n">Number of locations</param>
        /// <param name="startingVertc">Starting location</param>
        /// <returns></returns>
        public static double Solve(out int[] path, int[] clients, LocationObject[] locations, int startingVertice, DVRPData dat)
        {
            Data = dat;
            LocationsArray = locations;
            Distances = CalculateDistances(locations);
            ClientsCount = clients.Length;
            NextVertices = new Dictionary<int[], int>(new ArrayComparer());
            path = new int[clients.Length];
            List<int> vertices = new List<int>();
            List<int> currentPath = new List<int>();
            List<int> clientsList = new List<int>(clients);
            int time = 0;


            for (int i = 0; i < clients.Length; i++)
            {               
                vertices.Add(clients[i]);
            }

            double minCost = StartSolving(startingVertice, vertices, Data.VehicleCapacity, time, currentPath);

            for (int i = 0; i < clients.Length; i++)
            {
                path[i] = NextVertices[clientsList.ToArray()];
                clientsList.Remove(path[i]);
            }

            return minCost;

        }

        /// <summary>
        /// Calculate array of distances between all locations
        /// </summary>
        /// <param name="locations">Array of all locations</param>
        /// <returns>Array of distances</returns>
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
            for (int i = 0; i < Data.Depots.Count; i++)
            {
                Depot d = Data.Depots[i];
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

        private static double StartSolving(int startingVertice, List<int> vertices, int currentCapacity, double currentTime, List<int> currentPath)
        {
            if (vertices.Count == 0)
            {
                return Distances[startingVertice, FindOptimalAndOpenDepot(currentTime, LocationsArray[startingVertice].Location)];
            }

            double additionalCost = 0;
            double temporaryTime = currentTime;
            double min = int.MaxValue;
            int depotIndex = 0;
            int temporaryCapacity = currentCapacity;
            List<int> copy = new List<int>(vertices);

            if (currentCapacity <= 0)
            {
                depotIndex = FindOptimalAndOpenDepot(currentTime, LocationsArray[startingVertice].Location);
                currentCapacity += Data.VehicleCapacity;
                additionalCost += Distances[startingVertice, depotIndex];

            }

            foreach (int vertice in vertices)
            {
                if (((Request)LocationsArray[vertice]).AvailableTime > temporaryTime)
                {
                    temporaryTime = ((Request)LocationsArray[vertice]).AvailableTime;
                }

                int load = ((Request)LocationsArray[vertice]).Quantity;
                int timeLoad = ((Request)LocationsArray[vertice]).UnloadDuration;

                if (currentCapacity - load < 0 || currentCapacity - load > Data.VehicleCapacity)
                {
                    depotIndex = FindOptimalAndOpenDepot(currentTime, LocationsArray[vertice].Location);
                    temporaryCapacity = Data.VehicleCapacity;

                    additionalCost += Distances[depotIndex, vertice];
                }

                copy.Remove(vertice);
                currentPath.Add(vertice);

                double dist = Distances[startingVertice, vertice];
                double timeDistance = CalculateTravelingTime(dist, Data.VehicleSpeed);
                dist += StartSolving(vertice, copy, temporaryCapacity - load, temporaryTime + timeDistance + timeLoad, currentPath);

                if (dist < min)
                {
                    min = dist;

                    try
                    {
                        NextVertices.Add(vertices.ToArray(), vertice);
                    }
                    catch (Exception)
                    {
                        NextVertices[vertices.ToArray()] = vertice;
                    }

                }
                currentPath.Remove(vertice);
                copy.Add(vertice);
                temporaryCapacity = currentCapacity;
                temporaryTime = currentTime;
            }


            return min + additionalCost;

        }



    }
}
