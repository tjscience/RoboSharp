using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    public class ErrorEventArgs : EventArgs
    {
        public string Error { get; set; }

        public ErrorEventArgs(string error)
        {
            Error = error;
        }
    }
}
