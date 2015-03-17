using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;
using ClusterUtils;

namespace TaskManager
{
    public class TaskManager
    {
        private int maxParallelThreads;
        private int timeout;

        public string PrimaryServerAddress { get; set; }
        public string ServerPort { get; set; }

        public TaskManager(int maxPT)
        {   
            /*
            Do ustawiania maxParrallelThreads:
            Parallel.For(0, maxPT, new ParallelOptions { MaxDegreeOfParallelism = 4 }, count =>{Console.WriteLine(count);});               
            */

            ComponentConfig cc = ComponentConfig.GetConfigFromAppConfig();
            PrimaryServerAddress = cc.ServerAddress;
            ServerPort = cc.ServerPort;            
        }

        public void DivideProblem()
        {

        }
        public void ChooseFinalSolution()
        {
           
        }
        public void SendRegisterMessageToServer()
        {

        }

        public void SendMessageToServer()
        {

        }
    }
}
