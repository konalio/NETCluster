using ClusterUtils;

namespace TaskManager
{
    class Bootstrap
    {
        static void Main(string[] args)
        {
            var manager = new TaskManager(ComponentConfig.GetComponentConfig(args));

            manager.Start();
        }
    }
}
