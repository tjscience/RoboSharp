using System.Text;

namespace RoboSharp
{
    public class SelectionOptions
    {
        #region Option Constants

        private const string ONLY_COPY_ARCHIVE_FILES = "/A ";
        private const string ONLY_COPY_ARCHIVE_FILES_AND_RESET_ARCHIVE_FLAG = "/M ";
        private const string INCLUDE_ATTRIBUTES = "/IA:{0} ";
        private const string EXCLUDE_ATTRIBUTES = "/XA:{0} ";
        private const string EXCLUDE_FILES = "/XF {0} ";
        private const string EXCLUDE_DIRECTORIES = "/XD {0} ";
        private const string EXCLUDE_CHANGED = "/XC ";
        private const string EXCLUDE_NEWER = "/XN ";
        private const string EXCLUDE_OLDER = "/XO ";
        private const string EXCLUDE_EXTRA = "/XX ";
        private const string EXCLUDE_LONELY = "/XL ";
        private const string INCLUDE_SAME = "/IS ";
        private const string INCLUDE_TWEAKED = "/IT ";
        private const string MAX_FILE_SIZE = "/MAX:{0} ";
        private const string MIN_FILE_SIZE = "/MIN:{0} ";
        private const string MAX_FILE_AGE = "/MAXAGE:{0} ";
        private const string MIN_FILE_AGE = "/MINAGE:{0} ";
        private const string MAX_LAST_ACCESS_DATE = "/MAXLAD:{0} ";
        private const string MIN_LAST_ACCESS_DATE = "/MINLAD:{0} ";
        private const string EXCLUDE_JUNCTION_POINTS = "/XJ ";
        private const string USE_FAT_FILE_TIMES = "/FFT ";
        private const string COMPENSATE_FOR_DST_DIFFERENCE = "/DST ";
        private const string EXCLUDE_JUNCTION_POINTS_FOR_DIRECTORIES = "/XJD ";
        private const string EXCLUDE_JUNCTION_POINTS_FOR_FILES = "/XJF ";

        #endregion Option Constants

        #region Public Properties

        /// <summary>
        /// Copies only files for which the Archive attribute is set.
        /// [/A]
        /// </summary>
        public bool OnlyCopyArchiveFiles { get; set; }
        /// <summary>
        /// Copies only files for which the Archive attribute is set, and resets the Archive attribute.
        /// [/M]
        /// </summary>
        public bool OnlyCopyArchiveFilesAndResetArchiveFlag { get; set; }
        /// <summary>
        /// This property should be set to a string consisting of all the attributes to include (eg. AH; RASHCNETO).
        /// Includes only files for which any of the specified attributes are set.
        /// [/IA:attributes]
        /// </summary>
        public string IncludeAttributes { get; set; }
        /// <summary>
        /// This property should be set to a string consisting of all the attributes to exclude (eg. AH; RASHCNETO).
        /// Excludes files for which any of the specified attributes are set.
        /// [/XA:attributes]
        /// </summary>
        public string ExcludeAttributes { get; set; }
        /// <summary>
        /// Files should be separated by spaces.
        /// Excludes files that match the specified names or paths. Note that FileName can include wildcard characters (* and ?).
        /// [/XF File File ...]
        /// </summary>
        public string ExcludeFiles { get; set; }
        /// <summary>
        /// Directories should be separated by spaces.
        /// Excludes directories that match the specified names or paths.
        /// [/XD Directory Directory ...]
        /// </summary>
        public string ExcludeDirectories { get; set; }
        /// <summary>
        /// Excludes changed files.
        /// [/XC]
        /// </summary>
        public bool ExcludeChanged { get; set; }
        /// <summary>
        /// Excludes newer files.
        /// [/XN]
        /// </summary>
        public bool ExcludeNewer { get; set; }
        /// <summary>
        /// Excludes older files.
        /// [/XO]
        /// </summary>
        public bool ExcludeOlder { get; set; }
        /// <summary>
        /// Excludes extra files and directories.
        /// [/XX]
        /// </summary>
        public bool ExcludeExtra { get; set; }
        /// <summary>
        /// Excludes lonely files and directories.
        /// [/XL]
        /// </summary>
        public bool ExcludeLonely { get; set; }
        /// <summary>
        /// Includes the same files.
        /// [/IS]
        /// </summary>
        public bool IncludeSame { get; set; }
        /// <summary>
        /// Includes tweaked files.
        /// [/IT]
        /// </summary>
        public bool IncludeTweaked { get; set; }
        /// <summary>
        /// Zero indicates that this feature is turned off.
        /// Specifies the maximum file size (to exclude files bigger than N bytes).
        /// [/MAX:N]
        /// </summary>
        public long MaxFileSize { get; set; }
        /// <summary>
        /// Zero indicates that this feature is turned off.
        /// Specifies the minimum file size (to exclude files smaller than N bytes).
        /// [/MIN:N]
        /// </summary>
        public long MinFileSize { get; set; }
        /// <summary>
        /// Specifies the maximum file age (to exclude files older than N days or date).
        /// [/MAXAGE:N OR YYYYMMDD]
        /// </summary>
        public string MaxFileAge { get; set; }
        /// <summary>
        /// Specifies the minimum file age (exclude files newer than N days or date).
        /// [/MINAGE:N OR YYYYMMDD]
        /// </summary>
        public string MinFileAge { get; set; }
        /// <summary>
        /// Specifies the maximum last access date (excludes files unused since Date).
        /// [/MAXLAD:YYYYMMDD]
        /// </summary>
        public string MaxLastAccessDate { get; set; }
        /// <summary>
        /// Specifies the minimum last access date (excludes files used since N) If N is less 
        /// than 1900, N specifies the number of days. Otherwise, N specifies a date 
        /// in the format YYYYMMDD.
        /// [/MAXLAD:N or YYYYMMDD]
        /// </summary>
        public string MinLastAccessDate { get; set; }
        /// <summary>
        /// Excludes junction points, which are normally included by default.
        /// [/XJ]
        /// </summary>
        public bool ExcludeJunctionPoints { get; set; }
        /// <summary>
        /// Assumes FAT file times (two-second precision).
        /// [/FFT]
        /// </summary>
        public bool UseFatFileTimes { get; set; }
        /// <summary>
        /// Compensates for one-hour DST time differences.
        /// [/DST]
        /// </summary>
        public bool CompensateForDstDifference { get; set; }
        /// <summary>
        /// Excludes junction points for directories.
        /// [/XJD]
        /// </summary>
        public bool ExcludeJunctionPointsForDirectories { get; set; }
        /// <summary>
        /// Excludes junction points for files.
        /// [/XJF]
        /// </summary>
        public bool ExcludeJunctionPointsForFiles { get; set; }

