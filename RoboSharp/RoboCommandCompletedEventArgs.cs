using System;
using System.Security.Cryptography.X509Certificates;

namespace RoboSharp
{
    /// <summary>
    /// <inheritdoc cref="Results.RoboCopyResults"/>
    /// </summary>
    public class RoboCommandCompletedEventArgs : TimeSpanEventArgs
    {
        /// <summary>
        /// Return the Results object
        /// </summary>
        /// <param name="results"></param>
        internal RoboCommandCompletedEventArgs(Results.RoboCopyResults results, DateTime startTime, DateTime endTime) : base(startTime, endTime)
        {
            this.Results = results;
        }

        /// <inheritdoc cref="Results.RoboCopyResults"/>
        public Results.RoboCopyResults Results { get; }
    }
}
