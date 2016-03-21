using System;

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
