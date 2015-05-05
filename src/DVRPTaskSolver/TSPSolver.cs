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

        /// <summary>
        /// TSP Solving using Held–Karp algorithm
        /// </summary>
        /// <param name="distances">Array of distances between locations</param>
        /// <param name="n">Number of locations</param>
        /// <param name="startingVertc">Starting location</param>
        /// <returns></returns>
        public static double Solve(double[,] distances, int n, int startingVertc)
        {
            Distances = distances;
            List<int> vertices = new List<int>();
            for (int i = 0; i < n; i++)
            {
                if (i == startingVertc) continue;
                vertices.Add(i);
            }
            return StartSolving(startingVertc, startingVertc, vertices);
        }

        public static double StartSolving(int firstVertice, int startingVertice, List<int> vertices)
        {
            if (vertices.Count == 0)
            {
                return Distances[startingVertice, firstVertice];
            }

            double min = int.MaxValue;
            List<int> copy = new List<int>(vertices);

            foreach (int vertice in vertices)
            {
                copy.Remove(vertice);
                double dist = StartSolving(firstVertice, vertice, copy);
                dist += Distances[startingVertice, vertice];
                copy.Add(vertice);
                if (dist < min)
                {
                    min = dist;
                }
            }

            return min;

        }
    }
}
