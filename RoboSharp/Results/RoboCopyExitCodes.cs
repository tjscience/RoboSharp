using System;

namespace RoboSharp.Results
{
    /// <summary>
    /// RoboCopy Exit Codes
    /// </summary>
    /// <remarks><see href="https://ss64.com/nt/robocopy-exit.html"/></remarks>
    [Flags]
    public enum RoboCopyExitCodes
    {
        /// <summary>No Files Copied, No Errors Occured</summary>
        NoErrorNoCopy = 0x0,
        /// <summary>One or more files were copied successfully</summary>
        FilesCopiedSuccessful = 0x1,
        /// <summary>
        /// Some Extra files or directories were detected.<br/>
        /// Examine the output log for details. 
        /// </summary>
        ExtraFilesOrDirectoriesDetected = 0x2,
        /// <summary>
        /// Some Mismatched files or directories were detected.<br/>
        /// Examine the output log. Housekeeping might be required.
        /// </summary>
        MismatchedDirectoriesDetected = 0x4,
        /// <summary>
        /// Some files or directories could not be copied <br/>
        /// (copy errors occurred and the retry limit was exceeded).
        /// Check these errors further.
        /// </summary>
        SomeFilesOrDirectoriesCouldNotBeCopied = 0x8,
        /// <summary>
        /// Serious error. Robocopy did not copy any files.<br/>
        /// Either a usage error or an error due to insufficient access privileges on the source or destination directorie
        /// </summary>
        SeriousErrorOccoured = 0x10,
        /// <summary>The Robocopy process exited prior to completion</summary>
        Cancelled = -1,
    }
}