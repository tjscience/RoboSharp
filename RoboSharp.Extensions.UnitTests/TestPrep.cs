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


        public static async Task<RoboSharpTestResults[]> RunTests(RoboCommand roboCommand, RoboMover cachedRoboCommand, bool CleanBetweenRuns, Action actionBetweenRuns = null)
        {
            var results = new List<RoboSharpTestResults>();
            BetweenRuns();
            results.Add(await TestSetup.RunTest(roboCommand));
            BetweenRuns();
            cachedRoboCommand.OnError += CachedRoboCommand_OnError;
            cachedRoboCommand.OnCommandError += CachedRoboCommand_OnCommandError;
            results.Add(await TestSetup.RunTest(cachedRoboCommand));
            cachedRoboCommand.OnError -= CachedRoboCommand_OnError;
            cachedRoboCommand.OnCommandError -= CachedRoboCommand_OnCommandError;

            if (CleanBetweenRuns) CleanDestination();
            return results.ToArray();

            void BetweenRuns()
            {
                if (CleanBetweenRuns) CleanDestination();
                if (actionBetweenRuns != null) actionBetweenRuns();
            }


        }

        private static void CachedRoboCommand_OnCommandError(IRoboCommand sender, CommandErrorEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }

        private static void CachedRoboCommand_OnError(IRoboCommand sender, RoboSharp.ErrorEventArgs e)
        {
            Console.WriteLine(e.Error);
        }

        public static Task<RoboSharpTestResults> RunTest(IRoboCommand roboCommand)
        {
            return TestSetup.RunTest(roboCommand);
        }

    }
}
