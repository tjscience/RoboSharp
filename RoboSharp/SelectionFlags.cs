using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// Enum to define various selection options that can be toggled for the RoboCopy process.
    /// </summary>
    [Flags]
    public enum SelectionFlags
    {
        /// <summary>
        /// Set RoboCopy options to their defaults
        /// </summary>
        Default = 0,
        /// <inheritdoc cref="SelectionOptions.ExcludeChanged"/>
        ExcludeChanged = 1,
        /// <inheritdoc cref="SelectionOptions.ExcludeExtra"/>
        ExcludeExtra = 2,
        /// <inheritdoc cref="SelectionOptions.ExcludeLonely"/>
        ExcludeLonely = 4,
        /// <inheritdoc cref="SelectionOptions.ExcludeNewer"/>
        ExcludeNewer = 8,
        /// <inheritdoc cref="SelectionOptions.ExcludeOlder"/>
        ExcludeOlder = 16,
        /// <inheritdoc cref="SelectionOptions.ExcludeJunctionPoints"/>
        ExcludeJunctionPoints = 32,
        /// <inheritdoc cref="SelectionOptions.ExcludeJunctionPointsForDirectories"/>
        ExcludeJunctionPointsForDirectories = 64,
        /// <inheritdoc cref="SelectionOptions.ExcludeJunctionPointsForFiles"/>
        ExcludeJunctionPointsForFiles = 128,
        /// <inheritdoc cref="SelectionOptions.IncludeSame"/>
        IncludeSame = 256,
        /// <inheritdoc cref="SelectionOptions.IncludeTweaked"/>
        IncludeTweaked = 512,
        /// <inheritdoc cref="SelectionOptions.OnlyCopyArchiveFiles"/>
        OnlyCopyArchiveFiles = 1024,
        /// <inheritdoc cref="SelectionOptions.OnlyCopyArchiveFilesAndResetArchiveFlag"/>
        OnlyCopyArchiveFilesAndResetArchiveFlag = 2048,
        /// <inheritdoc cref="SelectionOptions.IncludeModified"/>
        IncludeModified = 4096
    }
}
