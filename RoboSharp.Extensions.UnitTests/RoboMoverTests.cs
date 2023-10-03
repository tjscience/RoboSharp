using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Interfaces;
using RoboSharp.UnitTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RoboSharp.Extensions.UnitTests
{
    [TestClass]
    public class RoboMoverEventTests : RoboSharp.UnitTests.RoboCommandEventTests
    {
        protected override IRoboCommand GenerateCommand(bool UseLargerFileSet, bool ListOnlyMode)
        {
            return UnitTests.TestPrep.GetRoboMover(base.GenerateCommand(UseLargerFileSet, ListOnlyMode));
        }
    }

    [TestClass]
    public class RoboMoverTests
    {
        const LoggingFlags DefaultLoggingAction = LoggingFlags.NoJobHeader | LoggingFlags.RoboSharpDefault;

        static string GetMoveSource() => Path.Combine(TestPrep.DestDirPath.Replace(Path.GetFileName(TestPrep.DestDirPath), ""), "MoveSource");

        /// <summary>
        /// Copy Test will use a standard ROBOCOPY command
        /// </summary>
        [TestMethod]
        [DataRow(data: new object[] { DefaultLoggingAction, SelectionFlags.Default, CopyActionFlags.Mirror }, DisplayName = "Mirror")]
        [DataRow(data: new object[] { DefaultLoggingAction, SelectionFlags.Default, CopyActionFlags.Default  }, DisplayName = "Defaults")]
        [DataRow(data: new object[] { DefaultLoggingAction, SelectionFlags.Default, CopyActionFlags.CopySubdirectories}, DisplayName = "Subdirectories")]
        [DataRow(data: new object[] { DefaultLoggingAction, SelectionFlags.Default, CopyActionFlags.CopySubdirectoriesIncludingEmpty }, DisplayName = "EmptySubdirectories")]
        public void CopyTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            TestPrep.CleanDestination();
            CopyActionFlags copyAction = (CopyActionFlags)flags[0];
            SelectionFlags selectionFlags = (SelectionFlags)flags[1];
            LoggingFlags loggingAction = (LoggingFlags)flags[2];
            
            var rc = TestPrep.GetRoboCommand(false, copyAction, selectionFlags, loggingAction);
            var crc = TestPrep.GetRoboMover(rc);
            
            rc.LoggingOptions.ListOnly = true;
            var results1 = TestPrep.RunTests(rc, crc, false).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], rc.LoggingOptions.ListOnly);

            rc.LoggingOptions.ListOnly = false;
            var results2 = TestPrep.RunTests(rc, crc, true).Result;
            TestPrep.CompareTestResults(results2[0], results2[1], rc.LoggingOptions.ListOnly);
        }

        private void PrepMoveTest(CopyActionFlags copyFlags, SelectionFlags selectionFlags, LoggingFlags loggingFlags, bool UseLargeFileSet, out RoboCommand rc, out RoboMover rm)
        {
            TestPrep.CleanDestination();
            rc = TestPrep.GetRoboCommand(false, copyFlags, selectionFlags, loggingFlags);
            var init = new RoboCommand(source: rc.CopyOptions.Source, destination: GetMoveSource(), copyActionFlags: CopyActionFlags.CopySubdirectoriesIncludingEmpty);
            init.Start().Wait();
            rc.CopyOptions.Source = GetMoveSource();
            rm = TestPrep.GetRoboMover(rc);
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
        public void MoveTest(object[] flags)
        {
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);
            var results1 = TestPrep.RunTests(rc, rm, false).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], rc.LoggingOptions.ListOnly);
        }

        [TestMethod]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files and Directories")]
        public void FileInclusionTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);
            rc.CopyOptions.FileFilter = new string[] { "*.txt" };
            var results1 = TestPrep.RunTests(rc, rm, false).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], rc.LoggingOptions.ListOnly);
        }

        [TestMethod]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files and Directories")]
        public void FileExclusionTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);
            rc.SelectionOptions.ExcludedFiles.Add("*.txt");
            var task = TestPrep.RunTests(rc, rm, false);
            task.Wait();
            var results1 = task.Result;
            TestPrep.CompareTestResults(results1[0], results1[1], rc.LoggingOptions.ListOnly);
        }


        [TestMethod]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files and Directories")]
        public void ExtraFileTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);
            var results1 = TestPrep.RunTests(rc, rm, false, CreateFile).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], rc.LoggingOptions.ListOnly);
            TestPrep.CleanDestination();
            
            void CreateFile()
            {
                Directory.CreateDirectory(TestPrep.DestDirPath);
                string path = Path.Combine(TestPrep.DestDirPath, "ExtraFileTest.txt");
                if (!File.Exists(path))
                    File.WriteAllText(path, "This is an extra file");
            }
        }

        [TestMethod]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        [DataRow(data: new object[] { Mov_, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files")]
        [DataRow(data: new object[] { Move, SelectionFlags.Default, DefaultLoggingAction | LoggingFlags.ListOnly }, DisplayName = "ListOnly | Move Files and Directories")]
        public void SameFileTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);
            var results1 = TestPrep.RunTests(rc, rm, false, CreateFile).Result;
            TestPrep.CompareTestResults(results1[0], results1[1], rc.LoggingOptions.ListOnly);

            void CreateFile()
            {
                Directory.CreateDirectory(TestPrep.DestDirPath);
                string fn = "1024_Bytes.txt";
                string dest = Path.Combine(TestPrep.DestDirPath, fn);
                if (!File.Exists(dest))
                    File.Copy(Path.Combine(TestPrep.SourceDirPath, fn), dest);
            }
        }


        
    }
}