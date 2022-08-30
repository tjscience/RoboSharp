using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboSharp.Interfaces;
using RoboSharp.Results;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Other extension methods for custom implementations
    /// </summary>
    public static class ProgressEstimatorExtensions
    {
        /// <summary>
        /// Get the Results object from the ProgressEstimator, then apply the supplied values to the results object to complete it
        /// </summary>
        /// <param name="estimator">The ProgressEstimator instance</param>
        /// <param name="startTime">The time the command was started</param>
        /// <param name="endTime">the time the command was stopped</param>
        /// <param name="logLines">any log lines to add to the results object</param>
        /// <param name="speedStatistic"></param>
        /// <returns>new Results Object</returns>
        public static RoboCopyResults GetResults(this ProgressEstimator estimator, DateTime startTime, DateTime endTime, ISpeedStatistic speedStatistic, params string[] logLines)
        {
            var results = estimator.GetResults();
            results.LogLines = logLines;
            results.JobName = estimator.command.Name;
            results.CommandOptions = estimator.command.CommandOptions;
            results.Destination = estimator.command.CopyOptions.Destination;
            results.Source = estimator.command.CopyOptions.Source;
            results.StartTime = startTime;
            results.EndTime = endTime;
            results.TimeSpan = endTime - startTime;
            results.SpeedStatistic = speedStatistic.Clone();
            return results;
        }

        /// <param name="cmd">The associated IRoboCommand - Used to pull the Name, Source, and Destination</param>
        /// <inheritdoc cref="GetResults(ProgressEstimator, DateTime, DateTime, ISpeedStatistic, string[])"/>
        /// <param name="estimator"/><param name="startTime"/><param name="endTime"/><param name="logLines"/><param name="speedStatistic"/>
        public static RoboCopyResults GetResults(this IProgressEstimator estimator, IRoboCommand cmd, DateTime startTime, DateTime endTime, ISpeedStatistic speedStatistic, params string[] logLines)
        {
            var results = new RoboCopyResults(cmd.CopyOptions.Source, cmd.CopyOptions.Destination, estimator.BytesStatistic, estimator.FilesStatistic, estimator.DirectoriesStatistic, speedStatistic, startTime, endTime, null, logLines);
            results.JobName = cmd.Name;
            results.CommandOptions = cmd.CommandOptions;
            //results.TimeSpan = endTime - startTime;
            return results;
        }
    }
}
