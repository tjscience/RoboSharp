using RoboSharp;
using RoboSharp.Interfaces;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSharpUnitTesting
{
    static class Test_Setup
    {
        public static string TestDestination { get; } = Path.Combine(Directory.GetCurrentDirectory(), "TEST_DESTINATION");
        public static string Source_LargerNewer { get; } = Path.Combine(Directory.GetCurrentDirectory(), "TEST_FILES", "LargerNewer");
        public static string Source_Standard { get; } = Path.Combine(Directory.GetCurrentDirectory(), "TEST_FILES", "STANDARD");

        /// <summary>
        /// Generate the Starter Options and Test Objects to compare
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="UseLargerFileSet">When set to TRUE, uses the larger file set (which is also newer save times)</param>
        public static RoboCommand GenerateCommand(bool UseLargerFileSet, bool ListOnlyMode)
        {
            // Build the base command
            var cmd = new RoboCommand();
            cmd.CopyOptions.Source = UseLargerFileSet ? Source_LargerNewer : Source_Standard;
            cmd.CopyOptions.Destination = TestDestination;
            cmd.CopyOptions.CopySubdirectoriesIncludingEmpty = true;
            cmd.LoggingOptions.ListOnly = ListOnlyMode;
            return cmd;
        }

        public static async Task<RoboSharpTestResults> RunTest(RoboCommand cmd)
        {
            IProgressEstimator prog = null;
            cmd.OnProgressEstimatorCreated += (o, e) => prog = e.ResultsEstimate;
            var results = await cmd.StartAsync();
            return new RoboSharpTestResults(results, prog);
        }

        /// <summary>
        /// Deletes all and folders in <see cref="TestDestination"/>
        /// </summary>
        public static void ClearOutTestDestination()
        {

            if (Directory.Exists(TestDestination))
            {
                var files = new DirectoryInfo(TestDestination).GetFiles("*", SearchOption.AllDirectories);
                foreach (var f in files)
                    File.SetAttributes(f.FullName, FileAttributes.Normal);
                Directory.Delete(TestDestination, true);
            }
        }

        /// <summary>
        /// Write the LogLines to the Test Log
        /// </summary>
        /// <param name="Results"></param>
        public static void WriteLogLines(RoboCopyResults Results, bool SummaryOnly = false)
        {
            //Write the summary at the top for easier reference
            if (Results is null)
            {
                Console.WriteLine("Results Object is null!");
                return;
            }
            int i = 0;
            Console.WriteLine("SUMMARY LINES:");
            foreach (string s in Results.LogLines)
            {
                if (s.Trim().StartsWith("---------"))
                    i++;
                else if (i > 3)
                    Console.WriteLine(s);
            }
            if (!SummaryOnly)
            {
                Console.WriteLine("\n\n LOG LINES:");
                //Write the log lines
                foreach (string s in Results.LogLines)
                    Console.WriteLine(s);
            }
        }

        public static string ConvertToLinedString(this IEnumerable<string> strings)
        {
            string ret = "";
            foreach (string s in strings)
                ret += s + "\n";
            return ret;
        }

        public static void SetValues(this Statistic stat, int total, int copied, int failed, int extras, int mismatch, int skipped)
        {
            stat.Reset();
            stat.Add(total, copied, extras, failed, mismatch, skipped);
        }

    }
}

