using RoboSharp;
using RoboSharp.Extensions;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.CopyFileEx
{
    /// <summary>
    /// Occurs when FileCopy completes copying - Occurrs after CopyProgress 100 occurrs
    /// </summary>
    public class FileCopyCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePair"></param>
        /// <param name="Start"/>
        /// <param name="End"/>
        public FileCopyCompletedEventArgs(IFilePair filePair, DateTime Start, DateTime End)
        {
            if (End < Start) throw new ArgumentException("End Time cannot be less than Start Time", nameof(End));
            if (filePair is null) throw new ArgumentNullException(nameof(filePair));
            FilePair = filePair;    
            StartTime = Start;
            EndTime = End;
            TimeSpan = EndTime - StartTime;
            Speed = new SpeedStatistic(filePair.GetFileLength(), TimeSpan);
        }

        /// <summary>
        /// Source/Destination Info
        /// </summary>
        public IFilePair FilePair { get; }

        /// <summary> </summary>
        public DateTime StartTime { get; }

        /// <summary> </summary>
        public DateTime EndTime { get; }

        /// <summary> </summary>
        public TimeSpan TimeSpan { get; }

        /// <summary>  </summary>
        public SpeedStatistic Speed { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RoboSharp.CopyProgressEventArgs ToRoboSharpCopyProgressEventArgs()
        {
            return new CopyProgressEventArgs(100, FilePair.ProcessResult, FilePair.Parent?.ProcessResult);
        }
    }
}
