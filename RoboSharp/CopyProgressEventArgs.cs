using System;

namespace RoboSharp
{
    public class CopyProgressEventArgs : EventArgs
    {
        public double CurrentFileProgress { get; set; }

        public CopyProgressEventArgs(double progress)
        {
            CurrentFileProgress = progress;
        }
    }
}
