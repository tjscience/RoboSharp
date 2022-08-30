using System;
using System.IO;

namespace RoboSharp.Extensions.SymbolicLinkSupport
{
    /// <summary>
    /// Extension methods for System.IO.FileInfo to provide symbolic link support.
    /// </summary>
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Creates a symbolic link to this file at the specified path.
        /// </summary>
        /// <param name="it">the source file for the symbolic link.</param>
        /// <param name="path">the path of the symbolic link.</param>
        /// <param name="makeTargetPathRelative">whether the target should be made relative to the symbolic link. Default <c>false</c>.</param>
        public static void CreateSymbolicLink(this FileInfo it, string path, bool makeTargetPathRelative)
        {
            SymbolicLink.CreateFileLink(path, it.FullName, makeTargetPathRelative);
        }

        /// <summary>
        /// Creates a symbolic link to this file at the specified path.
        /// </summary>
        /// <param name="it">the source file for the symbolic link.</param>
        /// <param name="path">the path of the symbolic link.</param>
        public static void CreateSymbolicLink(this FileInfo it, string path)
        {
            it.CreateSymbolicLink(path, false);
        }

        /// <summary>
        /// Determines whether this file is a symbolic link.
        /// </summary>
        /// <param name="it">the file in question.</param>
        /// <returns><code>true</code> if the file is, indeed, a symbolic link, <code>false</code> otherwise.</returns>
        public static bool IsSymbolicLink(this FileInfo it)
        {
            return SymbolicLink.GetTarget(it.FullName) != null;
        }

        /// <summary>
        /// Determines whether the target of this symbolic link still exists.
        /// </summary>
        /// <param name="it">The symbolic link in question.</param>
        /// <returns><code>true</code> if this symbolic link is valid, <code>false</code> otherwise.</returns>
        /// <exception cref="System.ArgumentException">If the file in question is not a symbolic link.</exception>
        public static bool IsSymbolicLinkValid(this FileInfo it)
        {
            return File.Exists(it.GetSymbolicLinkTarget());
        }

        /// <summary>
        /// Returns the full path to the target of this symbolic link.
        /// </summary>
        /// <param name="it">The symbolic link in question.</param>
        /// <returns>The path to the target of the symbolic link.</returns>
        /// <exception cref="System.ArgumentException">If the file in question is not a symbolic link.</exception>
        public static string GetSymbolicLinkTarget(this FileInfo it)
        {
            if (!it.IsSymbolicLink())
                throw new ArgumentException("file specified is not a symbolic link.");
            return SymbolicLink.GetTarget(it.FullName);
        }
    }
}