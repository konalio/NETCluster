using System.Xml;
using ClusterMessages;

namespace ClusterUtils.Communication
{
    public class MessagePackage
    {
        public XmlDocument XmlMessage;
        public IClusterMessage ClusterMessage;
        public byte[] Bytes;
    }
}
