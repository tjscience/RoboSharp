using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoboSharp.Interfaces;
using RoboSharp.Results;

namespace RoboSharp.EventArgObjects
{
    /// <summary>
    /// EventArgs to declare when a RoboCommand process starts
    /// </summary>
    public class RoboQueueCompletedEventArgs : TimeSpanEventArgs
    {
        internal RoboQueueCompletedEventArgs(RoboCopyResultsList runResults, DateTime startTime, DateTime endTime) : base(startTime, endTime)
        {
            RunResults = new RoboCopyResultsList(runResults);
        }

        /// <summary>
        /// Command that started.
        /// </summary>
        public RoboCopyResultsList RunResults { get; }

    }
}
