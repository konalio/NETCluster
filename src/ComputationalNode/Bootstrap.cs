using System;
using ClusterUtils;

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
