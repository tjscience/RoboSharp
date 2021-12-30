using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboSharp.Results
{
    /// <summary>
    /// Helper class to build a <see cref="RoboCopyResults"/> object.
    /// </summary>
    internal class ResultsBuilder
    {
         
        private readonly List<string> outputLines = new List<string>();

        #region < Command Options Properties >

        /// <see cref="RoboCommand.CommandOptions"/>
        internal string CommandOptions { get; set; }

        /// <inheritdoc cref="CopyOptions.Source"/>
        internal string Source { get; set; }
        
        /// <inheritdoc cref="CopyOptions.Destination"/>
        internal string Destination { get; set; }

        #endregion

        #region < Counters in case cancellation >

        /// <summary> Internal counter used to generate statistics if job is cancelled </summary>
        internal long TotalDirs { get; private set; } = 0;
        /// <summary> Internal counter used to generate statistics if job is cancelled </summary>
        internal long TotalDirsCopied { get; private set; } = 0;
        /// <summary> Internal counter used to generate statistics if job is cancelled </summary>
        internal long TotalFiles { get; private set; } = 0;
        /// <summary> Internal counter used to generate statistics if job is cancelled </summary>
        internal long TotalFilesCopied { get; private set; } = 0;

        // Methods to add to internal counters -> created as methods to allow inline null check since results?.TotalDirs++; won't compile
        /// <summary>Increment <see cref="TotalDirs"/></summary>
        internal void AddDir() => TotalDirs++;
        /// <summary>Increment <see cref="TotalDirsCopied"/></summary>
        internal void AddDirCopied() => TotalDirsCopied++;
        /// <summary>Increment <see cref="TotalFiles"/></summary>
        internal void AddFile() => TotalFiles++;
        /// <summary>Increment <see cref="TotalFilesCopied"/></summary>
        internal void AddFileCopied() => TotalFilesCopied++;

        #endregion

        internal void AddOutput(string output)
        {
            if (output == null)
                return;

            if (Regex.IsMatch(output, @"^\s*[\d\.,]+%\s*$"))
                return;

            outputLines.Add(output);
        }

        internal RoboCopyResults BuildResults(int exitCode)
        {
            var res = new RoboCopyResults();
            res.Status = new RoboCopyExitStatus(exitCode);

            var statisticLines = GetStatisticLines();

            if (exitCode >= 0 && statisticLines.Count >= 1)
                res.DirectoriesStatistic = Statistic.Parse(statisticLines[0]);
            else
                res.DirectoriesStatistic = new Statistic() { Total = TotalDirs, Copied = TotalDirsCopied};

            if (exitCode >= 0 && statisticLines.Count >= 2)
                res.FilesStatistic = Statistic.Parse(statisticLines[1]);
            else
                res.FilesStatistic = new Statistic() { Total = TotalFiles, Copied = TotalFilesCopied };

            if (exitCode >= 0 && statisticLines.Count >= 3)
                res.BytesStatistic = Statistic.Parse(statisticLines[2]);

            if (exitCode >= 0 && statisticLines.Count >= 6)
                res.SpeedStatistic = SpeedStatistic.Parse(statisticLines[4], statisticLines[5]);

            res.LogLines = outputLines.ToArray();

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

                if (line.Contains(":"))
                    res.Add(line);
            }

            res.Reverse();
            return res;
        }
    }
}