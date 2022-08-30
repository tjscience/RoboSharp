using System;

// Do Not change NameSpace here! -> Must be RoboSharp due to prior releases
namespace RoboSharp
{
    /// <summary>
    /// Current File Progress reported as <see cref="double"/>
    /// </summary>
    public class CopyProgressEventArgs : EventArgs
    {
        /// <summary><inheritdoc cref="CopyProgressEventArgs"/></summary>
        /// <param name="progress"><inheritdoc cref="CurrentFileProgress"/></param>
        public CopyProgressEventArgs(double progress)
        {
            CurrentFileProgress = progress;
        }

        /// <summary><inheritdoc cref="CopyProgressEventArgs"/></summary>
        /// <param name="progress"><inheritdoc cref="CurrentFileProgress" path="*"/></param>
        /// <param name="currentFile"><inheritdoc cref="CurrentFile" path="*"/></param>
        /// <param name="dirInfo"><inheritdoc cref="CurrentDirectory" path="*"/></param>
        public CopyProgressEventArgs(double progress, ProcessedFileInfo currentFile, ProcessedFileInfo dirInfo)
        {
            CurrentFileProgress = progress;
            CurrentFile = currentFile;
            CurrentDirectory = dirInfo;
        }

        /// <summary>
        /// Current File Progress Percentage
        /// </summary>
        public double CurrentFileProgress { get; internal set; }

        /// <inheritdoc cref="ProcessedFileInfo"/>
        public ProcessedFileInfo CurrentFile { get; internal set; }

        /// <summary>Contains information about the Last Directory RoboCopy reported into the log. </summary>
        public ProcessedFileInfo CurrentDirectory{ get; internal set; }
    }
}
