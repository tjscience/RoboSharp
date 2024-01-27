using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// This class houses the various helper functions used to parse and apply the parameters of an input robocopy command to an IRoboCommand
    /// </summary>
    /// <remarks>Exposed for unit testing</remarks>
    public static class RoboCommandParserFunctions
    {
        /// <summary>
        /// Helper object that reports the result from <see cref="ParseSourceAndDestination(string)"/>
        /// </summary>
        public readonly struct ParsedSourceDest
        {
            #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            
            public ParsedSourceDest(string source, string dest, string input, string sanitized)
            {
                Source = source;
                Dest = dest;
                InputString = input;
                SanitizedString = sanitized;
            }
            public readonly string Source;
            public readonly string Dest;
            public readonly string InputString;
            /// <summary> The InputString with the Source and Destination removed </summary>
            public readonly string SanitizedString;

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// Parse the input text, extracting the Source and Destination info.
        /// </summary>
        /// <param name="inputText">The input text to parse. 
        /// <br/>The expected pattern is :  robocopy "SOURCE" "DESTINATION" 
        /// <br/> - 'robocopy' is optional, but the source/destination must appear in the specified order at the beginning of the text. 
        /// <br/> - Quotes are only required if the path has whitespace.
        /// </param>
        /// <returns>A new <see cref="ParsedSourceDest"/> struct with the results</returns>
        public static ParsedSourceDest ParseSourceAndDestination(string inputText)
        {
            // Definition (prefix) (Source (quoted version) | (no quotes)) (dest (quoted version) | (no quotes))
            // This should handle all scenarios, including networks paths such as \\MyServer\DriveName$\Apps\
            // Note : if its contained within quotes, it simply accepts tall characters within the quotes.
            //lang=regex 
            const string fullPattern = @"^(robocopy\s*)?(?<source>(?<sQuote>"".+?[:$].+?"")|(?<sNoQuote>[^:*?""<>|\s]+?[:$][^:*?<>|\s]+))\s+(?<dest>(?<dQuote>"".+?[:$].+?"")|(?<dNoQuote>[^:*?""<>|\s]+?[:$][^:*?<>|\s]+)).*$";

            // Return the first match
            return PatternMatch(fullPattern) ?? LogNoMatch();

            ParsedSourceDest? PatternMatch(string pattern)
            {
                var match = Regex.Match(inputText, pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
                if (match.Success)
                {
                    string rawSource = match.Groups["source"].Value;
                    string rawDest = match.Groups["dest"].Value;
                    string source = rawSource.Trim('\"');
                    string dest = rawDest.Trim('\"');
                    source = source.IsPathFullyQualified() ? source : null;
                    dest = dest.IsPathFullyQualified() ? dest : null;
                    if (source is null && dest is null) return null;

                    Debugger.Instance.DebugMessage($"--> Source and Destination Pattern Match Success:");
                    Debugger.Instance.DebugMessage($"----> Pattern : " + pattern);
                    Debugger.Instance.DebugMessage($"----> Source : " + source);
                    Debugger.Instance.DebugMessage($"----> Destination : " + dest);
                    return new ParsedSourceDest(source ?? string.Empty, dest ?? string.Empty, inputText, inputText.Remove(rawSource).Remove(rawDest).TrimStart("robocopy"));
                }
                else
                {
                    return null;
                }
            }
            ParsedSourceDest LogNoMatch()
            {
                Debugger.Instance.DebugMessage($"--> Unable to detect a Source/Destination pattern match");
                return new ParsedSourceDest(string.Empty, string.Empty, inputText, inputText);
            }
        }

        /// <summary> Attempt to extract the parameter from a format pattern string </summary>
        /// <param name="inputText">The input text to evaluate</param>
        /// <param name="parameterFormatString">the parameter to extract. Example :   /LEV:{0}</param>
        /// <param name="value">The extracted value. (only if returns true)</param>
        /// <param name="modifiedText">
        /// When returning true, this will be the <paramref name="inputText"/> with the parameter pattern and the value removed.
        /// <br/> When returning false, this will be the <paramref name="inputText"/>
        /// <br/> Example:  " MyString /LEV:5"  -->  " MyString "
        /// </param>
        /// <returns>True if the value was extracted, otherwise false.</returns>
        public static bool TryExtractParameter(string inputText, string parameterFormatString, out string value, out string modifiedText)
        {
            value = string.Empty;
            string prefix = parameterFormatString.Substring(0, parameterFormatString.IndexOf('{')).TrimEnd('{').Trim(); // Turn /LEV:{0} into /LEV:

            if (!inputText.Contains(prefix, StringComparison.InvariantCultureIgnoreCase))
            {
                Debugger.Instance.DebugMessage($"--> Switch {prefix} not detected.");
                modifiedText = inputText;
                return false;
            }
            string subSection = inputText.Substring(inputText.IndexOf(prefix, StringComparison.InvariantCultureIgnoreCase)); // Get from that point forward

            int substringLength = subSection.IndexOf(" /");
            if (substringLength > 0)
            {
                subSection = subSection.Substring(0, substringLength); // Reduce the subsection down to the relevant portion by cutting off at the next parameter switch
            }

            value = subSection.Remove(prefix).Trim();
            Debugger.Instance.DebugMessage($"--> Switch {prefix} found. Value : {value}");
            modifiedText = inputText.Remove(subSection);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="flag"></param>
        /// <param name="modifiedText"></param>
        /// <param name="actionIfTrue"></param>
        /// <returns></returns>
        public static bool ExtractFlag(string inputText, string flag, out string modifiedText, Action actionIfTrue = null)
        {
            bool value = inputText.Contains(flag, StringComparison.InvariantCultureIgnoreCase);
            Debugger.Instance.DebugMessage($"--> Switch {flag}{(value ? "" : " not")} detected.");
            modifiedText = !value ? inputText : inputText.Remove(flag);
            if (value) actionIfTrue?.Invoke();
            return value;
        }

        /// <summary>
        /// Parse the string, extracting individual filters out to an IEnumerable string
        /// </summary>
        public static IEnumerable<string> ParseFilters(string stringToParse, string debugFormat)
        {
            List<string> filters = new List<string>();
            StringBuilder filterBuilder = new StringBuilder();
            bool isQuoted = false;
            bool isBuilding = false;
            foreach (char c in stringToParse)
            {
                if (isQuoted && c == '"')
                    NextFilter();
                else if (isQuoted)
                    filterBuilder.Append(c);
                else if (c == '"')
                {
                    isQuoted = true;
                    isBuilding = true;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (isBuilding) NextFilter(); // unquoted white space indicates end of one filter and start of next. Otherwise ignore whitepsace.
                }
                else
                {
                    isBuilding = true;
                    filterBuilder.Append(c);
                }
            }
            NextFilter();
            return filters;
            void NextFilter()
            {
                isQuoted = false;
                isBuilding = false;
                string value = filterBuilder.ToString();
                if (string.IsNullOrWhiteSpace(value)) return;
                Debugger.Instance.DebugMessage(string.Format(debugFormat, value));
                filters.Add(value);
                filterBuilder.Clear();
            }
        }
    }
}
