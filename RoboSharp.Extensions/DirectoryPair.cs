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
            RefreshLazy();
        }

        private Lazy<CachedEnumerable<DirectoryPair>> lazyExtraDirs;
        private Lazy<CachedEnumerable<DirectoryPair>> lazySourceDirs;
        private Lazy<CachedEnumerable<FilePair>> lazySourceFiles;
        private Lazy<CachedEnumerable<FilePair>> lazyExtraFiles;

        /// <inheritdoc cref="DirectoryPair(DirectoryInfo, DirectoryInfo)"/>
        public static DirectoryPair CreatePair(DirectoryInfo source, DirectoryInfo destination) => new DirectoryPair(source, destination);

        /// <inheritdoc/>
        public DirectoryInfo Source { get; }

        /// <inheritdoc/>
        public DirectoryInfo Destination { get; }

        /// <inheritdoc/>
        public ProcessedFileInfo ProcessResult { get; set; }

        /// <summary>
        /// The collection of <see cref="FilePair"/>s generated from scanning the <see cref="Destination"/> directory where <see cref="IFilePairExtensions.IsExtra(IFilePair)"/> returns true.
        /// </summary>
        /// <remarks>Refresh this via <see cref="Refresh"/></remarks>
        public CachedEnumerable<FilePair> ExtraFiles => lazyExtraFiles.Value;

        /// <summary>
        /// The collection of <see cref="IDirectoryPair"/>s generated from scanning the <see cref="Destination"/> directory where <see cref="IDirectoryPairExtensions.IsExtra(IDirectoryPair)"/> returns true.
        /// </summary>
        /// <remarks>Refresh this via <see cref="Refresh"/></remarks>
        public CachedEnumerable<DirectoryPair> ExtraDirectories => lazyExtraDirs.Value;

        /// <summary>
        /// The collection of <see cref="IFilePair"/>s generated from scanning the <see cref="Source"/> directory.
        /// </summary>
        /// <remarks>Refresh this via <see cref="Refresh"/></remarks>
        public CachedEnumerable<FilePair> SourceFiles => lazySourceFiles.Value;

        /// <summary>
        /// The collection of <see cref="IDirectoryPair"/>s generated from scanning the <see cref="Source"/> directory.
        /// </summary>
        /// <remarks>Refresh this via <see cref="Refresh"/></remarks>
        public CachedEnumerable<DirectoryPair> SourceDirectories => lazySourceDirs.Value;

        /// <inheritdoc cref="FileSystemInfo.Refresh"/>
        public void Refresh()
        {
            Source.Refresh();
            Destination.Refresh();
            RefreshLazy();
        }

        private void RefreshLazy()
        {
            lazyExtraDirs = new Lazy<CachedEnumerable<DirectoryPair>>(GetExtraDirectories);
            lazySourceDirs = new Lazy<CachedEnumerable<DirectoryPair>>(GetSourceDirectories);
            lazyExtraFiles = new Lazy<CachedEnumerable<FilePair>>(GetExtraFiles);
            lazySourceFiles = new Lazy<CachedEnumerable<FilePair>>(GetSourceFiles);
        }

        private CachedEnumerable<FilePair> GetExtraFiles()
        {
            return this.EnumerateDestinationFilePairs(FilePair.CreatePair).Where(IFilePairExtensions.IsExtra).AsCachedEnumerable();
        }

        private CachedEnumerable<DirectoryPair> GetExtraDirectories()
        {
            return this.EnumerateDestinationDirectoryPairs(DirectoryPair.CreatePair).Where(IDirectoryPairExtensions.IsExtra).AsCachedEnumerable();
        }

        private CachedEnumerable<FilePair> GetSourceFiles()
        {
            return this.EnumerateSourceFilePairs(FilePair.CreatePair);
        }

        private CachedEnumerable<DirectoryPair> GetSourceDirectories()
        {
            return this.EnumerateSourceDirectoryPairs(DirectoryPair.CreatePair);
        }
    }
}
