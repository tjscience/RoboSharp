using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RoboSharp.JobFileRegex
{
    internal static class LoggingOptionsRegex
    {
        /// <summary>
        /// Regex check if flag is for ListOnly
        /// </summary>
        internal static Regex REGEX_LIST_ONLY = new Regex("^\\s*(?<SWITCH>/L)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    }
}
