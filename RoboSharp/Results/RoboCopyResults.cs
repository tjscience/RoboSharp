using System.Collections.Generic;

namespace RoboSharp.Results
{
    /// <summary>
    /// Results provided by the RoboCopy command. Includes the Log, Exit Code, and statistics parsed from the log.
    /// </summary>
    public class RoboCopyResults
    {
        internal RoboCopyResults() { }

        #region < Properties >

        /// <inheritdoc cref="CopyOptions.Source"/>
        public string Source { get; internal set; }

        /// <inheritdoc cref="CopyOptions.Destination"/>
        public string Destination { get; internal set; }

        /// <inheritdoc cref="RoboCommand.CommandOptions"/>
        public string CommandOptions { get; internal set; }

        /// <inheritdoc cref="RoboCopyExitStatus"/>
        public RoboCopyExitStatus Status { get; internal set; }

        /// <summary> Information about number of Directories Copied, Skipped, Failed, etc.</summary>
        /// <remarks> 
        /// If the job was cancelled, this will contain incomplete/inaccurate results, showing only total number of directories handled. <br/>
        /// The 'Copied' stat may be inaccurate, as it assumes a directory was created if not running in List-Only mode. <br/>
        /// Extras, Mismatch, Errors &amp; Failed are not tracked if job was cancelled.
        /// </remarks>
        public Statistic DirectoriesStatistic { get; internal set; }

        /// <summary> Information about number of Files Copied, Skipped, Failed, etc.</summary>
        /// <remarks> 
        /// If the job was cancelled, this will contain incomplete / inaccurate results, showing only total number of files handled and how many hit 100% copy progress. <br/>
        /// Extras, Mismatch, Errors, &amp; Failed are not tracked if job was cancelled, and the number of files may be inaccurate if a failure or error occurred.
        /// </remarks>
        public Statistic FilesStatistic { get; internal set; }

        /// <summary> Information about number of Bytes processed.</summary>
        /// <remarks> If the job was cancelled, this be null.</remarks>
        public Statistic BytesStatistic { get; internal set; }

        /// <inheritdoc cref="RoboSharp.Results.SpeedStatistic"/>
        /// <remarks> If the job was cancelled, this be null.</remarks>
        public SpeedStatistic SpeedStatistic { get; internal set; }

        /// <summary> Output Text reported by RoboCopy </summary>
        public string[] LogLines { get; internal set; }

        #endregion

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            string str = $"ExitCode: {Status.ExitCode}, Directories: {DirectoriesStatistic?.Total.ToString() ?? "Unknown"}, Files: {FilesStatistic?.Total.ToString() ?? "Unknown"}, Bytes: {BytesStatistic?.Total.ToString() ?? "Unknown"}";

            if (SpeedStatistic != null)
            {
                str += $", Speed: {SpeedStatistic.BytesPerSec} Bytes/sec";
            }

            return str;
        }
    }
}