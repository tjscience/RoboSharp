using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
