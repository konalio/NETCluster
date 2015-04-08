using System;

namespace CommunicationServer
{
    /// <summary>
    /// Implementation of Communications Server.
    /// Supports processing all kinds of messages with no backup server.
    /// </summary>
    class CommunicationsServer
    {
        private readonly string _listeningPort;
        private bool _backupMode;
        private readonly int _componentTimeout;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration">Server config from App.config and arguments.</param>
        public CommunicationsServer(ServerConfig configuration)
        {
            _listeningPort = configuration.ServerPort;
            _backupMode = configuration.IsBackup;
            _componentTimeout = configuration.ComponentTimeout;
        }

        /// <summary>
        /// Prints info about the server:
        /// - Server mode (primary/backup),
        /// - Port that the server is listening on,
        /// - Component timeout.
        /// </summary>
        private void LogServerInfo()
        {
            Console.WriteLine("Server is running in {0} mode.", _backupMode ? "backup" : "primary");
            Console.WriteLine("Listening on port " + _listeningPort);
            Console.WriteLine("Componenet timeout = {0} [s]", _componentTimeout);
            
        }

        /// <summary>
        /// Starts listening and dispatching messages from components.
        /// </summary>
        public void Start()
        {
            LogServerInfo();
            var md = new MessageDispatcher(_listeningPort, _componentTimeout);
            md.BeginDispatching();    
        }
    }
}
