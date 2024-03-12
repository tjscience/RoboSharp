using System;
using System.IO;

namespace RoboSharp.Extensions.SymbolicLinkSupport
{
    /// <summary>
    /// A class which provides extensions for <see cref="DirectoryInfo"/> to handle symbolic directory links.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
#if !NET6_0_OR_GREATER  // Extension methods are not needed in .net6 as they are now native to FileSystemInfo
        /// <summary>
        /// Creates a symbolic link to this directory at the specified path.
        /// </summary>
        /// <param name="directoryInfo">the source directory for the symbolic link.</param>
        /// <param name="path">the path of the symbolic link.</param>
        /// <param name="makeTargetPathRelative">whether the target should be made relative to the symbolic link. Default <c>false</c>.</param>
        public static void CreateAsSymbolicLink(this DirectoryInfo directoryInfo, string path, bool makeTargetPathRelative)
        {
            SymbolicLink.CreateAsSymbolicLink(path, directoryInfo.FullName, true, makeTargetPathRelative);
        }

        /// <summary>
        /// Creates a symbolic link to this directory at the specified path.
        /// </summary>
        /// <param name="directoryInfo">the source directory for the symbolic link.</param>
        /// <param name="path">the path of the symbolic link.</param>
        public static void CreateAsSymbolicLink(this DirectoryInfo directoryInfo, string path)
        {
            SymbolicLink.CreateAsSymbolicLink(path, directoryInfo.FullName, true, false);
        }
#endif

        /// <summary>
        /// Determines whether this directory is a symbolic link.
        /// </summary>
        /// <param name="directoryInfo">the directory in question.</param>
        /// <returns><code>true</code> if the directory is, indeed, a symbolic link, <code>false</code> otherwise.</returns>
        public static bool IsSymbolicLink(this DirectoryInfo directoryInfo)
        {
#if NET6_0_OR_GREATER
            return directoryInfo.LinkTarget != null;
#else
            return SymbolicLink.GetLinkTarget(directoryInfo.FullName) != null;
#endif
        }

        /// <summary>
        /// Determines whether the target of this symbolic link still exists.
        /// </summary>
        /// <param name="directoryInfo">The symbolic link in question.</param>
        /// <returns><code>true</code> if this symbolic link is valid, <code>false</code> otherwise.</returns>
        /// <exception cref="System.ArgumentException">If the directory is not a symbolic link.</exception>
        public static bool IsSymbolicLinkValid(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo.IsSymbolicLink())
            {
                return File.Exists(GetSymbolicLinkTarget(directoryInfo));
            }
            return directoryInfo.Exists;
        }

        /// <summary>
        /// Returns the full path to the target of this symbolic link.
        /// </summary>
        /// <param name="directoryInfo">The symbolic link in question.</param>
        /// <returns>The path to the target of the symbolic link.</returns>
        /// <exception cref="System.ArgumentException">If the directory in question is not a symbolic link.</exception>
        public static string GetSymbolicLinkTarget(this DirectoryInfo directoryInfo)
        {
#if NET6_0_OR_GREATER
            return directoryInfo.LinkTarget ?? directoryInfo.FullName;
#else
            if (!directoryInfo.IsSymbolicLink())
                throw new ArgumentException("file specified is not a symbolic link.");
            return SymbolicLink.GetLinkTarget(directoryInfo.FullName);
#endif
        }
    }
}