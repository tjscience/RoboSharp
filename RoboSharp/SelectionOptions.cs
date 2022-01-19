using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RoboSharp
{
    /// <summary>
    /// RoboCopy Switches that determine which folders and files are selected for copying/moving
    /// </summary>
    /// <remarks>
    /// <see href="https://github.com/tjscience/RoboSharp/wiki/SelectionOptions"/>
    /// </remarks>
    public class SelectionOptions : ICloneable
    {
        #region Constructors 

        /// <summary>
        /// Create new SelectionOptions with Default Settings
        /// </summary>
        public SelectionOptions() { }

        /// <summary>
        /// Clone a SelectionOptions Object
        /// </summary>
        public SelectionOptions(SelectionOptions options)
        {
            OnlyCopyArchiveFiles = options.OnlyCopyArchiveFiles;
            OnlyCopyArchiveFilesAndResetArchiveFlag = options.OnlyCopyArchiveFilesAndResetArchiveFlag;
            IncludeAttributes = options.IncludeAttributes;
            ExcludeAttributes = options.ExcludeAttributes;
            ExcludedFiles.AddRange(options.ExcludedFiles);
            ExcludedDirectories.AddRange(options.ExcludedDirectories);
            ExcludeChanged = options.ExcludeChanged;
            ExcludeNewer = options.ExcludeNewer;
            ExcludeOlder = options.ExcludeOlder;
            ExcludeExtra = options.ExcludeExtra;
            ExcludeLonely = options.ExcludeLonely;
            IncludeSame = options.IncludeSame;
            IncludeTweaked = options.IncludeTweaked;
            MaxFileSize = options.MaxFileSize;
            MinFileSize = options.MinFileSize;
            MaxFileAge = options.MaxFileAge;
            MinFileAge = options.MinFileAge;
            MaxLastAccessDate = options.MaxLastAccessDate;
            MinLastAccessDate = options.MinLastAccessDate;
            ExcludeJunctionPoints = options.ExcludeJunctionPoints;
            UseFatFileTimes = options.UseFatFileTimes;
            CompensateForDstDifference = options.CompensateForDstDifference; ;
            ExcludeJunctionPointsForFiles = options.ExcludeJunctionPointsForFiles;

        }

        /// <summary>
        /// Clone this SelectionOptions Object
        /// </summary>
        public SelectionOptions Clone() => new SelectionOptions(this);

        object ICloneable.Clone() => Clone();

        #endregion

        #region Option Constants

        internal const string ONLY_COPY_ARCHIVE_FILES = "/A ";
        internal const string ONLY_COPY_ARCHIVE_FILES_AND_RESET_ARCHIVE_FLAG = "/M ";
        internal const string INCLUDE_ATTRIBUTES = "/IA:{0} ";
        internal const string EXCLUDE_ATTRIBUTES = "/XA:{0} ";
        internal const string EXCLUDE_FILES = "/XF {0} ";
        internal const string EXCLUDE_DIRECTORIES = "/XD {0} ";
        internal const string EXCLUDE_CHANGED = "/XC ";
        internal const string EXCLUDE_NEWER = "/XN ";
        internal const string EXCLUDE_OLDER = "/XO ";
        internal const string EXCLUDE_EXTRA = "/XX ";
        internal const string EXCLUDE_LONELY = "/XL ";
        internal const string INCLUDE_SAME = "/IS ";
        internal const string INCLUDE_TWEAKED = "/IT ";
        internal const string MAX_FILE_SIZE = "/MAX:{0} ";
        internal const string MIN_FILE_SIZE = "/MIN:{0} ";
        internal const string MAX_FILE_AGE = "/MAXAGE:{0} ";
        internal const string MIN_FILE_AGE = "/MINAGE:{0} ";
        internal const string MAX_LAST_ACCESS_DATE = "/MAXLAD:{0} ";
        internal const string MIN_LAST_ACCESS_DATE = "/MINLAD:{0} ";
        internal const string EXCLUDE_JUNCTION_POINTS = "/XJ ";
        internal const string USE_FAT_FILE_TIMES = "/FFT ";
        internal const string COMPENSATE_FOR_DST_DIFFERENCE = "/DST ";
        internal const string EXCLUDE_JUNCTION_POINTS_FOR_DIRECTORIES = "/XJD ";
        internal const string EXCLUDE_JUNCTION_POINTS_FOR_FILES = "/XJF ";

        #endregion Option Constants

        #region < ExcludedDirs and ExcludedFiles >

        private readonly List<string> excludedDirs = new List<string>();
        private readonly List<string> excludedFiles = new List<string>();
        
        /// <summary>
        /// Regex Tester to use with <see cref="Regex.Matches(string)"/> to get all the matches from a string <br/>
        /// Searches for a pattern of "{Non-WhiteSpaceChar}"
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        internal static Regex FileFolderNameRegexSplitter = new Regex("\\s*(?<VALUE>[\"]{0,1}.+?[\"]{0,1})(?:\\s+?)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private static void ParseAndAddToList(string value, List<string> list)
        {
            MatchCollection collection = FileFolderNameRegexSplitter.Matches(value);
            if (collection.Count == 0) return;
            foreach (Match c in collection)
            {
                string s = c.Groups["VALUE"].Value;
                list.Add(s);
            }
        }

        #endregion

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
        /// <remarks>
        /// This property is now backed by the ExcludedFiles List{String} property. <br/>
        /// Get -- String.Join(" ", ExcludExcludedFilesedDirs);<br/>
        /// Set -- Clears ExcludedFiles and splits this list using a regex to populate the list.
        /// </remarks>
        [Obsolete("This obsolete property is now backed by the ExcludedFiles List<String> property.")]
        public string ExcludeFiles
        {
            get => String.Join(" ", excludedFiles);
            set 
            {
                excludedFiles.Clear();
                if (value.IsNullOrWhiteSpace()) return;
                ParseAndAddToList(value, excludedFiles);
            }
        }
        /// <summary>
        /// Allows you to supply a set of files to copy or use wildcard characters (* or ?). <br/>
        /// JobOptions file saves these into the /IF (Include Files) section
        /// </summary>
        public List<string> ExcludedFiles
        {
            get
            {
                return excludedFiles;
            }
        }
        /// <summary>
        /// Directories should be separated by spaces.
        /// Excludes directories that match the specified names or paths.
        /// [/XD Directory Directory ...]
        /// </summary>
        /// <remarks>
        /// This property is now backed by the ExcludedDirectories List{String} property. <br/>
        /// Get -> String.Join(" ", ExcludedDirs);<br/>
        /// Set -> Clears ExcludedDirs and splits this list using a regex to populate the list.
        /// </remarks>
        [Obsolete("This obsolete property is now backed by the ExcludedDirectories List<String> property.")]
        public string ExcludeDirectories 
        {
            get => String.Join(" ", excludedDirs);
            set
            {
                excludedFiles.Clear();
                if (value.IsNullOrWhiteSpace()) return;
                ParseAndAddToList(value, excludedDirs);
            }
        }
        /// <summary>
        /// Allows you to supply a set of files to copy or use wildcard characters (* or ?). <br/>
        /// JobOptions file saves these into the /IF (Include Files) section
        /// </summary>
        public List<string> ExcludedDirectories
        {
            get
            {
                return excludedDirs;
            }
        }
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
        /// [/MINLAD:N or YYYYMMDD]
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
            if (!IncludeAttributes.IsNullOrWhiteSpace())
                options.Append(string.Format(INCLUDE_ATTRIBUTES, IncludeAttributes.CleanOptionInput()));
            if (!ExcludeAttributes.IsNullOrWhiteSpace())
                options.Append(string.Format(EXCLUDE_ATTRIBUTES, ExcludeAttributes.CleanOptionInput()));
#pragma warning disable CS0618 // Marked as Obsolete for consumers, but it originally functionality is still intact, so this still works properly.
            if (!ExcludeFiles.IsNullOrWhiteSpace())
                options.Append(string.Format(EXCLUDE_FILES, ExcludeFiles));
            if (!ExcludeDirectories.IsNullOrWhiteSpace())
                options.Append(string.Format(EXCLUDE_DIRECTORIES, ExcludeDirectories));
#pragma warning restore CS0618 
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
            if (!MaxFileAge.IsNullOrWhiteSpace())
                options.Append(string.Format(MAX_FILE_AGE, MaxFileAge.CleanOptionInput()));
            if (!MinFileAge.IsNullOrWhiteSpace())
                options.Append(string.Format(MIN_FILE_AGE, MinFileAge.CleanOptionInput()));
            if (!MaxLastAccessDate.IsNullOrWhiteSpace())
                options.Append(string.Format(MAX_LAST_ACCESS_DATE, MaxLastAccessDate.CleanOptionInput()));
            if (!MinLastAccessDate.IsNullOrWhiteSpace())
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

        /// <summary>
        /// Combine this object with another RetryOptions object. <br/>
        /// Any properties marked as true take priority. IEnumerable items are combined. <br/>
        /// String Values will only be replaced if the primary object has a null/empty value for that property.
        /// </summary>
        /// <param name="options"></param>
        public void Merge(SelectionOptions options)
        {
            //File Attributes
            IncludeAttributes = IncludeAttributes.CombineCharArr(options.IncludeAttributes);
            ExcludeAttributes = ExcludeAttributes.CombineCharArr(options.ExcludeAttributes);

            //File Age
            MaxFileAge = MaxFileAge.ReplaceIfEmpty(options.MaxFileAge);
            MinFileAge = MaxFileAge.ReplaceIfEmpty(options.MinFileAge);
            MaxLastAccessDate = MaxFileAge.ReplaceIfEmpty(options.MaxLastAccessDate);
            MinLastAccessDate = MaxFileAge.ReplaceIfEmpty(options.MinLastAccessDate);
            
            //Bools
            OnlyCopyArchiveFiles |= options.OnlyCopyArchiveFiles;
            OnlyCopyArchiveFilesAndResetArchiveFlag |= options.OnlyCopyArchiveFilesAndResetArchiveFlag;
            ExcludedFiles.AddRange(options.ExcludedFiles);
            ExcludedDirectories.AddRange(options.ExcludedDirectories);
            ExcludeChanged |= options.ExcludeChanged;
            ExcludeNewer |= options.ExcludeNewer;
            ExcludeOlder |= options.ExcludeOlder;
            ExcludeExtra |= options.ExcludeExtra;
            ExcludeLonely |= options.ExcludeLonely;
            IncludeSame |= options.IncludeSame;
            IncludeTweaked |= options.IncludeTweaked;
            MaxFileSize |= options.MaxFileSize;
            MinFileSize |= options.MinFileSize;
            ExcludeJunctionPoints |= options.ExcludeJunctionPoints;
            UseFatFileTimes |= options.UseFatFileTimes;
            CompensateForDstDifference |= options.CompensateForDstDifference; ;
            ExcludeJunctionPointsForFiles |= options.ExcludeJunctionPointsForFiles;
        }
    }
}
