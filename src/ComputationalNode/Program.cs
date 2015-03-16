using ClusterUtils;

namespace ComputationalNode
{
    class Program
    {
        static void Main(string[] args)
        {
            var node = new ComputationalNode(ComponentConfig.GetComponentConfig(args));

            node.Start();
        }
    }
}
