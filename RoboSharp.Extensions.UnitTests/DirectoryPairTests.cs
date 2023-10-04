using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Interfaces;
using RoboSharp.UnitTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RoboSharp.Extensions.UnitTests
{
    [TestClass]
    public class DirectoryPairTests
    {

        [DataRow(true, @"C:\")]
        [DataRow(false, @"C:\MyDocuments")]
        [DataRow(true, @"\\someServerShare\MyDrive$\")]
        [DataRow(false, @"\\someServerShare\MyDrive$\SomeFolder")]
        [TestMethod]
        public void Test_IsRootSource(bool expected, string path)
        {
            var dir = new DirectoryInfo(path);
            Assert.AreEqual(expected, new DirectoryPair(dir, dir).IsRootSource());
        }

        [DataRow(true, @"C:\")]
        [DataRow(false, @"C:\MyDocuments")]
        [DataRow(true, @"\\someServerShare\MyDrive$\")]
        [DataRow(false, @"\\someServerShare\MyDrive$\SomeFolder")]
        [TestMethod]
        public void Test_IsRootDestination(bool expected, string path)
        {
            var dir = new DirectoryInfo(path);
            Assert.AreEqual(expected, new DirectoryPair(dir, dir).IsRootDestination());
        }

        [DataRow(true, @"C:\")]
        [DataRow(false, @"C:\MyDocuments")]
        [DataRow(true, @"\\someServerShare\MyDrive$\")]
        [DataRow(false, @"\\someServerShare\MyDrive$\SomeFolder")]
        [TestMethod]
        public void Test_IsRootDIr(bool expected, string path)
        {
            Assert.AreEqual(expected, new DirectoryInfo(path).IsRootDir());
        }
    }
}
