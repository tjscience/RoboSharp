using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RoboSharp.Extensions.Helpers;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Helper Class that implements the <see cref="IDirectoryPair"/> interface
    /// </summary>
    public sealed class DirectoryPair : IDirectoryPair
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

        /// <summary>
        /// The collection of <see cref="IFilePair"/>s generated from scanning the <see cref="Source"/> directory.
        /// </summary>
        /// <remarks>Refresh this via <see cref="RefreshSourcePairs"/></remarks>
        public CachedEnumerable<FilePair> SourceFiles => lazySourceFiles.Value;

        /// <summary>
        /// The collection of <see cref="IDirectoryPair"/>s generated from scanning the <see cref="Source"/> directory.
        /// </summary>
        /// <remarks>Refresh this via <see cref="RefreshSourcePairs"/></remarks>
        public CachedEnumerable<DirectoryPair> SourceDirectories => lazySourceDirs.Value;

        /// <inheritdoc/>
        public ProcessedFileInfo ProcessResult { get; set; }

        /// <summary>
        /// Refreshes the <see cref="SourceFiles"/> and <see cref="SourceDirectories"/> cached enumerables
        /// </summary>
        public void RefreshSourcePairs()
        {
            RefreshDirs();
            RefreshFiles();
        }
        private void RefreshDirs() => lazySourceDirs = new Lazy<CachedEnumerable<DirectoryPair>>(() => this.EnumerateSourceDirectoryPairs(DirectoryPair.CreatePair));
        private void RefreshFiles() => lazySourceFiles = new Lazy<CachedEnumerable<FilePair>>(() => this.EnumerateSourceFilePairs(FilePair.CreatePair));

        /// <inheritdoc cref="IDirectoryPairExtensions.GetFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T})"/>
        public FilePair[] GetFilePairs()
        {
            return EnumerateFilePairs().ToArray();
        }

        /// <inheritdoc cref="IDirectoryPairExtensions.EnumerateFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T})"/>
        public CachedEnumerable<FilePair> EnumerateFilePairs()
        {
            RefreshFiles();
            var destPairs = this.EnumerateDestinationFilePairs(FilePair.CreatePair);
            if (destPairs is null && SourceFiles is null)
                return null;
            else if (destPairs is null)
                return SourceFiles;
            else
                return destPairs.Concat(SourceFiles).WhereUnique(Helpers.PairEqualityComparer.Singleton).AsCachedEnumerable();
        }


        /// <inheritdoc cref="IDirectoryPairExtensions.GetDirectoryPairs{T}(IDirectoryPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public DirectoryPair[] GetDirectoryPairs()
        {
            return EnumerateDirectoryPairs().ToArray();
        }

        /// <inheritdoc cref="IDirectoryPairExtensions.EnumerateDirectoryPairs{T}(IDirectoryPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public CachedEnumerable<DirectoryPair> EnumerateDirectoryPairs()
        {
            RefreshDirs();
            var destPairs = this.EnumerateDestinationDirectoryPairs(DirectoryPair.CreatePair);
            if (destPairs is null && SourceDirectories is null)
                return null;
            else if (destPairs is null)
                return SourceDirectories;
            else
                return destPairs.Concat(SourceDirectories).WhereUnique(Helpers.PairEqualityComparer.Singleton).AsCachedEnumerable();
        }

    }
}
