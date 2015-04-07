using System;
using System.Configuration;

namespace ClusterUtils
{
    /// <summary>
    /// Container for components' configuration info - server's address and port.
    /// </summary>
    public class ComponentConfig
    {
        public string ServerAddress { get; set; }
        public string ServerPort { get; set; }

        /// <summary>
        /// Retreives configuration from App.config file.
        /// </summary>
        /// <returns>Config from App.config</returns>
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
    
        /// <summary>
        /// Retreives configuration from command line arguments.
        /// </summary>
        /// <param name="arguments">Command line args.</param>
        /// <returns>Config from command line args.</returns>
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

        /// <summary>
        /// Retreives config from both App.config and command line.
        /// Command line arguments supress App.config settings.
        /// </summary>
        /// <param name="arguments">Command line arguments.</param>
        /// <returns>Actual component config.</returns>
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