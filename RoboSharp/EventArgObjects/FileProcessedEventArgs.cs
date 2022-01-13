using System;

// Do Not change NameSpace here! -> Must be RoboSharp due to prior releases
namespace RoboSharp
{
    /// <summary>
    /// <inheritdoc cref="ProcessedFileInfo"/>
    /// </summary>
    public class FileProcessedEventArgs : EventArgs
    {
        /// <inheritdoc cref="ProcessedFileInfo"/>
        public ProcessedFileInfo ProcessedFile { get; set; }

        /// <inheritdoc cref="EventArgs.EventArgs"/>
        public FileProcessedEventArgs(ProcessedFileInfo file)
        {
            ProcessedFile = file;
        }
    }
}
