using System.Collections.Generic;
using System.Net.Sockets;

namespace ClusterUtils.Communication
{
    /// <summary>
    /// Helper class for asynchronous sockets processing.
    /// </summary>
    public class StateObject
    {
        /// <summary>
        /// Remote endpoint socket.
        /// </summary>
        public Socket WorkSocket = null;
        
        /// <summary>
        /// Size of message buffer.
        /// </summary>
        public const int BufferSize = 256;

        /// <summary>
        /// MessageBuffer.
        /// </summary>
        public byte[] Buffer = new byte[BufferSize];

        /// <summary>
        /// Byte buffer accumulating multiple message parts.
        /// </summary>
        public List<byte> ByteBuffer = new List<byte>();
    }
}