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
        private static string GetJobFilePath() => Path.Combine(Path.GetDirectoryName(Test_Setup.Source_Standard), "JobFileTesting", "TestJobFile.rcj");

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
		
		
		[TestMethod]
        public async Task Test_SaveJobFile() 
		{
            RoboCommand cmd = new RoboCommand(source: Test_Setup.Source_Standard, destination: Path.Combine(Test_Setup.TestDestination, Path.GetRandomFileName()));
            await cmd.SaveAsJobFile(GetJobFilePath());
            Assert.IsTrue(File.Exists(GetJobFilePath()), "\n Job File was not saved.");
		}

        [DataRow(null, DisplayName = "Bad File Extension")]
        [DataRow("ZZ:\\SomeDestination\\BadFile.rcj", DisplayName = "Bad Input String")]
        [TestMethod]
        public async Task Test_SaveJobFileError(string savePath)
        {
            RoboCommand cmd = new RoboCommand(source: Test_Setup.Source_Standard, destination: Path.Combine(Test_Setup.TestDestination, Path.GetRandomFileName()));
            bool errorRaised = false;
            cmd.OnCommandError += (o, e) =>
            {
                Console.WriteLine(e.Error);
                Console.WriteLine(e.Exception);
                errorRaised = true;
            };
            if (savePath is null) savePath = Path.ChangeExtension(GetJobFilePath(), ".XYZ");
            await cmd.SaveAsJobFile(savePath);
            Assert.IsTrue(errorRaised);
        }

        [TestMethod]
        public async Task Test_LoadJobFile()
        {
            await (Test_SaveJobFile());
            RoboCommand cmd = RoboCommand.LoadFromJobFile(GetJobFilePath());
            Assert.IsNotNull(cmd);
        }

        
    }
}
