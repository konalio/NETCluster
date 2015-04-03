using System;

namespace ClusterUtils
{
    public abstract class Component
    {
        protected ServerInfo _serverInfo;
        protected string Type;

        protected Component(ComponentConfig config, string type) 
        {
            _serverInfo = new ServerInfo(config.ServerPort, config.ServerAddress);
            Type = type;
        }

        protected void LogRuntimeInfo()
        {
            Console.WriteLine("{0} is running...", Type);
            Console.WriteLine("Server address: {0}", _serverInfo.Address);
            Console.WriteLine("Server port: {0}", _serverInfo.Port);
        }
    }
}
