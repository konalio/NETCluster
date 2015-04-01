using ClusterUtils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ComputationalNode
{
    class Bootstrap
    {
        static void Main(string[] args)
        {
            var node = new ComputationalNode(ComponentConfig.GetComponentConfig(args));
            
            node.Start();

            Console.ReadLine();

        }
    }
}
