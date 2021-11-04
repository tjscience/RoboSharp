using System;

namespace RoboSharp
{
    /// <summary>
    /// Current File Progress reported as <see cref="double"/>
    /// </summary>
    public class CopyProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Current File Progress Percentage
        /// </summary>
        public double CurrentFileProgress { get; set; }

        /// <summary><inheritdoc cref="CopyProgressEventArgs"/></summary>
        /// <param name="progress"><inheritdoc cref="CurrentFileProgress"/></param>
        public CopyProgressEventArgs(double progress)
        {
            CurrentFileProgress = progress;
        }
    }
}
