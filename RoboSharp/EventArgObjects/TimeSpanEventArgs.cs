using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp.EventArgObjects
{
    /// <summary>
    /// Provide a base class that includes a StartTime, EndTime and will calculate the TimeSpan in between
    /// </summary>
    public abstract class TimeSpanEventArgs : EventArgs
    {
        private TimeSpanEventArgs() : base() { }
        
        /// <summary>
        /// Create New Args
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public TimeSpanEventArgs(DateTime startTime, DateTime endTime) : base()
        {
            StartTime = startTime;
            EndTime = endTime;
            LazyTimeSpan = new Lazy<TimeSpan>(() => EndTime.Subtract(StartTime));
        }

        private readonly Lazy<TimeSpan> LazyTimeSpan;

        /// <summary>
        /// Local time the command started.
        /// </summary>
        public virtual DateTime StartTime { get; }

        /// <summary>
        /// Local time the command started.
        /// </summary>
        public virtual DateTime EndTime { get; }

        /// <summary>
        /// Length of time the process took to run
        /// </summary>
        public virtual TimeSpan TimeSpan => LazyTimeSpan.Value;
    }
}
