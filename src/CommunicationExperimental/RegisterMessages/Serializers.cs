using System;
using System.IO;
using System.Xml.Serialization;

namespace RegisterMessages
{
    public static class Serializers
    {
        public static Byte[] ObjectToByteArray<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var xmlS = new XmlSerializer(typeof(T));
                xmlS.Serialize(ms, obj);

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
