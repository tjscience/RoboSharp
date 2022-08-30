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
    /// Extension Methods for the <see cref="IDirectorySourceDestinationPair"/> interface
    /// </summary>
    public static class ISourceDestinationPairExtensions
    {
        private static bool InverseAny<T>(this IEnumerable<T> collection, Func<T, bool> match) => !collection.Any(match);

        #region < Eval Functions >

        /// <summary> Evaluate the roots of the Source and Destination </summary>
        /// <returns>True if the Source and Destination have the same root, otherwise false.</returns>
        public static bool IsLocatedOnSameDrive(this IDirectorySourceDestinationPair pair) => Path.GetPathRoot(pair.Source.FullName) == Path.GetPathRoot(pair.Destination.FullName);

        /// <summary> Evaluate the roots of the Source and Destination </summary>
        /// <returns>True if the Source and Destination have the same root, otherwise false.</returns>
        public static bool IsLocatedOnSameDrive(this IFileSourceDestinationPair pair) => Path.GetPathRoot(pair.Source.FullName) == Path.GetPathRoot(pair.Destination.FullName);

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool FileDoesntExist(string destination) => !File.Exists(destination);

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
        public static bool IsSourceNewer(this IFileSourceDestinationPair copier)
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
        public static bool IsDestinationNewer(this IFileSourceDestinationPair copier)
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
            return File.GetLastWriteTime(source) < File.GetLastWriteTime(destination);
        }

        /// <inheritdoc cref="IsDestinationNewer(string, string)"/>
        public static bool IsSameDate(FileInfo source, FileInfo destination)
        {
            if (!source.Exists) return false;
            if (!destination.Exists) return false;
            return source.LastWriteTime == destination.LastWriteTime;
        }

        /// <inheritdoc cref="IsDestinationNewer(string, string)"/>
        public static bool IsSameDate(this IFileSourceDestinationPair copier)
        {
            return IsDestinationNewer(copier.Source, copier.Destination);
        }

        #endregion


        #region < Create Pair Functions >

        /// <summary>
        /// Create a new DirPair object using a child of the Source directory
        /// </summary>
        /// <typeparam name="T">type of object to create</typeparam>
        /// <param name="dir">the file that is a child of either the Source or Destination</param>
        /// <param name="parent">the parent pair</param>
        /// <param name="ctor">the method used to generate the new object</param>
        /// <returns>new <see cref="IDirectorySourceDestinationPair"/> object</returns>
        public static T CreateSourceChild<T>(this IDirectorySourceDestinationPair parent, DirectoryInfo dir, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectorySourceDestinationPair
        {
            if (!dir.FullName.StartsWith(parent.Source.FullName))
                throw new ArgumentException("Unable to create DirectorySourceDestinationPair - Directory provided is not a child of the parent Source");
            return ctor(
                dir,
                new DirectoryInfo(dir.FullName.Replace(parent.Source.FullName, parent.Destination.FullName))
                );
        }

        /// <summary>
        /// Create a new DirPair object using a child of the Destination directory
        /// </summary>
        /// <param name="dir">the file that is a child of the Destination</param>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectorySourceDestinationPair, DirectoryInfo, Func{DirectoryInfo, DirectoryInfo, T})"/>
        /// <param name="ctor"/><param name="parent"/><typeparam name="T"/>
        public static T CreateDestinationChild<T>(this IDirectorySourceDestinationPair parent, DirectoryInfo dir, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectorySourceDestinationPair
        {
            if (!dir.FullName.StartsWith(parent.Destination.FullName))
                throw new ArgumentException("Unable to create DirectorySourceDestinationPair - Directory provided is not a child of the parent Destination");
            return ctor(
                new DirectoryInfo(dir.FullName.Replace(parent.Source.FullName, parent.Destination.FullName)),
                dir);
        }

        /// <summary>
        /// Create a new DirPair object using a child of the Source directory
        /// </summary>
        /// <typeparam name="T">type of object to create</typeparam>
        /// <param name="file">the file that is a child of either the Source or Destination</param>
        /// <param name="parent">the parent pair</param>
        /// <param name="ctor">the method used to generate the new object</param>
        /// <returns>new <see cref="IDirectorySourceDestinationPair"/> object</returns>
        public static T CreateSourceChild<T>(this IDirectorySourceDestinationPair parent, FileInfo file, Func<FileInfo, FileInfo, T> ctor) where T : IFileSourceDestinationPair
        {
            if (!file.FullName.StartsWith(parent.Source.FullName))
                throw new ArgumentException("Unable to create DirectorySourceDestinationPair - Directory provided is not a child of the parent Source");
            return ctor(
                file,
                new FileInfo(file.FullName.Replace(parent.Source.FullName, parent.Destination.FullName))
                );
        }

        /// <summary>
        /// Create a new FilePair object using a child of the Destination directory
        /// </summary>
        /// <param name="file">the file that is a child of the Destination</param>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectorySourceDestinationPair, FileInfo, Func{FileInfo, FileInfo, T})"/>
        /// <param name="ctor"/><param name="parent"/><typeparam name="T"/>
        public static T CreateDestinationChild<T>(this IDirectorySourceDestinationPair parent, FileInfo file, Func<FileInfo, FileInfo, T> ctor) where T : IFileSourceDestinationPair
        {
            if (!file.FullName.StartsWith(parent.Destination.FullName))
                throw new ArgumentException("Unable to create DirectorySourceDestinationPair - Directory provided is not a child of the parent Destination");
            return ctor(
                new FileInfo(file.FullName.Replace(parent.Source.FullName, parent.Destination.FullName)),
                file);
        }

        #endregion

        #region < Get File Pairs >

        /// <summary>
        /// Gets all the File Pairs from the <see cref="IDirectorySourceDestinationPair"/>
        /// </summary>
        /// <returns>Array of the FilePairs that were foudn in both the Source and Destination via <see cref="DirectoryInfo.GetFiles()"/></returns>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectorySourceDestinationPair, FileInfo, Func{FileInfo, FileInfo, T})"/>
        public static T[] GetFilePairs<T>(this IDirectorySourceDestinationPair parent, Func<FileInfo, FileInfo, T> ctor) where T : IFileSourceDestinationPair
        {
            List<T> files = new List<T>();
            if (parent.Source.Exists)
                foreach (var f in parent.Source.GetFiles())
                    files.Add(CreateSourceChild(parent, f, ctor));
            if (parent.Destination.Exists)
                foreach (var f in parent.Destination.GetFiles())
                {
                    if (files.Any(p => p.Destination.FullName == f.FullName))
                    { /* Do Nothing - File Pair already exists */ }
                    else
                    {
                        files.Add(CreateDestinationChild(parent, f, ctor));
                    }
                }
            return files.ToArray();
        }

        /// <summary>
        /// Gets all the File Pairs from the <see cref="IDirectorySourceDestinationPair"/>
        /// </summary>
        /// <returns>cached Ienumerable of the FilePairs that were found in both the Source and Destination via <see cref="DirectoryInfo.GetFiles()"/></returns>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectorySourceDestinationPair, FileInfo, Func{FileInfo, FileInfo, T})"/>
        public static IEnumerable<T> GetFilePairsEnumerable<T>(this IDirectorySourceDestinationPair parent, Func<FileInfo, FileInfo, T> ctor) where T : IFileSourceDestinationPair
        {
            CachedEnumerable<T> sourceFiles = null;
            CachedEnumerable<T> destFiles = null;
            if (parent.Source.Exists)
                sourceFiles = new CachedEnumerable<T>(parent.Source.EnumerateFiles().Select((f) => CreateSourceChild(parent, f, ctor)));
            if (parent.Destination.Exists)
            {
                if (sourceFiles is null)
                    destFiles = new CachedEnumerable<T>(parent.Destination.EnumerateFiles().Select(f => CreateDestinationChild(parent, f, ctor)));
                else
                {
                    IEnumerable<FileInfo> destFilesInfos = Directory.EnumerateFiles(parent.Destination.FullName).Where((p) => sourceFiles.InverseAny(n => n.Destination.FullName != p)).Select(f => new FileInfo(f));
                    destFiles = new CachedEnumerable<T>(destFilesInfos.Select(f => CreateDestinationChild(parent, f, ctor)));
                }
            }
            if (sourceFiles is null && destFiles is null)
                return new T[] { };
            else if (sourceFiles is null)
                return destFiles;
            else if (destFiles is null)
                return sourceFiles;
            else
                return new CachedEnumerable<T>(sourceFiles.Concat(destFiles));
        }

        #endregion

        #region < Get Directory Pairs >

        /// <summary>
        /// Gets all the Directory Pairs from the <see cref="IDirectorySourceDestinationPair"/>
        /// </summary>
        /// <returns>Array of the DirectoryPairs that were foudn in both the Source and Destination via <see cref="DirectoryInfo.GetFiles()"/></returns>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectorySourceDestinationPair, DirectoryInfo, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public static T[] GetDirectoryPairs<T>(this IDirectorySourceDestinationPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectorySourceDestinationPair
        {
            List<T> dirs = new List<T>();
            if (parent.Source.Exists)
                foreach (var f in parent.Source.GetDirectories())
                    dirs.Add(CreateSourceChild(parent, f, ctor));
            if (parent.Destination.Exists)
                foreach (var f in parent.Destination.GetDirectories())
                {
                    if (dirs.Any(p => p.Destination.FullName == f.FullName))
                    { /* Do Nothing - File Pair already exists */ }
                    else
                    {
                        dirs.Add(CreateDestinationChild(parent, f, ctor));
                    }
                }
            return dirs.ToArray();
        }

        /// <returns> IEnumerable{T} of of the Directory Pairs</returns>
        /// <inheritdoc cref="GetDirectoryPairs{T}(IDirectorySourceDestinationPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public static IEnumerable<T> GetDirectoryPairsEnumerable<T>(this IDirectorySourceDestinationPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectorySourceDestinationPair
        {
            CachedEnumerable<T> sourceChildren = null;
            CachedEnumerable<T> destChildren = null;
            if (parent.Source.Exists)
                sourceChildren = new CachedEnumerable<T>(parent.Source.EnumerateDirectories().Select((f) => CreateSourceChild(parent, f, ctor)));
            if (parent.Destination.Exists)
            {
                if (sourceChildren is null)
                    destChildren = new CachedEnumerable<T>(parent.Destination.EnumerateDirectories().Select(f => CreateDestinationChild(parent, f, ctor)));
                else
                {
                    // Enumerate the directory names that don't exist in the source children into new DirectoryInfo Objects
                    IEnumerable<DirectoryInfo> destFilesInfos = Directory.EnumerateDirectories(parent.Destination.FullName).Where((p) => sourceChildren.InverseAny(n => n.Destination.FullName != p)).Select(f => new DirectoryInfo(f));
                    destChildren = new CachedEnumerable<T>(destFilesInfos.Select(f => CreateDestinationChild(parent, f, ctor)));
                }
            }

            if (sourceChildren is null && destChildren is null)
                return new T[] { };
            else if (sourceChildren is null)
                return destChildren;
            else if (destChildren is null)
                return sourceChildren;
            else
                return new CachedEnumerable<T>(sourceChildren.Concat(destChildren));
        }

        #endregion
    }
}
