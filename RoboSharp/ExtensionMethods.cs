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

            return path.RemoveTrailingSlashes();
        }

        /// <summary>
        ///  Get rid of trailing Directory Seperator Chars
        /// </summary>
        public static string RemoveTrailingSlashes(this string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            // Get rid of trailing Directory Seperator Chars
            while (path.Length > 0 && (path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString())))
            {
                path = path.Substring(0, path.Length - 1);
            }

            return path;
        }
    }
}
