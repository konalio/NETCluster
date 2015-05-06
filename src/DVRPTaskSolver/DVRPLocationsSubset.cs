using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DVRPTaskSolver
{
    [Serializable]
    public class DVRPLocationsSubset
    {
        public int[] Locations;

        public static byte[] Serialize(int[] locations)
        {
            if (locations == null)
                return null;
            var subset = new DVRPLocationsSubset { Locations = locations };

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, subset);
                return ms.ToArray();
            }
        }
        public static DVRPLocationsSubset GetFromByteArray(byte[] array)
        {
            DVRPLocationsSubset result;
            try
            {
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(array))
                {
                    result = (DVRPLocationsSubset)formatter.Deserialize(ms);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }
    }
}