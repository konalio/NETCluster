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
        //List<TaskSolver> ts_List;
        int maxParallelThreads;

        public string PrimaryServerAddress { get; set; }
        public string ServerPort { get; set; }

        //int port;
        //System.Net.IPAddress primaryServer;
        //List<System.Net.IPAddress> backupServers;
       
        public TaskManager(int mPT_)
        {
            //ts_List = new List<TaskSolver>(ts_);
            maxParallelThreads = mPT_;
          
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
