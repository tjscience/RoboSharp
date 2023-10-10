using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Results
{
    /// <summary>
    /// Interface consumers can use to easily build a set of <see cref="RoboCopyResults"/>
    /// </summary>
    public interface IResultsBuilder
    {
        /// <inheritdoc cref="RoboCopyResults.Source"/>
        string Source { get; }
        
        /// <inheritdoc cref="RoboCopyResults.Destination"/>
        string Destination { get; }
        
        /// <inheritdoc cref="RoboCopyResults.JobName"/>
        string JobName { get; }
        
        /// <inheritdoc cref="RoboCopyResults.CommandOptions"/>
        string CommandOptions { get; }
        
        /// <inheritdoc cref="RoboCopyResults.LogLines"/>
        IEnumerable<string> LogLines { get; }
        
        /// <inheritdoc cref="RoboCopyResults.BytesStatistic"/>
        IStatistic BytesStatistic { get; }
        
        /// <inheritdoc cref="RoboCopyResults.FilesStatistic"/>
        IStatistic FilesStatistic { get; }
        
        /// <inheritdoc cref="RoboCopyResults.DirectoriesStatistic"/>
        IStatistic DirectoriesStatistic { get; }

        /// <inheritdoc cref="RoboCopyResults.SpeedStatistic"/>
        ISpeedStatistic SpeedStatistic { get; }

        /// <inheritdoc cref="RoboCopyResults.StartTime"/>
        DateTime StartTime { get; }
        
        /// <inheritdoc cref="RoboCopyResults.EndTime"/>
        DateTime EndTime { get; }
        
        /// <inheritdoc cref="RoboCopyResults.Status"/>
        RoboCopyExitStatus ExitStatus { get; }
        
        /// <inheritdoc cref="RoboCopyResults.RoboCopyErrors"/>
        IEnumerable<ErrorEventArgs> CommandErrors { get; }
    }
}
