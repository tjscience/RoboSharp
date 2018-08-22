using System.Text;

namespace RoboSharp
{
    public class CopyOptions
    {
        #region Option Constants

        internal const string COPY_SUBDIRECTORIES = "/S ";
        internal const string COPY_SUBDIRECTORIES_INCLUDING_EMPTY = "/E ";
        internal const string DEPTH = "/LEV:{0} ";
        internal const string ENABLE_RESTART_MODE = "/Z ";
        internal const string ENABLE_BACKUP_MODE = "/B ";
        internal const string ENABLE_RESTART_MODE_WITH_BACKUP_FALLBACK = "/ZB ";
        internal const string USE_UNBUFFERED_IO = "/J ";
        internal const string ENABLE_EFSRAW_MODE = "/EFSRAW ";
        internal const string COPY_FLAGS = "/COPY:{0} ";
        internal const string COPY_FILES_WITH_SECURITY = "/SEC ";
        internal const string COPY_ALL = "/COPYALL ";
        internal const string REMOVE_FILE_INFORMATION = "/NOCOPY ";
        internal const string FIX_FILE_SECURITY_ON_ALL_FILES = "/SECFIX ";
        internal const string FIX_FILE_TIMES_ON_ALL_FILES = "/TIMFIX ";
        internal const string PURGE = "/PURGE ";
        internal const string MIRROR = "/MIR ";
        internal const string MOVE_FILES = "/MOV ";
        internal const string MOVE_FILES_AND_DIRECTORIES = "/MOVE ";
        internal const string ADD_ATTRIBUTES = "/A+:{0} ";
        internal const string REMOVE_ATTRIBUTES = "/A-:{0} ";
        internal const string CREATE_DIRECTORY_AND_FILE_TREE = "/CREATE ";
        internal const string FAT_FILES = "/FAT ";
        internal const string TURN_LONG_PATH_SUPPORT_OFF = "/256 ";
        internal const string MONITOR_SOURCE_CHANGES_LIMIT = "/MON:{0} ";
        internal const string MONITOR_SOURCE_TIME_LIMIT = "/MOT:{0} ";
        internal const string RUN_HOURS = "/RH:{0} ";
        internal const string CHECK_PER_FILE = "/PF ";
        internal const string INTER_PACKET_GAP = "/IPG:{0} ";
        internal const string COPY_SYMBOLIC_LINK = "/SL ";
        internal const string MULTITHREADED_COPIES_COUNT = "/MT:{0} ";
        internal const string DIRECTORY_COPY_FLAGS = "/DCOPY:{0} ";
        internal const string DO_NOT_COPY_DIRECTORY_INFO = "/NODCOPY ";
        internal const string DO_NOT_USE_WINDOWS_COPY_OFFLOAD = "/NOOFFLOAD ";

        #endregion Option Constants

        #region Option Defaults

        private string fileFilter = "*.*";
        private string copyFlags = "DAT";
        private string directoryCopyFlags = VersionManager.Version >= 6.2 ? "DA" : "T";

        #endregion Option Defaults

        #region Public Properties

        /// <summary>
        /// The source file path where the RoboCommand is copying files from.
        /// </summary>
        private string _source;
        public string Source { get { return _source; } set { _source = value.CleanDirectoryPath(); } }    
        /// <summary>
        /// The destination file path where the RoboCommand is copying files to.
        /// </summary>
        private string _destination;
        public string Destination { get { return _destination; } set { _destination = value.CleanDirectoryPath(); } }
        /// <summary>
        /// Allows you to supply a specific file to copy or use wildcard characters (* or ?).
        /// </summary>
        public string FileFilter
        {
            get
            {
                return fileFilter;
            }
            set
            {
                fileFilter = value;
            }
        }

