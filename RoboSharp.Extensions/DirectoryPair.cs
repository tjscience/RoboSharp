using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Helper Class that implements the <see cref="IDirectoryPair"/> interface
    /// </summary>
    public class DirectoryPair : IDirectoryPair
    {
        /// <summary>
        /// Create a new DirectoryPair object
        /// </summary>
        /// <param name="source">The Source DirectoryInfo object</param>
        /// <param name="destination">The Destination DirectoryInfo object</param>
        /// <exception cref="ArgumentNullException"/>
        public DirectoryPair(DirectoryInfo source, DirectoryInfo destination)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
            RefreshSourcePairs();
        }

        private Lazy<CachedEnumerable<DirectoryPair>> lazySourceDirs;
        private Lazy<CachedEnumerable<FilePair>> lazySourceFiles;

        /// <inheritdoc cref="DirectoryPair(DirectoryInfo, DirectoryInfo)"/>
        public static DirectoryPair CreatePair(DirectoryInfo source, DirectoryInfo destination) => new DirectoryPair(source, destination);

        /// <inheritdoc/>
        public DirectoryInfo Source { get; }

        /// <inheritdoc/>
        public DirectoryInfo Destination { get; }

        public CachedEnumerable<FilePair> SourceFiles => lazySourceFiles.Value;

        public CachedEnumerable<DirectoryPair> SourceDirectories => lazySourceDirs.Value;

        /// <inheritdoc/>
        public ProcessedFileInfo ProcessResult { get; set; }

        /// <summary>
        /// Refreshes the <see cref="SourceFiles"/> and <see cref="SourceDirectories"/> cached enumerables
        /// </summary>
        public void RefreshSourcePairs()
        {
            lazySourceDirs = new Lazy<CachedEnumerable<DirectoryPair>>(() => this.EnumerateSourceDirectoryPairs(DirectoryPair.CreatePair));
            lazySourceFiles = new Lazy<CachedEnumerable<FilePair>>(() => this.EnumerateSourceFilePairs(FilePair.CreatePair));
        }

        /// <inheritdoc cref="IDirectoryPairExtensions.GetFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T})"/>
        public FilePair[] GetFilePairs()
        {
            return this.GetFilePairs(FilePair.CreatePair);
        }

        /// <inheritdoc cref="IDirectoryPairExtensions.EnumerateFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T})"/>
        public CachedEnumerable<FilePair> EnumerateFilePairs()
        {
            return this.EnumerateFilePairs(FilePair.CreatePair);
        }


        /// <inheritdoc cref="IDirectoryPairExtensions.GetDirectoryPairs{T}(IDirectoryPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public DirectoryPair[] GetDirectoryPairs()
        {
            return this.GetDirectoryPairs(CreatePair);
        }

        /// <inheritdoc cref="IDirectoryPairExtensions.EnumerateDirectoryPairs{T}(IDirectoryPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public CachedEnumerable<DirectoryPair> EnumerateDirectoryPairs()
        {
            return this.EnumerateDirectoryPairs(CreatePair);
        }

    }
}
