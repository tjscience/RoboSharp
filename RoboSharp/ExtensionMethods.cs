using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoboSharp
{
    internal static class ExtensionMethods
    {
        public static string CleanOptionInput(this string option)
        {
            // Get rid of forward slashes
            option = option.Replace("/", "");
            // Get rid of padding
            option = option.Trim();

            return option;
        }

        public static string CleanDirectoryPath(this string path)
        {
            // Get rid of single and double quotes
            path = path?.Replace("\"", "");
            path = path?.Replace("\'", "");

            //Validate against null / empty strings. 
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            // Get rid of padding
            path = path.Trim();

            // Get rid of trailing Directory Seperator Chars
            // Length greater than 3 because E:\ is the shortest valid path
            while (path.Length > 3 && path.EndsWithDirectorySeperator())
            {
                path = path.Substring(0, path.Length - 1);
            }

            //Sanitize invalid paths -- Convert E: to E:\
            if (path.Length <= 2)
            {
                if (DriveRootRegex.IsMatch(path))
                    return path.ToUpper() + '\\';
                else
                    return path;
            }

            // Fix UNC paths that are the root directory of a UNC drive
            if (Uri.TryCreate(path, UriKind.Absolute, out Uri URI) && URI.IsUnc)
            {
                if (path.EndsWith("$"))
                {
                    path += '\\';
                }
            }

            return path;
        }

        private static readonly Regex DriveRootRegex = new Regex("[A-Za-z]:", RegexOptions.Compiled);

        /// <summary>
        /// Check if the string ends with a directory seperator character
        /// </summary>
        public static bool EndsWithDirectorySeperator(this string path) => path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString());

        /// <summary>
        /// Wait synchronously until this task has reached the specified <see cref="TaskStatus"/>
        /// </summary>
        public static void WaitUntil(this Task t, TaskStatus status )
        {
            while (t.Status < status)
                System.Threading.Thread.Sleep(150);
        }

    }
}
