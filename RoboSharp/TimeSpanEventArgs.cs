using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TimeSpanEventArgs : EventArgs
    {

        private TimeSpanEventArgs() : base() { }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public TimeSpanEventArgs(System.DateTime startTime, System.DateTime endTime) : base()
        {
            StartTime = startTime;
            EndTime = endTime;
            LazyTimeSpan = new Lazy<TimeSpan>(() => EndTime.Subtract(StartTime));
        }

        private readonly Lazy<TimeSpan> LazyTimeSpan;

        /// <summary>
        /// Local time the command started.
        /// </summary>
        public virtual System.DateTime StartTime { get; }

        /// <summary>
        /// Local time the command started.
        /// </summary>
        public virtual System.DateTime EndTime { get; }

        /// <summary>
        /// Length of time the process took to run
        /// </summary>
        public virtual TimeSpan TimeSpan => LazyTimeSpan.Value;
    }
}
