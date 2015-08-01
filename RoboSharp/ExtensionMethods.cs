using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    public static class ExtensionMethods
    {
        /*public static string RemoveLastSlash(this string path)
        {
            int index = path.LastIndexOf("\\");
            return path.Remove(index, "\\".Length).Insert(index, "");
        }*/

        /*public static string CleanPath(this string path)
        {
            // Get rid of forward slashes
            path = path.Replace("/", "");
            // Get rid of padding
            path = path.Trim();
            // Get rid of any initial quotes
            path = path.Replace("\"", "");
            // Get rid of any unnecessary trailing slashes
            if ((path.Length - path.Replace("\\", "").Length) > 1 && path.EndsWith("\\"))
                path = path.RemoveLastSlash();
            // Wrap the paths in quotes
            if (path.Length > 3)
                path = string.Format("\"{0}\"", path);

            return path;
        }*/

        public static string CleanOptionInput(this string option)
        {
            // Get rid of forward slashes
            option = option.Replace("/", "");
            // Get rid of padding
            option = option.Trim();

            return option;
        }
    }
}
