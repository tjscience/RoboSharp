using RoboSharp;
using RoboSharp.Interfaces;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharpUnitTesting
{
    static class Test_Setup
    {
        public static string TestDestination { get; } = "C:\\RoboSharpUnitTests";
        public static string Source_LargerNewer => Path.Combine(Directory.GetCurrentDirectory(), "TEST_FILES", "LargerNewer");
        public static string Source_Standard => Path.Combine(Directory.GetCurrentDirectory(), "TEST_FILES", "STANDARD");

        /// <summary>
        /// Generate the Starter Options and Test Objects to compare
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="UseLargerFileSet">When set to TRUE, uses the larger file set (which is also newer save times)</param>
        public static RoboCommand GenerateCommand(bool UseLargerFileSet)
        {
            // Build the base command
            var cmd = new RoboCommand();
            cmd.CopyOptions.Source = UseLargerFileSet ? Source_LargerNewer : Source_Standard;
            cmd.CopyOptions.Destination = TestDestination;
            cmd.CopyOptions.CopySubdirectoriesIncludingEmpty = true;
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
                Directory.Delete(TestDestination, true);
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

