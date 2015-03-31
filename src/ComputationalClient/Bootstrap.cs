using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterUtils;

namespace ComputationalClient
{
    class Bootstrap
    {
        static void Main(string[] args)
        {
            var client = new ComputationalClient(ComponentConfig.GetComponentConfig(args));

            client.Start();
        }
    }
}
