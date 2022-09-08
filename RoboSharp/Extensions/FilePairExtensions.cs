using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Extension Methods for the <see cref="IFilePair"/> interface
    /// </summary>
    public static class FilePairExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool FileDoesntExist(string destination) => !File.Exists(destination);

        /// <summary> Evaluate the roots of the Source and Destination </summary>
        /// <returns>True if the Source and Destination have the same root, otherwise false.</returns>
        public static bool IsLocatedOnSameDrive(this IFilePair pair) => Path.GetPathRoot(pair.Source.FullName) == Path.GetPathRoot(pair.Destination.FullName);

        /// <inheritdoc cref="SelectionOptionsExtensions.IsExtra(FileInfo, FileInfo)"/>
        public static bool IsExtra(this IFilePair pair) => SelectionOptionsExtensions.IsExtra(pair.Source, pair.Destination);

        /// <inheritdoc cref="SelectionOptionsExtensions.IsLonely(FileInfo, FileInfo)"/>
        public static bool IsLonely(this IFilePair pair) => SelectionOptionsExtensions.IsLonely(pair.Source, pair.Destination);

        /// <summary>
        /// Gets the file length from the pair, prioritizing the source file over the destination file.
        /// </summary>
        /// <param name="pair">the file pair</param>
        /// <returns>
        /// If the source file exists, return the source file's length in bytes. <br/>
        /// If the destination file exists, return the destination file's length in bytes. <br/>
        /// If neither exist, return 0;
        /// </returns>
        public static long GetFileLength(this IFilePair pair) => pair.Source.Exists ? pair.Source.Length : pair.Destination.Exists ? pair.Destination.Length : 0;

        #region < IsSourceNewer >

        /// <summary>
        /// Check if the Source file is newer than the Destination File
        /// </summary>
        /// <param name="source">This path is assumed to exist since it is the patht to copy from</param>
        /// <param name="destination">the destination path - may or may not exist</param>
        /// <returns>TRUE if the source is newer, or the destination does not exist. FALSE if the destination is newer.</returns>
        public static bool IsSourceNewer(string source, string destination)
        {
            if (FileDoesntExist(destination)) return true;
            return File.GetLastWriteTime(source) > File.GetLastWriteTime(destination);
        }
        /// <inheritdoc cref="IsSourceNewer(string, string)"/>
        public static bool IsSourceNewer(FileInfo source, FileInfo destination)
        {
            if (!destination.Exists) return source.Exists;
            return source.LastWriteTime > destination.LastWriteTime;
        }
        /// <inheritdoc cref="IsSourceNewer(string, string)"/>
        public static bool IsSourceNewer(this IFilePair copier)
        {
            return IsSourceNewer(copier.Source, copier.Destination);
        }

        #endregion

        #region < IsDestinationNewer >

        /// <summary>
        /// Check if the Destination file is newer than the Source file
        /// </summary>
        /// <param name="source">This path is assumed to exist since it is the patht to copy from</param>
        /// <param name="destination">the destination path - may or may not exist</param>
        /// <returns>TRUE if the destination file is newer, otherwise false</returns>
        public static bool IsDestinationNewer(string source, string destination)
        {
            if (FileDoesntExist(destination)) return false;
            return File.GetLastWriteTime(source) < File.GetLastWriteTime(destination);
        }
        /// <inheritdoc cref="IsDestinationNewer(string, string)"/>
        public static bool IsDestinationNewer(FileInfo source, FileInfo destination)
        {
            if (!destination.Exists) return false;
            return source.LastWriteTime < destination.LastWriteTime;
        }
        /// <inheritdoc cref="IsDestinationNewer(string, string)"/>
        public static bool IsDestinationNewer(this IFilePair copier)
        {
            return IsDestinationNewer(copier.Source, copier.Destination);
        }

        #endregion

        #region < IsSameDate >

        /// <summary>
        /// Check if the Source and Destination files have the same LastWriteTime
        /// </summary>
        /// <param name="source">This path is assumed to exist since it is the patht to copy from</param>
        /// <param name="destination">the destination path - may or may not exist</param>
        /// <returns>TRUE if both files exist, and their LastWriteTime is identical</returns>
        public static bool IsSameDate(string source, string destination)
        {
            if (FileDoesntExist(source)) return false;
            if (FileDoesntExist(destination)) return false;
            return File.GetLastWriteTime(source) == File.GetLastWriteTime(destination);
        }

        /// <inheritdoc cref="IsDestinationNewer(string, string)"/>
        public static bool IsSameDate(FileInfo source, FileInfo destination)
        {
            if (!source.Exists) return false;
            if (!destination.Exists) return false;
            return source.LastWriteTime == destination.LastWriteTime;
        }

        /// <inheritdoc cref="IsDestinationNewer(string, string)"/>
        public static bool IsSameDate(this IFilePair copier)
        {
            return IsSameDate(copier.Source, copier.Destination);
        }

        #endregion
    }
}
