using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace RoboSharp
{
    internal static class JobFileBuilder
    {
        #region < Constants & Regex >

        /// <summary>
        /// Any comments within the job file lines will start with this string
        /// </summary>
        public const string JOBFILE_CommentPrefix = ":: ";

        /// <inheritdoc cref="JobFile.JOBFILE_Extension"/>
        public const string JOBFILE_Extension = JobFile.JOBFILE_Extension;

        /// <inheritdoc cref="JobFile.JOBFILE_Extension"/>
        internal const string JOBFILE_JobName = ":: JOB_NAME: ";

        internal const string JOBFILE_StopIfDisposing = ":: StopIfDisposing: ";

        /// <summary>Pattern to Identify the SWITCH, DELIMITER and VALUE section</summary>
        private const string RegString_SWITCH = "\\s*(?<SWITCH>\\/[A-Za-z]+[-]{0,1})(?<DELIMITER>\\s*:?\\s*)(?<VALUE>.+?)";
        /// <summary>Pattern to Identify the SWITCH, DELIMIETER and VALUE section</summary>
        private const string RegString_SWITCH_NumericValue = "\\s*(?<SWITCH>\\/[A-Za-z]+[-]{0,1})(?<DELIMITER>\\s*:?\\s*)(?<VALUE>[0-9]+?)";
        /// <summary>Pattern to Identify COMMENT sections - Throws out white space and comment delimiter '::' </summary>
        private const string RegString_COMMENT = "((?:\\s*[:]{2,}\\s*[:]{0,})(?<COMMENT>.*))";


        /// <summary>
        /// Regex to check if an entire line is a comment
        /// </summary>
        /// <remarks>
        /// Captured Group Names: <br/>
        /// COMMENT
        /// </remarks>
        private readonly static Regex LINE_IsComment = new Regex("^(?:\\s*[:]{2,}\\s*)(?<COMMENT>.*)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex to check if the string is a flag for RoboCopy - These typically will have comments
        /// </summary>
        /// <remarks>
        /// Captured Group Names: <br/>
        /// SWITCH <br/>
        /// DELIMITER <br/>
        /// VALUE <br/>
        /// COMMENT
        /// </remarks>
        private readonly static Regex LINE_IsSwitch = new Regex($"^{RegString_SWITCH}{RegString_COMMENT}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private readonly static Regex LINE_IsSwitch_NoComment = new Regex($"^{RegString_SWITCH}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        
        private readonly static Regex LINE_IsSwitch_NumericValue = new Regex($"^{RegString_SWITCH_NumericValue}{RegString_COMMENT}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private readonly static Regex LINE_IsSwitch_NumericValue_NoComment = new Regex($"^{RegString_SWITCH_NumericValue}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// JobName for ROboCommand is not valid parameter for RoboCopy, so we save it into a comment within the file
        /// </summary>
        /// <remarks>
        /// Captured Group Names: <br/>
        /// FLAG <br/>
        /// NAME <br/>
        /// COMMENT
        /// </remarks>
        private readonly static Regex  JobNameRegex = new Regex("^\\s*(?<FLAG>:: JOB_NAME:)\\s*(?<NAME>.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private readonly static Regex StopIfDisposingRegex = new Regex("^\\s*(?<FLAG>:: StopIfDisposing:)\\s*(?<VALUE>TRUE|FALSE)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex used for parsing File and Directory filters for /IF /XD and /XF flags
        /// </summary>
        /// <remarks>
        /// Captured Group Names: <br/>
        /// PATH <br/>
        /// COMMENT
        /// </remarks>
        private readonly static Regex  DirFileFilterRegex = new Regex($"^\\s*{RegString_FileFilter}{RegString_COMMENT}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private readonly static Regex DirFileFilterRegex_NoComment = new Regex($"^\\s*{RegString_FileFilter}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private const string RegString_FileFilter = "(?<PATH>.+?)";


        #region < Copy Options Regex >

        /// <summary>
        /// Regex to find the SourceDirectory within the JobFile
        /// </summary>
        /// <remarks>
        /// Captured Group Names: <br/>
        /// SWITCH <br/>
        /// PATH <br/>
        /// COMMENT
        /// </remarks>
        private readonly static Regex  CopyOptionsRegex_SourceDir = new Regex("^\\s*(?<SWITCH>/SD:)(?<PATH>.*)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex to find the DestinationDirectory within the JobFile
        /// </summary>
        /// <remarks>
        /// Captured Group Names: <br/>
        /// SWITCH <br/>
        /// PATH <br/>
        /// COMMENT
        /// </remarks>
        private readonly static Regex  CopyOptionsRegex_DestinationDir = new Regex("^\\s*(?<SWITCH>/DD:)(?<PATH>.*)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex to determine if on the INCLUDE FILES section of the JobFile
        /// </summary>
        /// <remarks>
        /// Each new path / filename should be on its own line
        /// </remarks>
        private readonly static Regex  CopyOptionsRegex_IncludeFiles = new Regex("^\\s*(?<SWITCH>/IF)\\s*(.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        #endregion

        #region < Selection Options Regex >

        /// <summary>
        /// Regex to determine if on the EXCLUDE FILES section of the JobFile
        /// </summary>
        /// <remarks>
        /// Each new path / filename should be on its own line
        /// </remarks>
        private readonly static Regex SelectionRegex_ExcludeFiles = new Regex("^\\s*(?<SWITCH>/XF).*", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex to determine if on the EXCLUDE DIRECTORIES section of the JobFile
        /// </summary>
        /// <remarks>
        /// Each new path / filename should be on its own line
        /// </remarks>
        private readonly static Regex SelectionRegex_ExcludeDirs = new Regex("^\\s*(?<SWITCH>/XD).*", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        #endregion

        #endregion

        #region < Methods that feed Main Parse Routine >

        /// <summary>
        /// Read each line using <see cref="FileInfo.OpenText"/> and attempt to produce a Job File. <para/>
        /// If FileExtension != ".RCJ" -> returns null. Otherwise parses the file.
        /// </summary>
        /// <param name="file">FileInfo object for some Job File. File Path should end in .RCJ</param>
        /// <returns></returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static RoboCommand Parse(FileInfo file)
        {
            if (file.Extension != JOBFILE_Extension) return null;
            return Parse(file.OpenText());
        }

        /// <summary>
        /// Use <see cref="File.OpenText(string)"/> to read all lines from the supplied file path. <para/>
        /// If FileExtension != ".RCJ" -> returns null. Otherwise parses the file.
        /// </summary>
        /// <param name="path">File Path to some Job File. File Path should end in .RCJ</param>
        /// <returns></returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static RoboCommand Parse(string path)
        {
            if (Path.GetExtension(path) != JOBFILE_Extension) return null;
            return Parse(File.OpenText(path));
        }

        /// <summary>
        /// Read each line from a StreamReader and attempt to produce a Job File.
        /// </summary>
        /// <param name="streamReader">StreamReader for a file stream that represents a Job File</param>
        /// <returns></returns>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal static RoboCommand Parse(StreamReader streamReader)
        {
            List<string> Lines = new List<string>();
            using (streamReader)
            {
                while (!streamReader.EndOfStream)
                    Lines.Add(streamReader.ReadLine());
            }
            return Parse(Lines);
        }

        #endregion

        #region <> Main Parse Routine >

        /// <summary>
        /// Parse each line in <paramref name="Lines"/>, and attempt to create a new JobFile object.
        /// </summary>
        /// <param name="Lines">String[] read from a JobFile</param>
        /// <returns></returns>
        internal static RoboCommand Parse(IEnumerable<string> Lines)
        {
            //Extract information from the Lines to quicken processing in *OptionsRegex classes
            List<Group> Flags = new List<Group>();
            List<GroupCollection> ValueFlags = new List<GroupCollection>();
            RetryOptions retryOpt = new RetryOptions();

            string JobName = null;
            bool stopIfDisposing = true;

            foreach (string ln in Lines)
            {
                if (ln.IsNullOrWhiteSpace() | ln.Trim() == "::")
                { }
                else if (LINE_IsSwitch.IsMatch(ln) || LINE_IsSwitch_NoComment.IsMatch(ln))
                {
                    var groups = LINE_IsSwitch.Match(ln).Groups;

                    //Check RetryOptions inline since it only has 4 properties to check against
                    if (groups["SWITCH"].Value == "/R" && LINE_IsSwitch_NumericValue.IsMatch(ln))
                    {
                        string val = LINE_IsSwitch_NumericValue.Match(ln).Groups["VALUE"].Value;
                        retryOpt.RetryCount = val.IsNullOrWhiteSpace() ? retryOpt.RetryCount : Convert.ToInt32(val);
                    }
                    else if (groups["SWITCH"].Value == "/W")
                    {
                        string val = LINE_IsSwitch_NumericValue.Match(ln).Groups["VALUE"].Value;
                        retryOpt.RetryWaitTime = val.IsNullOrWhiteSpace() ? retryOpt.RetryWaitTime : Convert.ToInt32(val);
                    }
                    else if (groups["SWITCH"].Value == "/REG")
                    {
                        retryOpt.SaveToRegistry = true;
                    }
                    else if (groups["SWITCH"].Value == "/TBD")
                    {
                        retryOpt.WaitForSharenames = true;
                    }
                    //All Other flags
                    else
                    {
                        Flags.Add(groups["SWITCH"]);
                        if (groups["DELIMITER"].Success)
                            ValueFlags.Add(groups);
                    }
                }
                else if (JobName == null && JobNameRegex.IsMatch(ln))
                {
                    JobName = JobNameRegex.Match(ln).Groups["NAME"].Value.Trim();
                }
                else if (StopIfDisposingRegex.IsMatch(ln))
                {
                    stopIfDisposing = Convert.ToBoolean(StopIfDisposingRegex.Match(ln).Groups["VALUE"].Value);
                }
                
            }

            CopyOptions copyOpt = Build_CopyOptions(Flags, ValueFlags, Lines);
            SelectionOptions selectionOpt = Build_SelectionOptions(Flags, ValueFlags, Lines);
            LoggingOptions loggingOpt = Build_LoggingOptions(Flags, ValueFlags, Lines);
            JobOptions jobOpt = Build_JobOptions(Flags, ValueFlags, Lines);

            return new RoboCommand(JobName ?? "", StopIfDisposing: stopIfDisposing, source: null, destination: null, configuration: null,
                copyOptions: copyOpt,
                selectionOptions: selectionOpt,
                retryOptions: retryOpt,
                loggingOptions: loggingOpt,
                jobOptions: jobOpt);
        }

        #endregion

        #region < Copy Options >

        /// <summary>
        /// Parser to create CopyOptions object for JobFiles
        /// </summary>
        private static CopyOptions Build_CopyOptions(IEnumerable<Group> Flags, IEnumerable<GroupCollection> ValueFlags, IEnumerable<string> Lines)
        {
            var options = new CopyOptions();

            //Bool Checks 
            options.CheckPerFile = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.CHECK_PER_FILE.Trim());
            options.CopyAll = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.COPY_ALL.Trim());
            options.CopyFilesWithSecurity = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.COPY_FILES_WITH_SECURITY.Trim());
            options.CopySubdirectories = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.COPY_SUBDIRECTORIES.Trim());
            options.CopySubdirectoriesIncludingEmpty = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.COPY_SUBDIRECTORIES_INCLUDING_EMPTY.Trim());
            options.CopySymbolicLink = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.COPY_SYMBOLIC_LINK.Trim());
            options.CreateDirectoryAndFileTree = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.CREATE_DIRECTORY_AND_FILE_TREE.Trim());
            options.DoNotCopyDirectoryInfo = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.DO_NOT_COPY_DIRECTORY_INFO.Trim());
            options.DoNotUseWindowsCopyOffload = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.DO_NOT_USE_WINDOWS_COPY_OFFLOAD.Trim());
            options.EnableBackupMode = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.ENABLE_BACKUP_MODE.Trim());
            options.EnableEfsRawMode = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.ENABLE_EFSRAW_MODE.Trim());
            options.EnableRestartMode = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.ENABLE_RESTART_MODE.Trim());
            options.EnableRestartModeWithBackupFallback = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.ENABLE_RESTART_MODE_WITH_BACKUP_FALLBACK.Trim());
            options.FatFiles = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.FAT_FILES.Trim());
            options.FixFileSecurityOnAllFiles = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.FIX_FILE_SECURITY_ON_ALL_FILES.Trim());
            options.FixFileTimesOnAllFiles = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.FIX_FILE_TIMES_ON_ALL_FILES.Trim());
            options.Mirror = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.MIRROR.Trim());
            options.MoveFiles = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.MOVE_FILES.Trim());
            options.MoveFilesAndDirectories = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.MOVE_FILES_AND_DIRECTORIES.Trim());
            options.Purge = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.PURGE.Trim());
            options.RemoveFileInformation = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.REMOVE_FILE_INFORMATION.Trim());
            options.TurnLongPathSupportOff = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.TURN_LONG_PATH_SUPPORT_OFF.Trim());
            options.UseUnbufferedIo = Flags.Any(flag => flag.Success && flag.Value == CopyOptions.USE_UNBUFFERED_IO.Trim());

            //int / string values on same line as flag
            foreach (var match in ValueFlags)
            {
                string flag = match["SWITCH"].Value.Trim();
                string value = match["VALUE"].Value.Trim();

                switch (flag)
                {
                    case "/A+":
                        options.AddAttributes = value;
                        break;
                    case "/COPY":
                        options.CopyFlags = value;
                        break;
                    case "/LEV":
                        options.Depth = value.TryConvertInt();
                        break;
                    case "/DD":
                        options.Destination = value;
                        break;
                    case "/DCOPY":
                        options.DirectoryCopyFlags = value;
                        break;
                    case "/IPG":
                        options.InterPacketGap = value.TryConvertInt();
                        break;
                    case "/MON":
                        options.MonitorSourceChangesLimit = value.TryConvertInt();
                        break;
                    case "/MOT":
                        options.MonitorSourceTimeLimit = value.TryConvertInt();
                        break;
                    case "/MT":
                        options.MultiThreadedCopiesCount = value.TryConvertInt();
                        break;
                    case "/A-":
                        options.RemoveAttributes = value;
                        break;
                    case "/RH":
                        options.RunHours = value;
                        break;
                    case "/SD":
                        options.Source = value;
                        break;
                }
            }

            //Multiple Lines
            if (Flags.Any(f => f.Value == "/IF"))
            {
                bool parsingIF = false;
                List<string> filters = new List<string>();
                string path = null;
                //Find the line that starts with the flag
                foreach (string ln in Lines)
                {
                    if (ln.IsNullOrWhiteSpace() || LINE_IsComment.IsMatch(ln))
                    { }
                    else if (LINE_IsSwitch.IsMatch(ln))
                    {
                        if (parsingIF) break; //Moving onto next section -> IF already parsed.
                        parsingIF = ln.Trim().StartsWith("/IF");
                    }
                    else if (parsingIF)
                    {
                        //React to parsing the section - Comments are not expected on these lines
                        path = null;
                        if (DirFileFilterRegex.IsMatch(ln))
                        {
                            path = DirFileFilterRegex.Match(ln).Groups["PATH"].Value;
                        }
                        else if (DirFileFilterRegex_NoComment.IsMatch(ln))
                        {
                            path = DirFileFilterRegex_NoComment.Match(ln).Groups["PATH"].Value;
                        }
                        //Store the value
                        if (!path.IsNullOrWhiteSpace())
                        {
                            filters.Add(path.WrapPath());
                        }
                    }
                }
                if (filters.Count > 0) options.FileFilter = filters;
            } //End of FileFilter section

            return options;
        }

        #endregion

        #region < Selection Options >

        /// <summary>
        /// Parser to create SelectionOptions object for JobFiles
        /// </summary>
        private static SelectionOptions Build_SelectionOptions(IEnumerable<Group> Flags, IEnumerable<GroupCollection> ValueFlags, IEnumerable<string> Lines)
        {
            var options = new SelectionOptions();

            //Bool Checks 
            options.CompensateForDstDifference = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.COMPENSATE_FOR_DST_DIFFERENCE.Trim());
            options.ExcludeChanged = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.EXCLUDE_CHANGED.Trim());
            options.ExcludeExtra = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.EXCLUDE_EXTRA.Trim());
            options.ExcludeJunctionPoints = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.EXCLUDE_JUNCTION_POINTS.Trim());
            options.ExcludeJunctionPointsForDirectories = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.EXCLUDE_JUNCTION_POINTS_FOR_DIRECTORIES.Trim());
            options.ExcludeJunctionPointsForFiles = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.EXCLUDE_JUNCTION_POINTS_FOR_FILES.Trim());
            options.ExcludeLonely = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.EXCLUDE_LONELY.Trim());
            options.ExcludeNewer = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.EXCLUDE_NEWER.Trim());
            options.ExcludeOlder = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.EXCLUDE_OLDER.Trim());
            options.IncludeSame = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.INCLUDE_SAME.Trim());
            options.IncludeTweaked = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.INCLUDE_TWEAKED.Trim());
            options.OnlyCopyArchiveFiles = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.ONLY_COPY_ARCHIVE_FILES.Trim());
            options.OnlyCopyArchiveFilesAndResetArchiveFlag = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.ONLY_COPY_ARCHIVE_FILES_AND_RESET_ARCHIVE_FLAG.Trim());
            options.UseFatFileTimes = Flags.Any(flag => flag.Success && flag.Value == SelectionOptions.USE_FAT_FILE_TIMES.Trim());

            //int / string values on same line as flag
            foreach (var match in ValueFlags)
            {
                string flag = match["SWITCH"].Value;
                string value = match["VALUE"].Value;

                switch (flag)
                {
                    case "/XA":
                        options.ExcludeAttributes = value;
                        break;
                    case "/IA":
                        options.IncludeAttributes = value;
                        break;
                    case "/MAXAGE":
                        options.MaxFileAge = value;
                        break;
                    case "/MAX":
                        options.MaxFileSize = value.TryConvertLong();
                        break;
                    case "/MAXLAD":
                        options.MaxLastAccessDate = value;
                        break;
                    case "/MINAGE":
                        options.MinFileAge = value;
                        break;
                    case "/MIN":
                        options.MinFileSize = value.TryConvertLong();
                        break;
                    case "/MINLAD":
                        options.MinLastAccessDate = value;
                        break;
                }
            }

            //Multiple Lines
            bool parsingXD = false;
            bool parsingXF = false;
            bool xDParsed = false;
            bool xFParsed = false;
            string path = null;

            foreach (string ln in Lines)
            {
                // Determine if parsing some section
                if (ln.IsNullOrWhiteSpace() || LINE_IsComment.IsMatch(ln) )
                { }
                else if (!xFParsed && !parsingXF && SelectionRegex_ExcludeFiles.IsMatch(ln))
                {
                    // Paths are not expected to be on this output line
                    parsingXF = true;
                    parsingXD = false;
                }
                else if (!xDParsed && !parsingXD && SelectionRegex_ExcludeDirs.IsMatch(ln))
                {
                    // Paths are not expected to be on this output line
                    parsingXF = false;
                    parsingXD = true;
                }
                else if (LINE_IsSwitch.IsMatch(ln) || LINE_IsSwitch_NoComment.IsMatch(ln))
                {
                    if (parsingXD)
                    {
                        parsingXD = false;
                        xDParsed = true;
                    }
                    if (parsingXF)
                    {
                        parsingXF = false;
                        xFParsed = true;
                    }
                    if (xDParsed && xFParsed) break;
                }
                else
                {
                    //React to parsing the section - Comments are not expected on these lines
                    path = null;
                    if (DirFileFilterRegex.IsMatch(ln))
                    {
                        path = DirFileFilterRegex.Match(ln).Groups["PATH"].Value;
                    }
                    else if (DirFileFilterRegex_NoComment.IsMatch(ln))
                    {
                        path = DirFileFilterRegex_NoComment.Match(ln).Groups["PATH"].Value;
                    }
                    //Store the value
                    if (!path.IsNullOrWhiteSpace())
                    {
                        if (parsingXF)
                            options.ExcludedFiles.Add(path.WrapPath());
                        else if (parsingXD)
                            options.ExcludedDirectories.Add(path.WrapPath());
                    }

                }
            }
            return options;
        }

        #endregion

        #region < Logging Options >

        /// <summary>
        /// Parser to create LoggingOptions object for JobFiles
        /// </summary>
        private static LoggingOptions Build_LoggingOptions(IEnumerable<Group> Flags, IEnumerable<GroupCollection> ValueFlags, IEnumerable<string> Lines)
        {
            var options = new LoggingOptions();

            //Bool Checks 
            options.IncludeFullPathNames = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.INCLUDE_FULL_PATH_NAMES.Trim());
            options.IncludeSourceTimeStamps = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.INCLUDE_SOURCE_TIMESTAMPS.Trim());
            options.ListOnly = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.LIST_ONLY.Trim());
            options.NoDirectoryList = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.NO_DIRECTORY_LIST.Trim());
            options.NoFileClasses = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.NO_FILE_CLASSES.Trim());
            options.NoFileList = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.NO_FILE_LIST.Trim());
            options.NoFileSizes = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.NO_FILE_SIZES.Trim());
            options.NoJobHeader = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.NO_JOB_HEADER.Trim());
            options.NoJobSummary = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.NO_JOB_SUMMARY.Trim());
            options.NoProgress = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.NO_PROGRESS.Trim());
            options.OutputAsUnicode = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.OUTPUT_AS_UNICODE.Trim());
            options.OutputToRoboSharpAndLog = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.OUTPUT_TO_ROBOSHARP_AND_LOG.Trim());
            options.PrintSizesAsBytes = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.PRINT_SIZES_AS_BYTES.Trim());
            options.ReportExtraFiles = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.REPORT_EXTRA_FILES.Trim());
            options.ShowEstimatedTimeOfArrival = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.SHOW_ESTIMATED_TIME_OF_ARRIVAL.Trim());
            options.VerboseOutput = Flags.Any(flag => flag.Success && flag.Value == LoggingOptions.VERBOSE_OUTPUT.Trim());

            //int / string values on same line as flag
            foreach (var match in ValueFlags)
            {
                string flag = match["SWITCH"].Value;
                string value = match["VALUE"].Value;

                switch (flag)
                {
                    case "/LOG+":
                        options.AppendLogPath = value;
                        break;
                    case "/UNILOG+":
                        options.AppendUnicodeLogPath = value;
                        break;
                    case "/LOG":
                        options.LogPath = value;
                        break;
                    case "/UNILOG":
                        options.UnicodeLogPath = value;
                        break;
                }
            }

            return options;
        }

        #endregion

        #region < Job Options >

        /// <summary>
        /// Parser to create JobOptions object for JobFiles
        /// </summary>
        private static JobOptions Build_JobOptions(IEnumerable<Group> Flags, IEnumerable<GroupCollection> ValueFlags, IEnumerable<string> Lines)
        {
            return new JobOptions();
        }

        #endregion
    }
}
