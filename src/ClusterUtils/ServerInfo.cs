namespace ClusterUtils
{
    public class ServerInfo
    {
        public ServerInfo(string port, string address)
        {
            Port = port;
            Address = address;
        }

        public string Port { get; set; }
        public string Address { get; set; }
    }
}