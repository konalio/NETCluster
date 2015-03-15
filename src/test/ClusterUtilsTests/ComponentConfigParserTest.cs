using System;
using System.Configuration;
using ClusterUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClusterUtilsTests
{
    [TestClass]
    public class ComponentConfigParserTest
    {
        [TestMethod]
        public void CheckConfigAvailability()
        {
            var value = ConfigurationManager.AppSettings["ServerAddress"];
            Assert.IsFalse(String.IsNullOrEmpty(value), "No app config found.");
        }

        [TestMethod]
        public void OverrideServerAddressInArgs()
        {
            var args = new[] {"-address", "localhost"};
            var config = ComponentConfig.GetComponentConfig(args);

            Assert.AreEqual(config.ServerAddress, "localhost");
            Assert.AreEqual(config.ServerPort, "8080");
        }

        [TestMethod]
        public void OverrideServerPortInArgs()
        {
            var args = new[] {"-port", "7777"};
            var config = ComponentConfig.GetComponentConfig(args);

            Assert.AreEqual(config.ServerAddress, "127.0.0.1");
            Assert.AreEqual(config.ServerPort, "7777");
        }
    }
}
