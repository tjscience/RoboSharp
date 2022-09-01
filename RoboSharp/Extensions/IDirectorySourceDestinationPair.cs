using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    

    /// <summary>
    /// Interface used for extension methods for RoboSharp custom implementations that has File Source/Destination info
    /// </summary>
    /// <remarks>
    /// This interface should only include the Source and Destination information, and not any methods to implement copying/moving
    /// </remarks>
    public interface IDirectorySourceDestinationPair
    {
        /// <summary>
        /// Source File Information
        /// </summary>
        public DirectoryInfo Source { get; }

        /// <summary>
        /// Destination FIle Information
        /// </summary>
        public DirectoryInfo Destination { get; }
    }

    /// <summary>
    /// Helper Class that implements the <see cref="IDirectorySourceDestinationPair"/> interface
    /// </summary>
    public class DirectorySourceDestinationPair : IDirectorySourceDestinationPair
    {
        /// <summary></summary>
        public DirectorySourceDestinationPair(DirectoryInfo source, DirectoryInfo destination)
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

        /// <inheritdoc cref="ISourceDestinationPairExtensions.GetFilePairs{T}(IDirectorySourceDestinationPair, Func{FileInfo, FileInfo, T})"/>
        public FileSourceDestinationPair[] GetFilePairs()
        {
            return this.GetFilePairs((s, d) => new FileSourceDestinationPair(s, d));
        }

        /// <inheritdoc cref="ISourceDestinationPairExtensions.GetFilePairsEnumerable{T}(IDirectorySourceDestinationPair, Func{FileInfo, FileInfo, T})"/>
        public CachedEnumerable<FileSourceDestinationPair> GetFilePairsEnumerable()
        {
            return this.GetFilePairsEnumerable((s, d) => new FileSourceDestinationPair(s, d));
        }


        /// <inheritdoc cref="ISourceDestinationPairExtensions.GetDirectoryPairs{T}(IDirectorySourceDestinationPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public DirectorySourceDestinationPair[] GetDirectoryPairs()
        {
            return this.GetDirectoryPairs((s, d) => new DirectorySourceDestinationPair(s, d));
        }

        /// <inheritdoc cref="ISourceDestinationPairExtensions.GetDirectoryPairsEnumerable{T}(IDirectorySourceDestinationPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public CachedEnumerable<DirectorySourceDestinationPair> GetDirectoryPairsEnumerable()
        {
            return this.GetDirectoryPairsEnumerable((s, d) => new DirectorySourceDestinationPair(s, d));
        }

    }

}
