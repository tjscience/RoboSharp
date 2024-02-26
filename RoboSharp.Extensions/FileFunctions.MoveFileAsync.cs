using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RoboSharp.Extensions.CopyFileEx;

namespace RoboSharp.Extensions
{
    public static partial class FileFunctions
    {
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
        /// </returns>
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-movefilewithprogressa"/>
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveFileWithProgressA(
            string lpExistingFileName,
            string lpNewFileName,
            CopyProgressCallback lpProgressRoutine = null,
            object lpData = null,
            MoveFileOptions dwFlags = MoveFileOptions.Default
            );

        /// <summary>
        /// Moves a file using the MoveFileWithProgressA function directly.
        /// </summary>
        /// <param name="source">The path to the source file.</param>
        /// <param name="destination">The path to the destination file.</param>
        /// <param name="progressCallback">Callback function for progress notifications during the copy operation.
        ///  <para/>When moving a file across volumes, if <paramref name="progressCallback"/> returns <see cref="CopyProgressCallbackResult.CANCEL"/> : throw <see cref="OperationCanceledException"/>. The existing file is left intact.
        ///  <para/>When moving a file across volumes, if <paramref name="progressCallback"/> returns <see cref="CopyProgressCallbackResult.STOP"/> : throw <see cref="OperationCanceledException"/>. The existing file is left intact.
        /// </param>
        /// <param name="data">User-defined data passed to the progress callback.</param>
        /// <param name="flags">Flags specifying how the file should be moved.</param>
        /// <returns>If the function succeeds, the return value is nonzero. </returns>
        /// <inheritdoc cref="MoveFileWithProgressA(string, string, CopyProgressCallback, object, MoveFileOptions)"/>
        /// <exception cref="OperationCanceledException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="Exception"/>
        public static bool MoveFile(
            string source,
            string destination,
            CopyProgressCallback progressCallback = null,
            IntPtr data = default(IntPtr),
            MoveFileOptions flags = MoveFileOptions.Default
            )
        {
            //Check for locked file prior to starting the write process
            if (!File.Exists(source)) throw new FileNotFoundException("Source File Not Found.", source);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            bool returnVal = MoveFileWithProgressA(source, destination, progressCallback, data, flags);
            if (!returnVal) Win32Error.ThrowLastError(source, destination);
            return returnVal;
        }

        /// <summary>
        /// Executes the MoveFile function via Task.Run()
        /// </summary>
        /// <inheritdoc cref="MoveFile(string, string, CopyProgressCallback, IntPtr, MoveFileOptions)"/>
        public static Task<bool> MoveFileAsync(string source,
            string destination,
            CopyProgressCallback progressCallback = null,
            IntPtr data = default(IntPtr),
            MoveFileOptions flags = MoveFileOptions.Default,
            CancellationToken token = default
            )
        {
            if (token.CanBeCanceled)
            {
                progressCallback = CreateCallback(progressCallback, token);
            }
            return Task.Run(() => MoveFile(source, destination, progressCallback, data, flags), token);
        }

        /// <inheritdoc cref="MoveFileAsync(string, string, bool, CancellationToken)"/>
        public static Task<bool> MoveFileAsync(string source, string destination)
            => MoveFileAsync(source, destination, false, CancellationToken.None);

        /// <inheritdoc cref="MoveFileAsync(string, string, bool, CancellationToken)"/>
        public static Task<bool> MoveFileAsync(string source, string destination, CancellationToken token)
            => MoveFileAsync(source, destination, false, token);

