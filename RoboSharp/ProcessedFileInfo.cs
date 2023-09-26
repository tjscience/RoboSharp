using RoboSharp.Interfaces;
using System;
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
        /// <param name="description"><inheritdoc cref="FileClass" path="*" /></param>
        /// <param name="size"><inheritdoc cref="Size" path="*" /></param>
        public ProcessedFileInfo(string name, FileClassType type, string description, long size = 0)
        {
            Name = name;
            FileClassType = type;
            FileClass = description;
            Size = size;
        }

        /// <summary>
        /// Create a new ProcessedFileInfo object that represents some file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="status">The status of the file to look up from the config</param>
        /// <param name="config">The config the get the status string from</param>
        /// <param name="includeFullName">If TRUE, uses the full path. If FALSE, only uses the directory name.</param>
        public ProcessedFileInfo(System.IO.FileInfo file, RoboSharpConfiguration config, ProcessedFileFlag status = ProcessedFileFlag.None, bool includeFullName = false)
        {
            FileClassType = FileClassType.File;
            FileClass = config.GetFileClass(status);
            Name = includeFullName ? file.FullName : file.Name;
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
        public ProcessedFileInfo(System.IO.DirectoryInfo directory, RoboSharpConfiguration config, ProcessedDirectoryFlag status = ProcessedDirectoryFlag.None, long size = -1)
        {
            FileClassType = FileClassType.NewDir;
            FileClass = config.GetDirectoryClass(status);
            Name = directory.FullName;
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
            switch (FileClassType)
            {
                case FileClassType.SystemMessage: return Name;
                case FileClassType.NewDir: return DirInfoToString(true);
                case FileClassType.File: return FileInfoToString(true, true);
                default: throw new NotImplementedException("Unknown FileClassType");
            }
        }

        /// <summary>
        /// Evaluate <see cref="LoggingOptions.NoFileClasses"/> and <see cref="LoggingOptions.NoFileSizes"/> to determine if those value should be included in the output string. <br/>
        /// Name will always be included.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public string ToString(LoggingOptions options)
        {
            switch(FileClassType)
            {
                case FileClassType.SystemMessage: return Name;
                case FileClassType.NewDir: return DirInfoToString(true);
                case FileClassType.File: return FileInfoToString(!options.NoFileClasses, !options.NoFileSizes);
                default: throw new NotImplementedException("Unknown FileClassType");
            }
        }

        /// <summary>
        /// "\t                   [FileCount]\t[DirectoryPath]"
        /// </summary>
        private string DirInfoToString(bool includeSize)
        {
            string fc = FileClass.PadRight(10);
            string fs = (includeSize ? Size.ToString() : "").PadLeft(10);
            return string.Format("\t{0}{1}\t{2}", fc, fs, Name);
        }

        /// <summary>
        /// "\t    [FileClass]  \t\t    [FileSize]\t[FileName]"
        /// </summary>
        private string FileInfoToString(bool includeClass, bool includeSize)
        {
            string fc = (includeClass ? FileClass : "").PadLeft(10);
            string fs = (includeSize? Size.ToString() : "").PadLeft(10);
            return string.Format("\t   {0}  \t\t{1}\t{2}", fc, fs, Name);
        }

        /// <summary>
        /// Set the <see cref="FileClass"/> property <br/>
        /// Only meant for consumers to use upon custom implementations of IRoboCommand
        /// </summary>
        /// <param name="status">Status to set</param>
        /// <param name="config">configuration provider</param>
        public void SetDirectoryClass(ProcessedDirectoryFlag status, RoboSharpConfiguration config)
        {
            if (FileClassType != FileClassType.NewDir) throw new System.Exception("Unable to apply ProcessedDirectoryFlag to File or System Message");
            FileClass = config.GetDirectoryClass(status);
        }
        /// <inheritdoc cref="SetDirectoryClass(ProcessedDirectoryFlag, RoboSharpConfiguration)"/>
        public void SetDirectoryClass(ProcessedDirectoryFlag status, IRoboCommand config) => SetDirectoryClass(status, config.Configuration);


        /// <inheritdoc cref="SetDirectoryClass(ProcessedDirectoryFlag, RoboSharpConfiguration)"/>
        public void SetFileClass(ProcessedFileFlag status, RoboSharpConfiguration config)
        {
            if (FileClassType != FileClassType.File) throw new System.Exception("Unable to apply ProcessedFileFlag to Directory or System Message");
            FileClass = config.GetFileClass(status);
        }
        /// <inheritdoc cref="SetDirectoryClass(ProcessedDirectoryFlag, RoboSharpConfiguration)"/>
        public void SetFileClass(ProcessedFileFlag status, IRoboCommand config) => SetFileClass(status, config.Configuration);
    }
}
