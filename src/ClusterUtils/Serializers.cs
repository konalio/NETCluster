using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ClusterUtils
{
    public static class Serializers
    {
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