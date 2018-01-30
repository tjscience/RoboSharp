using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboSharp.Results
{
    public class ResultsBuilder
    {
        private readonly List<string> outputLines = new List<string>();

        public void AddOutput(string output)
        {
            if (output == null)
                return;

            if (Regex.IsMatch(output, @"^\s*[\d\.,]+%\s*$"))
                return;

            outputLines.Add(output);
        }

        public RoboCopyResults BuildResults(int exitCode)
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

            res.LogLines = outputLines.ToArray();

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