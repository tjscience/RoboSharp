using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// The Directory Classes available withi the configuration
    /// </summary>
    public enum DirectoryClasses
    {
        /// <summary>
        /// String.Empty
        /// </summary>
        None,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_DirectoryExclusion"/>
        Exclusion,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_ExistingDir"/>
        ExistingDir,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_ExtraDir"/>
        ExtraDir,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_NewDir"/>
        NewDir,
    }
}
