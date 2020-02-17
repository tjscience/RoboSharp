using System;

namespace RoboSharp
{
    public class ErrorEventArgs : EventArgs
    {
        public string Error { get; }
        public int ErrorCode { get; }

        public ErrorEventArgs(string error, int errorCode)
        {
            Error = error;
            ErrorCode = errorCode;
        }
    }
}
