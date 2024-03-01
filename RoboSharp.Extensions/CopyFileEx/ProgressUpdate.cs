using System;
using System.Collections.Generic;
using System.Text;

namespace RoboSharp.Extensions.CopyFileEx
{
    /// <summary>
    /// Provides details about a copy operation's progress
    /// </summary>
    public struct ProgressUpdate
    {
        /// <summary>
        /// The default constructor - Calculates the Progress
        /// </summary>
        /// <param name="fileSize"></param>
        /// <param name="bytesCopied"></param>
        public ProgressUpdate(long fileSize, long bytesCopied)
        {
            TotalBytes = fileSize;
            BytesCopied = bytesCopied;
            if (TotalBytes == bytesCopied)
                Progress = 100;
            else if (fileSize > bytesCopied)
                Progress = (double)100 * bytesCopied / fileSize;
            else
                Progress = 0;
        }

        /// <summary>
        /// The current progress expressed as a percentage
        /// </summary>
        public double Progress { get; }
        
        /// <summary>
        /// The file size being copied
        /// </summary>
        public long TotalBytes { get; }

        /// <summary>
        /// The number of bytes copied
        /// </summary>
        public long BytesCopied { get; }
    }
}
