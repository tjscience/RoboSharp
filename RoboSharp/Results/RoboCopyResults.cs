using System.Collections.Generic;
using System.Text;

namespace RoboSharp.Results
{
    /// <summary>
    /// Results provided by the RoboCopy command. Includes the Log, Exit Code, and statistics parsed from the log.
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

        #pragma warning disable CS1734 
        // paramref produces a cleaner remark statement, but since they aren't method params, it causes this warning.
        
        /// <summary>
        /// Combines the RoboCopyResults object with this RoboCopyResults object.
        /// <para/>
        /// </summary>
        /// <param name="resultsToCombine">RoboCopyResults object to combine with this one.</param>
        /// <remarks>
        /// The <paramref name="FilesStatistic"/>, <paramref name="BytesStatistic"/>, <paramref name="DirectoriesStatistic"/> properties will be added together. <br/>
        /// The <paramref name="SpeedStatistic"/> properties will be averaged together. <br/>
        /// The <paramref name="LogLines"/> will be appended into a single large array. <br/>
        /// The <paramref name="Status"/> property will represent the combined results.
        /// </remarks>
        public void CombineResult(RoboCopyResults resultsToCombine)
        {
            this.BytesStatistic.AddStatistic(resultsToCombine.BytesStatistic);
            this.FilesStatistic.AddStatistic(resultsToCombine.FilesStatistic);
            this.DirectoriesStatistic.AddStatistic(resultsToCombine.DirectoriesStatistic);
            this.SpeedStatistic.AverageStatistic(resultsToCombine.SpeedStatistic);

            List<string> combinedlog = new List<string>();
            combinedlog.AddRange(this.LogLines);
            combinedlog.AddRange(resultsToCombine.LogLines);
            this.LogLines = combinedlog.ToArray();
        }

        /// <summary>
        /// Combines the array of RoboCopyResults object with this RoboCopyResult object.
        /// </summary>
        /// <param name="resultsToCombine">Array or List of RoboCopyResults</param>
        /// <inheritdoc cref="CombineResult(RoboCopyResults)"/>
        public void CombineResult(IEnumerable<RoboCopyResults> resultsToCombine) 
        {
            //Initialize List Objects
            List<Statistic> bytes = new List<Statistic> { };
            List<Statistic> files = new List<Statistic> { };
            List<Statistic> dirs = new List<Statistic> { };
            List<SpeedStatistic> speed = new List<SpeedStatistic> { };
            List<RoboCopyExitStatus> status = new List<RoboCopyExitStatus> { };
            List<string> combinedlog = new List<string>();
            combinedlog.AddRange(this.LogLines);

            //Add all results to their respective lists
            foreach (RoboCopyResults R in resultsToCombine)
            {
                bytes.Add(R.BytesStatistic);
                files.Add(R.FilesStatistic);
                dirs.Add(R.DirectoriesStatistic);
                speed.Add(R.SpeedStatistic);
                status.Add(R.Status);
                combinedlog.AddRange(R.LogLines);
            }

            //Combine all results -> Done like this to reduce jumping in and out of functions repeatedly during the loop
            this.BytesStatistic.AddStatistic(bytes);
            this.FilesStatistic.AddStatistic(files);
            this.DirectoriesStatistic.AddStatistic(dirs);
            this.SpeedStatistic.AverageStatistic(speed);
            this.Status.CombineStatus(status);
            this.LogLines = combinedlog.ToArray();
        }


        /// <returns>
        /// New RoboCopy Results Object. <para/>
        /// </returns>
        /// <inheritdoc cref="CombineResult(RoboCopyResults)"/>
        public static RoboCopyResults CombineResults(RoboCopyResults resultsToCombine)
        {
            RoboCopyResults ret = new RoboCopyResults();
            ret.CombineResult(resultsToCombine);
            return ret;
        }

        /// <returns>
        /// New RoboCopy Results Object. <para/>
        /// </returns>
        /// <inheritdoc cref="CombineResult(IEnumerable{RoboCopyResults})"/>
        public static RoboCopyResults CombineResults(IEnumerable<RoboCopyResults> resultsToCombine) 
        {
            RoboCopyResults ret = new RoboCopyResults();
            ret.CombineResult(resultsToCombine);
            return ret;
        }

        #pragma warning restore CS1734
    }
}
