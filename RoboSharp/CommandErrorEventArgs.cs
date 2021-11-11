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
        /// If this CommandErrorEventArgs object was created in response to an exception, that exception is captured here. <br/>
        /// If no exception was thrown, this property will be null.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// <inheritdoc cref="CommandErrorEventArgs"/>
        /// </summary>
        /// <param name="error"><inheritdoc cref="Error"/></param>
        /// <param name="ex"><inheritdoc cref="Exception"/></param>
        public CommandErrorEventArgs(string error, Exception ex)
        {
            Error = error;
            this.Exception = ex;
        }

        /// <summary>
        /// <inheritdoc cref="CommandErrorEventArgs"/>
        /// </summary>
        /// <param name="ex">Exception to data to pass to the event handler</param>
        public CommandErrorEventArgs(Exception ex)
        {
            Error = ex.Message;
            this.Exception = ex;
        }

    }
}