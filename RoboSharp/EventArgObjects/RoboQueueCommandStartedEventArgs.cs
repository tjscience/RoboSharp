using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp.EventArgObjects
{
    /// <summary>
    /// EventArgs to declare when a RoboCommand process starts
    /// </summary>
    public class RoboQueueCommandStartedEventArgs : EventArgs
    {
        private RoboQueueCommandStartedEventArgs() : base() { }
        internal RoboQueueCommandStartedEventArgs(IRoboCommand cmd) : base() { Command = cmd; StartTime = DateTime.Now; }

        /// <summary>
        /// Command that started.
        /// </summary>
        public IRoboCommand Command { get; }

        /// <summary>
        /// Returns TRUE if the command's <see cref="RoboSharp.Results.ProgressEstimator"/> is available for binding
        /// </summary>
        public bool ProgressEstimatorAvailable => Command.IsRunning;

        /// <summary>
        /// Local time the command started.
        /// </summary>
        public DateTime StartTime { get; }
    }
}
