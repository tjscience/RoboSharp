using System.Collections.Generic;

namespace RoboSharp.Results
{
    /// <summary>
    /// 
    /// </summary>
    public class RoboCopyResults
    {
        /// <inheritdoc cref="RoboCopyExitStatus"/>
        public RoboCopyExitStatus Status { get; internal set; }
        /// <summary> Information about number of Directories Copied, Skipped, Failed, etc.</summary>
        public Statistic DirectoriesStatistic { get; internal set; }
        /// <summary> Information about number of Files Copied, Skipped, Failed, etc.</summary>
        public Statistic FilesStatistic { get; internal set; }
        /// <summary> Information about number of Bytes processed.</summary>
        public Statistic BytesStatistic { get; internal set; }
        /// <inheritdoc cref="RoboSharp.Results.SpeedStatistic"/>
        public SpeedStatistic SpeedStatistic { get; internal set; }
        /// <summary> Output Text reported by RoboCopy </summary>
        public string[] LogLines { get; internal set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            string str = $"ExitCode: {Status.ExitCode}, Directories: {DirectoriesStatistic.Total}, Files: {FilesStatistic.Total}, Bytes: {BytesStatistic.Total}";

            if (SpeedStatistic != null)
            {
                str = str + $", Speed: {SpeedStatistic.BytesPerSec} Bytes/sec";
            }

            return str;
        }
    }
}