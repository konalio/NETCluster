using System;

namespace CommunicationServer
{
    class CommunicationsServer
    {
        private readonly string listeningPort;
        private bool backupMode;
        private readonly int componentTimeout;
        
        public CommunicationsServer(ServerConfig configuration)
        {
            listeningPort = configuration.ServerPort;
            backupMode = configuration.IsBackup;
            componentTimeout = configuration.ComponentTimeout;
        }

        private void LogServerInfo()
        {
            Console.WriteLine("Server is running in {0} mode.", backupMode ? "backup" : "primary");
            Console.WriteLine("Listening on port " + listeningPort);
            Console.WriteLine("Componenet timeout = {0} [s]", componentTimeout);
        }

        public void Start()
        {
            LogServerInfo();
            while (true)
            {
                
            }
        }
    }
}
