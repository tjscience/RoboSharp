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
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        internal RoboCommandCompletedEventArgs(Results.RoboCopyResults results, DateTime startTime, DateTime endTime) : base(startTime, endTime)
        {
            this.Results = results;
        }

        /// <inheritdoc cref="Results.RoboCopyResults"/>
        public Results.RoboCopyResults Results { get; }
    }
}
