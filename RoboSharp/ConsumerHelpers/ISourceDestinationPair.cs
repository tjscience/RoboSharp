using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.ConsumerHelpers
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
            Source = source;
            Destination = destination;
        }
        /// <inheritdoc/>
        public FileInfo Source { get; }
        /// <inheritdoc/>
        public FileInfo Destination { get; }
    }

    /// <summary>
    /// Interface used for extension methods for RoboSharp custom implementations that has File Source/Destination info
    /// </summary>
    public interface IDirSourceDestinationPair
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
    /// Helper Class that implements the <see cref="IDirSourceDestinationPair"/> interface
    /// </summary>
    public class DirSourceDestinationPair : IDirSourceDestinationPair
    {
        /// <summary></summary>
        public DirSourceDestinationPair(DirectoryInfo source, DirectoryInfo destination)
        {
            Source = source;
            Destination = destination;
        }
        /// <inheritdoc/>
        public DirectoryInfo Source { get; }
        /// <inheritdoc/>
        public DirectoryInfo Destination { get; }
    }
}
