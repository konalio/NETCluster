using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

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

                return ms.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bObj">Object as bytes array.</param>
        /// <returns>Deserialized object.</returns>
        public static T ByteArrayObject<T>(Byte[] bObj)
        {
            using (var ms = new MemoryStream(bObj))
            {
                var xmlS = new XmlSerializer(typeof(T));
                return (T)xmlS.Deserialize(ms);
            }
        }
    }
}