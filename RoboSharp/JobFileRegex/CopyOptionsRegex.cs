using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RoboSharp.JobFileRegex
{
    internal static class CopyOptionsRegex
    {
        /// <summary>
        /// Regex to find the SourceDirectory within the JobFile
        /// </summary>
        internal static Regex REGEX_SourceDir = new Regex("^\\s*(?<SWITCH>/SD:)(?<PATH>.*)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex to find the DestinationDirectory within the JobFile
        /// </summary>
        internal static Regex REGEX_DestinationDir = new Regex("^\\s*(?<SWITCH>/DD:)(?<PATH>.*)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex to find the DestinationDirectory within the JobFile
        /// </summary>
        internal static Regex REGEX_DestinationDir = new Regex("^\\s*(?<SWITCH>/DD:)(?<PATH>.*)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    }
}
