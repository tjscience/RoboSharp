using System;

namespace RoboSharp.Extensions.CopyFileEx
{
    /// <summary>
    /// Handle the CallBack requested by CopyFileEx
    /// </summary>
    /// <param name="totalFileSize">Total File Size to be copied (bytes)</param>
    /// <param name="totalBytesTransferred">Total number of bytes transfered</param>
    /// <param name="streamSize">The total size of the current file stream, in bytes.</param>
    /// <param name="streamBytesTransferred">The total number of bytes in the current stream that have been transferred from the source file to the destination file since the copy operation began.</param>
    /// <param name="streamID">A handle to the current stream. The first time CopyProgressRoutine is called, the stream number is 1.</param>
    /// <param name="reason"><inheritdoc cref="CopyProgressCallbackReason" path="*"/></param>
    /// <param name="hSourceFile">Handle to the source file - DO Nothing With!</param>
    /// <param name="hDestinationFile">Handle to the destination file - DO Nothing With!</param>
    /// <param name="data">
    /// User-Defined data that will be passed into the callback.
    /// <para/> Example : CopyProgressData progressData = GCHandle.FromIntPtr(lpData).Target as CopyProgressData;
    /// </param>
    /// <returns><inheritdoc cref="CopyProgressCallbackResult"/></returns>
    /// <remarks>
    /// Signature : Func{long, long, long ,long, uint, CopyProgressCallbackReason, IntPtr, IntPtr, IntPtr, CopyProgressCallbackResult}
    /// <br/>
    /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nc-winbase-lpprogress_routine"/>
    /// </remarks>
    public delegate CopyProgressCallbackResult CopyProgressCallback(
        long totalFileSize,
        long totalBytesTransferred,
        long streamSize,
        long streamBytesTransferred,
        uint streamID,
        CopyProgressCallbackReason reason,
        IntPtr hSourceFile,
        IntPtr hDestinationFile,
        IntPtr data);

    /// <summary>Method signature for function to pass into <see cref="RoboSharp.Extensions.CopyFileEx.FileFunctions.CreateCallback(BytesTransferredCallback)"/></summary>
    /// <remarks>
    /// Signature : Func{long, long, CopyProgressCallbackResult}
    /// <br/>
    /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nc-winbase-lpprogress_routine"/>
    /// </remarks>
    /// <inheritdoc cref="CopyProgressCallback"/>
    public delegate CopyProgressCallbackResult BytesTransferredCallback(long totalFileSize, long totalTransferred);

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

    /// <summary>
    /// The result of the callback to be evaluated by CopyFileEx
    /// </summary>
    public enum CopyProgressCallbackResult : uint
    {
        /// <summary>
        /// Continue the copy operation. 
        /// </summary>
        CONTINUE = 0,

        /// <summary>
        /// Cancel the copy operation.
        /// The partially copied destination file is deleted.
        /// </summary>
        CANCEL = 1,

        /// <summary>
        /// Stop the copy operation. It can be restarted at a later time.
        /// The partially copied destination file is left intact.
        /// </summary>
        STOP = 2,

        /// <summary>
        /// Continue the copy operation, but prevent additional callbacks. 
        /// </summary>
        QUIET = 3
    }
}
