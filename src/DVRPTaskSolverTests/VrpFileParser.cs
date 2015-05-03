using System.IO;
using System.Text;
using DVRPTaskSolver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DVRPTaskSolverTests
{
    [TestClass]
    public class VrpFileParser
    {
        [TestMethod]
        [DeploymentItem("mainexample.vrp")]
        public void ExampleFromMail()
        {
            var file = File.ReadAllText("mainexample.vrp");

            var bytes = Encoding.UTF8.GetBytes(file);

            var dvrpData = DVRPData.GetFromBytes(bytes);

            Assert.AreEqual(dvrpData.VehicleCount, 8);
            Assert.AreEqual(dvrpData.DepotsCount, 1);
            Assert.AreEqual(dvrpData.VehicleCapacity, 100);
            Assert.AreEqual(dvrpData.RequestsCount, 8);

            Assert.AreEqual(dvrpData.Requests[3].Quantity, -20);
            Assert.AreEqual(dvrpData.Requests[7].Quantity, -29);

            Assert.AreEqual(dvrpData.Depots[0].Location.X, 0);
            Assert.AreEqual(dvrpData.Depots[0].Location.Y, 0);

            Assert.AreEqual(dvrpData.Requests[1].Location.X, 34);
            Assert.AreEqual(dvrpData.Requests[1].Location.Y, -45);
            Assert.AreEqual(dvrpData.Requests[7].Location.X, -93);
            Assert.AreEqual(dvrpData.Requests[7].Location.Y, -3);

            Assert.AreEqual(dvrpData.Requests[3].UnloadDuration, 20);

            Assert.AreEqual(dvrpData.Requests[4].AvailableTime, 479);
            Assert.AreEqual(dvrpData.Requests[6].AvailableTime, 376);

            Assert.AreEqual(dvrpData.Depots[0].TimeWindow.Start, 0);
            Assert.AreEqual(dvrpData.Depots[0].TimeWindow.End, 560);
        }
    }
}