        /// <summary>
        /// Copies subdirectories. Note that this option excludes empty directories. 
        /// [/S]
        /// </summary>
        public bool CopySubdirectories { get; set; }
        /// <summary>
        /// Copies subdirectories. Note that this option includes empty directories. 
        /// [/E]
        /// </summary>
        public bool CopySubdirectoriesIncludingEmpty { get; set; }
        /// <summary>
        /// Copies only the top N levels of the source directory tree. The default is 
        /// zero which does not limit the depth. 
        /// [/LEV:N]
        /// </summary>
        public int Depth { get; set; }
        /// <summary>
        /// Copies files in Restart mode. 
        /// [/Z]
        /// </summary>
        public bool EnableRestartMode { get; set; }
        /// <summary>
        /// Copies files in Backup mode. 
        /// [/B]
        /// </summary>
        public bool EnableBackupMode { get; set; }
        /// <summary>
        /// Uses Restart mode. If access is denied, this option uses Backup mode. 
        /// [/ZB]
        /// </summary>
        public bool EnableRestartModeWithBackupFallback { get; set; }
        /// <summary>
        /// Copy using unbuffered I/O (recommended for large files).
        /// [/J]
        /// </summary>
        public bool UseUnbufferedIo { get; set; }
        /// <summary>
        /// Copies all encrypted files in EFS RAW mode. 
        /// [/EFSRAW]
        /// </summary>
        public bool EnableEfsRawMode { get; set; }
        /// <summary>
        /// This property should be set to a string consisting of all the flags to include (eg. DAT; DATSOU)
        /// Specifies the file properties to be copied. The following are the valid values for this option:
        ///D Data
        ///A Attributes
        ///T Time stamps
        ///S NTFS access control list (ACL)
        ///O Owner information
        ///U Auditing information
        ///The default value for copyflags is DA (data, attributes, and time stamps).
        ///[/COPY:copyflags]
        /// </summary>
        public string CopyFlags
        {
            get
            {
                return copyFlags;
            }
            set
            {
                copyFlags = value;
            }
        }
        /// <summary>
        /// Copies files with security (equivalent to /copy:DAT).
        /// [/SEC]
        /// </summary>
        public bool CopyFilesWithSecurity { get; set; }
        /// <summary>
        /// Copies all file information (equivalent to /copy:DATSOU).
        /// [/COPYALL]
        /// </summary>
        public bool CopyAll { get; set; }
        /// <summary>
        /// Copies no file information (useful with Purge option).
        /// [/NOCOPY]
        /// </summary>
        public bool RemoveFileInformation { get; set; }
        /// <summary>
        /// Fixes file security on all files, even skipped ones.
        /// [/SECFIX]
        /// </summary>
        public bool FixFileSecurityOnAllFiles { get; set; }
        /// <summary>
        /// Fixes file times on all files, even skipped ones.
        /// [/TIMFIX]
        /// </summary>
        public bool FixFileTimesOnAllFiles { get; set; }
        /// <summary>
        /// Deletes destination files and directories that no longer exist in the source.
        /// [/PURGE]
        /// </summary>
        public bool Purge { get; set; }
        /// <summary>
        /// Mirrors a directory tree (equivalent to CopySubdirectoriesIncludingEmpty plus Purge).
        /// [/MIR]
        /// </summary>
        public bool Mirror { get; set; }
        /// <summary>
        /// Moves files, and deletes them from the source after they are copied.
        /// [/MOV]
        /// </summary>
        public bool MoveFiles { get; set; }
        /// <summary>
        /// Moves files and directories, and deletes them from the source after they are copied.
        /// [/MOVE]
        /// </summary>
        public bool MoveFilesAndDirectories { get; set; }
        /// <summary>
        /// This property should be set to a string consisting of all the attributes to add (eg. AH; RASHCNET).
        /// Adds the specified attributes to copied files.
        /// [/A+:attributes]
        /// </summary>
        public string AddAttributes { get; set; }
        /// <summary>
        /// This property should be set to a string consisting of all the attributes to remove (eg. AH; RASHCNET).
        /// Adds the specified attributes to copied files.
        /// [/A-:attributes]
        /// </summary>
        public string RemoveAttributes { get; set; }
        /// <summary>
        /// Creates a directory tree and zero-length files only.
        /// [/CREATE]
        /// </summary>
        public bool CreateDirectoryAndFileTree { get; set; }
        /// <summary>
        /// Creates destination files by using 8.3 character-length FAT file names only.
        /// [/FAT]
        /// </summary>
        public bool FatFiles { get; set; }
        /// <summary>
        /// Turns off support for very long paths (longer than 256 characters).
        /// [/256]
        /// </summary>
        public bool TurnLongPathSupportOff { get; set; }
        /// <summary>
        /// The default value of zero indicates that you do not wish to monitor for changes.
        /// Monitors the source, and runs again when more than N changes are detected.
        /// [/MON:N]
        /// </summary>
        public int MonitorSourceChangesLimit { get; set; }
        /// <summary>
        /// The default value of zero indicates that you do not wish to monitor for changes.
        /// Monitors source, and runs again in M minutes if changes are detected.
        /// [/MOT:M]
        /// </summary>
        public int MonitorSourceTimeLimit { get; set; }
        /// <summary>
        /// Specifies run times when new copies may be started.
        /// [/rh:hhmm-hhmm]
        /// </summary>
        public string RunHours { get; set; }
        /// <summary>
        /// Checks run times on a per-file (not per-pass) basis.
        /// [/PF]
        /// </summary>
        public bool CheckPerFile { get; set; }
        /// <summary>
        /// The default value of zero indicates that this feature is turned off.
        /// Specifies the inter-packet gap to free bandwidth on slow lines.
        /// [/IPG:N]
        /// </summary>
        public int InterPacketGap { get; set; }
        /// <summary>
        /// Copies the symbolic link instead of the target.
        /// [/SL]
        /// </summary>
        public bool CopySymbolicLink { get; set; }
        /// <summary>
        /// The default value of zero indicates that this feature is turned off.
        /// Creates multi-threaded copies with N threads. Must be an integer between 1 and 128.
        /// The MultiThreadedCopiesCount parameter cannot be used with the /IPG and EnableEfsRawMode parameters.
        /// [/MT:N]
        /// </summary>
        public int MultiThreadedCopiesCount { get; set; }
        /// <summary>
        /// What to copy for directories (default is DA).
        /// (copyflags: D=Data, A=Attributes, T=Timestamps).
        /// [/DCOPY:copyflags]
        /// </summary>
        public string DirectoryCopyFlags
        {
            get { return directoryCopyFlags; }
            set { directoryCopyFlags = value; }
        }
        /// <summary>
        /// Do not copy any directory info.
        /// [/NODCOPY]
        /// </summary>
        public bool DoNotCopyDirectoryInfo { get; set; }
        /// <summary>
        /// Copy files without using the Windows Copy Offload mechanism.
        /// [/NOOFFLOAD]
        /// </summary>
        public bool DoNotUseWindowsCopyOffload { get; set; }

