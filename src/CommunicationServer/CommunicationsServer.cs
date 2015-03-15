using System;

namespace CommunicationServer
{
    class CommunicationsServer
    {
        private string listeningPort;
        private bool backupMode;
        private int componentTimeout;
        
        public CommunicationsServer(ServerConfig configuration)
        {
            listeningPort = configuration.ServerPort;
            backupMode = configuration.IsBackup;
            componentTimeout = configuration.ComponentTimeout;
        }

        public void Start()
        {
            Console.WriteLine("Server is running...");
            while (true)
            {
                
            }
        }
    }
}
