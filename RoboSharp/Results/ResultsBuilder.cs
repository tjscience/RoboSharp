using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboSharp.Results
{
    /// <summary>
    /// Helper class to build a <see cref="RoboCopyResults"/> object.
    /// </summary>
    /// <remarks>
    /// <see href="https://github.com/tjscience/RoboSharp/wiki/ResultsBuilder"/>
    /// </remarks>
    internal class ResultsBuilder
    {
        private ResultsBuilder() { }

        internal ResultsBuilder(RoboCommand roboCommand) {
            RoboCommand = roboCommand;
            Estimator = new ProgressEstimator(roboCommand);
        }

        #region < Private Members >

        ///<summary>Reference back to the RoboCommand that spawned this object</summary>
        private readonly RoboCommand RoboCommand; 

        private readonly List<string> outputLines = new List<string>();

        /// <summary>This is the last line that was logged.</summary>
        internal string LastLine => outputLines.Count > 0 ? outputLines.Last() : "";

        #endregion

        #region < Command Options Properties >

        /// <see cref="RoboCommand.CommandOptions"/>
        internal string CommandOptions { get; set; }

        /// <inheritdoc cref="CopyOptions.Source"/>
        internal string Source { get; set; }
        
        /// <inheritdoc cref="CopyOptions.Destination"/>
        internal string Destination { get; set; }

        internal List<ErrorEventArgs> RoboCopyErrors { get; } = new List<ErrorEventArgs>();

        #endregion

        #region < Counters in case cancellation >

        /// <inheritdoc cref="ProgressEstimator"/>
        internal ProgressEstimator Estimator { get; }

        #endregion

        /// <summary>
        /// Add a LogLine reported by RoboCopy to the LogLines list.
        /// </summary>
        internal void AddOutput(string output)
        {
            if (output == null)
                return;

            if (Regex.IsMatch(output, @"^\s*[\d\.,]+%\s*$", RegexOptions.Compiled)) //Ignore Progress Indicators
                return;

            outputLines.Add(output);
        }

        /// <summary>
        /// Builds the results from parsing the logLines.
        /// </summary>
        /// <param name="exitCode"></param>
        /// <param name="IsProgressUpdateEvent">This is used by the ProgressUpdateEventArgs to ignore the loglines when generating the estimate </param>
        /// <returns></returns>
        internal RoboCopyResults BuildResults(int exitCode, bool IsProgressUpdateEvent = false)
        {
            var res = Estimator.GetResults(); //Start off with the estimated results, and replace if able

            res.JobName = RoboCommand.Name;
            res.Status = new RoboCopyExitStatus(exitCode);

            var statisticLines = IsProgressUpdateEvent ? new List<string>() : GetStatisticLines();

            //Dir Stats
            if (exitCode >= 0 && statisticLines.Count >= 1)
                res.DirectoriesStatistic = Statistic.Parse(Statistic.StatType.Directories, statisticLines[0]);

            //File Stats
            if (exitCode >= 0 && statisticLines.Count >= 2)
                res.FilesStatistic = Statistic.Parse(Statistic.StatType.Files, statisticLines[1]);

            //Bytes
            if (exitCode >= 0 && statisticLines.Count >= 3)
                res.BytesStatistic = Statistic.Parse(Statistic.StatType.Bytes, statisticLines[2]);

            //Speed Stats
            if (exitCode >= 0 && statisticLines.Count >= 6)
                res.SpeedStatistic = SpeedStatistic.Parse(statisticLines[4], statisticLines[5]);

            res.LogLines = outputLines.ToArray();
            res.RoboCopyErrors = this.RoboCopyErrors.ToArray();
            res.Source = this.Source;
            res.Destination = this.Destination;
            res.CommandOptions = this.CommandOptions;
            return res;
        }

        private List<string> GetStatisticLines()
        {
            var res = new List<string>();
            for (var i = outputLines.Count-1; i > 0; i--)
            {
                var line = outputLines[i];
                if (line.StartsWith("-----------------------"))
                    break;

                if (line.Contains(":") && !line.Contains("\\"))
                    res.Add(line);
            }

            res.Reverse();
            return res;
        }
    }
}
