using System.Collections.Generic;
using System.Linq;

namespace DVRPTaskSolver
{
    public class ArrayComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            var count = x.Where((t1, i) => x.Any(t => t == y[i])).Count();
            return count == x.Length;
        }

        public int GetHashCode(int[] obj)
        {
            var result = 17;
            foreach (var t in obj)
            {
                unchecked
                {
                    result = result + t;
                }
            }
            return result;
        }
    }
}
