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
        
        /// <see cref="RoboCommand.CommandOptions"/>
        internal string CommandOptions { get; set; }

        /// <inheritdoc cref="CopyOptions.Source"/>
        internal string Source { get; set; }
        
        /// <inheritdoc cref="CopyOptions.Destination"/>
        internal string Destination { get; set; }

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

            if (statisticLines.Count >= 1)
                res.DirectoriesStatistic = Statistic.Parse(statisticLines[0]);

            if (statisticLines.Count >= 2)
                res.FilesStatistic = Statistic.Parse(statisticLines[1]);

            if (statisticLines.Count >= 3)
                res.BytesStatistic = Statistic.Parse(statisticLines[2]);

            if (statisticLines.Count >= 6)
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