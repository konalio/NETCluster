using System;
using System.Configuration;

namespace ClusterUtils
{
    public class ComponentConfig
    {
        public string ServerAddress { get; set; }
        public string ServerPort { get; set; }

        public static ComponentConfig GetConfigFromAppConfig()
        {
            var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
            var serverPort = ConfigurationManager.AppSettings["ServerPort"];

            return new ComponentConfig
            {
                ServerAddress = serverAddress,
                ServerPort = serverPort
            };
        }
    

        public static ComponentConfig GetConfigFromArgs(string[] arguments)
        {
            var config = new ComponentConfig();

            for (var i = 0; i < arguments.Length; i++)
            {
                switch (arguments[i])
                {
                    case "-address":
                    {
                        try
                        {
                            config.ServerAddress = arguments[++i];
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        break;
                    }
                    case "-port":
                    {
                        try
                        {
                            config.ServerPort = arguments[++i];
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        break;
                    }
                    default:
                    {
                        Console.WriteLine("Unexpected argument {0}, ignored.", arguments[i]);
                        break;
                    }
                }
            }

            return config;
        }

        public static ComponentConfig GetComponentConfig(string[] arguments)
        {
            var appConfig = GetConfigFromAppConfig();
            var argConfig = GetConfigFromArgs(arguments);

            if (argConfig.ServerAddress != null)
                appConfig.ServerAddress = argConfig.ServerAddress;

            if (argConfig.ServerPort != null)
                appConfig.ServerPort = argConfig.ServerPort;

            return appConfig;
        }
    }
}