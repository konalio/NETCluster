using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ClusterMessages;
using ClusterUtils.Communication;

namespace ClusterUtils
{
    /// <summary>
    /// Helper class for serializing/deserializing objects.
    /// </summary>
    public static class Serializers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">Object to be serialized.</param>
        /// <returns>Object as bytes.</returns>
        public static Byte[] ObjectToByteArray<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var streamWriter = new StreamWriter(ms, Encoding.UTF8);
                var xmlS = new XmlSerializer(obj.GetType());
                xmlS.Serialize(streamWriter, obj);

                var streamBytes = ms.ToArray();
                var messageBytes = new List<byte>();

                for (var i = 3; i < streamBytes.Length; i++)
                {
                    messageBytes.Add(streamBytes[i]);
                }

                return messageBytes.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bObj">Object as bytes array.</param>
        /// <returns>Deserialized object.</returns>
        public static T ByteArrayObject<T>(Byte[] bObj)
        {
            bObj = RemoveETBIfPresent(bObj);

            using (var ms = new MemoryStream(bObj))
            {
                var streamWriter = new StreamReader(ms, Encoding.UTF8);
                var xmlS = new XmlSerializer(typeof(T));
                return (T)xmlS.Deserialize(ms);
            }
        }

        public static MessagePackage MessageFromByteArray(Byte[] bytes)
        {
            var xmlMessage = ByteArrayObject<XmlDocument>(bytes);
            var clusterMessage = ClusterMessageFromByteArray(xmlMessage, bytes);

            return new MessagePackage
            {
                ClusterMessage = clusterMessage,
                XmlMessage = xmlMessage,
                Bytes = bytes
            };
        }

        public static IClusterMessage ClusterMessageFromByteArray(XmlDocument message, Byte[] bytes)
        {
            if (message == null)
                throw new ArgumentNullException();

            if (message.DocumentElement == null)
                return null;

            var messageName = message.DocumentElement.Name;

            switch (messageName)
            {
                case "DivideProblem":
                    return ByteArrayObject<DivideProblem>(bytes);
                case "NoOperation":
                    return ByteArrayObject<NoOperation>(bytes);
                case "SolvePartialProblems":
                    return ByteArrayObject<SolvePartialProblems>(bytes);
                case "Register":
                    return ByteArrayObject<Register>(bytes);
                case "RegisterResponse":
                    return ByteArrayObject<RegisterResponse>(bytes);
                case "Solutions":
                    return ByteArrayObject<Solutions>(bytes);
                case "SolutionRequest":
                    return ByteArrayObject<SolutionRequest>(bytes);
                case "SolveRequest":
                    return ByteArrayObject<SolveRequest>(bytes);
                case "SolveRequestResponse":
                    return ByteArrayObject<SolveRequestResponse>(bytes);
                case "Status":
                    return ByteArrayObject<Status>(bytes);
                case "Error":
                    return ByteArrayObject<Error>(bytes);
                default:
                    return null;
            }
        }

        private static byte[] RemoveETBIfPresent(Byte[] bObj)
        {
            var byteLength = bObj.Length;

            if (bObj[byteLength - 1] == 23)
            {
                bObj = RemoveETB(bObj, byteLength);
            }
            return bObj;
        }

        private static byte[] RemoveETB(Byte[] bObj, int byteLength)
        {
            var buffer = new List<byte>();
            for (var i = 0; i < byteLength - 1; i++)
            {
                buffer.Add(bObj[i]);
            }
            bObj = buffer.ToArray();
            return bObj;
        }
    }
}