using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
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
            DVRPLocationsSubset subset = new DVRPLocationsSubset();
            subset.Locations = locations;

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, subset);
                return ms.ToArray();
            }
        }
        public static DVRPLocationsSubset GetFromByteArray(byte[] array)
        {
            var result = new DVRPLocationsSubset();
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