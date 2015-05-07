using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DVRPTaskSolver.Wrappers.DVRP
{
    [Serializable]
    public class DVRPFinalSolution
    {
        public double OptimalCost;
        public int[][] Visits;

        public DVRPFinalSolution(double optimalCost, int[][] visits)
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

        public DVRPFinalSolution() { }

        public static byte[] Serialize(double optimalTime, int[][] visits)
        {
            if (visits == null)
                return null;

            DVRPFinalSolution finalSolution = new DVRPFinalSolution(optimalTime, visits);

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, finalSolution);
                return ms.ToArray();
            }
        }

        public static DVRPFinalSolution GetFromByteArray(byte[] array)
        {
            DVRPFinalSolution result;
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(array))
                {
                    result = (DVRPFinalSolution)formatter.Deserialize(ms);
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
