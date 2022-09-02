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
        /// <summary></summary>
        public FilePair(FileInfo source, FileInfo destination)
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
