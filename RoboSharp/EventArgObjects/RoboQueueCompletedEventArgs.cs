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
        internal RoboQueueCompletedEventArgs(RoboQueueResults runResults, bool listOnlyRun) : base(runResults.StartTime, runResults.EndTime, runResults.TimeSpan)
        {
            RunResults = runResults;
            CopyOperation = !listOnlyRun;
        }

        /// <summary>
        /// RoboQueue Results Object
        /// </summary>
        public RoboQueueResults RunResults { get; }

        /// <summary>
        /// TRUE if this run was a COPY OPERATION, FALSE is the results were created after a <see cref="RoboQueue.StartAll_ListOnly(string, string, string)"/> call.
        /// </summary>
        public bool CopyOperation { get; }

    }
}
