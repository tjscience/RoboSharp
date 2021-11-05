using System;

namespace RoboSharp
{
    /// <summary>
    /// Information about an Error reported by RoboCopy
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Error Description
        /// </summary>
        public string Error { get; }
        
        /// <summary>
        /// Error Code
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// New Instance of ErrorEventArgs object
        /// </summary>
        /// <param name="error"><inheritdoc cref="Error"/></param>
        /// <param name="errorCode"><inheritdoc cref="ErrorCode"/></param>
        public ErrorEventArgs(string error, int errorCode)
        {
            Error = error;
            ErrorCode = errorCode;
        }
    }
}
