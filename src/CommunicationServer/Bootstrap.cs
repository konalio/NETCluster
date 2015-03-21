namespace CommunicationServer
{
    class Bootstrap
    {
        static void Main(string[] args)
        {
            var communicationServer = new CommunicationsServer(ServerConfig.GetServerConfig(args));

            communicationServer.Start();
        }
    }
}
