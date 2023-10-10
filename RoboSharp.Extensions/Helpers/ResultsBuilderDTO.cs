using RoboSharp.Interfaces;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.Helpers
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// This is a helper object implementing <see cref="IResultsBuilder"/> to easily create  <see cref="RoboCopyResults"/> objects.
    /// </summary>
    public class ResultsBuilderDTO : RoboSharp.Results.IResultsBuilder
    {
        public string Source { get; set; }

        public string Destination { get; set; }

        public string JobName { get; set; }

        public string CommandOptions { get; set; }

        public IEnumerable<string> LogLines { get; set; }

        public IStatistic BytesStatistic { get; set; }

        public IStatistic FilesStatistic { get; set; }

        public IStatistic DirectoriesStatistic { get; set; }

        public ISpeedStatistic SpeedStatistic { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public RoboCopyExitStatus ExitStatus { get; set; }
        public IEnumerable<ErrorEventArgs> CommandErrors { get; set; }

    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
