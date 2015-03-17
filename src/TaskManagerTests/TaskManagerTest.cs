using System;
using TaskManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UCCTaskSolver;

namespace TaskManagerTests
{
    [TestClass]
    public class TaskManagerTest
    {
        [TestMethod]
        public void TaskManagerInitialize()
        {
            TaskManager.TaskManager tm = new TaskManager.TaskManager(40);
            string server = tm.PrimaryServerAddress;
            string port = tm.ServerPort;

            //byte[] data = new byte[10];
            //TaskSolver ts;
            //ts = new TaskSolver(data);
            Assert.AreEqual(port, "8080");
        }
    }
}
