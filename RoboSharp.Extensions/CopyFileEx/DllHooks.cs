using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.CopyFileEx
{

    /// <summary>
    /// CopyFileEx is a file copy engine that reports copy progress.
    /// </summary>
    ///  Move File with progress: https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-movefilewithprogressa
    public static class DllHooks
    {

        /// <summary>
        /// Copies an existing file to a new file, notifying the application of its progress through a callback function
        /// </summary>
        /// <param name="lpExistingFileName">Source FilePath </param>
        /// <param name="lpNewFileName">Destination File Path</param>
        /// <param name="lpProgressRoutine">Progress Reporter Call-Back</param>
        /// <param name="lpData">The argument to be passed to the callback function. This parameter can be NULL.</param>
        /// <param name="pbCancel">Boolean to trigger cancellation</param>
        /// <param name="dwCopyFlags">Copy Flags</param>
        /// <returns>TRUE if the copy operation completed</returns>
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CopyFileEx(
            string lpExistingFileName, 
            string lpNewFileName, 
            ProgressCallback lpProgressRoutine,
            object lpData,
            ref bool pbCancel, 
            CopyFileEx.CopyFileExFlags dwCopyFlags);

        /// <summary>
        /// Moves a file or directory, including its children. You can provide a callback function that receives progress notifications.
        /// </summary>
        /// <param name="lpExistingFileName">The name of the existing file or directory on the local computer.</param>
        /// <param name="lpNewFileName">The new name of the file or directory on the local computer.</param>
        /// <param name="lpProgressRoutine">A pointer to a CopyProgressRoutine callback function that is called each time another portion of the file has been moved. The callback function can be useful if you provide a user interface that displays the progress of the operation. This parameter can be NULL.</param>
        /// <param name="lpData">An argument to be passed to the CopyProgressRoutine callback function. This parameter can be NULL.</param>
        /// <param name="dwFlags">The move options. </param>
        /// <returns>If the function succeeds, the return value is nonzero.
        ///  If the function fails, the return value is zero.To get extended error information, call GetLastError. 
        ///  <para/>When moving a file across volumes, if lpProgressRoutine returns PROGRESS_CANCEL due to the user canceling the operation, MoveFileWithProgress will return zero and GetLastError will return ERROR_REQUEST_ABORTED.The existing file is left intact.
        ///  <para/>When moving a file across volumes, if lpProgressRoutine returns PROGRESS_STOP due to the user stopping the operation, MoveFileWithProgress will return zero and GetLastError will return ERROR_REQUEST_ABORTED.The existing file is left intact.
        ///  </returns>
        /// <remarks><see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-movefilewithprogressa"/></remarks>
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool MoveFileWithProgressA(
            string lpExistingFileName, 
            string lpNewFileName, 
            ProgressCallback lpProgressRoutine = null,
            object lpData = null,
            MoveFileFlags dwFlags = MoveFileFlags.Default
            );


        /// <summary>
        /// Handle the CallBack requested by CopyFileEx
        /// </summary>
        /// <param name="TotalFileSize">Total File Size to be copied (bytes)</param>
        /// <param name="TotalBytesTransferred">Total number of bytes transfered</param>
        /// <param name="StreamSize">The total size of the current file stream, in bytes.</param>
        /// <param name="StreamBytesTransferred">The total number of bytes in the current stream that have been transferred from the source file to the destination file since the copy operation began.</param>
        /// <param name="dwStreamNumber">A handle to the current stream. The first time CopyProgressRoutine is called, the stream number is 1.</param>
        /// <param name="dwCallbackReason"><inheritdoc cref="CopyProgressCallbackReason"/></param>
        /// <param name="hSourceFile">pointer to the source file - DO Nothing With!</param>
        /// <param name="hDestinationFile">pointer to the destination file - DO Nothing With!</param>
        /// <param name="lpData">This is the argument passed into the function via the lpData parameter</param>
        /// <returns><inheritdoc cref="CopyProgressCallbackResult"/></returns>
        /// <remarks><see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nc-winbase-lpprogress_routine"/></remarks>
        public delegate CopyProgressCallbackResult ProgressCallback(
            long TotalFileSize, 
            long TotalBytesTransferred, 
            long StreamSize, 
            long StreamBytesTransferred, 
            uint dwStreamNumber, 
            CopyProgressCallbackReason dwCallbackReason,
            IntPtr hSourceFile, 
            IntPtr hDestinationFile, 
            object lpData);

        /// <summary>
        /// Default Handler just always states to continue copying
        /// </summary>
        private static CopyProgressCallbackResult DefaultHandler(long total, long transferred, long streamSize, long streamByteTrans, uint dwStreamNumber,
                                                CopyProgressCallbackReason reason, IntPtr hSourceFile, IntPtr hDestinationFile, object lpData) => CopyProgressCallbackResult.PROGRESS_CONTINUE;


        /// <inheritdoc cref="CopyFileEx"/>
        public static bool CopyFile(string lpExistingFileName, string lpNewFileName, ProgressCallback lpProgressRoutine, ref bool pbCancel, object lpData = null, CopyFileExFlags dwCopyFlags = CopyFileExFlags.COPY_FILE_RESTARTABLE)
        {
            //Check for locked file prior to starting the write process
            using (var stream = File.OpenWrite(lpNewFileName))
                stream.Close();
            bool returnVal = CopyFileEx(lpExistingFileName, lpNewFileName, lpProgressRoutine ?? DefaultHandler, lpData ?? IntPtr.Zero, ref pbCancel, dwCopyFlags);
            if (!returnVal) ThrowWin32Error(GetLastWinError(), lpExistingFileName, lpNewFileName);
            return returnVal;
        }

        /// <inheritdoc cref="MoveFileWithProgressA(string, string, ProgressCallback, object, MoveFileFlags)"/>
        public static bool MoveFile(string lpExistingFileName, string lpNewFileName, ProgressCallback lpProgressRoutine = null, object lpData = null, MoveFileFlags dwFlags = MoveFileFlags.Default)
        {
            //Check for locked file prior to starting the write process
            using (var stream = File.OpenWrite(lpNewFileName))
                stream.Close();
            bool returnVal = MoveFileWithProgressA(lpExistingFileName, lpNewFileName, lpProgressRoutine ?? DefaultHandler, lpData ?? IntPtr.Zero, dwFlags);
            if (!returnVal) ThrowWin32Error(GetLastWinError(), lpExistingFileName, lpNewFileName);
            return returnVal;
        }

#if Net6OrGreater
        /// <inheritdoc cref="Marshal.GetLastPInvokeError"/>
        public static int GetLastWinError() => Marshal.GetLastPInvokeError();
#else
        /// <inheritdoc cref="Marshal.GetLastWin32Error"/>
        public static int GetLastWinError() =>Marshal.GetLastWin32Error();
#endif

        ///<summary>
        /// Look up the Win32 error code and throw the appropriate exception. If not defined in the statement, throw a generic exception with the fault code.
        /// </summary>
        /// <remarks>
        /// <see href="https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/18d8fbe8-a967-4f1c-ae50-99ca8e491d2d"/>
        /// </remarks>
        public static void ThrowWin32Error(int errorCode, string sourceFile, string destFile)
        {
            
            switch (errorCode)
            {
                case 0x00000050: //File already exists (occurs if Prevent file copy is exists flag is set)
                case 0x000004D3: // Cancelled
                case 0: return;
                case 1: throw new InvalidOperationException("Invalid Operation");
                case 2: throw new FileNotFoundException(message: "Unable to locate the file", fileName: sourceFile);
                case 3: throw new DirectoryNotFoundException("Unable to locate the directory: " + Path.GetDirectoryName(destFile));
                case 4: throw new FileNotFoundException("Unable to open the file: ", sourceFile);
                case 8: throw new InsufficientMemoryException();
                case 0x0000000E: throw new InsufficientMemoryException("Not enough storage is available to complete this operation.");
                case 0x0000000F: throw new DriveNotFoundException("The system cannot find the drive specified.");
                case 0x00000013: throw new UnauthorizedAccessException("The media is write-protected.");
                case 0x00000014: throw new DriveNotFoundException("The system cannot find the device specified.");
                case 0x00000015: throw new IOException("The device is not ready.");
                case 0x00000021: throw new IOException("The process cannot access the file because another process has locked a portion of the file.");
                case 0x00000027: throw new IOException("The destination disk is full: " + Path.GetPathRoot(destFile));
                case 0x00000032: throw new IOException(); // Occurs when the file is locked
                //case 0x00000033: throw new IOException("Windows cannot find the network path. Verify that the network path is correct and the destination computer is not busy or turned off. If Windows still cannot find the network path, contact your network administrator.");
                default: throw new IOException($@"CopyFileEx Error Code: {errorCode}{Environment.NewLine} See https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/18d8fbe8-a967-4f1c-ae50-99ca8e491d2d for details");
            };
    }
    }
}
