using System;
using System.Security.Cryptography.X509Certificates;
using RoboSharp.EventArgObjects;

// Do Not change NameSpace here! -> Must be RoboSharp due to prior releases
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
        internal RoboCommandCompletedEventArgs(Results.RoboCopyResults results) : base(results.StartTime, results.EndTime, results.TimeSpan)
        {
            this.Results = results;
        }

        /// <inheritdoc cref="Results.RoboCopyResults"/>
        public Results.RoboCopyResults Results { get; }
    }
}
