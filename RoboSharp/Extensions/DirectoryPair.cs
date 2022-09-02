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
        /// <summary></summary>
        public DirectoryPair(DirectoryInfo source, DirectoryInfo destination)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            Source = source;
            Destination = destination;
        }
        /// <inheritdoc/>
        public DirectoryInfo Source { get; }
        /// <inheritdoc/>
        public DirectoryInfo Destination { get; }

        /// <inheritdoc cref="DirectoryPairExtensions.GetFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T})"/>
        public FilePair[] GetFilePairs()
        {
            return this.GetFilePairs((s, d) => new FilePair(s, d));
        }

        /// <inheritdoc cref="DirectoryPairExtensions.EnumerateFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T})"/>
        public CachedEnumerable<FilePair> EnumerateFilePairs()
        {
            return this.EnumerateFilePairs((s, d) => new FilePair(s, d));
        }


        /// <inheritdoc cref="DirectoryPairExtensions.EnumerateDirectoryPairs{T}(IDirectoryPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public DirectoryPair[] GetDirectoryPairs()
        {
            return this.GetDirectoryPairs((s, d) => new DirectoryPair(s, d));
        }

        /// <inheritdoc cref="DirectoryPairExtensions.EnumerateDirectoryPairs{T}(IDirectoryPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public CachedEnumerable<DirectoryPair> EnumerateDirectoryPairs()
        {
            return this.EnumerateDirectoryPairs((s, d) => new DirectoryPair(s, d));
        }

    }
}
