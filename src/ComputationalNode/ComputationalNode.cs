using System;
using ClusterUtils;

namespace ComputationalNode
{
    class ComputationalNode
    {
        public string ServerPort { get; set; }

        public string ServerAddress { get; set; }
     
        public ComputationalNode(ComponentConfig componentConfig)
        {
            ServerPort = componentConfig.ServerPort;
            ServerAddress = componentConfig.ServerAddress;
        }

        public void Start()
        {
            LogNodeInfo();

            Register();

            while (true)
            {
                
            }
        }

        private void Register()
        {
            
        }

        private void LogNodeInfo()
        {
            Console.WriteLine("Node is running...");
            Console.WriteLine("Server address: {0}", ServerAddress);
            Console.WriteLine("Server port: {0}", ServerPort);
        }
    }
}
