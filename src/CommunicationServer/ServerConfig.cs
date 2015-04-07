using System;
using System.Configuration;

namespace CommunicationServer
{
    /// <summary>
    /// Container for server configuration info - listening port, backup/no backup, timeout for components.
    /// </summary>
    public class ServerConfig
    {
        public string ServerPort { get; set; }
        public bool IsBackup { get; set; }
        public int ComponentTimeout { get; set; }

        /// <summary>
        /// Retreives configuration from App.config file.
        /// </summary>
        /// <returns>Config from App.config</returns>
        public static ServerConfig LoadFromAppConfig()
        {
            var serverPort = ConfigurationManager.AppSettings["ServerPort"];
            var isBackup = bool.Parse(ConfigurationManager.AppSettings["IsBackup"]);
            var componentTimeout = int.Parse(ConfigurationManager.AppSettings["ComponentTimeout"]);

            return new ServerConfig
            {
                ServerPort = serverPort,
                IsBackup = isBackup,
                ComponentTimeout = componentTimeout
            };
        }

        /// <summary>
        /// Retreives configuration from command line arguments.
        /// </summary>
        /// <param name="arguments">Command line args.</param>
        /// <returns>Config from command line args.</returns>
        public static ServerConfig LoadFromArguments(string[] arguments)
        {
            var config = new ServerConfig();

            for (var i = 0; i < arguments.Length; i++)
            {
                switch (arguments[i])
                {
                    case "-port":
                    {
                        try
                        {
                            config.ServerPort = arguments[++i];
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
                    case "-backup":
                    {
                        config.IsBackup = true;
                        break;
                    }
                    case "-t":
                    {
                        try
                        {
                            var value = arguments[++i];
                            config.ComponentTimeout = int.Parse(value);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
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
        /// <returns>Actual server config.</returns>
        public static ServerConfig GetServerConfig(string[] arguments)
        {
            var configFromArguments = LoadFromArguments(arguments);
            var configFromAppConf = LoadFromAppConfig();

            if (configFromArguments.ComponentTimeout != 0)
                configFromAppConf.ComponentTimeout = configFromArguments.ComponentTimeout;
            if (configFromArguments.IsBackup)
                configFromAppConf.IsBackup = configFromArguments.IsBackup;
            if (configFromArguments.ServerPort != null)
                configFromAppConf.ServerPort = configFromArguments.ServerPort;

            return configFromAppConf;
        }
    }
}
