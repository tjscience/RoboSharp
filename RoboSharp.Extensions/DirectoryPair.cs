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
        }

        /// <inheritdoc cref="DirectoryPair(DirectoryInfo, DirectoryInfo)"/>
        public static DirectoryPair CreatePair(DirectoryInfo source, DirectoryInfo destination) => new DirectoryPair(source, destination);

        /// <inheritdoc/>
        public DirectoryInfo Source { get; }

        /// <inheritdoc/>
        public DirectoryInfo Destination { get; }


    }
}
