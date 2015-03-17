using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;
using MessageHandler;

namespace TaskManager
{
    class TaskManager
    {
        List<TaskSolver> ts_List;
        int maxParallelThreads;
        int port;

        System.Net.IPAddress primaryServer;
        List<System.Net.IPAddress> backupServers;
       
        public TaskManager(List<TaskSolver> ts_, int mPT_, System.Net.IPAddress primary_, int port_)
        {
            ts_List = new List<TaskSolver>(ts_);
            maxParallelThreads = mPT_;
            primaryServer = primary_;
            port = port_;
        }

        public void DivideProblem()
        {

        }
        public void ChooseFinalSolution()
        {
           
        }
        public void SendRegisterMessageToServer()
        {
            List<string> problemNames = new List<string>();

            foreach(TaskSolver ts in ts_List)
            {
                problemNames.Add(ts.Name);
            }

            RegisterMessage message = new RegisterMessage(problemNames, 1, maxParallelThreads, false);
            message.SerializeAndSend(primaryServer, port);
        }

        public void SendMessageToServer()
        {

        }
    }
}
