﻿using System;
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
        /// <summary>
        /// Create a new DirectoryPair object
        /// </summary>
        /// <param name="source">The source FileInfo object</param>
        /// <param name="destination">The Destination FileInfo object</param>
        /// <exception cref="ArgumentNullException"/>
        public FilePair(FileInfo source, FileInfo destination)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        }

        /// <inheritdoc cref="FilePair(FileInfo, FileInfo)"/>
        public static FilePair CreatePair(FileInfo source, FileInfo destination) => new FilePair(source, destination);

        /// <inheritdoc/>
        public FileInfo Source { get; }

        /// <inheritdoc/>
        public FileInfo Destination { get; }
    }
}