using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp.Interfaces;
using System;
using System.IO;
using System.Linq;
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


        [DataRow(true)]
        [DataRow(false)]
        [TestMethod]
        public async Task Test_SaveJobFile(bool savePaths) 
		{
            if (File.Exists(GetJobFilePath())) File.Delete(GetJobFilePath());
            string dest = Path.Combine(Test_Setup.TestDestination, Path.GetRandomFileName());
            RoboCommand cmd = new RoboCommand(source: Test_Setup.Source_Standard, destination: dest);
            await cmd.SaveAsJobFile(GetJobFilePath(), savePaths, savePaths);
            Assert.IsTrue(File.Exists(GetJobFilePath()), "\n Job File was not saved.");

            var jb = JobFile.ParseJobFile(GetJobFilePath());
            Assert.AreEqual(savePaths, jb.CopyOptions.Source == Test_Setup.Source_Standard, "Source was " + (savePaths ? "not saved" : "saved when set to not save"));
            Assert.AreEqual(savePaths, jb.CopyOptions.Destination == dest, "Destination was " + (savePaths ? "not saved" : "saved when set to not save"));

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
            await (Test_SaveJobFile(true));
            RoboCommand cmd = RoboCommand.LoadFromJobFile(GetJobFilePath());
            Assert.IsNotNull(cmd);
        }

        [TestMethod]
        public async Task Test_LoadJobFile_RoboQueue()
        {
            await (Test_SaveJobFile(true));
            var Q = new RoboQueue();
            Q.AddCommand(RoboCommand.LoadFromJobFile(GetJobFilePath()));
            Q.AddCommand(JobFile.ParseJobFile(GetJobFilePath()));
            Assert.IsTrue(Q.Count() == 2);
            Assert.IsNotNull(Q.First());
            Assert.IsNotNull(Q.Last());
            Assert.IsTrue(Q.Last().CopyOptions.Source == Test_Setup.Source_Standard);
        }

    }
}
