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

        internal ProgressUpdateEventArgs(Results.ResultsBuilder resultsBuilder) {
            Results.RoboCopyExitCodes code = 0;
            
            //Files Copied
            if (resultsBuilder.TotalFiles_Copied > 0) 
                code |= Results.RoboCopyExitCodes.FilesCopiedSuccessful;
            
            //Extra
            if (resultsBuilder.TotalDirs_Extras > 0 | resultsBuilder.TotalFiles_Extras > 0) 
                code |= Results.RoboCopyExitCodes.ExtraFilesOrDirectoriesDetected;
            
            //MisMatch
            if (resultsBuilder.TotalDirs_MisMatch > 0 | resultsBuilder.TotalFiles_Mismatch > 0) 
                code |= Results.RoboCopyExitCodes.MismatchedDirectoriesDetected;
            
            //Failed
            if (resultsBuilder.TotalFiles_Failed > 0) 
                code |= Results.RoboCopyExitCodes.SomeFilesOrDirectoriesCouldNotBeCopied;
            
            //Build Results Object
            ResultsEstimate = resultsBuilder.BuildResults((int)code, true);
        }

        /// <summary>
        /// Current resultsBuilder based on parsed log lines. Note: The Job is likely still running, so ExitStatus is preliminary.
        /// </summary>
        public Results.RoboCopyResults ResultsEstimate { get; }
        
    }
}

