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
        /// Default Constructor
        /// </summary>
        public ProcessedFileInfo() { }

        /// <summary>
        /// Get the value from the FileInfo
        /// </summary>
        /// <param name="file"></param>
        /// <param name="useFullName"/>
        public ProcessedFileInfo(System.IO.FileInfo file, bool useFullName = false)
        {
            FileClassType = FileClassType.File;
            FileClass = "File";
            Name = useFullName ? file.FullName : file.Name;
            Size = file.Length;
        }

        /// <summary>
        /// Report a message
        /// </summary>
        /// <param name="systemMessage"></param>
        public ProcessedFileInfo(string systemMessage)
        {
            FileClassType = FileClassType.SystemMessage;
            FileClass = "SystemMessage";
            Name = systemMessage;
            Size = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory">the Directory</param>
        /// <param name="size">number of files/folders in the directory (if desired to fill this out)</param>
        /// <param name="useFullName" />
        public ProcessedFileInfo(System.IO.DirectoryInfo directory, long size = 0, bool useFullName = false)
        {
            FileClassType = FileClassType.NewDir;
            FileClass = "NewDir";
            Name = useFullName ? directory.FullName : directory.Name;
            Size = size;
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
    }
}
