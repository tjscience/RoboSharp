using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RoboSharp.JobFileRegex
{
    internal static class SelectionOptionsRegex
    {
        /// <summary>
        /// Regex to determine if on the INCLUDE FILES section of the JobFile
        /// </summary>
        /// <remarks>
        /// Each new path / filename should be on its own line
        /// </remarks>
        internal static Regex REGEX_IncludeFiles = new Regex("^\\s*(?<SWITCH>/IF:)(?<PATH>.*)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex to determine if on the EXCLUDE FILES section of the JobFile
        /// </summary>
        /// <remarks>
        /// Each new path / filename should be on its own line
        /// </remarks>
        internal static Regex REGEX_ExcludeFiles = new Regex("^\\s*(?<SWITCH>/XF:)(?<PATH>.*)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Regex to determine if on the EXCLUDE DIRECTORIES section of the JobFile
        /// </summary>
        /// <remarks>
        /// Each new path / filename should be on its own line
        /// </remarks>
        internal static Regex REGEX_ExcludeDirs = new Regex("^\\s*(?<SWITCH>/XD:)(?<PATH>.*)(?<COMMENT>::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    }
}
