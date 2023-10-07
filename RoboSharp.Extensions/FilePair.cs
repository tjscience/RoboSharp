using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Helper Class that implements the <see cref="IFilePair"/> interface
    /// </summary>
    public class FilePair : IFilePair
    {
        /// <summary>
        /// Create a new DirectoryPair object
        /// </summary>
        /// <param name="source">The source FileInfo object</param>
        /// <param name="destination">The Destination FileInfo object</param>
        /// <param name="parent">The Parent Directory Pair - this is allowed to be null.</param>
        /// <exception cref="ArgumentNullException"/>
        public FilePair(FileInfo source, FileInfo destination, IDirectoryPair parent)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
            if (parent is null)
                Parent = new DirectoryPair(source.Directory, destination.Directory);
            else
                Parent = parent;
        }

        /// <inheritdoc cref="FilePair(FileInfo, FileInfo, IDirectoryPair)"/>
        public static FilePair CreatePair(FileInfo source, FileInfo destination, IDirectoryPair parent) => new FilePair(source, destination, parent);

        /// <summary>
        /// Stores the result of <see cref="PairEvaluator.ShouldCopyFile(IFilePair)"/>
        /// </summary>
        public bool ShouldCopy { get; internal set; }

        /// <summary>
        /// Stores the result of <see cref="PairEvaluator.ShouldPurge(IFilePair)"/> if this object has been run through that method.
        /// </summary>
        public bool ShouldPurge { get; internal set; }

        /// <inheritdoc/>
        public FileInfo Source { get; }

        /// <inheritdoc/>
        public FileInfo Destination { get; }

        /// <inheritdoc/>
        public ProcessedFileInfo ProcessedFileInfo { get; set; }

        /// <inheritdoc/>
        public IDirectoryPair Parent { get; }
    }
}
