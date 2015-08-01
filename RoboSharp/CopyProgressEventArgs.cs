using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
