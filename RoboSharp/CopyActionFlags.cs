using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// Enum to define the high-level copy action to be taken by RoboCopy process.
    /// </summary>
    [Flags]
    public enum CopyActionFlags
    {
        /// <summary>
        /// Default Functionality is to only copy the files within the source directory - does not copy any files within the subfolders.
        /// </summary>
        Default = 0,
        /// <inheritdoc cref="CopyOptions.CopySubdirectories"/>
        CopySubdirectories = 1,
        /// <inheritdoc cref="CopyOptions.CopySubdirectoriesIncludingEmpty"/>
        CopySubdirectoriesIncludingEmpty = 2,
        /// <inheritdoc cref="CopyOptions.Purge"/>
        Purge = 4,
        /// <inheritdoc cref="CopyOptions.CreateDirectoryAndFileTree"/>
        CreateDirectoryAndFileTree = 8,
        /// <inheritdoc cref="CopyOptions.MoveFiles"/>
        MoveFiles = 16,
        /// <inheritdoc cref="CopyOptions.MoveFilesAndDirectories"/>
        MoveFilesAndDirectories = 32,
        /// <inheritdoc cref="CopyOptions.Mirror"/>
        Mirror = CopySubdirectoriesIncludingEmpty | Purge, //6
    }
}
