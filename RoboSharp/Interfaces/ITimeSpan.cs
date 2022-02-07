using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Interfaces
{
    /// <summary>
    /// Interface to normalize all the Start/End/TimeSpan properties of various objects
    /// </summary>
    internal interface ITimeSpan
    {
        /// <summary>
        /// Local time the command started.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Local time the command stopped.
        /// </summary>
        DateTime EndTime { get; }

        /// <summary>
        /// Length of time the process took to run
        /// </summary>
        TimeSpan TimeSpan { get; }
    }

}
