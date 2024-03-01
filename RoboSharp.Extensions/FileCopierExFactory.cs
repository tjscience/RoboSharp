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
    public sealed class FileCopierExFactory : AbstractFileCopierFactory<FileCopierEx>, IFileCopierFactory
    {
        /// <summary>
        /// The options to apply to generated copiers
        /// </summary>
        public CopyFileEx.CopyFileExOptions Options { get; set; }

        /// <inheritdoc/>
        public override FileCopierEx Create(FileInfo source, FileInfo destination, IDirectoryPair parent)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            return new FileCopierEx(source, destination, parent)
            {
                CopyOptions = Options
            };
        }
    }
}
