using System.IO;

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
            path = path.Replace("\"", "");
            path = path.Replace("\'", "");

            // Get rid of padding
            path = path.Trim();

            // Get rid of trailing Directory Seperator Chars
            // Length greater than 3 because E:\ is the shortest valid path
            while (path.Length > 3 && path.EndsWithDirectorySeperator())
            {
                path = path.Substring(0, path.Length - 1);
            }

            // Fix UNC paths that are the root directory of a UNC drive
            if (new System.Uri(path).IsUnc)
            {
                if (path.EndsWith("$"))
                {
                    path += '\\';
                }
            }

            return path;
        }

        /// <summary>
        /// Check if the string ends with a directory seperator character
        /// </summary>
        public static bool EndsWithDirectorySeperator(this string path) => path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString());

    }
}
