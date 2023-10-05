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
    public class RoboMoverEventTests : RoboSharp.UnitTests.RoboCommandEventTests
    {
        protected override IRoboCommand GenerateCommand(bool UseLargerFileSet, bool ListOnlyMode)
        {
            return TestPrep.GetIRoboCommand<RoboMover>(base.GenerateCommand(UseLargerFileSet, ListOnlyMode));
        }
    }

    [TestClass]
    public class RoboMoverTests
    {
        const LoggingFlags DefaultLoggingAction = LoggingFlags.RoboSharpDefault | LoggingFlags.NoJobHeader;

        static string GetMoveSource()
        {
            string original = TestPrep.SourceDirPath;
            return Path.Combine(original.Replace(Path.GetFileName(original), ""), "MoveSource");
        }

        [DataRow(true, @"C:\SomeDir")]
        [DataRow(false, @"D:\System Volume Information")]
        [TestMethod]
        public void IsAllowedDir(bool expected, string path)
        {
            Assert.AreEqual(expected, RoboMover.IsAllowedRootDirectory(new DirectoryInfo(path)));
        }

        /// <summary>
        /// Copy Test will use a standard ROBOCOPY command
        /// </summary>
        [TestMethod]
        [DataRow(data: new object[] { DefaultLoggingAction, SelectionFlags.Default, CopyActionFlags.Mirror }, DisplayName = "Mirror")]
        [DataRow(data: new object[] { DefaultLoggingAction, SelectionFlags.Default, CopyActionFlags.Default }, DisplayName = "Defaults")]
        [DataRow(data: new object[] { DefaultLoggingAction, SelectionFlags.Default, CopyActionFlags.CopySubdirectories }, DisplayName = "Subdirectories")]
        [DataRow(data: new object[] { DefaultLoggingAction, SelectionFlags.Default, CopyActionFlags.CopySubdirectoriesIncludingEmpty }, DisplayName = "EmptySubdirectories")]
        public void CopyTest(object[] flags)
        {
            Test_Setup.ClearOutTestDestination();
            CopyActionFlags copyAction = (CopyActionFlags)flags[2];
            SelectionFlags selectionFlags = (SelectionFlags)flags[1];
            LoggingFlags loggingAction = (LoggingFlags)flags[0];

            var rc = TestPrep.GetRoboCommand(false, copyAction, selectionFlags, loggingAction);
            var crc = TestPrep.GetIRoboCommand<RoboMover>(rc);

            rc.LoggingOptions.ListOnly = true;
            var results1 = TestPrep.RunTests(rc, crc, false).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], rc.LoggingOptions.ListOnly);

            rc.LoggingOptions.ListOnly = false;
            var results2 = TestPrep.RunTests(rc, crc, true).Result;
            TestPrep.CompareTestResults(results2[0], results2[1], rc.LoggingOptions.ListOnly);
        }

        private static void GetMoveCommands(CopyActionFlags copyFlags, SelectionFlags selectionFlags, LoggingFlags loggingFlags, out RoboCommand rc, out RoboMover rm)
        {
            rc = TestPrep.GetRoboCommand(false, copyFlags, selectionFlags, loggingFlags);
            rc.CopyOptions.Source = GetMoveSource();
            rm = TestPrep.GetIRoboCommand<RoboMover>(rc);
        }

        private static void PrepMoveFiles()
        {
            var rc = TestPrep.GetRoboCommand(false, CopyActionFlags.CopySubdirectoriesIncludingEmpty, SelectionFlags.Default, DefaultLoggingAction);
            rc.CopyOptions.Destination = GetMoveSource();
            Directory.CreateDirectory(rc.CopyOptions.Destination);
            rc.Start().Wait();
            var results = rc.GetResults();
            if (results.RoboCopyErrors.Length > 0)
                throw new Exception(
                    "Prep Failed  \n" +
                    string.Concat(args: results.RoboCopyErrors.Select(e => "\n RoboCommandError :\t" + e.GetType() + "\t" + e.ErrorDescription + "\t:\t" + e.ErrorPath).ToArray()) +
                    "\n"
                    );
        }

        private const CopyActionFlags Mov_ = CopyActionFlags.MoveFiles;
        private const CopyActionFlags Move = CopyActionFlags.MoveFilesAndDirectories;

        /// <summary>
        /// This uses the actual logic provided by the RoboMover object
        /// </summary>
        [TestMethod]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files and Directories")]
        [DataRow(data: new object[] { Mov_ | CopyActionFlags.CopySubdirectories, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Subdirectories | Move Files")]
        [DataRow(data: new object[] { Move | CopyActionFlags.CopySubdirectories, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Subdirectories | Move Files and Directories")]
        [DataRow(data: new object[] { Mov_ | CopyActionFlags.CopySubdirectoriesIncludingEmpty, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Subdirectories-Empty | Move Files")]
        [DataRow(data: new object[] { Move | CopyActionFlags.CopySubdirectoriesIncludingEmpty, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Subdirectories-Empty | Move Files and Directories")]
        public void MoveTest(object[] flags)
        {
            if (Test_Setup.IsRunningOnAppVeyor()) return;
            GetMoveCommands((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], out var rc, out var rm);
            bool listOnly = rc.LoggingOptions.ListOnly;
            var results1 = TestPrep.RunTests(rc, rm, !listOnly, PrepMoveFiles).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], listOnly);
        }

        [TestMethod]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files and Directories")]
        public void FileInclusionTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            if (Test_Setup.IsRunningOnAppVeyor()) return;
            GetMoveCommands((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], out var rc, out var rm);
            bool listOnly = rc.LoggingOptions.ListOnly;
            rc.CopyOptions.FileFilter = new string[] { "*.txt" };
            var results1 = TestPrep.RunTests(rc, rm, !listOnly, PrepMoveFiles).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], listOnly);
        }

        [TestMethod]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files and Directories")]
        public void FileExclusionTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            if (Test_Setup.IsRunningOnAppVeyor()) return;
            GetMoveCommands((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], out var rc, out var rm);
            rc.SelectionOptions.ExcludedFiles.Add("*.txt");
            bool listOnly = rc.LoggingOptions.ListOnly;
            var results1 = TestPrep.RunTests(rc, rm, !listOnly, PrepMoveFiles).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], listOnly);
        }


        [TestMethod]
        [DataRow(data: new object[] { Move | CopyActionFlags.CopySubdirectories, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Include Subdirectories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files and Directories")]
        public void ExtraFileTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            if (Test_Setup.IsRunningOnAppVeyor()) return;
            GetMoveCommands((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], out var rc, out var rm);
            bool listOnly = rc.LoggingOptions.ListOnly;
            var results1 = TestPrep.RunTests(rc, rm, !listOnly, CreateFile).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], listOnly);

            void CreateFile()
            {
                PrepMoveFiles();
                string path = Path.Combine(TestPrep.DestDirPath, "ExtraFileTest.txt");
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(TestPrep.DestDirPath);
                    File.WriteAllText(path, "This is an extra file");
                }
            }
        }

        [TestMethod]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files and Directories")]
        public void SameFileTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            if (Test_Setup.IsRunningOnAppVeyor()) return;
            GetMoveCommands((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], out var rc, out var rm);
            bool listOnly = rc.LoggingOptions.ListOnly;
            var results1 = TestPrep.RunTests(rc, rm, !listOnly, CreateFile).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], listOnly);

            void CreateFile()
            {
                PrepMoveFiles();
                Directory.CreateDirectory(TestPrep.DestDirPath);
                string fn = "1024_Bytes.txt";
                string dest = Path.Combine(TestPrep.DestDirPath, fn);
                if (!File.Exists(dest))
                    File.Copy(Path.Combine(TestPrep.SourceDirPath, fn), dest);
            }
        }
        
        // purge all
        [DataRow(0, true, Mov_)]
        [DataRow(0, false, Move)]
        [DataRow(0, true, Mov_ | CopyActionFlags.CopySubdirectories)]
        [DataRow(0, true, Move | CopyActionFlags.CopySubdirectories)]
        [DataRow(0, false, Mov_ | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [DataRow(0, false, Move | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        // purge depth 1 
        [DataRow(1, true, Mov_)]
        [DataRow(1, false, Move)]
        [DataRow(1, true, Mov_ | CopyActionFlags.CopySubdirectories)]
        [DataRow(1, true, Move | CopyActionFlags.CopySubdirectories)]
        [DataRow(1, false, Mov_ | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [DataRow(1, false, Move | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        // purge depth 2
        [DataRow(2, true, Mov_)]
        [DataRow(2, false, Move)]
        [DataRow(2, true, Mov_ | CopyActionFlags.CopySubdirectories)]
        [DataRow(2, true, Move | CopyActionFlags.CopySubdirectories)]
        [DataRow(2, false, Mov_ | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [DataRow(2, false, Move | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [TestMethod]
        public void Purge_Depth(int depth, bool listOnly, CopyActionFlags flags)
        {
            GetMoveCommands(flags, SelectionFlags.Default, DefaultLoggingAction, out var cmd, out var mover);
            cmd.LoggingOptions.ListOnly = listOnly;
            cmd.CopyOptions.Depth = depth;
            RunPurge(cmd, mover);
        }

        [DataRow(true, Mov_)]
        [DataRow(false, Move)]
        [DataRow(true, Mov_ | CopyActionFlags.CopySubdirectories)]
        [DataRow(true, Move | CopyActionFlags.CopySubdirectories)]
        [DataRow(false, Mov_ | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [DataRow(false, Move | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [TestMethod]
        public void Purge_ExludeFiles(bool listOnly, CopyActionFlags flags)
        {
            GetMoveCommands(flags, SelectionFlags.Default, DefaultLoggingAction, out var cmd, out var mover);
            cmd.LoggingOptions.ListOnly = listOnly;
            cmd.SelectionOptions.ExcludedFiles.Add("*0*_Bytes.txt");
            RunPurge(cmd, mover);
        }

        [DataRow(true, Mov_)]
        [DataRow(false, Move)]
        [DataRow(true, Mov_ | CopyActionFlags.CopySubdirectories)]
        [DataRow(true, Move | CopyActionFlags.CopySubdirectories)]
        [DataRow(false, Mov_ | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [DataRow(false, Move | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [TestMethod]
        public void Purge_IncludedFiles(bool listOnly, CopyActionFlags flags)
        {
            GetMoveCommands(flags, SelectionFlags.Default, DefaultLoggingAction, out var cmd, out var mover);
            cmd.LoggingOptions.ListOnly = listOnly;
            cmd.CopyOptions.FileFilter = new string[] { "*0*_Bytes.txt" };
            RunPurge(cmd, mover);
        }

        [DataRow(true, Mov_)]
        [DataRow(false, Move)]
        [DataRow(false, Mov_ | CopyActionFlags.CopySubdirectories)]
        [DataRow(false, Move | CopyActionFlags.CopySubdirectories)]
        [DataRow(false, Mov_ | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [DataRow(false, Move | CopyActionFlags.CopySubdirectoriesIncludingEmpty)]
        [TestMethod]
        public void Purge_ExludeFolders(bool listOnly, CopyActionFlags flags)
        {
            GetMoveCommands(flags, SelectionFlags.Default, DefaultLoggingAction, out var cmd, out var mover);
            cmd.LoggingOptions.ListOnly = listOnly;
            cmd.SelectionOptions.ExcludedDirectories.Add("EmptyFolder1"); // Top level empty
            cmd.SelectionOptions.ExcludedDirectories.Add("EmptyFolder4"); // Bottom level empty
            cmd.SelectionOptions.ExcludedDirectories.Add("SubFolder_2a"); // folder with contents
            RunPurge(cmd, mover);
        }

        private void RunPurge(RoboCommand cmd, RoboMover mover)
        {
            //if (Test_Setup.IsRunningOnAppVeyor()) return;
            var results = TestPrep.RunTests(cmd, mover, !cmd.LoggingOptions.ListOnly, CreateFilesToPurge).Result;
            TestPrep.CompareTestResults(results[0], results[1], cmd.LoggingOptions.ListOnly);

            void CreateFilesToPurge()
            {
                PrepMoveFiles();
                RoboCommand prep = new RoboCommand();
                prep.CopyOptions.Source = Path.Combine(Test_Setup.Source_Standard, "SubFolder_1");
                prep.CopyOptions.Destination = Path.Combine(Test_Setup.TestDestination, "SubFolder_3");
                prep.CopyOptions.ApplyActionFlags(CopyActionFlags.CopySubdirectoriesIncludingEmpty);
                Directory.CreateDirectory(Path.Combine(prep.CopyOptions.Destination, "EmptyFolder1", "EmptyFolder2"));
                prep.Start().Wait();
                prep.CopyOptions.Source = Path.Combine(Test_Setup.Source_Standard, "SubFolder_2");
                prep.CopyOptions.Destination = Path.Combine(prep.CopyOptions.Destination, "SubFolder_2a");
                prep.Start().Wait();
                Directory.CreateDirectory(Path.Combine(prep.CopyOptions.Destination, "EmptyFolder3", "EmptyFolder4"));
            }
        }
    }
}