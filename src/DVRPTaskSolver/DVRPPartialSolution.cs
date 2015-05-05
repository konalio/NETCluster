using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DVRPTaskSolver
{
    [Serializable]
    public class DVRPPartialSolution
    {
        public int[] RequestsSubset;
        public int[] Visits;
        public int OptimalTime;

        public DVRPPartialSolution(int[] requestsSubset, int[] visits, int optimalTime)
        {
            this.RequestsSubset = requestsSubset;
            this.Visits = visits;
            this.OptimalTime = optimalTime;
        }

        public DVRPPartialSolution() { }

        public static byte[] Serialize(int[] requestsSubset, int[] visits, int optimalTime)
        {
            if (requestsSubset == null || visits == null)
                return null;

            DVRPPartialSolution partialSolution = new DVRPPartialSolution(requestsSubset, visits, optimalTime);

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, partialSolution);
                return ms.ToArray();
            }
        }

        public static DVRPPartialSolution GetFromByteArray(byte[] array)
        {
            var result = new DVRPPartialSolution();
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