        #endregion Public Properties

        internal string Parse()
        {
            var options = new StringBuilder();

            #region Set Options

            if (OnlyCopyArchiveFiles)
                options.Append(ONLY_COPY_ARCHIVE_FILES);
            if (OnlyCopyArchiveFilesAndResetArchiveFlag)
                options.Append(ONLY_COPY_ARCHIVE_FILES_AND_RESET_ARCHIVE_FLAG);
            if (!string.IsNullOrWhiteSpace(IncludeAttributes))
                options.Append(string.Format(INCLUDE_ATTRIBUTES, IncludeAttributes.CleanOptionInput()));
            if (!string.IsNullOrWhiteSpace(ExcludeAttributes))
                options.Append(string.Format(EXCLUDE_ATTRIBUTES, ExcludeAttributes.CleanOptionInput()));
            if (!string.IsNullOrWhiteSpace(ExcludeFiles))
                options.Append(string.Format(EXCLUDE_FILES, ExcludeFiles));
            if (!string.IsNullOrWhiteSpace(ExcludeDirectories))
                options.Append(string.Format(EXCLUDE_DIRECTORIES, ExcludeDirectories));
            if (ExcludeChanged)
                options.Append(EXCLUDE_CHANGED);
            if (ExcludeNewer)
                options.Append(EXCLUDE_NEWER);
            if (ExcludeOlder)
                options.Append(EXCLUDE_OLDER);
            if (ExcludeExtra)
                options.Append(EXCLUDE_EXTRA);
            if (ExcludeLonely)
                options.Append(EXCLUDE_LONELY);
            if (IncludeSame)
                options.Append(INCLUDE_SAME);
            if (IncludeTweaked)
                options.Append(INCLUDE_TWEAKED);
            if (MaxFileSize > 0)
                options.Append(string.Format(MAX_FILE_SIZE, MaxFileSize));
            if (MinFileSize > 0)
                options.Append(string.Format(MIN_FILE_SIZE, MinFileSize));
            if (!string.IsNullOrWhiteSpace(MaxFileAge))
                options.Append(string.Format(MAX_FILE_AGE, MaxFileAge.CleanOptionInput()));
            if (!string.IsNullOrWhiteSpace(MinFileAge))
                options.Append(string.Format(MIN_FILE_AGE, MinFileAge.CleanOptionInput()));
            if (!string.IsNullOrWhiteSpace(MaxLastAccessDate))
                options.Append(string.Format(MAX_LAST_ACCESS_DATE, MaxLastAccessDate.CleanOptionInput()));
            if (!string.IsNullOrWhiteSpace(MinLastAccessDate))
                options.Append(string.Format(MIN_LAST_ACCESS_DATE, MinLastAccessDate.CleanOptionInput()));
            if (ExcludeJunctionPoints)
                options.Append(EXCLUDE_JUNCTION_POINTS);
            if (ExcludeJunctionPointsForDirectories)
                options.Append(EXCLUDE_JUNCTION_POINTS_FOR_DIRECTORIES);
            if (ExcludeJunctionPointsForFiles)
                options.Append(EXCLUDE_JUNCTION_POINTS_FOR_FILES);
            if (UseFatFileTimes)
                options.Append(USE_FAT_FILE_TIMES);
            if (CompensateForDstDifference)
                options.Append(COMPENSATE_FOR_DST_DIFFERENCE);

            #endregion Set Options

            return options.ToString();
        }
    }
}
