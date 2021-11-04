using System;
using System.Security.Cryptography.X509Certificates;

namespace RoboSharp
{
    /// <summary>
    /// <inheritdoc cref="Results.RoboCopyResults"/>
    /// </summary>
    public class RoboCommandCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Return the Results object
        /// </summary>
        /// <param name="results"></param>
        public RoboCommandCompletedEventArgs(Results.RoboCopyResults results)
        {
            this.Results = results;
        }

        /// <inheritdoc cref="Results.RoboCopyResults"/>
        public Results.RoboCopyResults Results { get; }
    }
}
