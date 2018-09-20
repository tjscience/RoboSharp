using System.Collections.Generic;

namespace RoboSharp.Results
{
    public class RoboCopyResults
    {
        public RoboCopyExitStatus Status { get; internal set; }
        public Statistic DirectoriesStatistic { get; internal set; }
        public Statistic FilesStatistic { get; internal set; }
        public Statistic BytesStatistic { get; internal set; }
        public string[] LogLines { get; internal set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"ExitCode: {Status.ExitCode}, Directories: {DirectoriesStatistic.Total}, Files: {FilesStatistic.Total}, Bytes: {BytesStatistic.Total}";
        }
    }
}