namespace CommunicationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var communicationServer = new CommunicationsServer(ServerConfig.GetServerConfig(args));

            communicationServer.Start();
        }
    }
}
