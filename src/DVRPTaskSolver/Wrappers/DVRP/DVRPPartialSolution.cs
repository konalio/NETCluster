using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DVRPTaskSolver.Wrappers.DVRP
{
    [Serializable]
    public class DVRPPartialSolution
    {
        public int[] RequestsSubset;
        public int[] Visits;
        public double OptimalCost;

        public DVRPPartialSolution(int[] requestsSubset, int[] visits, double optimalCost)
        {
            RequestsSubset = requestsSubset;
            Visits = visits;
            OptimalCost = optimalCost;
        }

        public DVRPPartialSolution() { }

        public static byte[] Serialize(int[] requestsSubset, int[] visits, double optimalTime)
        {
            if (requestsSubset == null || visits == null)
                return null;

            var partialSolution = new DVRPPartialSolution(requestsSubset, visits, optimalTime);

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, partialSolution);
                return ms.ToArray();
            }
        }

        public static DVRPPartialSolution GetFromByteArray(byte[] array)
        {
            DVRPPartialSolution result;
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(array))
                {
                    result = (DVRPPartialSolution)formatter.Deserialize(ms);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }
    }
}
