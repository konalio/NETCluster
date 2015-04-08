using System.Collections.Generic;
using System.Net.Sockets;

namespace ClusterUtils.Communication
{
    public class StateObject
    {
        public Socket WorkSocket = null;

        public const int BufferSize = 2048;

        public byte[] Buffer = new byte[BufferSize];

        public List<byte> ByteBuffer = new List<byte>();
    }
}