using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboSharp;
using RoboSharp.Results;
using RoboSharp.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoboSharpUnitTesting
{
    class RoboSharpTestResults
    {
        private RoboSharpTestResults() { }
        public RoboSharpTestResults(RoboCopyResults results, IProgressEstimator estimator)
        {
            Results = results;
            Estimator = estimator;
            Errors = CompareIResults(results, (ProgressEstimator)estimator).ToArray();
        }

        public IProgressEstimator Estimator { get; }
        public RoboCopyResults Results { get; }
        public string[] Errors{ get; }
        public bool IsErrored => Errors.Length > 0;

        


        /// <summary>
        /// Throws <see cref="AssertFailedException"/> if either Results or Estimator are errored.
        /// </summary>
        public void AssertTest()
        {
            try
            {
                int i = 0;
                //Write the summary at the top for easier reference
                Console.WriteLine("SUMMARY LINES:");
                foreach (string s in Results.LogLines)
                {
                    if (s.Trim().StartsWith("---------")) 
                        i++;
                    else if (i>3)
                    {
                        Console.WriteLine(s);
                    }
                }
                Console.WriteLine("\n\n LOG LINES:");
                //Write the log lines
                foreach (string s in Results.LogLines)
                {
                    Console.WriteLine(s);
                }
                Assert.IsTrue(!IsErrored);
            }catch(AssertFailedException e)
            {
                throw CustomAssertException.Factory(this, e);
            }
        }

        class CustomAssertException : AssertFailedException
        {
            private CustomAssertException() { }
            public CustomAssertException(string message, AssertFailedException e) : base(message, e) { }

            public static CustomAssertException Factory(RoboSharpTestResults testResult, AssertFailedException e)
            {
                string msg = testResult.Errors.ConvertToLinedString();
                return new CustomAssertException(msg, e);
            }
        }

        public static List<string> CompareIResults(IResults expected, IResults actual)
        {
            var Errors = new List<string>();
            CompareStatistics(expected.DirectoriesStatistic, actual.DirectoriesStatistic, ref Errors);
            CompareStatistics(expected.FilesStatistic, actual.FilesStatistic, ref Errors);
            CompareStatistics(expected.BytesStatistic, actual.BytesStatistic, ref Errors);
            return Errors;
        }

        public static bool CompareStatistics(IStatistic expectedResults, IStatistic results, ref List<string> Errors)
        {
            ErrGenerator("Total", expectedResults, results, ref Errors);
            ErrGenerator("Copied", expectedResults, results, ref Errors);
            ErrGenerator("Extras", expectedResults, results, ref Errors);
            ErrGenerator("Skipped", expectedResults, results, ref Errors);
            ErrGenerator("Failed", expectedResults, results, ref Errors);
            ErrGenerator("Mismatch", expectedResults, results, ref Errors);
            return Errors.Count == 0;
        }

        private static void ErrGenerator(string propName, IStatistic expected, IStatistic actual, ref List<string> Errors)
        {
            long eSize = 0; long aSize = 0;
            switch(propName)
            {
                case "Total": eSize = expected.Total; aSize = actual.Total; break;
                case "Copied": eSize = expected.Copied; aSize = actual.Copied; break;
                case "Extras": eSize = expected.Extras; aSize = actual.Extras; break;
                case "Failed": eSize = expected.Failed; aSize = actual.Failed; break;
                case "Mismatch": eSize = expected.Mismatch; aSize = actual.Mismatch; break;
                case "Skipped": eSize = expected.Skipped; aSize = actual.Skipped; break;
            }
            if (eSize != aSize)
                Errors.Add($"{expected.Type}.{propName} -- Expected: {eSize}  || Actual: {aSize}");
        }
    }
}
