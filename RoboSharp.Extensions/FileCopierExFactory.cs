using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// <see cref="IFileCopierFactory"/> that creates <see cref="FileCopierEx"/> objects with a specified set of <see cref="CopyFileEx.CopyFileExOptions"/>
    /// </summary>
    public sealed class FileCopierExFactory : IFileCopierFactory
    {
        /// <summary>
        /// The options to apply to generated copiers
        /// </summary>
        public CopyFileEx.CopyFileExOptions Options { get; set; }

        /// <inheritdoc cref="IFileCopierFactory.Create(string, string)"/>
        public FileCopierEx Create(string source, string destination)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Source can not be empty", nameof(source));
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Destination can not be empty", nameof(destination));
            return Create(new FileInfo(source), new FileInfo(destination));
        }

        /// <inheritdoc cref="IFileCopierFactory.Create(FileInfo, FileInfo)"/>
        public FileCopierEx Create(FileInfo source, FileInfo destination)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            return new FileCopierEx(source, destination)
            {
                CopyOptions = Options
            };
        }

        /// <inheritdoc cref="IFileCopierFactory.Create(IFilePair)"/>
        public FileCopierEx Create(IFilePair filePair)
        {
            if (filePair is null) throw new ArgumentNullException(nameof(filePair));
            if (filePair is IProcessedFilePair fp)
            {
                return new FileCopierEx(filePair)
                {
                    CopyOptions = Options,
                    ProcessedFileInfo = fp.ProcessedFileInfo,
                    ShouldCopy = fp.ShouldCopy,
                    ShouldPurge = fp.ShouldPurge,
                };
            }
            else
            {
                return new FileCopierEx(filePair)
                {
                    CopyOptions = Options
                };
            }
            
        }

        IFileCopier IFileCopierFactory.Create(string source, string destination) => Create(source, destination);
        IFileCopier IFileCopierFactory.Create(FileInfo source, FileInfo destination) => Create(source, destination);
        IFileCopier IFileCopierFactory.Create(IFilePair filePair) => Create(filePair);
    }
}
