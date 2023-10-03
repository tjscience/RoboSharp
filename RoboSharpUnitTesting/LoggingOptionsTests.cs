﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RoboSharp.UnitTests
{
    [TestClass]
    public class LoggingOptionsTests
    {
        /// <summary>
        /// This test ensures that the destination directory is not created when using the /QUIT function
        /// </summary>
        [TestMethod]
        public void TestListOnlyDestinationCreation() 
        {
            RoboCommand cmd = new RoboCommand(source: Test_Setup.Source_Standard, destination: Path.Combine(Test_Setup.TestDestination, Path.GetRandomFileName()));
            Console.WriteLine("Destination Path: " + cmd.CopyOptions.Destination);
            cmd.CopyOptions.Depth = 1;
            cmd.CopyOptions.FileFilter = new string[] { "*.ABCDEF" };

            cmd.LoggingOptions.ListOnly = true;
            Authentication.AuthenticateDestination(cmd);
            Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nDestination Directory was created during authentication!");

            cmd.LoggingOptions.ListOnly = false;
            cmd.Start_ListOnly().Wait();
            Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nStart_ListOnly() - Destination Directory was created!");

            cmd.LoggingOptions.ListOnly = false;
            cmd.StartAsync_ListOnly().Wait();
            Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nStartAsync_ListOnly() - Destination Directory was created!");

            cmd.LoggingOptions.ListOnly = true;
            cmd.Start().Wait();
            Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nList-Only Setting - Destination Directory was created!");

            cmd.LoggingOptions.ListOnly = false;
            cmd.Start().Wait();
            Assert.IsTrue(Directory.Exists(cmd.CopyOptions.Destination), "\nDestination Directory was not created.");
        }
    }
}
