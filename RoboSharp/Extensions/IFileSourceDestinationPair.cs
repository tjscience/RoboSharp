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
    public interface IFileSourceDestinationPair
    {
        /// <summary>
        /// Source File Information
        /// </summary>
        public FileInfo Source { get; }

        /// <summary>
        /// Destination FIle Information
        /// </summary>
        public FileInfo Destination { get; }
    }

    /// <summary>
    /// Helper Class that implements the <see cref="IFileSourceDestinationPair"/> interface
    /// </summary>
    public class FileSourceDestinationPair : IFileSourceDestinationPair
    {
        /// <summary></summary>
        public FileSourceDestinationPair(FileInfo source, FileInfo destination)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            Source = source;
            Destination = destination;
        }
        /// <inheritdoc/>
        public FileInfo Source { get; }
        /// <inheritdoc/>
        public FileInfo Destination { get; }
    }
}
