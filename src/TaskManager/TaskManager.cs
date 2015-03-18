using System;
using ClusterUtils;

namespace TaskManager
{
    public class TaskManager
    {
        public string ServerAddress { get; set; }
        public string ServerPort { get; set; }

        public TaskManager(ComponentConfig cc)
        {   
            ServerAddress = cc.ServerAddress;
            ServerPort = cc.ServerPort;            
        }

        public void Start()
        {
            LogManagerInfo();

            var registrationHandler = new ComponentRegistration();

            var response = registrationHandler.Register(ServerAddress, ServerPort, "TaskManager");

            Console.WriteLine("Registered at server with Id: {0}.", response.Id);

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private void LogManagerInfo()
        {
            Console.WriteLine("Manager is running...");
            Console.WriteLine("Server address: {0}", ServerAddress);
            Console.WriteLine("Server port: {0}", ServerPort);
        }
    }
}
