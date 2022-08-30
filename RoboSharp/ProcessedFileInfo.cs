using RoboSharp.Interfaces;
using System.IO;

namespace RoboSharp
{
    /// <summary>
    /// Message Type reported by RoboCopy
    /// </summary>
    public enum FileClassType
    {
        /// <summary>Details about a Directory</summary>
        NewDir,
        /// <summary>Details about a FILE</summary>
        File,
        /// <summary>Status Message reported by RoboCopy</summary>
        SystemMessage
    }

    /// <summary>Contains information about the current item being processed by RoboCopy</summary>
    public class ProcessedFileInfo
    {
        /// <summary>
        /// String used to identify the 'FileClass' property of a System Message
        /// </summary>
        public const string SystemMessageFileClass = "System Message";

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ProcessedFileInfo() { }

        /// <summary>
        /// Generate a new object by explicitly defining the values
        /// </summary>
        /// <param name="name"><inheritdoc cref="Name" path="*" /></param>
        /// <param name="type"><inheritdoc cref="FileClassType" path="*" /></param>
        /// <param name="class"><inheritdoc cref="FileClass" path="*" /></param>
        /// <param name="size"><inheritdoc cref="Size" path="*" /></param>
        public ProcessedFileInfo(string name, FileClassType type, string @class, long size = 0)
        {
            Name = name;
            FileClassType = type;
            FileClass = @class;
            Size = size;
        }

        /// <summary>
        /// Create a new ProcessedFileInfo object that represents some file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="status">The status of the file to look up from the config</param>
        /// <param name="config">The config the get the status string from</param>
        public ProcessedFileInfo(System.IO.FileInfo file, RoboSharpConfiguration config, FileClasses status = FileClasses.None)
        {
            FileClassType = FileClassType.File;
            FileClass = config.GetFileClass(status);
            Name = file.Name;
            Size = file.Length;
        }

        /// <summary>
        /// Report a message from the process
        /// </summary>
        /// <param name="systemMessage"></param>
        public ProcessedFileInfo(string systemMessage)
        {
            FileClassType = FileClassType.SystemMessage;
            FileClass = SystemMessageFileClass;
            Name = systemMessage;
            Size = 0;
        }

        /// <summary>
        /// Create a new ProcessedFileInfo object that represents some directory
        /// </summary>
        /// <param name="directory">the Directory</param>
        /// <param name="size">number of files/folders in the directory - If left at -1, then it will check check using the <paramref name="directory"/> length</param>
        /// <param name="status">The status of the file to look up from the config</param>
        /// <param name="config">The config the get the status string from</param>
        public ProcessedFileInfo(System.IO.DirectoryInfo directory, RoboSharpConfiguration config, DirectoryClasses status = DirectoryClasses.None, long size = -1)
        {
            FileClassType = FileClassType.NewDir;
            FileClass = config.GetDirectoryClass(status);
            Name = directory.Name;
            Size = size != -1 ? size : Directory.GetFiles(directory.FullName).Length;
        }

        /// <summary>Description of the item as reported by RoboCopy</summary>
        public string FileClass { get; set; }
        
        /// <inheritdoc cref="RoboSharp.FileClassType"/>
        public FileClassType FileClassType { get; set; }

        /// <summary>
        /// File -> File Size <br/>
        /// Directory -> Number files in folder -> Can be negative if PURGE is used <br/>
        /// SystemMessage -> Should be 0
        /// </summary>
        public long Size { get; set; }

        /// <summary>Folder or File Name / Message Text</summary>
        public string Name { get; set; }

        /// <summary>
        /// Translates the object back to the log line.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (FileClassType == FileClassType.SystemMessage) return FileClass;
            return string.Format(@"{0}\t{1}\t{2}", FileClass, Size, Name);
        }

        /// <summary>
        /// Evaluate <see cref="LoggingOptions.NoFileClasses"/> and <see cref="LoggingOptions.NoFileSizes"/> to determine if those value should be included in the output string. <br/>
        /// Name will always be included.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public string ToString(LoggingOptions options)
        {
            if (this.FileClassType == FileClassType.SystemMessage) return ToString();
            return string.Format(@"{0}{1}{2}",
                options.NoFileClasses ? string.Empty : FileClass + @"\t",
                options.NoFileSizes ? string.Empty : Size + @"\t",
                Name);
        }

        /// <summary>
        /// Set the <see cref="FileClass"/> property <br/>
        /// Only meant for consumers to use upon custom implementations of IRoboCommand
        /// </summary>
        /// <param name="status">Status to set</param>
        /// <param name="config">configuration provider</param>
        public void SetDirectoryClass(DirectoryClasses status, RoboSharpConfiguration config)
        {
            if (FileClassType != FileClassType.NewDir) throw new System.Exception("Unable to apply DirectoryClass status to File or System Message");
            FileClass = config.GetDirectoryClass(status);
        }
        /// <inheritdoc cref="SetDirectoryClass(DirectoryClasses, RoboSharpConfiguration)"/>
        public void SetDirectoryClass(DirectoryClasses status, IRoboCommand config) => SetDirectoryClass(status, config.Configuration);


        /// <inheritdoc cref="SetDirectoryClass(DirectoryClasses, RoboSharpConfiguration)"/>
        public void SetFileClass(FileClasses status, RoboSharpConfiguration config)
        {
            if (FileClassType != FileClassType.File) throw new System.Exception("Unable to apply DirectoryClass status to File or System Message");
            FileClass = config.GetFileClass(status);
        }
        /// <inheritdoc cref="SetDirectoryClass(DirectoryClasses, RoboSharpConfiguration)"/>
        public void SetFileClass(FileClasses status, IRoboCommand config) => SetFileClass(status, config.Configuration);
    }
}
