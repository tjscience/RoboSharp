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

        string GetMoveSource() => Path.Combine(TestPrep.DestDirPath.Replace(Path.GetFileName(TestPrep.DestDirPath), ""), "MoveSource");
        
        /// <summary>
        /// Copy Test will use a standard ROBOCOPY command
        /// </summary>
        [TestMethod]
        [DataRow(data: new object[] { CopyActionFlags.Default, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Defaults")]
        [DataRow(data: new object[] { CopyActionFlags.CopySubdirectories, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Subdirectories")]
        [DataRow(data: new object[] { CopyActionFlags.CopySubdirectoriesIncludingEmpty, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "EmptySubdirectories")]
        [DataRow(data: new object[] { CopyActionFlags.Mirror, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Mirror")]
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
            AssertResults(results1, rc.LoggingOptions.ListOnly);

            rc.LoggingOptions.ListOnly = false;
            var results2 = TestPrep.RunTests(rc, crc, true).Result;
            AssertResults(results2, rc.LoggingOptions.ListOnly);
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

        /// <summary>
        /// This uses the actual logic provided by the RoboMover object
        /// </summary>
        [TestMethod]
        [DataRow(data: new object[] { CopyActionFlags.MoveFiles, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { CopyActionFlags.MoveFilesAndDirectories, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        public void MoveTest(object[] flags)
        {
            
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);
           
            rc.LoggingOptions.ListOnly = true;
            var results1 = TestPrep.RunTests(rc, rm, false).Result;
            AssertResults(results1, rc.LoggingOptions.ListOnly);

            rc.LoggingOptions.ListOnly = false;
            var results2 = TestPrep.RunTests(rc, rm, true).Result;
            AssertResults(results2, rc.LoggingOptions.ListOnly);
        }

        [TestMethod]
        [DataRow(data: new object[] { CopyActionFlags.MoveFiles, SelectionFlags.Default, LoggingFlags.ReportExtraFiles }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { CopyActionFlags.MoveFilesAndDirectories, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        public void FileInclusionTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);

            rc.LoggingOptions.ListOnly = true;
            rc.CopyOptions.FileFilter = new string[] { "*.txt" };
            var results1 = TestPrep.RunTests(rc, rm, false).Result;
            AssertResults(results1, rc.LoggingOptions.ListOnly);

            rm.LoggingOptions.ListOnly = false;
            var results2 = TestPrep.RunTests(rc, rm, true).Result;
            AssertResults(results2, rc.LoggingOptions.ListOnly);
        }

        [TestMethod]
        [DataRow(data: new object[] { CopyActionFlags.MoveFiles, SelectionFlags.Default, LoggingFlags.ReportExtraFiles }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { CopyActionFlags.MoveFilesAndDirectories, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        public void FileExclusionTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);
            rc.LoggingOptions.ListOnly = true;
            rc.SelectionOptions.ExcludedFiles.Add("*.txt");
            var results1 = TestPrep.RunTests(rc, rm, false).Result;
            AssertResults(results1, rc.LoggingOptions.ListOnly);

            rc.LoggingOptions.ListOnly = false;
            var results2 = TestPrep.RunTests(rc, rm, true).Result;
            AssertResults(results2, rc.LoggingOptions.ListOnly);
        }


        [TestMethod]
        [DataRow(data: new object[] { CopyActionFlags.MoveFiles, SelectionFlags.Default, LoggingFlags.ReportExtraFiles }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { CopyActionFlags.MoveFilesAndDirectories, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        public void ExtraFileTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);
            rc.LoggingOptions.ListOnly = true;
            
            var results1 = TestPrep.RunTests(rc, rm, false, CreateFile).Result;
            AssertResults(results1, rc.LoggingOptions.ListOnly);

            rc.LoggingOptions.ListOnly = false;
            var results2 = TestPrep.RunTests(rc, rm, true, CreateFile).Result;
            AssertResults(results2, rc.LoggingOptions.ListOnly);
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
        [DataRow(data: new object[] { CopyActionFlags.MoveFiles, SelectionFlags.Default, LoggingFlags.ReportExtraFiles }, DisplayName = "Move Files")]
        [DataRow(data: new object[] { CopyActionFlags.MoveFilesAndDirectories, SelectionFlags.Default, DefaultLoggingAction }, DisplayName = "Move Files and Directories")]
        public void SameFileTest(object[] flags) //CopyActionFlags copyAction, SelectionFlags selectionFlags, LoggingFlags loggingAction
        {
            PrepMoveTest((CopyActionFlags)flags[0], (SelectionFlags)flags[0], (LoggingFlags)flags[2], false, out var rc, out var rm);
            rc.LoggingOptions.ListOnly = true;

            var results1 = TestPrep.RunTests(rc, rm, false, CreateFile).Result;
            AssertResults(results1, rc.LoggingOptions.ListOnly);

            rc.LoggingOptions.ListOnly = false;
            var results2 = TestPrep.RunTests(rc, rm, true, CreateFile).Result;
            AssertResults(results2, rc.LoggingOptions.ListOnly);
            TestPrep.CleanDestination();

            void CreateFile()
            {
                Directory.CreateDirectory(TestPrep.DestDirPath);
                string fn = "1024_Bytes.txt";
                string dest = Path.Combine(TestPrep.DestDirPath, fn);
                if (!File.Exists(dest))
                    File.Copy(Path.Combine(TestPrep.SourceDirPath, fn), dest);
            }
        }


        private void AssertResults(RoboSharpTestResults[] results, bool ListOnly)
        {
            var RCResults = results[0].Results;
            var CRCResults = results[1].Results;
            Console.Write("---------------------------------------------------");
            Console.WriteLine($"Is List Only: {ListOnly}");
            Console.WriteLine($"RoboCopy Completion Time: {results[0].Results.TimeSpan.TotalMilliseconds} ms");
            Console.WriteLine($"CachedRoboCopy Completion Time: {results[1].Results.TimeSpan.TotalMilliseconds} ms");
            IStatistic RCStat = null, CRCStat = null;
            string evalSection = "";

            try
            {
                //Files
                //Console.Write("Evaluating File Stats...");
                AssertStat(results[0].Results.FilesStatistic, results[1].Results.FilesStatistic, "Files");
                //Console.WriteLine("OK");

                //Bytes
                //Console.Write("Evaluating Byte Stats...");
                AssertStat(results[0].Results.BytesStatistic, results[1].Results.BytesStatistic, "Bytes");
                //Console.WriteLine("OK");

                //Directories
                //Console.Write("Evaluating Directory Stats...");
                AssertStat(results[0].Results.DirectoriesStatistic, results[1].Results.DirectoriesStatistic, "Directory");
                //Console.WriteLine("OK");

                Console.WriteLine("Test Passed.");
                //Console.WriteLine("RoboCopy Results:");
                //Console.WriteLine($"{results[0].Results.DirectoriesStatistic}");
                //Console.WriteLine($"{results[0].Results.BytesStatistic}");
                //Console.WriteLine($"{results[0].Results.FilesStatistic}");
                //Console.WriteLine("-----------------------------");
                //Console.WriteLine("CachedRoboCopy Results:");
                //Console.WriteLine($"{results[1].Results.DirectoriesStatistic}");
                //Console.WriteLine($"{results[1].Results.BytesStatistic}");
                //Console.WriteLine($"{results[1].Results.FilesStatistic}");
                //Console.WriteLine("-----------------------------");

                void AssertStat(IStatistic rcStat, IStatistic crcSTat, string eval)
                {
                    RCStat = rcStat;
                    CRCStat = crcSTat;
                    evalSection = eval;
                    Assert.AreEqual(RCStat.Total, CRCStat.Total, "Stat Category: TOTAL");
                    Assert.AreEqual(RCStat.Copied, CRCStat.Copied, "Stat Category: COPIED");
                    Assert.AreEqual(RCStat.Skipped, CRCStat.Skipped, "Stat Category: SKIPPED");
                    Assert.AreEqual(RCStat.Extras, CRCStat.Extras, "Stat Category: EXTRAS");
                }

                Console.WriteLine("");
                Console.WriteLine("-----------------------------");
                Console.WriteLine("RoboCopy Results:");
                Console.Write("Directory : "); Console.WriteLine(RCResults.DirectoriesStatistic);
                Console.Write("    Files : "); Console.WriteLine(RCResults.FilesStatistic);
                Console.Write("    Bytes : "); Console.WriteLine(RCResults.BytesStatistic);
                Console.WriteLine(RCResults.SpeedStatistic);
                Console.WriteLine("-----------------------------");
                Console.WriteLine("");
                Console.WriteLine("CachedRoboCopy Results:");
                Console.Write("Directory : "); Console.WriteLine(CRCResults.DirectoriesStatistic);
                Console.Write("    Files : "); Console.WriteLine(CRCResults.FilesStatistic);
                Console.Write("    Bytes : "); Console.WriteLine(CRCResults.BytesStatistic);
                Console.WriteLine(CRCResults.SpeedStatistic);
                Console.WriteLine("-----------------------------");
                Console.WriteLine("");
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine("");
                Console.WriteLine("-----------------------------");
                Console.WriteLine("RoboCopy Results:");
                foreach (string s in results[0].Results.LogLines)
                    Console.WriteLine(s);

                Console.WriteLine("-----------------------------");
                Console.WriteLine("");
                Console.WriteLine("CachedRoboCopy Results:");
                foreach (string s in results[1].Results.LogLines)
                    Console.WriteLine(s);

                Console.WriteLine("-----------------------------");
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine("-----------------------------");
                throw new AssertFailedException(e.Message +
                    $"\nIs List Only: {ListOnly}" +
                    $"\n{evalSection} Stats: \n" +
                    $"RoboCopy Results: {RCStat}\n" +
                    $"CachedRC Results: {CRCStat}" +
                    (e.GetType() == typeof(AssertFailedException) ? "" : $" \nStackTrace: \n{e.StackTrace}"));
            }
        }

    }
}