using System.Net.Sockets;
using ClusterUtils.Communication;

namespace CommunicationServer
{
    class ThreadPackage
    {
        public Socket Handler;
        public MessagePackage Message;
    }
}
