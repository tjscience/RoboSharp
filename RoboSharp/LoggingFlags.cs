using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// Enum to define the logging actions to be taken by RoboCopy process.
    /// </summary>
    [Flags]
    public enum LoggingFlags
    {
        /// <summary>
        /// All Logging Options are set to <see langword="false"/>
        /// </summary>
        None = 0,
        /// <inheritdoc cref="LoggingOptions.IncludeFullPathNames"/>
        IncludeFullPathNames = 1,
        /// <inheritdoc cref="LoggingOptions.IncludeSourceTimeStamps"/>
        IncludeSourceTimeStamps = 2,
        /// <inheritdoc cref="LoggingOptions.ListOnly"/>
        ListOnly = 4,
        /// <inheritdoc cref="LoggingOptions.NoDirectoryList"/>
        NoDirectoryList = 8,
        /// <inheritdoc cref="LoggingOptions.NoFileClasses"/>
        NoFileClasses = 16,
        /// <inheritdoc cref="LoggingOptions.NoFileList"/>
        NoFileList = 32,
        /// <inheritdoc cref="LoggingOptions.NoFileSizes"/>
        NoFileSizes = 64,
        /// <inheritdoc cref="LoggingOptions.NoJobHeader"/>
        NoJobHeader = 128,
        /// <inheritdoc cref="LoggingOptions.NoJobSummary"/>
        NoJobSummary = 256,
        /// <inheritdoc cref="LoggingOptions.NoProgress"/>
        NoProgress = 512,
        /// <inheritdoc cref="LoggingOptions.OutputAsUnicode"/>
        OutputAsUnicode = 1024,
        /// <inheritdoc cref="LoggingOptions.OutputToRoboSharpAndLog"/>
        OutputToRoboSharpAndLog = 2048,
        /// <inheritdoc cref="LoggingOptions.PrintSizesAsBytes"/>
        PrintSizesAsBytes = 4096,
        /// <inheritdoc cref="LoggingOptions.ReportExtraFiles"/>
        ReportExtraFiles = 8192,
        /// <inheritdoc cref="LoggingOptions.ShowEstimatedTimeOfArrival"/>
        ShowEstimatedTimeOfArrival = 16384,
        /// <inheritdoc cref="LoggingOptions.VerboseOutput"/>
        VerboseOutput = 32768,
        /// <summary>
        /// The flags that are automatically applied by RoboSharp to allow it to function properly. <br/>
        /// The ApplyLoggingFlags method will not remove these flags, only apply them.
        /// </summary>
        RoboSharpDefault = OutputToRoboSharpAndLog | PrintSizesAsBytes | VerboseOutput
    }
}