        /// <summary> Copy the file asynchronously. If the operation is successfull, delete the source file. </summary>
        /// <inheritdoc cref="CopyFileAsync(string, string, bool, CancellationToken)"/>
        public static async Task<bool> MoveFileAsync(string source, string destination, bool overwrite, CancellationToken token = default)
        {
            MoveFileOptions options = MoveFileOptions.COPY_ALLOWED | MoveFileOptions.WRITE_THROUGH;
            if (overwrite) options |= MoveFileOptions.REPLACE_EXISTSING;
            return await MoveFileAsync(source, destination, flags: options, token: token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="MoveFileProgressAsync"/>
        public static async Task<bool> MoveFileAsync(string source, string destination, IProgress<ProgressUpdate> progress, int updateInterval = 100, bool overwrite = false, CancellationToken token = default)
        {
            return await MoveFileProgressAsync(source, destination, overwrite, updateInterval, progress: progress, token: token);
        }

        /// <inheritdoc cref="MoveFileProgressAsync"/>
        public static async Task<bool> MoveFileAsync(string source, string destination, IProgress<long> progress, int updateInterval = 100, bool overwrite = false, CancellationToken token = default)
        {
            return await MoveFileProgressAsync(source, destination, overwrite, updateInterval, sizeProgress: progress, token: token);
        }

        /// <inheritdoc cref="MoveFileProgressAsync"/>
        public static async Task<bool> MoveFileAsync(string source, string destination, IProgress<double> progress, int updateInterval = 100, bool overwrite = false, CancellationToken token = default)
        {
            return await MoveFileProgressAsync(source, destination, overwrite, updateInterval, percentProgress: progress, token: token);
        }

        /// <summary>
        /// Move the file asynchronously with a progress reporter. If the operation is successfull, delete the soure file.
        /// </summary>
        /// <param name="progress">An IProgress object that will accept a progress notification</param>
        /// <param name="updateInterval">Time interval in milliseconds to update the <paramref name="progress"/> object</param>
        /// <returns>A task that returns when the operation has completed successfully or has been cancelled.</returns>
        /// <inheritdoc cref="CopyFileAsync(string, string, bool, CancellationToken)"/>
        /// <param name="source"/><param name="destination"/><param name="overwrite"/><param name="token"/>
        /// <param name="percentProgress"/><param name="sizeProgress"/>
        internal static async Task<bool> MoveFileProgressAsync(
            string source, string destination, bool overwrite,
            int updateInterval = 100,
            IProgress<ProgressUpdate> progress = null,
            IProgress<long> sizeProgress = null,
            IProgress<double> percentProgress = null,
            CancellationToken token = default)
        {
            FileInfo sourceFile = new FileInfo(source);
            if (!sourceFile.Exists) throw new FileNotFoundException("Source file does not exist", source);
            if (!overwrite && File.Exists(destination)) throw new IOException("The destination already file exists");
            token.ThrowIfCancellationRequested();
            bool result = false;

            // Updater
            Task updateTask = null;
            long fileSize = 0;
            long totalBytesRead = 0;
            updateInterval = updateInterval > 25 ? updateInterval : 100;
            var updateToken = CancellationTokenSource.CreateLinkedTokenSource(token);
            if (sourceFile.Length > 0 && (progress != null | sizeProgress != null | percentProgress != null))
            {
                updateTask = Task.Run(async () =>
                {
                    while (totalBytesRead < sourceFile.Length)
                    {
                        Report();
                        await Task.Delay(updateInterval, updateToken.Token);
                        updateToken.Token.ThrowIfCancellationRequested();
                    }
                }, updateToken.Token);
            }

            try
            {
                var callback = CreateCallback(ProgressHandler, token);
                MoveFileOptions options = MoveFileOptions.COPY_ALLOWED | MoveFileOptions.WRITE_THROUGH;
                if (overwrite) options |= MoveFileOptions.REPLACE_EXISTSING;
                result = await MoveFileAsync(source, destination, callback, flags: options, token: token).ConfigureAwait(false);
            }
            finally
            {
                updateToken.Cancel();
                await updateTask.ConfigureAwait(false);
            }
            Report();
            return result;

            void Report()
            {
                progress?.Report(new ProgressUpdate(sourceFile.Length, totalBytesRead));
                sizeProgress?.Report(totalBytesRead);
                percentProgress?.Report((double)100 * totalBytesRead / sourceFile.Length);
            }

            void ProgressHandler(long size, long copied)
            {
                fileSize = size;
                totalBytesRead = copied;
            }
        }

    }
}
