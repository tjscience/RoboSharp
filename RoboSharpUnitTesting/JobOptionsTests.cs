using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RoboSharp.UnitTests
{
    [TestClass]
    public class JobOptionsTests
    {
        /// <summary>
        /// This test ensures that the destination directory is not created when using the /QUIT function
        /// </summary>
        [TestMethod]
        public void TestPreventCopy() 
        {
            RoboCommand cmd = new RoboCommand(source: Test_Setup.Source_Standard, destination: Path.Combine(Test_Setup.TestDestination, Path.GetRandomFileName()));
            Console.WriteLine("Destination Path: " + cmd.CopyOptions.Destination);
            cmd.CopyOptions.Depth = 1;
            cmd.CopyOptions.FileFilter = new string[] { "*.ABCDEF" };
            cmd.JobOptions.PreventCopyOperation = true; // QUIT
            try
            {
                Authentication.AuthenticateDestination(cmd);
                Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nDestination Directory was created during authentication!");

                cmd.Start().Wait();
                Assert.IsFalse(Directory.Exists(cmd.CopyOptions.Destination), "\nDestination Directory was created when running the command!");
                cmd.JobOptions.PreventCopyOperation = false;
                cmd.Start().Wait();
                Assert.IsTrue(Directory.Exists(cmd.CopyOptions.Destination), "\nDestination Directory was not created.");
            }
            finally
            {
                if (Directory.Exists(cmd.CopyOptions.Destination))
                    try { Directory.Delete(cmd.CopyOptions.Destination); } catch { }
            }
        }
    }
}
