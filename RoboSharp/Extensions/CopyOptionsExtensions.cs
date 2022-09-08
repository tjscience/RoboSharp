using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Extension Methods for Copy options to assist with custom implementations
    /// </summary>
    public static class CopyOptionsExtensions
    {

        /// <summary>
        /// Evaluate the pair and determine if it the file at the destination should be purged or not
        /// </summary>
        /// <param name="command"></param>
        /// <param name="pair"></param>
        /// <returns>TRUE if destination file should be deleted, otherwise false.</returns>
        public static bool ShouldPurge(this IRoboCommand command, IFilePair pair)
        {
            bool destExists = pair.Destination.Exists;
            bool sourceExists = pair.Source.Exists;
            if ((command.CopyOptions.Mirror | command.CopyOptions.Purge) && destExists && !sourceExists)
                return command.SelectionOptions.ExcludeExtra;
            else
                return false;
        }

        #region < IncludedFiles >

        /// <summary>
        /// Determine if the file should be rejected based on its filename by checking against the FileFilter 
        /// </summary>
        /// <param name="options">Evaluates <see cref="CopyOptions.FileFilter"/> and generates a regex collection based on the wildcard pattern.</param>
        /// <param name="fileName">filename to compare</param>
        /// <param name="inclusionCollection">
        /// The collection of regex objects to compare against - If this is null, a new array will be generated from <see cref="CopyOptions.FileFilter"/>. <br/>
        /// ref is used for optimization during the course of the run, to avoid creating regex for every file check.
        /// </param>
        /// <returns>
        /// If <see cref="CopyOptions.FileFilter"/> has no values, or only contains <see cref="CopyOptions.DefaultFileFilter"/>, this method will always return true.
        /// </returns>
        public static bool ShouldIncludeFileName(this CopyOptions options, string fileName, ref Regex[] inclusionCollection)
        {
            if (inclusionCollection is null)
            {
                //Check if any filters exist, or if the single filter is equivalent to the default filter
                if (options.FileFilter.None() || !options.FileFilter.HasMultiple() && options.FileFilter.Single() == CopyOptions.DefaultFileFilter)
                {
                    inclusionCollection = new Regex[] { };
                    return true;
                } 
                //Non-Default filters have been specified - convert into regex
                List<Regex> reg = new List<Regex>();
                foreach (string s in options.FileFilter)
                {
                    reg.Add(SelectionOptionsExtensions.SanitizeFileNameRegex(s));
                }
                inclusionCollection = reg.ToArray();
            }
            if (inclusionCollection.Length == 0) return true;
            return inclusionCollection.Any(r => r.IsMatch(fileName));
        }

        /// <inheritdoc cref="ShouldIncludeFileName(CopyOptions, string, ref Regex[])"/>
        public static bool ShouldIncludeFileName(this CopyOptions options, FileInfo Source, ref Regex[] inclusionCollection) => ShouldIncludeFileName(options, Source.Name, ref inclusionCollection);

        /// <inheritdoc cref="ShouldIncludeFileName(CopyOptions, string, ref Regex[])"/>
        public static bool ShouldIncludeFileName(this CopyOptions options, IFilePair comparer, ref Regex[] inclusionCollection) => ShouldIncludeFileName(options, comparer.Source.Name, ref inclusionCollection);

        #endregion

        #region < SetAttributes >

        /// <summary>
        /// Parses <see cref="CopyOptions.AddAttributes"/> and <see cref="CopyOptions.RemoveAttributes"/> and applies them to the destination. <br/> Any attribute addition comes before attribute removal.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="destination"></param>
        public static void SetFileAttributes(this CopyOptions options, FileInfo destination)
        {
            var Addattr = SelectionOptions.ConvertFileAttrStringToEnum(options.AddAttributes);
            var Subattr = SelectionOptions.ConvertFileAttrStringToEnum(options.RemoveAttributes);
            if (Addattr.HasValue)
                destination.Attributes &= Addattr.Value;
            if (Subattr.HasValue)
                destination.Attributes &= ~Subattr.Value;
        }

        /// <inheritdoc cref="SetFileAttributes(CopyOptions, FileInfo)"/>
        public static void SetFileAttributes(this CopyOptions options, string destination)
        {
            SetFileAttributes(options, new FileInfo(destination));
        }

        /// <inheritdoc cref="SetFileAttributes(CopyOptions, FileInfo)"/>
        public static void SetFileAttributes(this CopyOptions options, IFilePair comparer)
        {
            SetFileAttributes(options, comparer.Destination);
            
        }

        #endregion

        #region < AddAttributes >

        /// <summary>
        /// Add the attributes to the destination
        /// </summary>
        /// <param name="options"></param>
        /// <param name="destination"></param>
        public static void AddFileAttributes(this CopyOptions options, FileInfo destination)
        {
            var attr = SelectionOptions.ConvertFileAttrStringToEnum(options.AddAttributes);
            if (attr is null) return;
            destination.Attributes &= attr.Value;
        }

        /// <inheritdoc cref="AddFileAttributes(CopyOptions, FileInfo)"/>
        public static void AddFileAttributes(this CopyOptions options, string destination)
        {
            AddFileAttributes(options, new FileInfo(destination));
        }

        /// <inheritdoc cref="AddFileAttributes(CopyOptions, FileInfo)"/>
        public static void AddFileAttributes(this CopyOptions options, IFilePair comparer)
        {
            AddFileAttributes(options, comparer.Destination);
        }

        #endregion

        #region < RemoveAttributes >

        /// <summary>
        /// Removes the attributes from the destination
        /// </summary>
        /// <param name="options"></param>
        /// <param name="destination"></param>
        public static void RemoveFileAttributes(this CopyOptions options, FileInfo destination)
        {
            var attr = SelectionOptions.ConvertFileAttrStringToEnum(options.RemoveAttributes);
            if (attr is null) return;
            destination.Attributes &= ~attr.Value;
        }

        /// <inheritdoc cref="RemoveFileAttributes(CopyOptions, FileInfo)"/>
        public static void RemoveFileAttributes(this CopyOptions options, string destination)
        {
            RemoveFileAttributes(options, new FileInfo(destination));
        }

        /// <inheritdoc cref="RemoveFileAttributes(CopyOptions, FileInfo)"/>
        public static void RemoveFileAttributes(this CopyOptions options, IFilePair comparer)
        {
            RemoveFileAttributes(options, comparer.Destination);
        }

        #endregion

    }
}
