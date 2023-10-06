using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.CopyFileEx
{
    /// <summary>
    /// The result of the callback to be evaluated by CopyFileEx
    /// </summary>
    public enum CopyProgressCallbackResult: uint
    {
        /// <summary>
        /// Continue the copy operation. 
        /// </summary>
        PROGRESS_CONTINUE = 0,

        /// <summary>
        /// Cancel the copy operation and delete the destination file. 
        /// </summary>
        PROGRESS_CANCEL = 1,

        /// <summary>
        /// Stop the copy operation. It can be restarted at a later time. 
        /// </summary>
        PROGRESS_STOP = 2,

        /// <summary>
        /// Continue the copy operation, but stop invoking CopyProgressRoutine to report progress. 
        /// </summary>
        PROGRESS_QUIET = 3
    }

    /// <summary>
    /// Event description from CopyFileEx (why its performing the callback)
    /// </summary>
    public enum CopyProgressCallbackReason : uint
    {
        /// <summary>
        /// Copy Progress Updated
        /// </summary>
        CALLBACK_CHUNK_FINISHED = 0x00000000,

        /// <summary>
        /// Another stream was created and is about to be copied. This is the callback reason given when the callback routine is first invoked. 
        /// </summary>
        CALLBACK_STREAM_SWITCH = 0x00000001
    }
}
