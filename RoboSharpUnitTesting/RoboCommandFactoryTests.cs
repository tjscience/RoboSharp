using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Tests
{
    [TestClass()]
    public class RoboCommandFactoryTests
    {

        [TestMethod()]
        public void GetRoboCommandTest()
        {
            Assert.IsNotNull(RoboCommand.Factory.GetRoboCommand());
        }

        [TestMethod()]
        public void FromSourceAndDestinationTest()
        {
            string source = @"C:\TestSource";
            string dest = @"C:\TestDest";
            var cmd = RoboCommand.Factory.FromSourceAndDestination(source, dest);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(source, cmd.CopyOptions.Source);
            Assert.AreEqual(dest, cmd.CopyOptions.Destination);
        }

        [TestMethod()]
        public void FromSourceAndDestinationTest1()
        {
            string source = @"C:\TestSource";
            string dest = @"C:\TestDest";
            var cmd = RoboCommand.Factory.FromSourceAndDestination(source, dest, CopyOptions.CopyActionFlags.Mirror, SelectionOptions.SelectionFlags.ExcludeNewer);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(source, cmd.CopyOptions.Source);
            Assert.AreEqual(dest, cmd.CopyOptions.Destination);
            Assert.IsTrue(cmd.CopyOptions.Mirror);
            Assert.IsTrue(cmd.SelectionOptions.ExcludeNewer);
        }
    }
}