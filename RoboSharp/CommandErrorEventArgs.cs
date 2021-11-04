using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp
{
    /// <summary>
    /// Describes an error that occured when generating the command
    /// </summary>
    public class CommandErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Error Description
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// New Instance of CommandErrorEventArgs object
        /// </summary>
        /// <param name="error"></param>
        public CommandErrorEventArgs(string error)
        {
            Error = error;
        }
    }
}
