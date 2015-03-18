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

            var registrationHander = new ComponentRegistration();

            var response = registrationHander.Register(ServerAddress, ServerPort, "ComputationalNode");

            Console.WriteLine("Registered at server with Id: {0}.", response.Id);

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }


        private void LogNodeInfo()
        {
            Console.WriteLine("Node is running...");
            Console.WriteLine("Server address: {0}", ServerAddress);
            Console.WriteLine("Server port: {0}", ServerPort);
        }
    }
}
