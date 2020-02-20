using System;
using System.Security.Cryptography.X509Certificates;

namespace RoboSharp
{
    public class RoboCommandCompletedEventArgs : EventArgs
    {
        public RoboCommandCompletedEventArgs(Results.RoboCopyResults results)
        {
            this.Results = results;
        }

        public Results.RoboCopyResults Results { get; }
    }
}
