using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.ConsumerHelpers
{
    /// <summary>
    /// Class to assist creation of ProcessedFiles for custom implementations of IRoboCommand
    /// </summary>
    public static class ProcessedFileFactory
    {
        #region < System Messages >

        /// <summary>
        /// Create a SystemMessage ProcessedFileInfo
        /// </summary>
        /// <param name="message">Message reported by the process</param>
        /// <returns></returns>
        public static ProcessedFileInfo CreateSystemMessage(string message)
        {
            return new ProcessedFileInfo(message);
        }

        #endregion


        #region < Files >

        /// <summary>
        /// Create a new ProcessedFileInfo that represents a File
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="fileSize">the file syze in bytes</param>
        /// <param name="config">The configuration file to pull the log parsing string from</param>
        /// <param name="status">The status to look up from the configuration</param>
        /// <returns> new ProcessedFileInfo object</returns>
        public static ProcessedFileInfo CreateFileInfo(string fileName, long fileSize, RoboSharpConfiguration config, FileClasses status = FileClasses.NewFile)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            // Get the status from the configuration
            return new ProcessedFileInfo()
            {
                FileClass = config.GetFileClass(status),
                FileClassType = FileClassType.File,
                Name = fileName,
                Size = fileSize
            };
        }

        /// <inheritdoc cref="CreateFileInfo(string, long, RoboSharpConfiguration, FileClasses)"/>        
        public static ProcessedFileInfo CreateFileInfo(FileInfo file, RoboSharpConfiguration config, FileClasses status = FileClasses.NewFile)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));
            return CreateFileInfo(file.Name, file.Length, config, status);
        }

        #endregion


        #region < Directories >


        /// <summary>
        /// Create a new ProcessedFileInfo that represents a File
        /// </summary>
        /// <param name="directoryName">The directory nname</param>
        /// <param name="numberOfFiles">the file syze in bytes</param>
        /// <param name="config">The configuration file to pull the log parsing string from</param>
        /// <param name="status">The status to look up from the configuration</param>
        /// <returns> new ProcessedFileInfo object</returns>
        public static ProcessedFileInfo CreateDirectoryInfo(string directoryName, long numberOfFiles, RoboSharpConfiguration config, DirectoryClasses status = DirectoryClasses.NewDir)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            // Get the status from the configuration
            return new ProcessedFileInfo()
            {
                FileClass = config.GetDirectoryClass(status),
                FileClassType = FileClassType.NewDir,
                Name = directoryName,
                Size = numberOfFiles
            };
        }

        /// <inheritdoc cref="CreateDirectoryInfo(string, long, RoboSharpConfiguration, DirectoryClasses)"/>        
        public static ProcessedFileInfo CreateDirectoryInfo(DirectoryInfo dir, RoboSharpConfiguration config, DirectoryClasses status = DirectoryClasses.NewDir)
        {
            if (dir is null) throw new ArgumentNullException(nameof(dir));
            return CreateDirectoryInfo(dir.Name, Directory.GetFiles(dir.FullName).Length, config, status);
        }

        #endregion


    }
}
