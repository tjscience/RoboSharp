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
    public static class IDirectoryPairExtensions
    {


        /// <summary> Evaluate the roots of the Source and Destination </summary>
        /// <returns>True if the Source and Destination have the same root string, otherwise false.</returns>
        public static bool IsLocatedOnSameDrive(this IDirectoryPair pair) 
            => Path.GetPathRoot(pair.Source.FullName).Equals(Path.GetPathRoot(pair.Destination.FullName), StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc cref="Helpers.SelectionOptionsExtensions.IsExtra{T}(T, T)"/>
        public static bool IsExtra(this IDirectoryPair pair)
            => pair is null ? throw new ArgumentNullException(nameof(pair)) : Helpers.SelectionOptionsExtensions.IsExtra(pair.Source, pair.Destination);

        /// <inheritdoc cref="Helpers.SelectionOptionsExtensions.IsLonely{T}(T, T)"/>
        public static bool IsLonely(this IDirectoryPair pair)
            => pair is null ? throw new ArgumentNullException(nameof(pair)) : Helpers.SelectionOptionsExtensions.IsLonely(pair.Source, pair.Destination);

        /// <summary>
        /// Check if the  <see cref="IDirectoryPair.Source"/> directory is the root of its drive
        /// </summary>
        /// <returns><see langword="true"/> if the FullName of the source == Root.FullName, otherwise <see langword="false"/></returns>
        public static bool IsRootSource(this IDirectoryPair pair) => pair.Source.IsRootDir();

        /// <summary>
        /// Check if the  <see cref="IDirectoryPair.Destination"/> directory is the root of its drive
        /// </summary>
        /// <inheritdoc cref="IsRootDestination(IDirectoryPair)"/>
        public static bool IsRootDestination(this IDirectoryPair pair) => pair.Destination.IsRootDir();

        /// <summary>
        /// Check if the <paramref name="directory"/> is the root of its drive
        /// </summary>
        /// <inheritdoc cref="IsRootDestination(IDirectoryPair)"/>
        public static bool IsRootDir(this DirectoryInfo directory)
            => directory.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Equals(directory.Root.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        #region < Create Pair Functions >

        /// <summary>
        /// Create a new DirPair object using a child of the Source directory
        /// </summary>
        /// <typeparam name="T">type of object to create</typeparam>
        /// <param name="dir">the file that is a child of either the Source</param>
        /// <param name="parent">the parent pair</param>
        /// <param name="ctor">the method used to generate the new object</param>
        /// <returns>new <see cref="IDirectoryPair"/> object</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public static T CreateSourceChild<T>(this IDirectoryPair parent, DirectoryInfo dir, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
        {
            if (parent is null) throw new ArgumentNullException(nameof(parent));
            if (dir is null) throw new ArgumentNullException(nameof(dir));
            if (ctor is null) throw new ArgumentNullException(nameof(ctor));

            if (!dir.FullName.StartsWith(parent.Source.FullName))
                throw new ArgumentException("Unable to create DirectoryPair - Directory provided is not a child of the parent Source");
            return ctor(
                dir,
                new DirectoryInfo(Path.Combine(parent.Destination.FullName, dir.Name))
                );
        }

        /// <summary>
        /// Create a new DirPair object using a child of the Destination directory
        /// </summary>
        /// <param name="dir">the file that is a child of the Destination</param>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectoryPair, DirectoryInfo, Func{DirectoryInfo, DirectoryInfo, T})"/>
        /// <param name="ctor"/><param name="parent"/><typeparam name="T"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public static T CreateDestinationChild<T>(this IDirectoryPair parent, DirectoryInfo dir, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
        {
            if (parent is null) throw new ArgumentNullException(nameof(parent));
            if (dir is null) throw new ArgumentNullException(nameof(dir));
            if (ctor is null) throw new ArgumentNullException(nameof(ctor));

            if (!dir.FullName.StartsWith(parent.Destination.FullName))
                throw new ArgumentException("Unable to create DirectoryPair - Directory provided is not a child of the parent Destination");
            return ctor(
                new DirectoryInfo(Path.Combine(parent.Source.FullName, dir.Name)),
                dir);
        }

        /// <summary>
        /// Create a new DirPair object using a child of the Source directory
        /// </summary>
        /// <typeparam name="T">type of IFilePair to create</typeparam>
        /// <param name="file">the file that is a child of either the Source</param>
        /// <param name="parent">the parent pair</param>
        /// <param name="ctor">the method used to generate the new object</param>
        /// <returns>new <see cref="IDirectoryPair"/> object</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public static T CreateSourceChild<T>(this IDirectoryPair parent, FileInfo file, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
        {
            if (parent is null) throw new ArgumentNullException(nameof(parent));
            if (file is null) throw new ArgumentNullException(nameof(file));
            if (ctor is null) throw new ArgumentNullException(nameof(ctor));
            
            if (!file.FullName.StartsWith(parent.Source.FullName))
                throw new ArgumentException("Unable to create DirectoryPair - Directory provided is not a child of the parent Source");
            return ctor(
                file,
                new FileInfo(Path.Combine(parent.Destination.FullName, file.Name))
                );
        }

        /// <summary>
        /// Create a new FilePair object using a child of the Destination directory
        /// </summary>
        /// <param name="file">the file that is a child of the Destination</param>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectoryPair, FileInfo, Func{FileInfo, FileInfo, T})"/>
        /// <param name="ctor"/><param name="parent"/><typeparam name="T"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public static T CreateDestinationChild<T>(this IDirectoryPair parent, FileInfo file, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
        {
            if (parent is null) throw new ArgumentNullException(nameof(parent));
            if (file is null) throw new ArgumentNullException(nameof(file));
            if (ctor is null) throw new ArgumentNullException(nameof(ctor));
            
            if (!file.FullName.StartsWith(parent.Destination.FullName))
                throw new ArgumentException("Unable to create DirectoryPair - Directory provided is not a child of the parent Destination");
            return ctor(
                new FileInfo(Path.Combine(parent.Source.FullName, file.Name)),
                file);
        }

        #endregion

        #region < Get File Pairs >

        /// <returns>Array of the FilePairs that were found in both the Source and Destination via <see cref="DirectoryInfo.GetFiles()"/></returns>
        /// <inheritdoc cref="EnumerateFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T})"/>
        public static T[] GetFilePairs<T>(this IDirectoryPair parent, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
        {
            return EnumerateFilePairs(parent, ctor).ToArray();
        }

        /// <summary>
        /// Enumerate the pairs from the source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent">the parent directory pair</param>
        /// <param name="whereTrue">Function to decide to include the file in the enumeration or not</param>
        /// <param name="ctor">the constructor to create the filepair</param>
        /// <returns>A CachedEnumerable if the directory exists, otherwise null</returns>
        public static CachedEnumerable<T> EnumerateSourceFilePairs<T>(this IDirectoryPair parent, Func<FileInfo, FileInfo, T> ctor, Func<FileInfo, bool> whereTrue = null) where T : IFilePair
        {
            T createPair(FileInfo f) => CreateSourceChild(parent, f, ctor);
            if (!parent.Source.Exists) return null;
            if (whereTrue is null)
                return parent.Source.EnumerateFiles().Select(createPair).AsCachedEnumerable();
            else
                return parent.Source.EnumerateFiles().Where(whereTrue).Select(createPair).AsCachedEnumerable();
        }

        /// <summary>
        /// Enumerate the pairs from the destination
        /// </summary>
        /// <inheritdoc cref="EnumerateSourceFilePairs{T}(IDirectoryPair, Func{FileInfo, FileInfo, T}, Func{FileInfo, bool})"/>
        public static CachedEnumerable<T> EnumerateDestinationFilePairs<T>(this IDirectoryPair parent, Func<FileInfo, FileInfo, T> ctor, Func<FileInfo, bool> whereTrue = null) where T : IFilePair
        {
            T createPair(FileInfo f) => CreateDestinationChild(parent, f, ctor);
            if (!parent.Destination.Exists) return null;
            if (whereTrue is null)
                return parent.Destination.EnumerateFiles().Select(createPair).AsCachedEnumerable();
            else
                return parent.Destination.EnumerateFiles().Where(whereTrue).Select(createPair).AsCachedEnumerable();
            
        }

        /// <summary>
        /// Gets all the File Pairs from the <see cref="IDirectoryPair"/>
        /// </summary>
        /// <returns>cached Ienumerable of the FilePairs that were found in both the Source and Destination via <see cref="DirectoryInfo.GetFiles()"/></returns>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectoryPair, FileInfo, Func{FileInfo, FileInfo, T})"/>
        public static CachedEnumerable<T> EnumerateFilePairs<T>(this IDirectoryPair parent, Func<FileInfo, FileInfo, T> ctor) where T : IFilePair
        {
            T createPair(string f) => CreateDestinationChild(parent, new FileInfo(f), ctor);
            CachedEnumerable<T> sourceFiles = null;
            CachedEnumerable<T> destFiles = null;

            if (parent.Source.Exists)
                sourceFiles = EnumerateSourceFilePairs(parent, ctor);
            if (parent.Destination.Exists)
            {
                if (sourceFiles is null)
                    destFiles = EnumerateDestinationFilePairs(parent, ctor);
                else
                {
                    destFiles =
                        Directory.EnumerateFiles(parent.Destination.FullName)
                        .Where(destPath => sourceFiles.None(sourceChild => sourceChild.Destination.FullName == destPath))
                        .Select(createPair)
                        .AsCachedEnumerable();
                }
            }
            if (sourceFiles is null && destFiles is null)
                return CachedEnumerable<T>.Empty;
            else if (sourceFiles is null)
                return destFiles;
            else if (destFiles is null)
                return sourceFiles;
            else
                return new CachedEnumerable<T>(destFiles.Concat(sourceFiles));
        }

        #endregion

        #region < Get Directory Pairs >

        /// <returns>Array of the DirectoryPairs that were foudn in both the Source and Destination via <see cref="DirectoryInfo.GetFiles()"/></returns>
        /// <inheritdoc cref="EnumerateDirectoryPairs{T}(IDirectoryPair, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public static T[] GetDirectoryPairs<T>(this IDirectoryPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
        {
            return EnumerateDirectoryPairs(parent, ctor).ToArray();
        }

        /// <summary>
        /// Enumerate the pairs from the source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent">the parent directory pair</param>
        /// <param name="whereTrue">Function to decide to include the directory in the enumeration or not</param>
        /// <param name="ctor">the constructor to create the Directory Pair</param>
        /// <returns>A CachedEnumerable if the directory exists, otherwise null</returns>
        public static CachedEnumerable<T> EnumerateSourceDirectoryPairs<T>(this IDirectoryPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor, Func<DirectoryInfo, bool> whereTrue = null) where T : IDirectoryPair
        {
            T createPair(DirectoryInfo d) => CreateSourceChild(parent, d, ctor);
            if (!parent.Source.Exists) return null;
            if (whereTrue is null)
                return parent.Source.EnumerateDirectories().Select(createPair).AsCachedEnumerable();
            else
                return parent.Source.EnumerateDirectories().Where(whereTrue).Select(createPair).AsCachedEnumerable();
        }

        /// <summary>
        /// Enumerate the pairs from the destination
        /// </summary>
        /// <inheritdoc cref="EnumerateSourceDirectoryPairs{T}(IDirectoryPair, Func{DirectoryInfo, DirectoryInfo, T}, Func{DirectoryInfo, bool})"/>
        public static CachedEnumerable<T> EnumerateDestinationDirectoryPairs<T>(this IDirectoryPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor, Func<DirectoryInfo, bool> whereTrue = null) where T : IDirectoryPair
        {
            T createPair(DirectoryInfo d) => CreateDestinationChild(parent, d, ctor);
            if (!parent.Destination.Exists) return null;
            if (whereTrue is null)
                return parent.Destination.EnumerateDirectories().Select(createPair).AsCachedEnumerable();
            else
                return parent.Destination.EnumerateDirectories().Where(whereTrue).Select(createPair).AsCachedEnumerable();
        }

        /// <summary>
        /// Gets all the Directory Pairs from the <see cref="IDirectoryPair"/>
        /// </summary>
        /// <returns> IEnumerable{T} of of the Directory Pairs</returns>
        /// <inheritdoc cref="CreateSourceChild{T}(IDirectoryPair, DirectoryInfo, Func{DirectoryInfo, DirectoryInfo, T})"/>
        public static CachedEnumerable<T> EnumerateDirectoryPairs<T>(this IDirectoryPair parent, Func<DirectoryInfo, DirectoryInfo, T> ctor) where T : IDirectoryPair
        {
            T createPair(string d) => CreateDestinationChild(parent, new DirectoryInfo(d), ctor);
            CachedEnumerable<T> sourceChildren = null;
            CachedEnumerable<T> destChildren = null;
            if (parent.Source.Exists)
                sourceChildren = parent.Source.EnumerateDirectories().Select((f) => CreateSourceChild(parent, f, ctor)).AsCachedEnumerable();
            if (parent.Destination.Exists)
            {
                if (sourceChildren is null)
                    destChildren = parent.Destination.EnumerateDirectories().Select((f) => CreateDestinationChild(parent, f, ctor)).AsCachedEnumerable();
                else
                {
                    // Enumerate the directory names that don't exist in the source children into new DirectoryInfo Objects
                    destChildren =
                        Directory.EnumerateDirectories(parent.Destination.FullName)
                        .Where(destName => sourceChildren.None(sourceChild => sourceChild.Destination.FullName == destName))
                        .Select(createPair)
                        .AsCachedEnumerable();
                }
            }

            if (sourceChildren is null && destChildren is null)
                return CachedEnumerable<T>.Empty;
            else if (sourceChildren is null)
                return destChildren;
            else if (destChildren is null)
                return sourceChildren;
            else
                return new CachedEnumerable<T>(destChildren.Concat(sourceChildren));
        }

        #endregion

    }
}
