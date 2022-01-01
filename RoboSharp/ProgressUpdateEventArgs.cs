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
            if (resultsBuilder.Estimator.FileStats.Copied > 0) 
                code |= Results.RoboCopyExitCodes.FilesCopiedSuccessful;
            
            //Extra
            if (resultsBuilder.Estimator.DirStats.Extras > 0 | resultsBuilder.Estimator.FileStats.Extras > 0) 
                code |= Results.RoboCopyExitCodes.ExtraFilesOrDirectoriesDetected;
            
            //MisMatch
            if (resultsBuilder.Estimator.DirStats.Mismatch > 0 | resultsBuilder.Estimator.FileStats.Mismatch > 0) 
                code |= Results.RoboCopyExitCodes.MismatchedDirectoriesDetected;
            
            //Failed
            if (resultsBuilder.Estimator.DirStats.Failed > 0 | resultsBuilder.Estimator.FileStats.Failed > 0) 
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

