using RoboSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.CopyFileEx
{
    /// <summary>
    /// Args thrown when a copy operation fails
    /// </summary>
    public class FileCopyFailedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public FileCopyFailedEventArgs(IFilePair filePair, string error, Exception e, bool failed = true, bool cancelled = false)
        {
            FilePair = filePair ?? throw new ArgumentNullException(nameof(filePair));
            Error = error;
            //WasSkipped = skipped;
            WasFailed = failed;
            WasCancelled = cancelled;
            Exception = e;
        }

        /// <summary>
        /// Source/Destination information
        /// </summary>
        public IFilePair FilePair { get; }

        /// <summary>
        /// File copy failed
        /// </summary>
        public bool WasFailed { get; }

        /// <summary>
        /// File copy was cancelled
        /// </summary>
        public bool WasCancelled { get; }

        /// <summary>
        /// Error Text provided by the caller
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// The exception that was raised
        /// </summary>
        public Exception Exception { get; }

    }
}
