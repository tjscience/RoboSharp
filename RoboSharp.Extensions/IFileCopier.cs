using System;
using System.IO;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Interface for objects that can be used within custom IRoboCommands
    /// </summary>
    public interface IFileCopier : IProcessedFilePair
    {
        /// <summary>
        /// Notify of Progress updates
        /// </summary>
        event EventHandler<CopyProgressEventArgs> ProgressUpdated;

        /// <summary>
        /// Cancel the operation
        /// </summary>
        void Cancel();

        /// <summary>
        /// Start a task that copies from the source to the destination
        /// </summary>
        /// <param name="overwrite">Overwrite the destination file if it exists</param>
        /// <returns>True if the copy was successful</returns>
        Task<bool> CopyAsync(bool overwrite = false);

        /// <summary>
        /// Start a task that moves from the source to the destination
        /// </summary>
        /// <param name="overwrite">Overwrite the destination file if it exists</param>
        /// <returns>True if the copy was successful</returns>
        Task<bool> MoveAsync(bool overwrite = false);
        
        /// <summary>
        /// Pause the current operation
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume the paused operation
        /// </summary>
        void Resume();
    }

    /// <summary>
    /// Factory to create <see cref="IFileCopier"/> objects
    /// </summary>
    public interface IFileCopierFactory
    {
        /// <summary>
        /// Create a new copier with the specified options
        /// </summary>
        /// <param name="source">The fully qualified source file path</param>
        /// <param name="destination">The fully qualified destination file path</param>
        /// <returns>A new <see cref="IFileCopier"/></returns>
        public IFileCopier Create(string source, string destination);

        /// <summary>
        /// Create a new copier with the specified options
        /// </summary>
        /// <param name="source">The fully qualified source file path</param>
        /// <param name="destination">The fully qualified destination file path</param>
        /// <returns>A new <see cref="IFileCopier"/></returns>
        public IFileCopier Create(FileInfo source, FileInfo destination);

        /// <summary>
        /// Create a new copier with the specified options
        /// </summary>
        /// <param name="filePair">THe object that provides Source/Destination information</param>
        /// <returns>A new <see cref="IFileCopier"/></returns>
        public IFileCopier Create(IFilePair filePair);
    }
}