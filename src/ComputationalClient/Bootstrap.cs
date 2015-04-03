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
