using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboSharp;
using RoboSharp.Interfaces;
using RoboSharp.UnitTests;
using TestSetup = RoboSharp.UnitTests.Test_Setup;

namespace RoboSharp.Extensions.UnitTests
{
    public static class TestPrep
    {

        public static string CopyFileExTestSourcePath => Path.GetDirectoryName(SourceDirPath);
        public static string SourceDirPath => RoboSharp.UnitTests.Test_Setup.Source_Standard;
        public static string DestDirPath => RoboSharp.UnitTests.Test_Setup.TestDestination;

        /// <inheritdoc cref="Test_Setup.ClearOutTestDestination"/>
        public static void CleanDestination()
        {
             TestSetup.ClearOutTestDestination();
        }

        /// <inheritdoc cref="Test_Setup.WriteLogLines(RoboSharp.Results.RoboCopyResults, bool)"/>
        public static void WriteLogLines(RoboSharp.Results.RoboCopyResults results, bool summaryOnly = false)
        {
            TestSetup.WriteLogLines(results, summaryOnly);
        }

        /// <inheritdoc cref="Test_Setup.GenerateCommand(bool, bool)"/>
        public static RoboCommand GetRoboCommand(bool useLargerFileSet, CopyActionFlags copyActionFlags, SelectionFlags selectionFlags, LoggingFlags loggingAction)
        {
            var cmd = TestSetup.GenerateCommand(useLargerFileSet, false);
            cmd.CopyOptions.ApplyActionFlags(copyActionFlags);
            cmd.SelectionOptions.ApplySelectionFlags(selectionFlags);
            cmd.LoggingOptions.ApplyLoggingFlags(loggingAction);
            cmd.CopyOptions.MultiThreadedCopiesCount = 0;
            return cmd;
        }

        /// <summary>
        /// Generate a new CachedRoboCommand
        /// </summary>
        /// <returns></returns>
        public static RoboMover GetRoboMover(IRoboCommand rc)
        {
            var cmd = new RoboMover(rc);
            cmd.CopyOptions = rc.CopyOptions;
            cmd.SelectionOptions = rc.SelectionOptions;
            cmd.LoggingOptions = rc.LoggingOptions;
            cmd.RetryOptions = rc.RetryOptions;
            return cmd;
        }


        public static async Task<RoboSharpTestResults[]> RunTests(RoboCommand roboCommand, IRoboCommand customCommand, bool CleanBetweenRuns, Action actionBetweenRuns = null)
        {
            var results = new List<RoboSharpTestResults>();
            BetweenRuns();
            results.Add(await TestSetup.RunTest(roboCommand));
            BetweenRuns();
            
            customCommand.OnError += CachedRoboCommand_OnError;
            customCommand.OnCommandError += CachedRoboCommand_OnCommandError;
            
            results.Add(await TestSetup.RunTest(customCommand));
            
            customCommand.OnError -= CachedRoboCommand_OnError;
            customCommand.OnCommandError -= CachedRoboCommand_OnCommandError;

            if (CleanBetweenRuns) CleanDestination();
            return results.ToArray();

            void BetweenRuns()
            {
                if (CleanBetweenRuns) CleanDestination();
                if (actionBetweenRuns != null) actionBetweenRuns();
            }


        }
        private static void CachedRoboCommand_OnCommandError(IRoboCommand sender, CommandErrorEventArgs e) => Console.WriteLine(e.Exception);
        private static void CachedRoboCommand_OnError(IRoboCommand sender, RoboSharp.ErrorEventArgs e) => Console.WriteLine(e.Error);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="results"></param>
        /// <param name="ListOnly"></param>
        public static void CompareTestResults(RoboSharpTestResults roboCommandResults, RoboSharpTestResults iCommandResults, bool ListOnly)
        {
            var RCResults = roboCommandResults.Results;
            var customResults = iCommandResults.Results;
            Console.Write("---------------------------------------------------");
            Console.WriteLine($"Is List Only: {ListOnly}");
            Console.WriteLine(string.Format("RoboCopy Completion Time     : {0} ms", RCResults.TimeSpan.TotalMilliseconds));
            Console.WriteLine(string.Format("IRoboCommand Completion Time : {0} ms", customResults.TimeSpan.TotalMilliseconds));
            IStatistic RCStat = null, CRCStat = null;
            string evalSection = "";

            try
            {
                //Files
                //Console.Write("Evaluating File Stats...");
                AssertStat(RCResults.FilesStatistic, customResults.FilesStatistic, "Files");
                //Console.WriteLine("OK");

                //Bytes
                //Console.Write("Evaluating Byte Stats...");
                AssertStat(RCResults.BytesStatistic, customResults.BytesStatistic, "Bytes");
                //Console.WriteLine("OK");

                //Directories
                //Console.Write("Evaluating Directory Stats...");
                AssertStat(RCResults.DirectoriesStatistic, customResults.DirectoriesStatistic, "Directory");
                //Console.WriteLine("OK");

                Console.WriteLine("Test Passed.");

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
                Console.Write("Directory : "); Console.WriteLine(customResults.DirectoriesStatistic);
                Console.Write("    Files : "); Console.WriteLine(customResults.FilesStatistic);
                Console.Write("    Bytes : "); Console.WriteLine(customResults.BytesStatistic);
                Console.WriteLine(customResults.SpeedStatistic);
                Console.WriteLine("-----------------------------");
                Console.WriteLine("");
                Console.WriteLine("");

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
            }
            catch (Exception e)
            {
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

            finally
            {

                Console.WriteLine("");
                Console.WriteLine("-----------------------------");
                Console.WriteLine("RoboCopy Log Lines:");
                foreach (string s in RCResults.LogLines)
                    Console.WriteLine(s);

                Console.WriteLine("-----------------------------");
                Console.WriteLine("");
                Console.WriteLine("CachedRoboCopy Log Lines:");
                foreach (string s in customResults.LogLines)
                    Console.WriteLine(s);
            }
        }


    }
}
