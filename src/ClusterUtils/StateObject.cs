using System.Collections.Generic;
using System.Net.Sockets;

namespace ClusterUtils
{
    public class StateObject
    {
        public Socket WorkSocket = null;

        public const int BufferSize = 1024;

        public byte[] Buffer = new byte[BufferSize];

        public List<byte> ByteBuffer = new List<byte>();
    }
}