using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp
{
    public class CommandErrorEventArgs : EventArgs
    {
        public string Error { get; }

        public CommandErrorEventArgs(string error)
        {
            Error = error;
        }
    }
}
