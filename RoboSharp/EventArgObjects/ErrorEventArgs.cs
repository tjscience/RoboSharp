using System;

// Do Not change NameSpace here! -> Must be RoboSharp due to prior releases
namespace RoboSharp
{
    /// <summary>
    /// Information about an Error reported by the RoboCopy process
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
        /// Error FileName
        /// </summary>
        public string ErrorFileName { get; }

        /// <summary>
        /// <inheritdoc cref="ErrorEventArgs"/>
        /// </summary>
        /// <param name="error"><inheritdoc cref="Error"/></param>
        /// <param name="errorCode"><inheritdoc cref="ErrorCode"/></param>
        /// <param name="errorFileName"><inheritdoc cref="ErrorFileName"/></param>
        public ErrorEventArgs(string error, int errorCode, string errorFileName)
        {
            Error = error;
            ErrorCode = errorCode;
            ErrorFileName = errorFileName;
        }
    }
}
