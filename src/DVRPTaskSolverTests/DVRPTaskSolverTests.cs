﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using DVRPTaskSolver;
using System.Collections.Generic;

namespace DVRPTaskSolverTests
{
    [TestClass]
    public class DVRPTaskSolverTests
    {
        [TestMethod]
        [DeploymentItem("test_of_size_1.vrp")]
        public void DivideProblemOfSize1Test()
        {
            var file = File.ReadAllText("test_of_size_1.vrp");
            var problemBytes = Encoding.UTF8.GetBytes(file);

            var dvrpTaskSolver = new DVRPTaskSolver.DVRPTaskSolver(problemBytes);

            var partialProblemsBytes = dvrpTaskSolver.DivideProblem(0);

            var partialProblems = new int[partialProblemsBytes.Length][];
            for (var i = 0; i < partialProblemsBytes.Length; i++)
                partialProblems[i] = DVRPLocationsSubset.GetFromByteArray(partialProblemsBytes[i]).Locations;

            var expected = new int[2][];
            expected[0] = new int[0];
            expected[1] = new[] { 1 };

            Assert.AreEqual(expected.Length, partialProblemsBytes.Length);
            for (var i = 0; i < expected.Length; i++)
                CollectionAssert.AreEqual(expected[i], partialProblems[i]);
        }

        [TestMethod]
        [DeploymentItem("test_of_size_2.vrp")]
        public void DivideProblemOfSize2Test()
        {
            var file = File.ReadAllText("test_of_size_2.vrp");
            var problemBytes = Encoding.UTF8.GetBytes(file);

            var dvrpTaskSolver = new DVRPTaskSolver.DVRPTaskSolver(problemBytes);

            var partialProblemsBytes = dvrpTaskSolver.DivideProblem(0);

            var partialProblems = new int[partialProblemsBytes.Length][];
            for (var i = 0; i < partialProblemsBytes.Length; i++)
                partialProblems[i] = DVRPLocationsSubset.GetFromByteArray(partialProblemsBytes[i]).Locations;

            var expected = new int[4][];
            expected[0] = new int[0];
            expected[1] = new [] { 1 };
            expected[2] = new [] { 1, 2 };
            expected[3] = new [] { 2 };

            Assert.AreEqual(expected.Length, partialProblemsBytes.Length);
            for (var i = 0; i < expected.Length; i++)
                CollectionAssert.AreEqual(expected[i], partialProblems[i]);
        }

        [TestMethod]
        [DeploymentItem("test_of_size_3.vrp")]
        public void DivideProblemOfSize3Test()
        {
            var file = File.ReadAllText("test_of_size_3.vrp");
            var problemBytes = Encoding.UTF8.GetBytes(file);

            var dvrpTaskSolver = new DVRPTaskSolver.DVRPTaskSolver(problemBytes);

            var partialProblemsBytes = dvrpTaskSolver.DivideProblem(0);

            var partialProblems = new int[partialProblemsBytes.Length][];
            for (var i = 0; i < partialProblemsBytes.Length; i++)
                partialProblems[i] = DVRPLocationsSubset.GetFromByteArray(partialProblemsBytes[i]).Locations;

            var expected = new int[8][];
            expected[0] = new int[0];
            expected[1] = new[] { 1 };
            expected[2] = new[] { 1, 2 };
            expected[3] = new[] { 1, 2, 3 };
            expected[4] = new[] { 1, 3 };
            expected[5] = new[] { 2 };
            expected[6] = new[] { 2, 3 };
            expected[7] = new[] { 3 };

            Assert.AreEqual(expected.Length, partialProblemsBytes.Length);
            for (var i = 0; i < expected.Length; i++)
                CollectionAssert.AreEqual(expected[i], partialProblems[i]);
        }

        [TestMethod]
        [DeploymentItem("test_of_size_5.vrp")]
        public void DivideProblemOfSize5Test()
        {
            var file = File.ReadAllText("test_of_size_5.vrp");
            var problemBytes = Encoding.UTF8.GetBytes(file);

            var dvrpTaskSolver = new DVRPTaskSolver.DVRPTaskSolver(problemBytes);

            var partialProblemsBytes = dvrpTaskSolver.DivideProblem(0);

            Assert.AreEqual(32, partialProblemsBytes.Length);
        }

        [TestMethod]
        [DeploymentItem("test_of_size_15.vrp")]
        public void DivideProblemOfSize15Test()
        {
            var file = File.ReadAllText("test_of_size_15.vrp");
            var problemBytes = Encoding.UTF8.GetBytes(file);

            var dvrpTaskSolver = new DVRPTaskSolver.DVRPTaskSolver(problemBytes);

            var partialProblemsBytes = dvrpTaskSolver.DivideProblem(0);

            Assert.AreEqual(32768, partialProblemsBytes.Length);
        }

        [TestMethod]
        [DeploymentItem("mainexample.vrp")]
        public void TSPSolverTests()
        {
            var file = File.ReadAllText("mainexample.vrp");
            var problemBytes = Encoding.UTF8.GetBytes(file);

            var dvrpTaskSolver = new DVRPTaskSolver.DVRPTaskSolver(problemBytes);

            var requestsSubsets = new List<List<int>> {new List<int>(new[] {1, 4, 7, 8})};
            //requestsSubsets.Add(new List<int>(new int[] { 2, 3, 5, 6 }));
            var dvrpSolutions = new List<DVRPPartialSolution>();

            var returnSubsets = new byte[requestsSubsets.Count][];
            var results = new byte[requestsSubsets.Count][];
            for (var i = 0; i < requestsSubsets.Count; i++)
                returnSubsets[i] = DVRPLocationsSubset.Serialize(requestsSubsets[i].ToArray());
            var j = 0;

            foreach (var b in returnSubsets)
            {
                results[j] = dvrpTaskSolver.Solve(b, new TimeSpan() { });
                dvrpSolutions.Add(DVRPPartialSolution.GetFromByteArray(results[j]));
                j++;
            }

            foreach (var dp in dvrpSolutions)
            {
                Console.WriteLine(dp.OptimalTime);
            }
        }
    }
}
