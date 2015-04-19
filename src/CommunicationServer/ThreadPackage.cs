using System.Net.Sockets;
using System.Xml;

namespace CommunicationServer
{
    class ThreadPackage
    {
        public Socket Handler;
        public XmlDocument Message;
        public byte[] MessageBytes;
    }
}
