using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Interfaces
{
    /// <summary>
    /// Interface for the <see cref="RoboSharp.Results.RoboQueueResults"/> object. <br/>
    /// Implements <see cref="IRoboCopyResultsList"/>
    /// </summary>
    public interface IRoboQueueResults : IRoboCopyResultsList
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
