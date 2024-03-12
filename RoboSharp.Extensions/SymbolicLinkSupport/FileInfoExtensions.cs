using System;
using System.IO;

namespace RoboSharp.Extensions.SymbolicLinkSupport
{
    /// <summary>
    /// Extension methods for System.IO.FileInfo to provide symbolic link support.
    /// </summary>
    public static class FileInfoExtensions
    {

#if !NET6_0_OR_GREATER  // Extension methods are not needed in .net6 as they are now native to FileSystemInfo
        /// <summary>
        /// Creates a symbolic link to this file at the specified path.
        /// </summary>
        /// <param name="it">the source file for the symbolic link.</param>
        /// <param name="path">the path of the symbolic link.</param>
        /// <param name="makeTargetPathRelative">whether the target should be made relative to the symbolic link. Default <c>false</c>.</param>
        public static void CreateAsSymbolicLink(this FileInfo it, string path, bool makeTargetPathRelative)
        {
            SymbolicLink.CreateAsSymbolicLink(path, it.FullName, false, makeTargetPathRelative);
        }

        /// <summary>
        /// Creates a symbolic link to this file at the specified path.
        /// </summary>
        /// <param name="it">the source file for the symbolic link.</param>
        /// <param name="path">the path of the symbolic link.</param>
        public static void CreateAsSymbolicLink(this FileInfo it, string path)
        {
            SymbolicLink.CreateAsSymbolicLink(path, it.FullName, false, false);
        }
#endif

        /// <summary>
        /// Determines whether this file is a symbolic link.
        /// </summary>
        /// <param name="it">the file in question.</param>
        /// <returns><code>true</code> if the file is, indeed, a symbolic link, <code>false</code> otherwise.</returns>
        public static bool IsSymbolicLink(this FileInfo it)
        {
#if NET6_0_OR_GREATER
            return it.LinkTarget != null;
#else
            return SymbolicLink.GetLinkTarget(it.FullName) != null;
#endif
        }

        /// <summary>
        /// Determines whether the target of this symbolic link exists.
        /// </summary>
        /// <param name="it">The symbolic link in question.</param>
        /// <returns><code>true</code> if this symbolic link is valid, <code>false</code> otherwise.</returns>
        /// <exception cref="System.ArgumentException">If the file in question is not a symbolic link.</exception>
        public static bool IsSymbolicLinkValid(this FileInfo it)
        {
            if (it.IsSymbolicLink())
            {
                return File.Exists(GetSymbolicLinkTarget(it));
            }
            return it.Exists;
        }

        /// <summary>
        /// Returns the full path to the target of this symbolic link.
        /// </summary>
        /// <param name="it">The symbolic link in question.</param>
        /// <returns>The path to the target of the symbolic link.</returns>
        /// <exception cref="System.ArgumentException">If the file in question is not a symbolic link.</exception>
        public static string GetSymbolicLinkTarget(this FileInfo it)
        {
#if NET6_0_OR_GREATER
            return it.LinkTarget ?? it.FullName;
#else
            if (!it.IsSymbolicLink())
                throw new ArgumentException("file specified is not a symbolic link.");
            return SymbolicLink.GetLinkTarget(it.FullName);
#endif
        }
    }
}