using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DVRPTaskSolver
{
    [Serializable]
    public class FinalSolution
    {
        public double OptimalCost;
        public int[][] Visits;

        public FinalSolution(double optimalCost, int[][] visits)
        {
            OptimalCost = optimalCost;
            Visits = visits;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(string.Format("Optimal cost: {0}", OptimalCost));

            for (var i = 0; i < Visits.Length; i++)
            {
                stringBuilder.Append(string.Format("Visits for vehicle {0}: ", i));

                for (var j = 0; j < Visits[i].Length; j++)
                {
                    stringBuilder.Append(string.Format("{0} ", Visits[i][j]));
                }

                stringBuilder.AppendLine("");
            }

            return stringBuilder.ToString();
        }

        public FinalSolution() { }

        public static byte[] Serialize(double optimalTime, int[][] visits)
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
