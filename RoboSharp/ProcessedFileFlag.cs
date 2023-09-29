using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// Enum for the various descriptors provided by RoboSharp that can be identified by the ProgressEstimator object, 
    /// and can be applied to the ProcessedFileInfo object
    /// </summary>
    public enum ProcessedFileFlag
    {
        /// <summary>
        /// String.Empty
        /// </summary>
        None,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_AttribExclusion"/>
        AttribExclusion,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_ChangedExclusion"/>
        ChangedExclusion,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_ExtraFile"/>
        ExtraFile,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_FailedFile"/>
        Failed,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_FileExclusion"/>
        FileExclusion,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_MaxAgeOrAccessExclusion"/>
        MaxAgeSizeExclusion,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_MaxFileSizeExclusion"/>
        MaxFileSizeExclusion,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_MinAgeOrAccessExclusion"/>
        MinAgeSizeExclusion,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_MinFileSizeExclusion"/>
        MinFileSizeExclusion,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_MismatchFile"/>
        MisMatch,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_NewerFile"/>
        NewerFile,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_NewFile"/>
        NewFile,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_OlderFile"/>
        OlderFile,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_SameFile"/>
        SameFile,
        /// <inheritdoc cref="RoboSharpConfiguration.LogParsing_TweakedInclusion"/>
        TweakedInclusion,

    }

}
