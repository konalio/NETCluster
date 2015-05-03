using System;
using System.Collections.Generic;
using System.Text;

namespace DVRPTaskSolver
{
    public class DVRPTaskSolver : UCCTaskSolver.TaskSolver
    {
        public DVRPTaskSolver(byte[] problemData) : base(problemData) { }

        public override byte[][] DivideProblem(int threadCount)
        {
            throw new NotImplementedException();
        }

        public override byte[] MergeSolution(byte[][] solutions)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            var message = System.Text.Encoding.Default.GetString(base._problemData);
            int end = message.IndexOf("EOF") - 2;

            int depotsNumber = GetIntValueFromFile(message, "NUM_DEPOTS: ", end);
            int capacitiesNumber = GetIntValueFromFile(message, "NUM_CAPACITIES: ", end);
            int visits = GetIntValueFromFile(message, "NUM_VISITS: ", end);
            int locationsNumber = GetIntValueFromFile(message, "NUM_LOCATIONS: ", end);
            int vehicles = GetIntValueFromFile(message, "NUM_VEHICLES: ", end);
            int capacity = GetIntValueFromFile(message, "CAPACITIES: ", end);
            int speed = GetIntValueFromFile(message, "SPEED: ", end);
            int maxTime = GetIntValueFromFile(message, "MAX_TIME: ", end);


            string edgeWeightType = GetStringValueFromFile(message, "EDGE_WEIGHT_TYPE: ", end);
            string edgeWeightFormat = GetStringValueFromFile(message, "EDGE_WEIGHT_FORMAT: ", end);
            string objective = GetStringValueFromFile(message, "OBJECTIVE: ", end);

            int[][] demands = GetArrayFromFile(message, " DEMAND_SECTION", end, locationsNumber,1, locationsNumber - depotsNumber);
  

            throw new NotImplementedException();
        }

        public int GetIntValueFromFile(string file, string name, int end)
        {
            int index = file.IndexOf(name);           
            StringBuilder sb = new StringBuilder();

            if (index < 0)
                return 0;

            index++;
            while (file[index] != '\n' && index < end)
            {
                sb.Append(file[index]);
                index++;
            }

            return int.Parse(sb.ToString());

        }

        public string GetStringValueFromFile(string file, string name, int end)
        {
            int index = file.IndexOf(name);
            StringBuilder sb = new StringBuilder();

            if (index < 0)
                return null;

            index++;
            while (file[index] != '\n' && index < end)
            {
                sb.Append(file[index]);
                index++;
            }

            return sb.ToString();
        }

        public int[][] GetArrayFromFile(string file, string name, int end, int arraySize1, int arraySize2, int clientsNumber)
        {
            int index = file.IndexOf(name);
            int row = 0;
            int column = 0;
            var array = new int[arraySize1][];

            StringBuilder[] values = new StringBuilder[arraySize2];             

            if (index < 0)
                return null;

            index++;

            while (row < clientsNumber)
            {
                int[] arrayValues = new int[arraySize2];
                while (file[index] != '\n' && index < end)
                {
                   
                    while (file[index] != ' ')
                    {
                        values[column].Append(file[index]);
                        index++;
                    }
                    arrayValues[column] = int.Parse(values[column].ToString());
                    column++;
                    index++;

                }
                array[int.Parse(values[0].ToString())] = arrayValues;
                column = 0;
                row++;
                foreach (StringBuilder sb in values)
                    sb.Clear();
                

            }
            return array;

        }
        
    }
}
