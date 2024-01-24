using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// Factory class used to parse a string that represents a Command Line call to robocommand, and return a command with those parameters.
    /// </summary>
    public static class RoboCommandParser
    {

        /// <returns>A new <see cref="RoboCommand"/></returns>
        /// <inheritdoc cref="Parse(string, Interfaces.IRoboCommandFactory)"/>
        public static Interfaces.IRoboCommand Parse(string command) => Parse(command, new RoboCommandFactory());

        /// <summary>
        /// Parse the <paramref name="command"/> text into a new IRoboCommand.
        /// </summary>
        /// <param name="command">The Command-Line string of options to parse. <br/>Example:  robocopy "source" "destination" /xc /copyall </param>
        /// <param name="factory">The factory used to generate the robocommand</param>
        /// <returns>A new IRoboCommand object generated from the <paramref name="factory"/></returns>
        public static Interfaces.IRoboCommand Parse(string command, Interfaces.IRoboCommandFactory factory)
        {
            Debugger.Instance.DebugMessage($"RoboCommandParser - Begin parsing input string : {command}");
            command = command.Replace("\"*.*\"", "").Replace(" *.* ", " "); // Remove the DEFAULT FILTER wildcard from the text
            ParsedSourceDest paths = ParseSourceAndDestination(command);
            string sanitizedCmd = SanitizeCommandString(command);
            var roboCommand = factory.GetRoboCommand(paths.Source, paths.Dest, ParseCopyFlags(sanitizedCmd), ParseSelectionFlags(sanitizedCmd));
            
           roboCommand
                .ParseCopyOptions(command, sanitizedCmd)
                .ParseLoggingOptions(command, sanitizedCmd)
                .ParseSelectionOptions(command, sanitizedCmd)
                .ParseRetryOptions(command, sanitizedCmd);
            Debugger.Instance.DebugMessage("RoboCommandParser.Parse completed.\n");
            return roboCommand;
        }

        /// <summary> Prep the command text for use with the HasFlag function </summary>
        private static string SanitizeCommandString(string command) => command.ToLowerInvariant() + " ";

        /// <summary> Check if the string contains a the flag string - both are sanitized to lower invariant </summary>
        private static bool HasFlag(this string cmd, string flag) 
        {
            bool value = cmd.Contains(flag.ToLowerInvariant());
            Debugger.Instance.DebugMessage($"--> Switch {flag}{(value ? "" : " not")} detected.");
            return value;
        }
        

        /// <summary> Attempt to extract the parameter from a format pattern string </summary>
        private static bool TryExtractParameter(string commandText, string formatString, out string parameter)
        {
            parameter = null;
            string prefix = formatString.Substring(0, formatString.IndexOf('{')).TrimEnd('{').Trim(); // Turn /LEV:{0} into /LEV:
            
            if (!commandText.Contains(prefix, StringComparison.InvariantCultureIgnoreCase))
            {
                Debugger.Instance.DebugMessage($"--> Switch {prefix} not detected.");
                return false;
            }
            string subSection = commandText.Substring(commandText.IndexOf(prefix, StringComparison.InvariantCultureIgnoreCase)); // Get from that point forward
            
            int substringLength = subSection.IndexOf(" /");
            if (substringLength > 0)
            {
                subSection = subSection.Substring(0, substringLength); // Reduce the subsection down to the relevant portion by cutting off at the next parameter switch
            }

            parameter = subSection.Replace(prefix, string.Empty).Trim();
            Debugger.Instance.DebugMessage($"--> Switch {prefix} found. Value : {parameter}");
            return true;
        }


        #region < Source and Destination Parsing >

        private readonly struct ParsedSourceDest
        {
            public ParsedSourceDest(string source, string dest)
            {
                Source = source;
                Dest = dest;
            }
            public readonly string Source;
            public readonly string Dest;
        }

        private static ParsedSourceDest ParseSourceAndDestination(string command)
        {
            //lang=regex
            //const string validPathChars = "[^:*?\"<>\\|\\s]";
            //lang=regex
            const string quotedPattern = "^(?<rc>robocopy\\s*)?\"(?<source>.+?:.+?)\"\\s+\"(?<dest>.+?:.+?)\".*";
            //lang=regex
            const string sourceQuotedPattern = "^(?<rc>robocopy\\s*)?\"(?<source>.+?:[^:*?\"<>\\|\\s]+)\"\\s+(?<dest>.+?:[^:*?\"<>\\|\\s]+).*";
            //lang=regex
            const string destQuotedPattern = "^(?<rc>robocopy\\s*)?(?<source>/.+?:[^:*?\"<>\\|\\s]+)\\s+\"(?<dest>.+?:[^:*?\"<>\\|\\s]+)\".*";
            //lang=regex
            const string nonQuotedPattern = "^(?<rc>robocopy\\s*)?(?<source>.+?:[^:*?\"<>\\|\\s]+)\\s+(?<dest>.+?:[^:*?\"<>\\|\\s]+).*"; // Non-Quoted strings search until they encounter white-space

            // Return the first match
            return PatternMatch(quotedPattern)
                ?? PatternMatch(nonQuotedPattern)
                ?? PatternMatch(sourceQuotedPattern)
                ?? PatternMatch(destQuotedPattern)
                ?? LogNoMatch();
            
            ParsedSourceDest? PatternMatch(string pattern)
            {
                var match = Regex.Match(command, pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
                if (match.Success)
                {
                    string source = match.Groups["source"].Value.Trim('\"');
                    string dest = match.Groups["dest"].Value.Trim('\"');
                    source = source.IsPathFullyQualified() ? source : null;
                    dest = dest.IsPathFullyQualified() ? dest : null;
                    if (source is null && dest is null) return null;

                    Debugger.Instance.DebugMessage($"--> Source and Destination Pattern Match Success:");
                    Debugger.Instance.DebugMessage($"----> Pattern : " + pattern);
                    Debugger.Instance.DebugMessage($"----> Source : " + source);
                    Debugger.Instance.DebugMessage($"----> Destination : " + dest);
                    return new ParsedSourceDest(source ?? string.Empty, dest ?? string.Empty);
                }else
                {
                    return null;
                }
            }
            ParsedSourceDest LogNoMatch()
            {
                Debugger.Instance.DebugMessage($"--> Unable to detect a Source/Destination pattern match");
                return new ParsedSourceDest(string.Empty, string.Empty);
            }
        }

        #endregion

        #region < Copy Options Parsing >

        private static CopyActionFlags ParseCopyFlags(string sanitizedCmd)
        {
            CopyActionFlags flags = CopyActionFlags.Default;
            if (sanitizedCmd.HasFlag(CopyOptions.NETWORK_COMPRESSION)) flags |= CopyActionFlags.Compress;
            if (sanitizedCmd.HasFlag(CopyOptions.COPY_SUBDIRECTORIES)) flags |= CopyActionFlags.CopySubdirectories;
            if (sanitizedCmd.HasFlag(CopyOptions.COPY_SUBDIRECTORIES_INCLUDING_EMPTY)) flags |= CopyActionFlags.CopySubdirectoriesIncludingEmpty;
            if (sanitizedCmd.HasFlag(CopyOptions.CREATE_DIRECTORY_AND_FILE_TREE)) flags |= CopyActionFlags.CreateDirectoryAndFileTree;
            if (sanitizedCmd.HasFlag(CopyOptions.MIRROR)) flags |= CopyActionFlags.Mirror;
            if (sanitizedCmd.HasFlag(CopyOptions.MOVE_FILES)) flags |= CopyActionFlags.MoveFiles;
            if (sanitizedCmd.HasFlag(CopyOptions.MOVE_FILES_AND_DIRECTORIES)) flags |= CopyActionFlags.MoveFilesAndDirectories;
            if (sanitizedCmd.HasFlag(CopyOptions.PURGE)) flags |= CopyActionFlags.Purge;
            return flags;
        }

        /// <summary>
        /// Parse the Copy Options not discovered by ParseCopyFlags
        /// </summary>
        private static IRoboCommand ParseCopyOptions(this IRoboCommand roboCommand, string command, string sanitizedCmd)
        {
            Debugger.Instance.DebugMessage($"Parsing Copy Options");
            var options = roboCommand.CopyOptions;
            options.CheckPerFile |= sanitizedCmd.HasFlag(CopyOptions.CHECK_PER_FILE);
            options.CopyAll |= sanitizedCmd.HasFlag(CopyOptions.COPY_ALL);
            options.CopyFilesWithSecurity |= sanitizedCmd.HasFlag(CopyOptions.COPY_FILES_WITH_SECURITY);
            options.CopySymbolicLink |= sanitizedCmd.HasFlag(CopyOptions.COPY_SYMBOLIC_LINK);
            options.DoNotCopyDirectoryInfo |= sanitizedCmd.HasFlag(CopyOptions.DO_NOT_COPY_DIRECTORY_INFO);
            options.DoNotUseWindowsCopyOffload |= sanitizedCmd.HasFlag(CopyOptions.DO_NOT_USE_WINDOWS_COPY_OFFLOAD);
            options.EnableBackupMode |= sanitizedCmd.HasFlag(CopyOptions.ENABLE_BACKUP_MODE);
            options.EnableEfsRawMode |= sanitizedCmd.HasFlag(CopyOptions.ENABLE_EFSRAW_MODE);
            options.EnableRestartMode |= sanitizedCmd.HasFlag(CopyOptions.ENABLE_RESTART_MODE);
            options.EnableRestartModeWithBackupFallback |= sanitizedCmd.HasFlag(CopyOptions.ENABLE_RESTART_MODE_WITH_BACKUP_FALLBACK);
            options.FatFiles |= sanitizedCmd.HasFlag(CopyOptions.FAT_FILES);
            options.FixFileSecurityOnAllFiles |= sanitizedCmd.HasFlag(CopyOptions.FIX_FILE_SECURITY_ON_ALL_FILES);
            options.FixFileTimesOnAllFiles |= sanitizedCmd.HasFlag(CopyOptions.FIX_FILE_TIMES_ON_ALL_FILES);
            options.RemoveFileInformation |= sanitizedCmd.HasFlag(CopyOptions.REMOVE_FILE_INFORMATION);
            options.TurnLongPathSupportOff |= sanitizedCmd.HasFlag(CopyOptions.TURN_LONG_PATH_SUPPORT_OFF);
            options.UseUnbufferedIo |= sanitizedCmd.HasFlag(CopyOptions.USE_UNBUFFERED_IO);

            // Non-Boolean Options

            if (TryExtractParameter(command, CopyOptions.ADD_ATTRIBUTES, out string param))
            {
                options.AddAttributes = param;
            }
            if (TryExtractParameter(command, CopyOptions.COPY_FLAGS, out param))
            {
                options.CopyFlags = param;
            }
            if (TryExtractParameter(command, CopyOptions.DEPTH, out param) && int.TryParse(param, out int value))
            {
                options.Depth = value;
            }
            if (TryExtractParameter(command, CopyOptions.DIRECTORY_COPY_FLAGS, out param))
            {
                options.DirectoryCopyFlags = param;
            }
            if (TryExtractParameter(command, CopyOptions.INTER_PACKET_GAP, out param) && int.TryParse(param, out value))
            {
                options.InterPacketGap = value;
            }
            if (TryExtractParameter(command, CopyOptions.MONITOR_SOURCE_CHANGES_LIMIT, out param) && int.TryParse(param, out value))
            {
                options.MonitorSourceChangesLimit = value;
            }
            if (TryExtractParameter(command, CopyOptions.MONITOR_SOURCE_TIME_LIMIT, out param) && int.TryParse(param, out value))
            {
                options.MonitorSourceTimeLimit = value;
            }
            if (TryExtractParameter(command, CopyOptions.MULTITHREADED_COPIES_COUNT, out param) && int.TryParse(param, out value))
            {
                options.MultiThreadedCopiesCount = value;
            }
            if (TryExtractParameter(command, CopyOptions.REMOVE_ATTRIBUTES, out param))
            {
                options.RemoveAttributes = param;
            }
            if (TryExtractParameter(command, CopyOptions.RUN_HOURS, out param) && CopyOptions.IsRunHoursStringValid(param))
            {
                options.RunHours = param;
            }

            options.FileFilter = GetFilters();

            IEnumerable<string> GetFilters()
            {
                // Filters SHOULD be immediately following the source/destination string at the very beginning of the text
                Debugger.Instance.DebugMessage($"Parsing Copy Options - Extracting File Filters");
                string tmp = command.Remove(options.Source).Remove(options.Destination).TrimStart("robocopy").Remove("\"\"").Trim(); // Sanitize the beginning of the string
                var match = Regex.Match(tmp, @"^(?<filters>.+?)\s+(?<firstSwitch>/[A-Z])", RegexOptions.IgnoreCase| RegexOptions.ExplicitCapture | RegexOptions.Compiled);
                var foundFilters = match.Groups["filters"].Value;
                if (!match.Success | (string.IsNullOrWhiteSpace(foundFilters))) return options.FileFilter; // No Match Found
                List<string> filters = new List<string>();
                StringBuilder filterBuilder = new StringBuilder();
                bool isQuoted = false;
                bool isBuilding = false;
                foreach(char c in foundFilters)
                {
                    if (isQuoted && c == '"')
                    {
                        isQuoted = false;
                        isBuilding = false;
                        filters.Add(filterBuilder.ToString());
                        filterBuilder.Clear();
                    }
                    else if (isQuoted)
                        filterBuilder.Append(c);
                    else if (c == '"')
                    {
                        isQuoted = true;
                        isBuilding = true;
                    }
                    else if (!isBuilding && char.IsWhiteSpace(c))
                    { /* Do Nothing because this whitespace preceeds the start of a filter */}
                    else
                        filterBuilder.Append(c);
                }
                return filters;
            }
            return roboCommand;
        }

        #endregion

        #region < Selection Options Parsing  >
        private static SelectionFlags ParseSelectionFlags(string sanitizedCmd)
        {
            SelectionFlags flags = SelectionFlags.Default;
            if (sanitizedCmd.HasFlag(SelectionOptions.EXCLUDE_CHANGED)) flags |= SelectionFlags.ExcludeChanged;
            if (sanitizedCmd.HasFlag(SelectionOptions.EXCLUDE_EXTRA)) flags |= SelectionFlags.ExcludeExtra;
            if (sanitizedCmd.HasFlag(SelectionOptions.EXCLUDE_JUNCTION_POINTS)) flags |= SelectionFlags.ExcludeJunctionPoints;
            if (sanitizedCmd.HasFlag(SelectionOptions.EXCLUDE_JUNCTION_POINTS_FOR_DIRECTORIES)) flags |= SelectionFlags.ExcludeJunctionPointsForDirectories;
            if (sanitizedCmd.HasFlag(SelectionOptions.EXCLUDE_JUNCTION_POINTS_FOR_FILES)) flags |= SelectionFlags.ExcludeJunctionPointsForFiles;
            if (sanitizedCmd.HasFlag(SelectionOptions.EXCLUDE_LONELY)) flags |= SelectionFlags.ExcludeLonely;
            if (sanitizedCmd.HasFlag(SelectionOptions.EXCLUDE_NEWER)) flags |= SelectionFlags.ExcludeNewer;
            if (sanitizedCmd.HasFlag(SelectionOptions.EXCLUDE_OLDER)) flags |= SelectionFlags.ExcludeOlder;
            if (sanitizedCmd.HasFlag(SelectionOptions.INCLUDE_SAME)) flags |= SelectionFlags.IncludeSame;
            if (sanitizedCmd.HasFlag(SelectionOptions.INCLUDE_TWEAKED)) flags |= SelectionFlags.IncludeTweaked;
            if (sanitizedCmd.HasFlag(SelectionOptions.ONLY_COPY_ARCHIVE_FILES)) flags |= SelectionFlags.OnlyCopyArchiveFiles;
            if (sanitizedCmd.HasFlag(SelectionOptions.ONLY_COPY_ARCHIVE_FILES_AND_RESET_ARCHIVE_FLAG)) flags |= SelectionFlags.OnlyCopyArchiveFilesAndResetArchiveFlag;

            return flags;
        }

        /// <summary>
        /// Parse the Selection Options not discovered by ParseSelectionFlags
        /// </summary>
        private static IRoboCommand ParseSelectionOptions(this IRoboCommand roboCommand, string command, string sanitizedCmd)
        {
            Debugger.Instance.DebugMessage($"Parsing Selection Options");
            var options = roboCommand.SelectionOptions;
            options.CompensateForDstDifference |= command.HasFlag(SelectionOptions.COMPENSATE_FOR_DST_DIFFERENCE);
            options.UseFatFileTimes |= command.HasFlag(SelectionOptions.USE_FAT_FILE_TIMES);

            if (TryExtractParameter(command, SelectionOptions.INCLUDE_ATTRIBUTES, out string param))
            {
                options.IncludeAttributes = param;
            }
            if (TryExtractParameter(command, SelectionOptions.EXCLUDE_ATTRIBUTES, out param))
            {
                options.ExcludeAttributes = param;
            }
            if (TryExtractParameter(command, SelectionOptions.MAX_FILE_AGE, out param))
            {
                options.MaxFileAge = param;
            }
            if (TryExtractParameter(command, SelectionOptions.MAX_FILE_SIZE, out param) && long.TryParse(param, out var value))
            {
                options.MaxFileSize = value;
            }
            if (TryExtractParameter(command, SelectionOptions.MIN_FILE_AGE, out param))
            {
                options.MinFileAge = param;
            }
            if (TryExtractParameter(command, SelectionOptions.MIN_FILE_SIZE, out param) && long.TryParse(param, out value))
            {
                options.MinFileSize = value;
            }
            if (TryExtractParameter(command, SelectionOptions.MAX_LAST_ACCESS_DATE, out param))
            {
                options.MaxLastAccessDate = param;
            }
            if (TryExtractParameter(command, SelectionOptions.MIN_LAST_ACCESS_DATE, out param))
            {
                options.MinLastAccessDate = param;
            }


            /*         
            Debugger.Instance.DebugMessage($"Parsing Copy Options - Extracting Excluded Files");
            Debugger.Instance.DebugMessage($"Parsing Copy Options - Extracting Excluded Directories");
            options.ExcludedFiles;
            options.ExcludedDirectories;
            */

            return roboCommand;
        }

        #endregion

        private static IRoboCommand ParseLoggingOptions(this IRoboCommand roboCommand, string command, string sanitizedCmd)
        {
            Debugger.Instance.DebugMessage($"Parsing Logging Options");
            var options = roboCommand.LoggingOptions;
            options.IncludeFullPathNames |= sanitizedCmd.HasFlag(LoggingOptions.INCLUDE_FULL_PATH_NAMES);
            options.IncludeSourceTimeStamps |= sanitizedCmd.HasFlag(LoggingOptions.INCLUDE_SOURCE_TIMESTAMPS);
            options.ListOnly |= sanitizedCmd.HasFlag(LoggingOptions.LIST_ONLY);
            options.NoDirectoryList |= sanitizedCmd.HasFlag(LoggingOptions.NO_DIRECTORY_LIST);
            options.NoFileClasses |= sanitizedCmd.HasFlag(LoggingOptions.NO_FILE_CLASSES);
            options.NoFileList |= sanitizedCmd.HasFlag(LoggingOptions.NO_FILE_LIST);
            options.NoFileSizes |= sanitizedCmd.HasFlag(LoggingOptions.NO_FILE_SIZES);
            options.NoJobHeader |= sanitizedCmd.HasFlag(LoggingOptions.NO_JOB_HEADER);
            options.NoJobSummary |= sanitizedCmd.HasFlag(LoggingOptions.NO_JOB_SUMMARY);
            options.NoProgress |= sanitizedCmd.HasFlag(LoggingOptions.NO_PROGRESS);
            options.OutputAsUnicode |= sanitizedCmd.HasFlag(LoggingOptions.OUTPUT_AS_UNICODE);
            options.OutputToRoboSharpAndLog |= sanitizedCmd.HasFlag(LoggingOptions.OUTPUT_TO_ROBOSHARP_AND_LOG);
            options.PrintSizesAsBytes |= sanitizedCmd.HasFlag(LoggingOptions.PRINT_SIZES_AS_BYTES);
            options.ReportExtraFiles |= sanitizedCmd.HasFlag(LoggingOptions.REPORT_EXTRA_FILES);
            options.ShowEstimatedTimeOfArrival |= sanitizedCmd.HasFlag(LoggingOptions.SHOW_ESTIMATED_TIME_OF_ARRIVAL);
            options.VerboseOutput |= sanitizedCmd.HasFlag(LoggingOptions.VERBOSE_OUTPUT);

            options.LogPath = ExtractLogPath(LoggingOptions.LOG_PATH);
            options.AppendLogPath = ExtractLogPath(LoggingOptions.APPEND_LOG_PATH);
            options.UnicodeLogPath = ExtractLogPath(LoggingOptions.UNICODE_LOG_PATH);
            options.AppendUnicodeLogPath = ExtractLogPath(LoggingOptions.APPEND_UNICODE_LOG_PATH);
            
            return roboCommand;

            string ExtractLogPath(string filter)
            {
                if (TryExtractParameter(command, filter, out string path))
                {
                    return path.Trim('\"');
                }
                return string.Empty;
            }
        }

        private static IRoboCommand ParseRetryOptions(this IRoboCommand roboCommand, string command, string sanitizedCmd)
        {
            Debugger.Instance.DebugMessage($"Parsing Retry Options");
            var options = roboCommand.RetryOptions;
            options.SaveToRegistry |= sanitizedCmd.HasFlag(RetryOptions.SAVE_TO_REGISTRY);
            options.WaitForSharenames |= sanitizedCmd.HasFlag(RetryOptions.WAIT_FOR_SHARENAMES);

            if (TryExtractParameter(command, RetryOptions.RETRY_COUNT, out string param) && int.TryParse(param, out int value))
            {
                options.RetryCount = value;
            }
            if (TryExtractParameter(command, RetryOptions.RETRY_WAIT_TIME, out param) && int.TryParse(param, out value))
            {
                options.RetryWaitTime = value;
            }
            return roboCommand;
        }
    }
}
