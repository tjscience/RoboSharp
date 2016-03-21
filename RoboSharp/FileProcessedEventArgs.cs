using System;

namespace RoboSharp
{
    public class FileProcessedEventArgs : EventArgs
    {
        public ProcessedFileInfo ProcessedFile { get; set; }

        public FileProcessedEventArgs(ProcessedFileInfo file)
        {
            ProcessedFile = file;
        }
    }
}