        #endregion Public Properties

        internal string Parse()
        {
            Debugger.Instance.DebugMessage("Parsing CopyOptions...");
            var version = VersionManager.Version;
            var options = new StringBuilder();

            // Set Source, Destination and FileFilter
            options.Append($"\"{Source}\" ");
            options.Append($"\"{Destination}\" ");
            options.Append($"\"{FileFilter}\" ");
            Debugger.Instance.DebugMessage(string.Format("Parsing CopyOptions progress ({0}).", options.ToString()));

            #region Set Options
            var cleanedCopyFlags = CopyFlags.CleanOptionInput();
            var cleanedDirectoryCopyFlags = DirectoryCopyFlags.CleanOptionInput();

            if (!cleanedCopyFlags.IsNullOrWhiteSpace())
            {
                options.Append(string.Format(COPY_FLAGS, cleanedCopyFlags));
                Debugger.Instance.DebugMessage(string.Format("Parsing CopyOptions progress ({0}).", options.ToString()));
            }
            if (!cleanedDirectoryCopyFlags.IsNullOrWhiteSpace() && version >= 5.1260026)
            {
                options.Append(string.Format(DIRECTORY_COPY_FLAGS, cleanedDirectoryCopyFlags));
                Debugger.Instance.DebugMessage(string.Format("Parsing CopyOptions progress ({0}).", options.ToString()));
            }
            if (CopySubdirectories)
            {
                options.Append(COPY_SUBDIRECTORIES);
                Debugger.Instance.DebugMessage(string.Format("Parsing CopyOptions progress ({0}).", options.ToString()));
            }
            if (CopySubdirectoriesIncludingEmpty)
                options.Append(COPY_SUBDIRECTORIES_INCLUDING_EMPTY);
            if (Depth > 0)
                options.Append(string.Format(DEPTH, Depth));
            if (EnableRestartMode)
                options.Append(ENABLE_RESTART_MODE);
            if (EnableBackupMode)
                options.Append(ENABLE_BACKUP_MODE);
            if (EnableRestartModeWithBackupFallback)
                options.Append(ENABLE_RESTART_MODE_WITH_BACKUP_FALLBACK);
            if (UseUnbufferedIo && version >= 6.2)
                options.Append(USE_UNBUFFERED_IO);
            if (EnableEfsRawMode)
                options.Append(ENABLE_EFSRAW_MODE);
            if (CopyFilesWithSecurity)
                options.Append(COPY_FILES_WITH_SECURITY);
            if (CopyAll)
                options.Append(COPY_ALL);
            if (RemoveFileInformation)
                options.Append(REMOVE_FILE_INFORMATION);
            if (FixFileSecurityOnAllFiles)
                options.Append(FIX_FILE_SECURITY_ON_ALL_FILES);
            if (FixFileTimesOnAllFiles)
                options.Append(FIX_FILE_TIMES_ON_ALL_FILES);
            if (Purge)
                options.Append(PURGE);
            if (Mirror)
                options.Append(MIRROR);
            if (MoveFiles)
                options.Append(MOVE_FILES);
            if (MoveFilesAndDirectories)
                options.Append(MOVE_FILES_AND_DIRECTORIES);
            if (!AddAttributes.IsNullOrWhiteSpace())
                options.Append(string.Format(ADD_ATTRIBUTES, AddAttributes.CleanOptionInput()));
            if (!RemoveAttributes.IsNullOrWhiteSpace())
                options.Append(string.Format(REMOVE_ATTRIBUTES, RemoveAttributes.CleanOptionInput()));
            if (CreateDirectoryAndFileTree)
                options.Append(CREATE_DIRECTORY_AND_FILE_TREE);
            if (FatFiles)
                options.Append(FAT_FILES);
            if (TurnLongPathSupportOff)
                options.Append(TURN_LONG_PATH_SUPPORT_OFF);
            if (MonitorSourceChangesLimit > 0)
                options.Append(string.Format(MONITOR_SOURCE_CHANGES_LIMIT, MonitorSourceChangesLimit));
            if (MonitorSourceTimeLimit > 0)
                options.Append(string.Format(MONITOR_SOURCE_TIME_LIMIT, MonitorSourceTimeLimit));
            if (!RunHours.IsNullOrWhiteSpace())
                options.Append(string.Format(RUN_HOURS, RunHours.CleanOptionInput()));
            if (CheckPerFile)
                options.Append(CHECK_PER_FILE);
            if (InterPacketGap > 0)
                options.Append(string.Format(INTER_PACKET_GAP, InterPacketGap));
            if (CopySymbolicLink)
                options.Append(COPY_SYMBOLIC_LINK);
            if (MultiThreadedCopiesCount > 0)
                options.Append(string.Format(MULTITHREADED_COPIES_COUNT, MultiThreadedCopiesCount));
            if (DoNotCopyDirectoryInfo && version >= 6.2)
                options.Append(DO_NOT_COPY_DIRECTORY_INFO);
            if (DoNotUseWindowsCopyOffload && version >= 6.2)
                options.Append(DO_NOT_USE_WINDOWS_COPY_OFFLOAD);
            #endregion Set Options

            var parsedOptions = options.ToString();
            Debugger.Instance.DebugMessage(string.Format("CopyOptions parsed ({0}).", parsedOptions));
            return parsedOptions;
        }
    }
}
