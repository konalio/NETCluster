using ClusterUtils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ComputationalNode
{
    class Bootstrap
    {
        static void Main(string[] args)
        {
            //var node = new ComputationalNode(ComponentConfig.GetComponentConfig(args));
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string localIP = "";
            foreach (IPAddress ip in host.AddressList)
            {
                 if (ip.AddressFamily == AddressFamily.InterNetwork)
                 {
                     localIP = ip.ToString();
                     break;
                 }
            }
            var node = new ComputationalNode(new ComponentConfig()
            {
                ServerPort = "11000",
                ServerAddress = localIP
            });

            node.Start();

            List<StatusThread> statusThreads = new List<StatusThread>();
            statusThreads.Add(new StatusThread()
            {
                State = StatusThreadState.Busy,
                // not sure if it's even needed
                //ProblemInstanceIdSpecified = true,
                //HowLongSpecified = true,
                //TaskIdSpecified = true,

                ProblemInstanceId = 11,
                HowLong = 1000,
                TaskId = 111,
                ProblemType = "First problem type",
                
            });
            statusThreads.Add(new StatusThread()
            {
                State = StatusThreadState.Busy,
                ProblemInstanceId = 22,
                HowLong = 2000,
                TaskId = 222,
                ProblemType = "Second problem type",
            });

            node.StatusThreads = statusThreads;
            node.AssignedId = 420;

            node.StartSendingStatus();

            Console.ReadLine();

        }
    }
}
