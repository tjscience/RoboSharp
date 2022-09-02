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
    /// Extension Methods for the <see cref="IDirectoryPair"/> interface
    /// </summary>
    public static class DirectoryPairExtensions
    {
        /// <summary>
        /// Check if any of the items in the collection is a <paramref name="match"/>
        /// </summary>
        /// <remarks>
        /// Only Positive verification should be used here.
        /// <br/>Arr = { 1, 2, 3, 4 }   
        /// <br/> - Arr.None( x => x == 5) -- Returns TRUE since none equal 5 (checks all items)
        /// <br/> - Arr.None( x => x == 3) -- Returns FALSE since 3 exists (Stops checking after the match is found)
        /// <br/> - Arr.None( x => x != 3) -- Returns FALSE since 1 != 3 ( never checked if 3 exists, because 1 passed the check )
        /// </remarks>
        /// <returns>TRUE if no matches found, FALSE if any matches found</returns>
        /// <inheritdoc cref="System.Linq.Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        public static bool None<T>(this IEnumerable<T> collection, Func<T, bool> match) => collection is null || !collection.Any(match);

        /// <summary>
        /// Check if the collection is empty
        /// </summary>
        /// <returns>TRUE if the collection is empty, otherwise false</returns>
        /// <inheritdoc cref="System.Linq.Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        public static bool None<T>(this IEnumerable<T> collection) => collection is null || !collection.Any();

        #region < Eval Functions >

        /// <summary> Evaluate the roots of the Source and Destination </summary>
        /// <returns>True if the Source and Destination have the same root, otherwise false.</returns>
        public static bool IsLocatedOnSameDrive(this IDirectoryPair pair) => Path.GetPathRoot(pair.Source.FullName) == Path.GetPathRoot(pair.Destination.FullName);

        /// <inheritdoc cref="SelectionOptionsExtensions.IsExtra(DirectoryInfo, DirectoryInfo)"/>
        public static bool IsExtra(this IDirectoryPair pair) => SelectionOptionsExtensions.IsExtra(pair.Source, pair.Destination);

        /// <inheritdoc cref="SelectionOptionsExtensions.IsLonely(DirectoryInfo, DirectoryInfo)"/>
        public static bool IsLonely(this IDirectoryPair pair) => SelectionOptionsExtensions.IsLonely(pair.Source, pair.Destination);

        #endregion

        #region < Create Pair Functions >

        /// <summary>
        /// Create a new DirPair object using a child of the Source directory
        /// </summary>
        /// <typeparam name="T">type of object to create</typeparam>
        /// <param name="dir">the file that is a child of either the Source or Destination</param>
        /// <param name="parent">the parent pair</param>
        /// <param name="ctor">the method used to generate the new object</param>
        /// <returns>new <see cref="IDirectoryPair"/> object</returns>
        public static T CreateSourceChild<T>(this IDirectoryPair parent, DirectoryInfo dir, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
        {
            if (!dir.FullName.StartsWith(parent.Source.FullName))
                throw new ArgumentException("Unable to create DirectoryPair - Directory provided is not a child of the parent Source");
            return ctor(
                dir,
                new DirectoryInfo(dir.FullName.Replace(parent.Source.FullName, parent.Destination.FullName))
                );
        }

        /// <summary>
        /// Create a new DirPair object using a child of the Destination directory
        /// </summary>
        /// <param name="dir">the file that is a child of the Destination</param>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectoryPair, DirectoryInfo, Func{DirectoryInfo, DirectoryInfo, T})"/>
        /// <param name="ctor"/><param name="parent"/><typeparam name="T"/>
        public static T CreateDestinationChild<T>(this IDirectoryPair parent, DirectoryInfo dir, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
        {
            if (!dir.FullName.StartsWith(parent.Destination.FullName))
                throw new ArgumentException("Unable to create DirectoryPair - Directory provided is not a child of the parent Destination");
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
        /// <returns>new <see cref="IDirectoryPair"/> object</returns>
        public static T CreateSourceChild<T>(this IDirectoryPair parent, FileInfo file, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
        {
            if (!file.FullName.StartsWith(parent.Source.FullName))
                throw new ArgumentException("Unable to create DirectoryPair - Directory provided is not a child of the parent Source");
            return ctor(
                file,
                new FileInfo(file.FullName.Replace(parent.Source.FullName, parent.Destination.FullName))
                );
        }

        /// <summary>
        /// Create a new FilePair object using a child of the Destination directory
        /// </summary>
        /// <param name="file">the file that is a child of the Destination</param>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectoryPair, FileInfo, Func{FileInfo, FileInfo, T})"/>
        /// <param name="ctor"/><param name="parent"/><typeparam name="T"/>
        public static T CreateDestinationChild<T>(this IDirectoryPair parent, FileInfo file, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
        {
            if (!file.FullName.StartsWith(parent.Destination.FullName))
                throw new ArgumentException("Unable to create DirectoryPair - Directory provided is not a child of the parent Destination");
            return ctor(
                new FileInfo(file.FullName.Replace(parent.Source.FullName, parent.Destination.FullName)),
                file);
        }

        #endregion

        #region < Get File Pairs >

        /// <summary>
        /// Gets all the File Pairs from the <see cref="IDirectoryPair"/>
        /// </summary>
        /// <returns>Array of the FilePairs that were foudn in both the Source and Destination via <see cref="DirectoryInfo.GetFiles()"/></returns>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectoryPair, FileInfo, Func{FileInfo, FileInfo, T})"/>
        public static T[] GetFilePairs<T>(this IDirectoryPair parent, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
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
        /// Enumerates only the <see cref="IFilePair"/> objects from the Source
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc cref="EnumerateFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T})"/>
        public static CachedEnumerable<T> EnumerateSourceFilePairs<T>(this IDirectoryPair parent, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
        {
            return new CachedEnumerable<T>(parent.Source.EnumerateFiles().Select((f) => CreateSourceChild(parent, f, ctor)));
        }

        /// <summary>
        /// Enumerates only the <see cref="IFilePair"/> objects from the Destination
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc cref="EnumerateFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T})"/>
        public static CachedEnumerable<T> EnumerateDestinationFilePairs<T>(this IDirectoryPair parent, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
        {
            return new CachedEnumerable<T>(parent.Destination.EnumerateFiles().Select((f) => CreateDestinationChild(parent, f, ctor)));
        }

        /// <summary>
        /// Gets all the File Pairs from the <see cref="IDirectoryPair"/>
        /// </summary>
        /// <returns>cached Ienumerable of the FilePairs that were found in both the Source and Destination via <see cref="DirectoryInfo.GetFiles()"/></returns>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectoryPair, FileInfo, Func{FileInfo, FileInfo, T})"/>
        public static CachedEnumerable<T> EnumerateFilePairs<T>(this IDirectoryPair parent, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
        {
            CachedEnumerable<T> sourceFiles = null;
            CachedEnumerable<T> destFiles = null;
            if (parent.Source.Exists)
                sourceFiles = parent.EnumerateSourceFilePairs(ctor);
            if (parent.Destination.Exists)
            {
                if (sourceFiles is null)
                    destFiles = parent.EnumerateDestinationFilePairs(ctor);
                else
                {
                    destFiles =
                        Directory.EnumerateFiles(parent.Destination.FullName)
                        .Where(destPath => sourceFiles.None(sourceChild => sourceChild.Destination.FullName == destPath))
                        .Select(f => new FileInfo(f))
                        .Select(f => CreateDestinationChild(parent, f, ctor))
                        .AsCachedEnumerable();
                }
            }
            if (sourceFiles is null && destFiles is null)
                return (new T[] { }).AsCachedEnumerable();
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
        /// Gets all the Directory Pairs from the <see cref="IDirectoryPair"/>
        /// </summary>
        /// <returns>Array of the DirectoryPairs that were foudn in both the Source and Destination via <see cref="DirectoryInfo.GetFiles()"/></returns>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectoryPair, DirectoryInfo, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public static T[] GetDirectoryPairs<T>(this IDirectoryPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
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


        /// <summary>
        /// Enumerates only the <see cref="IDirectoryPair"/> objects from the Source
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc cref="EnumerateDirectoryPairs"/>
        public static CachedEnumerable<T> EnumerateSourceDirectoryPairs<T>(this IDirectoryPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
        {
            return new CachedEnumerable<T>(parent.Source.EnumerateDirectories().Select((f) => CreateSourceChild(parent, f, ctor)));
        }

        /// <summary>
        /// Enumerates only the <see cref="IDirectoryPair"/> objects from the Destination
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc cref="EnumerateDirectoryPairs"/>
        public static CachedEnumerable<T> EnumerateDestinationDirectoryPairs<T>(this IDirectoryPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
        {
            return new CachedEnumerable<T>(parent.Destination.EnumerateDirectories().Select((f) => CreateDestinationChild(parent, f, ctor)));
        }

        /// <returns> IEnumerable{T} of of the Directory Pairs</returns>
        /// <inheritdoc cref="GetDirectoryPairs{T}(IDirectoryPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public static CachedEnumerable<T> EnumerateDirectoryPairs<T>(this IDirectoryPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
        {
            CachedEnumerable<T> sourceChildren = null;
            CachedEnumerable<T> destChildren = null;
            if (parent.Source.Exists)
                sourceChildren = parent.EnumerateSourceDirectoryPairs(ctor);
            if (parent.Destination.Exists)
            {
                if (sourceChildren is null)
                    destChildren = parent.EnumerateDestinationDirectoryPairs(ctor);
                else
                {
                    // Enumerate the directory names that don't exist in the source children into new DirectoryInfo Objects
                    destChildren =
                        Directory.EnumerateDirectories(parent.Destination.FullName)
                        .Where(destName => sourceChildren.None(sourceChild => sourceChild.Destination.FullName == destName))
                        .Select(d => CreateDestinationChild(parent, new DirectoryInfo(d), ctor))
                        .AsCachedEnumerable();
                }
            }

            if (sourceChildren is null && destChildren is null)
                return (new T[] { }).AsCachedEnumerable();
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
