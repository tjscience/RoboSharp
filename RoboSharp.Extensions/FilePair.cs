using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Class that implements the <see cref="IFilePair"/> and <see cref="IProcessedFilePair"/> interfaces
    /// </summary>
    public class FilePair : IFilePair, IProcessedFilePair
    {
        /// <summary>
        /// Create a new DirectoryPair object
        /// </summary>
        /// <param name="source">The source FileInfo object</param>
        /// <param name="destination">The Destination FileInfo object</param>
        /// <param name="parent">The Parent Directory Pair - this is allowed to be null.</param>
        /// <exception cref="ArgumentNullException"/>
        public FilePair(FileInfo source, FileInfo destination, IProcessedDirectoryPair parent = null)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
            if (parent is null)
                Parent = new DirectoryPair(source.Directory, destination.Directory);
            else
                Parent = parent;
        }

        /// <summary>
        /// Create a new FilePair object from an existing <see cref="IFilePair"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public FilePair(IFilePair filePair, IProcessedDirectoryPair parent = null)
        {
            if (filePair is null) throw new ArgumentNullException(nameof(filePair));
            Source = filePair.Source ?? throw new ArgumentException("filePair.Source is null");
            Destination = filePair.Destination ?? throw new ArgumentException("filePair.Destination is null");
            if (parent is null)
                Parent = new DirectoryPair(filePair.Source.Directory, filePair.Destination.Directory);
            else
                Parent = parent;
        }

        /// <inheritdoc cref="FilePair(FileInfo, FileInfo, IProcessedDirectoryPair)"/>
        public static FilePair CreatePair(FileInfo source, FileInfo destination, IProcessedDirectoryPair parent = null) => new FilePair(source, destination, parent);

        /// <inheritdoc cref="FilePair(FileInfo, FileInfo, IProcessedDirectoryPair)"/>
        public static FilePair CreatePair(IFilePair filePair, IProcessedDirectoryPair parent = null) => new FilePair(filePair, parent);

        /// <inheritdoc/>
        public bool ShouldCopy { get; set; }

        /// <inheritdoc/>
        public bool ShouldPurge { get; set; }

        /// <inheritdoc/>
        public FileInfo Source { get; }

        /// <inheritdoc/>
        public FileInfo Destination { get; }

        /// <inheritdoc/>
        public ProcessedFileInfo ProcessedFileInfo { get; set; }

        /// <inheritdoc/>
        public IProcessedDirectoryPair Parent { get; }
    }
}
