using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Xml;
namespace CommunicationServer
{
    class ThreadPackage
    {
        public Socket Handler;
        public XmlDocument Message;
        public ThreadPackage(Socket h, XmlDocument m)
        {
            Handler = h;
            Message = m;
        }
    }
}
