using System;
using System.Configuration;
using CommunicationServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunicationServerTests
{
    [TestClass]
    public class ServerConfigUnitTest
    {
        [TestMethod]
        public void CheckConfigAvailability()
        {
            var value = ConfigurationManager.AppSettings["ServerPort"];
            Assert.IsFalse(String.IsNullOrEmpty(value), "No App.Config found.");
        }

        [TestMethod]
        public void OverrideServerPortInArguments()
        {
            var args = new[] {"-port", "7777"};

            var config = ServerConfig.GetServerConfig(args);

            Assert.AreEqual(config.ServerPort, "7777");
            Assert.AreEqual(config.IsBackup, false);
            Assert.AreEqual(config.ComponentTimeout, 20);
        }

        [TestMethod]
        public void OverrideBackupStatusInArguments()
        {
            var args = new[] { "-backup" };

            var config = ServerConfig.GetServerConfig(args);

            Assert.AreEqual(config.ServerPort, "8080");
            Assert.AreEqual(config.IsBackup, true);
            Assert.AreEqual(config.ComponentTimeout, 20);
        }

        [TestMethod]
        public void OverrideTimeoutInArguments()
        {
            var args = new[] { "-t", "30" };

            var config = ServerConfig.GetServerConfig(args);

            Assert.AreEqual(config.ServerPort, "8080");
            Assert.AreEqual(config.IsBackup, false);
            Assert.AreEqual(config.ComponentTimeout, 30);
        }
    }
}
