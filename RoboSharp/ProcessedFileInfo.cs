namespace RoboSharp
{
    /// <summary>
    /// Message Type reported by RoboCopy
    /// </summary>
    public enum FileClassType
    {
        /// <summary>Details about a FOLDER</summary>
        NewDir,
        /// <summary>Details about a FILE</summary>
        File,
        /// <summary>Status Message reported by RoboCopy</summary>
        SystemMessage
    }

    /// <summary>Contains information about the current item being processed by RoboCopy</summary>
    public class ProcessedFileInfo
    {
        /// <summary>Description of the item as reported by RoboCopy</summary>
        public string FileClass { get; set; }
        /// <inheritdoc cref="RoboSharp.FileClassType"/>
        public FileClassType FileClassType { get; set; }
        /// <summary>File Size</summary>
        public long Size { get; set; }
        /// <summary>Folder or File Name / Message Text</summary>
        public string Name { get; set; }
    }
}
