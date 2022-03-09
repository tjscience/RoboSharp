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
                Test_Setup.WriteLogLines(Results);
                Assert.IsTrue(!IsErrored);
            }catch(AssertFailedException e)
            {
                throw CustomAssertException.Factory(this, e);
            }
            try
            {
                Assert.IsFalse(Results.LogLines.Any(s => s.Contains("Invalid Parameter")));
            }catch (Exception e )
            {
                throw new AssertFailedException("INVALID Parameter! -- See LogLines");
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

        public static List<string> CompareIResults(IResults RCResults, IResults PEResults)
        {
            var Errors = new List<string>();
            CompareStatistics(RCResults.DirectoriesStatistic, PEResults.DirectoriesStatistic, ref Errors);
            CompareStatistics(RCResults.FilesStatistic, PEResults.FilesStatistic, ref Errors);
            CompareStatistics(RCResults.BytesStatistic, PEResults.BytesStatistic, ref Errors);
            return Errors;
        }

        public static bool CompareStatistics(IStatistic RCResults, IStatistic PEResults, ref List<string> Errors)
        {
            ErrGenerator("Total", RCResults, PEResults, ref Errors);
            ErrGenerator("Copied", RCResults, PEResults, ref Errors);
            ErrGenerator("Extras", RCResults, PEResults, ref Errors);
            ErrGenerator("Skipped", RCResults, PEResults, ref Errors);
            ErrGenerator("Failed", RCResults, PEResults, ref Errors);
            ErrGenerator("Mismatch", RCResults, PEResults, ref Errors);
            return Errors.Count == 0;
        }

        private static void ErrGenerator(string propName, IStatistic RCResults, IStatistic PEResults, ref List<string> Errors)
        {
            long eSize = 0; long aSize = 0;
            switch(propName)
            {
                case "Total": eSize = RCResults.Total; aSize = PEResults.Total; break;
                case "Copied": eSize = RCResults.Copied; aSize = PEResults.Copied; break;
                case "Extras": eSize = RCResults.Extras; aSize = PEResults.Extras; break;
                case "Failed": eSize = RCResults.Failed; aSize = PEResults.Failed; break;
                case "Mismatch": eSize = RCResults.Mismatch; aSize = PEResults.Mismatch; break;
                case "Skipped": eSize = RCResults.Skipped; aSize = PEResults.Skipped; break;
            }
            if (eSize != aSize)
                Errors.Add($"{RCResults.Type}.{propName} -- RoboCopy: {eSize}  || ProgressEstimator: {aSize}");
        }
    }
}
