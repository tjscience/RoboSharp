using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp
{
    /// <summary>
    /// Reports estimated progress based on the log lines reported during the run
    /// </summary>
    public class ProgressUpdateEventArgs
    {
        private ProgressUpdateEventArgs() { }

        internal ProgressUpdateEventArgs(Results.ResultsBuilder results) {
            Results.RoboCopyExitCodes code = 0;
            if (results.TotalFiles_Copied > 0) code = code | Results.RoboCopyExitCodes.FilesCopiedSuccessful;
            if (results.TotalDirs_Extras > 0 | results.TotalFiles_Extras > 0) code = code | Results.RoboCopyExitCodes.ExtraFilesOrDirectoriesDetected;
            if (results.TotalDirs_MisMatch > 0 | results.TotalFiles_Mismatch > 0) code = code | Results.RoboCopyExitCodes.MismatchedDirectoriesDetected;
            if (results.TotalFiles_Failed > 0) code = code | Results.RoboCopyExitCodes.SomeFilesOrDirectoriesCouldNotBeCopied;
            ResultsEstimate = results.BuildResults((int)code);
        }

        /// <summary>
        /// Current results based on parsed log lines. Note: The Job is likely still running, so ExitStatus is preliminary.
        /// </summary>
        public Results.RoboCopyResults ResultsEstimate { get; }
        
    }
}

