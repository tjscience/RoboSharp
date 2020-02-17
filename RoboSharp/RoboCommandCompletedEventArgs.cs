using System;

namespace RoboSharp
{
    public class RoboCommandCompletedEventArgs : EventArgs
    {
        public int RoboCopyExitCode { get; set; }
        public RoboCommandCompletedEventArgs(int roboCopyExitCode)
        {
            RoboCopyExitCode = roboCopyExitCode;
        }
    }
}
