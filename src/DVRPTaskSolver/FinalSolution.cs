using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DVRPTaskSolver
{
    [Serializable]
    public class FinalSolution
    {
        public int OptimalTime;
        public int[][] Visits;

        public FinalSolution(int optimalTime, int[][] visits)
        {
            this.OptimalTime = optimalTime;
            this.Visits = visits;
        }

        public FinalSolution() { }

        public static byte[] Serialize(int optimalTime, int[][] visits)
        {
            if (visits == null)
                return null;

            FinalSolution finalSolution = new FinalSolution(optimalTime, visits);

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, finalSolution);
                return ms.ToArray();
            }
        }

        public static FinalSolution GetFromByteArray(byte[] array)
        {
            var result = new FinalSolution();
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(array))
                {
                    result = (FinalSolution)formatter.Deserialize(ms);
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
