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
    public class CopyOptionsExtensionsTests
    {
        [DataRow(true, CopyActionFlags.CopySubdirectories)]
        [DataRow(true, CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [DataRow(true, CopyActionFlags.Mirror)]
        [DataRow(false, CopyActionFlags.Default)]
        [DataRow(false, CopyActionFlags.MoveFiles)]
        [DataRow(false, CopyActionFlags.MoveFilesAndDirectories)]
        [TestMethod]
        public void IsRecursive(bool expected, CopyActionFlags flags)
        {
            var opt = new CopyOptions();
            opt.ApplyActionFlags(flags);
            Assert.AreEqual(expected, Helpers.CopyOptionsExtensions.IsRecursive(opt), "CopyOptions Extension method Failed");
            Assert.AreEqual(expected, Helpers.CopyOptionsExtensions.IsRecursive(flags), "Flags extension method Failed");
            Assert.AreEqual(expected, Helpers.CopyOptionsExtensions.IsRecursive(opt.GetCopyActionFlags()), "CopyOptions.GetCopyActionFlags() method Failed");
        }
    }
}
