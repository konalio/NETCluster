namespace ClusterUtils
{
    /// <summary>
    /// Simple container for basic server informations.
    /// </summary>
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