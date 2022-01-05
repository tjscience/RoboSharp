using System;

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
        /// <param name="progress"><inheritdoc cref="CurrentFileProgress"/></param>
        /// <param name="currentFile"><inheritdoc cref="CurrentFile"/></param>
        /// <param name="SourceDir"><inheritdoc cref="CurrentDirectory"/></param>
        public CopyProgressEventArgs(double progress, ProcessedFileInfo currentFile, ProcessedFileInfo SourceDir)
        {
            CurrentFileProgress = progress;
            CurrentFile = currentFile;
            CurrentDirectory = SourceDir;
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
