using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static RoboSharp.RoboCommandParserFunctions;

namespace RoboSharp
{
    /// <summary>
    /// Factory class used to parse a string that represents a Command Line call to robocommand, and return a command with those parameters.
    /// </summary>
    public static class RoboCommandParser
    {
        /// <summary>Attempt the parse the <paramref name="command"/> into a new IRoboCommand object</summary>
        /// <returns>True if successful, otherwise false</returns>
        /// <inheritdoc cref="Parse(string, IRoboCommandFactory)"/>
        /// <param name="command"/>
        /// <param name="result">If successful, a new IRobocommand, otherwise null</param>
        /// <param name="factory"/>
        public static bool TryParse(string command, out IRoboCommand result, IRoboCommandFactory factory = default)
        {
            try
            {
                result = Parse(command, factory ?? RoboCommandFactory.DefaultFactory);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <returns>A new <see cref="RoboCommand"/></returns>
        /// <inheritdoc cref="Parse(string, Interfaces.IRoboCommandFactory)"/>
        public static Interfaces.IRoboCommand Parse(string command) => Parse(command, RoboCommandFactory.DefaultFactory);

        /// <summary>
        /// Parse the <paramref name="command"/> text into a new IRoboCommand.
        /// </summary>
        /// <param name="command">The Command-Line string of options to parse. <br/>Example:  robocopy "source" "destination" /xc /copyall </param>
        /// <param name="factory">The factory used to generate the robocommand</param>
        /// <returns>A new IRoboCommand object generated from the <paramref name="factory"/></returns>
        public static Interfaces.IRoboCommand Parse(string command, Interfaces.IRoboCommandFactory factory)
        {
            Debugger.Instance.DebugMessage($"RoboCommandParser - Begin parsing input string : {command}");
                        
            // Trim robocopy.exe from the beginning of the string, then extract the source/destination.
            string sanitizedCmd = TrimRobocopy(command);
            ParsedSourceDest paths = ParseSourceAndDestination(sanitizedCmd);

            // Filters SHOULD be immediately following the source/destination string at the very beginning of the text
            // Also Ensure white space at end of string because all constants have it
            sanitizedCmd = paths.SanitizedString.Replace("\"*.*\"", "").Replace(" *.* ", " "); // Remove the DEFAULT FILTER wildcard from the text
            var filters = RoboCommandParserFunctions.ExtractFileFilters(sanitizedCmd + " ", out sanitizedCmd);
            
            // Get the command
            var roboCommand = factory.GetRoboCommand(paths.Source, paths.Dest, ParseCopyFlags(sanitizedCmd, out sanitizedCmd), ParseSelectionFlags(sanitizedCmd, out sanitizedCmd));
            
            // apply the file filters, if any were discovered
            if (filters.Any()) roboCommand.CopyOptions.AddFileFilter(filters.ToArray());

            // apply the remaining options
            roboCommand
                .ParseCopyOptions(sanitizedCmd, out sanitizedCmd)
                .ParseLoggingOptions(sanitizedCmd, out sanitizedCmd)
                .ParseSelectionOptions(sanitizedCmd, out sanitizedCmd)
                .ParseRetryOptions(sanitizedCmd, out sanitizedCmd);
            Debugger.Instance.DebugMessage("RoboCommandParser.Parse completed.\n");
            return roboCommand;
        }

        #region < Copy Options Parsing >

        private static CopyActionFlags ParseCopyFlags(string cmd, out string updatedText)
        {
            CopyActionFlags flags = CopyActionFlags.Default;
            ExtractFlag(cmd, CopyOptions.NETWORK_COMPRESSION, out cmd, () => flags |= CopyActionFlags.Compress);
            ExtractFlag(cmd, CopyOptions.COPY_SUBDIRECTORIES, out cmd, () => flags |= CopyActionFlags.CopySubdirectories);
            ExtractFlag(cmd, CopyOptions.COPY_SUBDIRECTORIES_INCLUDING_EMPTY, out cmd, () => flags |= CopyActionFlags.CopySubdirectoriesIncludingEmpty);
            ExtractFlag(cmd, CopyOptions.CREATE_DIRECTORY_AND_FILE_TREE, out cmd, () => flags |= CopyActionFlags.CreateDirectoryAndFileTree);
            ExtractFlag(cmd, CopyOptions.MIRROR, out cmd, () => flags |= CopyActionFlags.Mirror);
            ExtractFlag(cmd, CopyOptions.MOVE_FILES, out cmd, () => flags |= CopyActionFlags.MoveFiles);
            ExtractFlag(cmd, CopyOptions.MOVE_FILES_AND_DIRECTORIES, out cmd, () => flags |= CopyActionFlags.MoveFilesAndDirectories);
            ExtractFlag(cmd, CopyOptions.PURGE, out cmd, () => flags |= CopyActionFlags.Purge);
            updatedText = cmd;
            return flags;
        }

        /// <summary>
        /// Parse the Copy Options not discovered by ParseCopyFlags
        /// </summary>
        private static IRoboCommand ParseCopyOptions(this IRoboCommand roboCommand, string command, out string sanitizedCmd)
        {
            Debugger.Instance.DebugMessage($"Parsing Copy Options");
            var options = roboCommand.CopyOptions;
            sanitizedCmd = command;

            options.CheckPerFile |= ExtractFlag(sanitizedCmd, CopyOptions.CHECK_PER_FILE, out sanitizedCmd);
            options.CopyAll |= ExtractFlag(sanitizedCmd, CopyOptions.COPY_ALL, out sanitizedCmd);
            options.CopyFilesWithSecurity |= ExtractFlag(sanitizedCmd, CopyOptions.COPY_FILES_WITH_SECURITY, out sanitizedCmd);
            options.CopySymbolicLink |= ExtractFlag(sanitizedCmd, CopyOptions.COPY_SYMBOLIC_LINK, out sanitizedCmd);
            options.DoNotCopyDirectoryInfo |= ExtractFlag(sanitizedCmd, CopyOptions.DO_NOT_COPY_DIRECTORY_INFO, out sanitizedCmd);
            options.DoNotUseWindowsCopyOffload |= ExtractFlag(sanitizedCmd, CopyOptions.DO_NOT_USE_WINDOWS_COPY_OFFLOAD, out sanitizedCmd);
            options.EnableBackupMode |= ExtractFlag(sanitizedCmd, CopyOptions.ENABLE_BACKUP_MODE, out sanitizedCmd);
            options.EnableEfsRawMode |= ExtractFlag(sanitizedCmd, CopyOptions.ENABLE_EFSRAW_MODE, out sanitizedCmd);
            options.EnableRestartMode |= ExtractFlag(sanitizedCmd, CopyOptions.ENABLE_RESTART_MODE, out sanitizedCmd);
            options.EnableRestartModeWithBackupFallback |= ExtractFlag(sanitizedCmd, CopyOptions.ENABLE_RESTART_MODE_WITH_BACKUP_FALLBACK, out sanitizedCmd);
            options.FatFiles |= ExtractFlag(sanitizedCmd, CopyOptions.FAT_FILES, out sanitizedCmd);
            options.FixFileSecurityOnAllFiles |= ExtractFlag(sanitizedCmd, CopyOptions.FIX_FILE_SECURITY_ON_ALL_FILES, out sanitizedCmd);
            options.FixFileTimesOnAllFiles |= ExtractFlag(sanitizedCmd, CopyOptions.FIX_FILE_TIMES_ON_ALL_FILES, out sanitizedCmd);
            options.RemoveFileInformation |= ExtractFlag(sanitizedCmd, CopyOptions.REMOVE_FILE_INFORMATION, out sanitizedCmd);
            options.TurnLongPathSupportOff |= ExtractFlag(sanitizedCmd, CopyOptions.TURN_LONG_PATH_SUPPORT_OFF, out sanitizedCmd);
            options.UseUnbufferedIo |= ExtractFlag(sanitizedCmd, CopyOptions.USE_UNBUFFERED_IO, out sanitizedCmd);

            // Non-Boolean Options

            if (TryExtractParameter(sanitizedCmd, CopyOptions.ADD_ATTRIBUTES, out string param, out sanitizedCmd))
            {
                options.AddAttributes = param;
            }

            _ = TryExtractParameter(sanitizedCmd, CopyOptions.COPY_FLAGS, out param, out sanitizedCmd); // Always set this value
            options.CopyFlags = param;
            
            if (TryExtractParameter(sanitizedCmd, CopyOptions.DEPTH, out param, out sanitizedCmd) && int.TryParse(param, out int value))
            {
                options.Depth = value;
            }
            
            _ = TryExtractParameter(sanitizedCmd, CopyOptions.DIRECTORY_COPY_FLAGS, out param, out sanitizedCmd); // Always set this value
            options.DirectoryCopyFlags = param;

            if (TryExtractParameter(sanitizedCmd, CopyOptions.INTER_PACKET_GAP, out param, out sanitizedCmd) && int.TryParse(param, out value))
            {
                options.InterPacketGap = value;
            }
            if (TryExtractParameter(sanitizedCmd, CopyOptions.MONITOR_SOURCE_CHANGES_LIMIT, out param, out sanitizedCmd) && int.TryParse(param, out value))
            {
                options.MonitorSourceChangesLimit = value;
            }
            if (TryExtractParameter(sanitizedCmd, CopyOptions.MONITOR_SOURCE_TIME_LIMIT, out param, out sanitizedCmd) && int.TryParse(param, out value))
            {
                options.MonitorSourceTimeLimit = value;
            }
            if (TryExtractParameter(sanitizedCmd, CopyOptions.MULTITHREADED_COPIES_COUNT, out param, out sanitizedCmd) && int.TryParse(param, out value))
            {
                options.MultiThreadedCopiesCount = value;
            }
            if (TryExtractParameter(sanitizedCmd, CopyOptions.REMOVE_ATTRIBUTES, out param, out sanitizedCmd))
            {
                options.RemoveAttributes = param;
            }
            if (TryExtractParameter(sanitizedCmd, CopyOptions.RUN_HOURS, out param, out sanitizedCmd) && CopyOptions.IsRunHoursStringValid(param))
            {
                options.RunHours = param;
            }
            return roboCommand;
        }

        #endregion

        #region < Selection Options Parsing  >
        private static SelectionFlags ParseSelectionFlags(string cmd, out string updatedText)
        {
            SelectionFlags flags = SelectionFlags.Default;
            ExtractFlag(cmd, SelectionOptions.EXCLUDE_CHANGED, out cmd, () => flags |= SelectionFlags.ExcludeChanged);
            ExtractFlag(cmd, SelectionOptions.EXCLUDE_EXTRA, out cmd, () => flags |= SelectionFlags.ExcludeExtra);
            ExtractFlag(cmd, SelectionOptions.EXCLUDE_JUNCTION_POINTS, out cmd, () => flags |= SelectionFlags.ExcludeJunctionPoints);
            ExtractFlag(cmd, SelectionOptions.EXCLUDE_JUNCTION_POINTS_FOR_DIRECTORIES, out cmd, () => flags |= SelectionFlags.ExcludeJunctionPointsForDirectories);
            ExtractFlag(cmd, SelectionOptions.EXCLUDE_JUNCTION_POINTS_FOR_FILES, out cmd, () => flags |= SelectionFlags.ExcludeJunctionPointsForFiles);
            ExtractFlag(cmd, SelectionOptions.EXCLUDE_LONELY, out cmd, () => flags |= SelectionFlags.ExcludeLonely);
            ExtractFlag(cmd, SelectionOptions.EXCLUDE_NEWER, out cmd, () => flags |= SelectionFlags.ExcludeNewer);
            ExtractFlag(cmd, SelectionOptions.EXCLUDE_OLDER, out cmd, () => flags |= SelectionFlags.ExcludeOlder);
            ExtractFlag(cmd, SelectionOptions.INCLUDE_SAME, out cmd, () => flags |= SelectionFlags.IncludeSame);
            ExtractFlag(cmd, SelectionOptions.INCLUDE_TWEAKED, out cmd, () => flags |= SelectionFlags.IncludeTweaked);
            ExtractFlag(cmd, SelectionOptions.ONLY_COPY_ARCHIVE_FILES, out cmd, () => flags |= SelectionFlags.OnlyCopyArchiveFiles);
            ExtractFlag(cmd, SelectionOptions.ONLY_COPY_ARCHIVE_FILES_AND_RESET_ARCHIVE_FLAG, out cmd, () => flags |= SelectionFlags.OnlyCopyArchiveFilesAndResetArchiveFlag);
            updatedText = cmd;
            return flags;
        }

        /// <summary>
        /// Parse the Selection Options not discovered by ParseSelectionFlags
        /// </summary>
        private static IRoboCommand ParseSelectionOptions(this IRoboCommand roboCommand, string command, out string sanitizedCmd)
        {
            Debugger.Instance.DebugMessage($"Parsing Selection Options");
            var options = roboCommand.SelectionOptions;
            options.CompensateForDstDifference |= ExtractFlag(command, SelectionOptions.COMPENSATE_FOR_DST_DIFFERENCE, out sanitizedCmd);
            options.UseFatFileTimes |= ExtractFlag(sanitizedCmd,SelectionOptions.USE_FAT_FILE_TIMES, out sanitizedCmd);

            if (TryExtractParameter(sanitizedCmd, SelectionOptions.INCLUDE_ATTRIBUTES, out string param, out sanitizedCmd))
            {
                options.IncludeAttributes = param;
            }
            if (TryExtractParameter(sanitizedCmd, SelectionOptions.EXCLUDE_ATTRIBUTES, out param, out sanitizedCmd))
            {
                options.ExcludeAttributes = param;
            }
            if (TryExtractParameter(sanitizedCmd, SelectionOptions.MAX_FILE_AGE, out param, out sanitizedCmd))
            {
                options.MaxFileAge = param;
            }
            if (TryExtractParameter(sanitizedCmd, SelectionOptions.MAX_FILE_SIZE, out param, out sanitizedCmd) && long.TryParse(param, out var value))
            {
                options.MaxFileSize = value;
            }
            if (TryExtractParameter(sanitizedCmd, SelectionOptions.MIN_FILE_AGE, out param, out sanitizedCmd))
            {
                options.MinFileAge = param;
            }
            if (TryExtractParameter(sanitizedCmd, SelectionOptions.MIN_FILE_SIZE, out param, out sanitizedCmd) && long.TryParse(param, out value))
            {
                options.MinFileSize = value;
            }
            if (TryExtractParameter(sanitizedCmd, SelectionOptions.MAX_LAST_ACCESS_DATE, out param, out sanitizedCmd))
            {
                options.MaxLastAccessDate = param;
            }
            if (TryExtractParameter(sanitizedCmd, SelectionOptions.MIN_LAST_ACCESS_DATE, out param, out sanitizedCmd))
            {
                options.MinLastAccessDate = param;
            }

            options.ExcludedDirectories.AddRange(RoboCommandParserFunctions.ExtractExclusionDirectories(sanitizedCmd, out sanitizedCmd));
            options.ExcludedFiles.AddRange(RoboCommandParserFunctions.ExtractExclusionFiles(sanitizedCmd, out sanitizedCmd));


            return roboCommand;
        }

        #endregion

        private static IRoboCommand ParseLoggingOptions(this IRoboCommand roboCommand, string command, out string sanitizedCmd)
        {
            Debugger.Instance.DebugMessage($"Parsing Logging Options");
            var options = roboCommand.LoggingOptions;
            sanitizedCmd = command;

            options.IncludeFullPathNames |= ExtractFlag(sanitizedCmd, LoggingOptions.INCLUDE_FULL_PATH_NAMES, out sanitizedCmd, null);
            options.IncludeSourceTimeStamps |= ExtractFlag(sanitizedCmd, LoggingOptions.INCLUDE_SOURCE_TIMESTAMPS, out sanitizedCmd, null);
            options.ListOnly |= ExtractFlag(sanitizedCmd, LoggingOptions.LIST_ONLY, out sanitizedCmd, null);
            options.NoDirectoryList |= ExtractFlag(sanitizedCmd, LoggingOptions.NO_DIRECTORY_LIST, out sanitizedCmd, null);
            options.NoFileClasses |= ExtractFlag(sanitizedCmd, LoggingOptions.NO_FILE_CLASSES, out sanitizedCmd, null);
            options.NoFileList |= ExtractFlag(sanitizedCmd, LoggingOptions.NO_FILE_LIST, out sanitizedCmd, null);
            options.NoFileSizes |= ExtractFlag(sanitizedCmd, LoggingOptions.NO_FILE_SIZES, out sanitizedCmd, null);
            options.NoJobHeader |= ExtractFlag(sanitizedCmd, LoggingOptions.NO_JOB_HEADER, out sanitizedCmd, null);
            options.NoJobSummary |= ExtractFlag(sanitizedCmd, LoggingOptions.NO_JOB_SUMMARY, out sanitizedCmd, null);
            options.NoProgress |= ExtractFlag(sanitizedCmd, LoggingOptions.NO_PROGRESS, out sanitizedCmd, null);
            options.OutputAsUnicode |= ExtractFlag(sanitizedCmd, LoggingOptions.OUTPUT_AS_UNICODE, out sanitizedCmd, null);
            options.OutputToRoboSharpAndLog |= ExtractFlag(sanitizedCmd, LoggingOptions.OUTPUT_TO_ROBOSHARP_AND_LOG, out sanitizedCmd, null);
            options.PrintSizesAsBytes |= ExtractFlag(sanitizedCmd, LoggingOptions.PRINT_SIZES_AS_BYTES, out sanitizedCmd, null);
            options.ReportExtraFiles |= ExtractFlag(sanitizedCmd, LoggingOptions.REPORT_EXTRA_FILES, out sanitizedCmd, null);
            options.ShowEstimatedTimeOfArrival |= ExtractFlag(sanitizedCmd, LoggingOptions.SHOW_ESTIMATED_TIME_OF_ARRIVAL, out sanitizedCmd, null);
            options.VerboseOutput |= ExtractFlag(sanitizedCmd, LoggingOptions.VERBOSE_OUTPUT, out sanitizedCmd, null);

            options.LogPath = ExtractLogPath(LoggingOptions.LOG_PATH, sanitizedCmd, out sanitizedCmd);
            options.AppendLogPath = ExtractLogPath(LoggingOptions.APPEND_LOG_PATH, sanitizedCmd, out sanitizedCmd);
            options.UnicodeLogPath = ExtractLogPath(LoggingOptions.UNICODE_LOG_PATH, sanitizedCmd, out sanitizedCmd);
            options.AppendUnicodeLogPath = ExtractLogPath(LoggingOptions.APPEND_UNICODE_LOG_PATH, sanitizedCmd, out sanitizedCmd);
            
            return roboCommand;

            string ExtractLogPath(string filter, string input, out string output)
            {
                if (TryExtractParameter(input, filter, out string path, out output))
                {
                    return path.Trim('\"');
                }
                return string.Empty;
            }
        }

        private static IRoboCommand ParseRetryOptions(this IRoboCommand roboCommand, string command, out string sanitizedCmd)
        {
            Debugger.Instance.DebugMessage($"Parsing Retry Options");
            var options = roboCommand.RetryOptions;
            
            options.SaveToRegistry |= ExtractFlag(command, RetryOptions.SAVE_TO_REGISTRY, out sanitizedCmd, null);
            options.WaitForSharenames |= ExtractFlag(sanitizedCmd, RetryOptions.WAIT_FOR_SHARENAMES, out sanitizedCmd, null);

            if (TryExtractParameter(sanitizedCmd, RetryOptions.RETRY_COUNT, out string param, out sanitizedCmd) && int.TryParse(param, out int value))
            {
                options.RetryCount = value;
            }
            if (TryExtractParameter(sanitizedCmd, RetryOptions.RETRY_WAIT_TIME, out param, out sanitizedCmd) && int.TryParse(param, out value))
            {
                options.RetryWaitTime = value;
            }
            return roboCommand;
        }
    }
}
