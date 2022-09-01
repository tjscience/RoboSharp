using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.IO;

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
        /// Create new SelectionOptions using the provided <paramref name="selectionFlags"/>
        /// </summary>
        public SelectionOptions(SelectionFlags selectionFlags)
        {
            ApplySelectionFlags(selectionFlags);
        }

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
        /// This regex is used when the { <see cref="ExcludeFiles"/> } and { <see cref="ExcludeDirectories"/> } properties are set in order to split the input string to a List{string}
        /// </summary>
        /// <remarks>
        /// Regex Tester to use with <see cref="Regex.Matches(string)"/> to get all the matches from a string.
        /// </remarks>
        public static Regex FileFolderNameRegexSplitter = new Regex("(?<VALUE>\".+?\"|[^\\s\\,\"\\|]+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Use { <see cref="FileFolderNameRegexSplitter"/> } to split the <paramref name="inputString"/>, then add the matches to the suppplied <paramref name="list"/>.
        /// </summary>
        /// <param name="inputString">String to perform <see cref="Regex.Matches(string)"/> against</param>
        /// <param name="list">List to add regex matches to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ParseAndAddToList(string inputString, List<string> list)
        {
            MatchCollection collection = FileFolderNameRegexSplitter.Matches(inputString);
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
        public virtual bool OnlyCopyArchiveFiles { get; set; }
        /// <summary>
        /// Copies only files for which the Archive attribute is set, and resets the Archive attribute.
        /// [/M]
        /// </summary>
        public virtual bool OnlyCopyArchiveFilesAndResetArchiveFlag { get; set; }
        /// <summary>
        /// This property should be set to a string consisting of all the attributes to include (eg. AH; RASHCNETO).
        /// Includes only files for which any of the specified attributes are set.
        /// [/IA:attributes]
        /// </summary>
        public virtual string IncludeAttributes { get => IncludedAttributesField; set { IncludedAttributesField = value; IncludedAttributesValue = GetIncludedAttributes(); } }
        private string IncludedAttributesField;
        internal FileAttributes? IncludedAttributesValue { get; private set; }

        /// <summary>
        /// This property should be set to a string consisting of all the attributes to exclude (eg. AH; RASHCNETO).
        /// Excludes files for which any of the specified attributes are set.
        /// [/XA:attributes]
        /// </summary>
        public virtual string ExcludeAttributes { get => ExcludedAttributesField; set { ExcludedAttributesField = value; ExcludedAttributesValue = GetExcludedAttributes(); } }
        private string ExcludedAttributesField;
        internal FileAttributes? ExcludedAttributesValue { get; private set; }

        /// <summary>
        /// Files should be separated by spaces.
        /// Excludes files that match the specified names or paths. Note that FileName can include wildcard characters (* and ?).
        /// [/XF File File ...]
        /// </summary>
        /// <remarks>
        /// This property is now backed by the ExcludedFiles List{String} property. <br/>
        /// Get -> Ensures all strings in { <see cref="ExcludedFiles"/> } are wrapped in quotes if needed, and concats the items into a single string. <br/>
        /// Set -- Clears ExcludedFiles and splits this list using a regex to populate the list.
        /// </remarks>
        [Obsolete("This property is now backed by the ExcludedFiles List<String> property. \n Both Get/Set accessors still work similar to previous:\n" +
            "- 'Get' sanitizies then Joins all strings in the list into a single output string that is passed into RoboCopy.\n" +
            "- 'Set' clears the ExcludedFiles list, then splits the input string using regex to repopulate the list."
            )]
        public string ExcludeFiles
        {
            get
            {
                string RetString = "";
                foreach (string s in excludedFiles)
                {
                    RetString += s.WrapPath() + " ";
                }
                return RetString.Trim();
            }
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
        /// Get -> Ensures all strings in { <see cref="ExcludedDirectories"/> } are wrapped in quotes if needed, and concats the items into a single string. <br/>
        /// Set -> Clears ExcludedDirs and splits this list using a regex to populate the list.
        /// </remarks>
        [Obsolete("This property is now backed by the ExcludedDirectories List<String> property. \n Both Get/Set accessors still work similar to previous:\n" +
            "- 'Get' sanitizies then Joins all strings in the list into a single output string that is passed into RoboCopy.\n" +
            "- 'Set' clears the ExcludedDirectories list, then splits the input string using regex to repopulate the list."
            )]
        public string ExcludeDirectories
        {
            get
            {
                string RetString = "";
                foreach (string s in excludedDirs)
                {
                    RetString += s.WrapPath() + " ";
                }
                return RetString.Trim();
            }
            set
            {
                excludedDirs.Clear();
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
        public virtual bool ExcludeChanged { get; set; }
        /// <summary>
        /// Excludes newer files.
        /// [/XN]
        /// </summary>
        public virtual bool ExcludeNewer { get; set; }
        /// <summary>
        /// Excludes older files.
        /// [/XO]
        /// </summary>
        public virtual bool ExcludeOlder { get; set; }
        /// <summary>
        /// Excludes extra files and directories.
        /// [/XX]
        /// </summary>
        public virtual bool ExcludeExtra { get; set; }
        /// <summary>
        /// Excludes lonely files and directories.
        /// [/XL]
        /// </summary>
        public virtual bool ExcludeLonely { get; set; }
        /// <summary>
        /// Includes the same files.
        /// [/IS]
        /// </summary>
        public virtual bool IncludeSame { get; set; }
        /// <summary>
        /// Includes tweaked files.
        /// [/IT]
        /// </summary>
        public virtual bool IncludeTweaked { get; set; }
        /// <summary>
        /// Zero indicates that this feature is turned off.
        /// Specifies the maximum file size (to exclude files bigger than N bytes).
        /// [/MAX:N]
        /// </summary>
        public virtual long MaxFileSize { get; set; }
        /// <summary>
        /// Zero indicates that this feature is turned off.
        /// Specifies the minimum file size (to exclude files smaller than N bytes).
        /// [/MIN:N]
        /// </summary>
        public virtual long MinFileSize { get; set; }
        /// <summary>
        /// Specifies the maximum file age (to exclude files older than N days or date).
        /// [/MAXAGE:N OR YYYYMMDD]
        /// </summary>
        public virtual string MaxFileAge { get; set; }
        /// <summary>
        /// Specifies the minimum file age (exclude files newer than N days or date).
        /// [/MINAGE:N OR YYYYMMDD]
        /// </summary>
        public virtual string MinFileAge { get; set; }
        /// <summary>
        /// Specifies the maximum last access date (excludes files unused since Date).
        /// [/MAXLAD:YYYYMMDD]
        /// </summary>
        public virtual string MaxLastAccessDate { get; set; }
        /// <summary>
        /// Specifies the minimum last access date (excludes files used since N) If N is less 
        /// than 1900, N specifies the number of days. Otherwise, N specifies a date 
        /// in the format YYYYMMDD.
        /// [/MINLAD:N or YYYYMMDD]
        /// </summary>
        public virtual string MinLastAccessDate { get; set; }
        /// <summary>
        /// Excludes junction points and symbolic links, which are normally included by default.
        /// [/XJ]
        /// </summary>
        public virtual bool ExcludeJunctionPoints { get; set; }
        /// <summary>
        /// Assumes FAT file times (two-second precision).
        /// [/FFT]
        /// </summary>
        public virtual bool UseFatFileTimes { get; set; }
        /// <summary>
        /// Compensates for one-hour DST time differences.
        /// [/DST]
        /// </summary>
        public virtual bool CompensateForDstDifference { get; set; }
        /// <summary>
        /// Excludes junction points symbolic links for directories.
        /// [/XJD]
        /// </summary>
        public virtual bool ExcludeJunctionPointsForDirectories { get; set; }
        /// <summary>
        /// Excludes symbolic links for files.
        /// [/XJF]
        /// </summary>
        public virtual bool ExcludeJunctionPointsForFiles { get; set; }

        #endregion Public Properties

        /// <param name="AttributesToInclude"><inheritdoc cref="ConvertFileAttrToString(FileAttributes?)"/></param>
        /// <inheritdoc cref="ConvertFileAttrToString(FileAttributes?)"/>
        public void SetIncludedAttributes(FileAttributes? AttributesToInclude) => this.IncludeAttributes = ConvertFileAttrToString(AttributesToInclude);

        /// <param name="AttributesToExclude"><inheritdoc cref="ConvertFileAttrToString(FileAttributes?)"/></param>
        /// <inheritdoc cref="ConvertFileAttrToString(FileAttributes?)"/>
        public void SetExcludedAttributes(FileAttributes? AttributesToExclude) => this.ExcludeAttributes = ConvertFileAttrToString(AttributesToExclude);

        /// <summary> Gets the <see cref="FileAttributes"/> representation of the <see cref="IncludeAttributes"/> string</summary>
        /// <inheritdoc cref="ConvertFileAttrStringToEnum"/>
        public FileAttributes? GetIncludedAttributes() => IncludedAttributesValue; // ConvertFileAttrStringToEnum(this.IncludeAttributes);

        /// <summary> Gets the <see cref="FileAttributes"/> representation of the <see cref="ExcludeAttributes"/> string</summary>
        /// <inheritdoc cref="ConvertFileAttrStringToEnum"/>
        public FileAttributes? GetExcludedAttributes() => ExcludedAttributesValue;//ConvertFileAttrStringToEnum(this.ExcludeAttributes);

        /// <summary>
        /// Converts a <see cref="FileAttributes"/> enum to its RASHCNETO string.
        /// </summary>
        /// <param name="attributes">
        /// Accepts: ReadOnly, Archive, System, Hidden, Compressed, NotContentIndexed, Encrypted, Temporary, Offline <br/>
        /// Ignores: All Other Attributes <br/>
        /// Pass in NULL value to return empty string.
        /// </param>
        /// <returns>RASHCNETO depending on submitted enum</returns>
        public static string ConvertFileAttrToString(FileAttributes? attributes)
        {
            if (attributes is null) return String.Empty;
            string s = "";
            var Attr = (FileAttributes)attributes;
            if (Attr.HasFlag(FileAttributes.ReadOnly)) s += "R";
            if (Attr.HasFlag(FileAttributes.Archive)) s += "A";
            if (Attr.HasFlag(FileAttributes.System)) s += "S";
            if (Attr.HasFlag(FileAttributes.Hidden)) s += "H";
            if (Attr.HasFlag(FileAttributes.Compressed)) s += "C";
            if (Attr.HasFlag(FileAttributes.NotContentIndexed)) s += "N";
            if (Attr.HasFlag(FileAttributes.Encrypted)) s += "E";
            if (Attr.HasFlag(FileAttributes.Temporary)) s += "T";
            if (Attr.HasFlag(FileAttributes.Offline)) s += "O";
            return s;
        }

        /// <summary>
        /// Converts a RASHCNETO string to its <see cref="FileAttributes"/> enum.
        /// </summary>
        /// <param name="attributes">
        /// Accepts: ReadOnly, Archive, System, Hidden, Compressed, NotContentIndexed, Encrypted, Temporary, Offline <br/>
        /// Ignores: All Other Attributes <br/>
        /// Pass in NULL value to return empty string.
        /// </param>
        /// <returns>If the string is parsable, returns the enum. Otherwise returns null.</returns>
        public static FileAttributes? ConvertFileAttrStringToEnum(string attributes)
        {
            if (string.IsNullOrWhiteSpace(attributes)) return null;
            attributes = attributes.ToUpper();
            if (!System.Text.RegularExpressions.Regex.IsMatch(attributes, @"^[RASHCNETO]{0,9}$", RegexOptions.Compiled | RegexOptions.IgnoreCase)) throw new Exception("Invalid RASHCNETO string!");
            FileAttributes? attr = null;
            if (attributes.Contains('R')) attr |= FileAttributes.ReadOnly;
            if (attributes.Contains('A')) attr |= FileAttributes.Archive;
            if (attributes.Contains('S')) attr |= FileAttributes.System;
            if (attributes.Contains('H')) attr |= FileAttributes.Hidden;
            if (attributes.Contains('C')) attr |= FileAttributes.Compressed;
            if (attributes.Contains('N')) attr |= FileAttributes.NotContentIndexed;
            if (attributes.Contains('E')) attr |= FileAttributes.Encrypted;
            if (attributes.Contains('T')) attr |= FileAttributes.Temporary;
            if (attributes.Contains('O')) attr |= FileAttributes.Offline;
            return attr;
        }

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
        /// Returns the Parsed Options as it would be applied to RoboCopy
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Parse();
        }

        /// <summary>
        /// Combine this object with another RetryOptions object. <br/>
        /// Any properties marked as true take priority. IEnumerable items are combined. <br/>
        /// String\Long Values will only be replaced if the primary object has a null/empty value for that property.
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

            //Long
            MaxFileSize |= options.MaxFileSize;
            MinFileSize |= options.MinFileSize;

            //Lists
            ExcludedFiles.AddRange(options.ExcludedFiles);
            ExcludedDirectories.AddRange(options.ExcludedDirectories);

            //Bools
            OnlyCopyArchiveFiles |= options.OnlyCopyArchiveFiles;
            OnlyCopyArchiveFilesAndResetArchiveFlag |= options.OnlyCopyArchiveFilesAndResetArchiveFlag;
            ExcludeChanged |= options.ExcludeChanged;
            ExcludeNewer |= options.ExcludeNewer;
            ExcludeOlder |= options.ExcludeOlder;
            ExcludeExtra |= options.ExcludeExtra;
            ExcludeLonely |= options.ExcludeLonely;
            IncludeSame |= options.IncludeSame;
            IncludeTweaked |= options.IncludeTweaked;
            ExcludeJunctionPoints |= options.ExcludeJunctionPoints;
            ExcludeJunctionPointsForFiles |= options.ExcludeJunctionPointsForFiles;
            ExcludeJunctionPointsForDirectories |= options.ExcludeJunctionPointsForDirectories;

            UseFatFileTimes |= options.UseFatFileTimes;
            CompensateForDstDifference |= options.CompensateForDstDifference; ;
            
        }

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
        }

        /// <summary>
        /// Apply the <see cref="SelectionFlags"/> to this command
        /// </summary>
        /// <param name="flags">Options to apply</param>
        public virtual void ApplySelectionFlags(SelectionFlags flags)
        {
            this.ExcludeChanged = flags.HasFlag(SelectionFlags.ExcludeChanged);
            this.ExcludeExtra = flags.HasFlag(SelectionFlags.ExcludeExtra);
            this.ExcludeJunctionPoints = flags.HasFlag(SelectionFlags.ExcludeJunctionPoints);
            this.ExcludeJunctionPointsForDirectories = flags.HasFlag(SelectionFlags.ExcludeJunctionPointsForDirectories);
            this.ExcludeJunctionPointsForFiles = flags.HasFlag(SelectionFlags.ExcludeJunctionPointsForFiles);
            this.ExcludeLonely = flags.HasFlag(SelectionFlags.ExcludeLonely);
            this.ExcludeNewer = flags.HasFlag(SelectionFlags.ExcludeNewer);
            this.ExcludeOlder = flags.HasFlag(SelectionFlags.ExcludeOlder);
            this.IncludeSame = flags.HasFlag(SelectionFlags.IncludeSame);
            this.IncludeTweaked = flags.HasFlag(SelectionFlags.IncludeTweaked);
            this.OnlyCopyArchiveFiles = flags.HasFlag(SelectionFlags.OnlyCopyArchiveFiles);
            this.OnlyCopyArchiveFilesAndResetArchiveFlag = flags.HasFlag(SelectionFlags.OnlyCopyArchiveFilesAndResetArchiveFlag);
        }

        /// <summary>
        /// Translate the selection bools of this object to its <see cref="SelectionFlags"/> representation
        /// </summary>
        /// <returns>The <see cref="SelectionFlags"/> representation of this object.</returns>
        public SelectionFlags GetSelectionFlags()
        {
            var flags = SelectionFlags.Default;

            if (this.ExcludeChanged) flags |= SelectionFlags.ExcludeChanged;
            if (this.ExcludeExtra) flags |= SelectionFlags.ExcludeExtra;
            if (this.ExcludeJunctionPoints) flags |= SelectionFlags.ExcludeJunctionPoints;
            if (this.ExcludeJunctionPointsForDirectories) flags |= SelectionFlags.ExcludeJunctionPointsForDirectories;
            if (this.ExcludeJunctionPointsForFiles) flags |= SelectionFlags.ExcludeJunctionPointsForFiles;
            if (this.ExcludeLonely) flags |= SelectionFlags.ExcludeLonely;
            if (this.ExcludeNewer) flags |= SelectionFlags.ExcludeNewer;
            if (this.ExcludeOlder) flags |= SelectionFlags.ExcludeOlder;
            if (this.IncludeSame) flags |= SelectionFlags.IncludeSame;
            if (this.IncludeTweaked) flags |= SelectionFlags.IncludeTweaked;
            if (this.OnlyCopyArchiveFiles) flags |= SelectionFlags.OnlyCopyArchiveFiles;
            if (this.OnlyCopyArchiveFilesAndResetArchiveFlag) flags |= SelectionFlags.OnlyCopyArchiveFilesAndResetArchiveFlag;
            
            return flags;
        }
    }
}
