using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RoboSharp.JobFileRegex
{
    internal static class RetryOptionsRegex
    {
        /// <summary>
        /// Retry Count flag
        /// </summary>
        internal static Regex REGEX_RETRY = new Regex("^\\s*(?<SWITCH>/R:)(?<PATH>[0-9]*)(?<COMMENT>.*::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Retry Wait Time flag
        /// </summary>
        internal static Regex REGEX_WAIT_RETRY = new Regex("^\\s*(?<SWITCH>/W:)(?<PATH>[0-9]*)(?<COMMENT>.*::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// WAIT_FOR_SHARENAMES flag
        /// </summary>
        internal static Regex REGEX_SAVE_TO_REGISTRY = new Regex("^\\s*(?<SWITCH>/REG)(?<COMMENT>.*::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);


        /// <summary>
        /// SAVE_TO_REGISTRY flag
        /// </summary>
        internal static Regex REGEX_WAIT_FOR_SHARENAMES = new Regex("^\\s*(?<SWITCH>/TBD)(?<COMMENT>.*::.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    }
}
