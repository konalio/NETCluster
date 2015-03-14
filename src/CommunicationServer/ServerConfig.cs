using System;
using System.Configuration;

namespace CommunicationServer
{
    public class ServerConfig
    {
        public string ServerPort { get; set; }
        public bool IsBackup { get; set; }
        public int ComponentTimeout { get; set; }

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
                            throw;
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
                            throw;
                        }
                        break;
                    }
                }
            }
            return config;
        }

    }
}
