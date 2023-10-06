using RoboSharp;
using RoboSharp.Extensions;
using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions.CopyFileEx
{
    /// <summary>
    /// Interface for objects that allow copying/moving with events for success or failure.
    /// </summary>
    public interface IFileCopier : IFilePair, IDisposable
    {

        /// <summary>
        /// Occurs when the copy progress is updated
        /// </summary>
        event EventHandler<RoboSharp.CopyProgressEventArgs> CopyProgressUpdated;

        /// <summary>
        /// Occurs when a file copy/move is successfully completed
        /// </summary>
        event EventHandler<FileCopyCompletedEventArgs> CopyCompleted;

        /// <summary>
        /// Occurs when a file copy/move operation fails. May contain exception information.
        /// </summary>
        event EventHandler<FileCopyFailedEventArgs> CopyFailed;

        /// <summary>
        /// TRUE if the task that performs the copy is currently running, otherwise false.
        /// </summary>
        bool IsCopying { get; }

        /// <summary>
        /// TRUE if the copy operation was cancelled
        /// </summary>
        bool WasCancelled { get; }

        /// <summary>
        /// The current Progress of the copy operation
        /// </summary>
        double Progress { get; }

        /// <summary>
        /// Copy the File(s) to their destination
        /// </summary>
        /// <param name="overWrite">OverWrite the files in the destination if they already exist. Set false if overwriting existing files should not occur.</param>
        /// <returns>
        /// Task that completes when the operation is either successfull, or fails. <br/>
        /// A TRUE result means copy operation performed successfully, FALSE means file was not copied.
        /// </returns>
        Task<bool> Copy(bool overWrite = false);

        /// <summary>
        /// Move the File(s) to their destination
        /// </summary>
        /// <param name="overWrite"><inheritdoc cref="Copy(bool)" path="/param[@name='overWrite']" /></param>
        /// <returns>
        /// Task that completes when the operation is either successfull, or fails. <br/>
        /// A TRUE result means move operation performed successfully, FALSE means file was not moved.
        /// </returns>
        Task<bool> Move(bool overWrite = false);

        /// <summary>
        /// Cancel the Copy/Move Operation
        /// </summary>
        void Cancel();

    }
}
