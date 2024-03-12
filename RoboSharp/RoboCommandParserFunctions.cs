using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("RoboSharp.UnitTests")]
namespace RoboSharp
{
    /// <summary>
    /// This class houses the various helper functions used to parse and apply the parameters of an input robocopy command to an IRoboCommand
    /// </summary>
    /// <remarks>Exposed for unit testing</remarks>
    
    internal static class RoboCommandParserFunctions
    {
        /// <summary>
        /// Helper object that reports the result from <see cref="ParseSourceAndDestination(string)"/>
        /// </summary>
        public readonly struct ParsedSourceDest
        {
            public ParsedSourceDest(string input) : this(string.Empty, string.Empty, input, input) { }

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
        }

        /// <summary>
        /// Trim robocopy from that beginning of the input string
        /// </summary>
        /// <returns>The trimmed string</returns>
        public static string TrimRobocopy(string input)
        {
            //lang=regex 
            const string rc = @"^(?<rc>\s*((?<sQuote>"".+?[:$].+?robocopy(\.exe)?"")|(?<sNoQuote>([^:*?""<>|\s]+?[:$][^:*?<>|\s]+?)?robocopy(\.exe)?))\s+)";
            var match = Regex.Match(input, rc, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture| RegexOptions.CultureInvariant);
            string ret = match.Success ? input.RemoveFirstOccurrence(match.Groups[0].Value) : input;
            return ret;
        }

        internal const string SourceDestinationUnableToParseMessage = "Source and Destination were unable to be parsed.";

        /// <summary>
        /// Parse the input text, extracting the Source and Destination info.
        /// </summary>
        /// <param name="inputText">The input text to parse. 
        /// <br/>The expected pattern is :  robocopy "SOURCE" "DESTINATION" 
        /// <br/> - 'robocopy' is optional, but the source/destination must appear in the specified order at the beginning of the text. 
        /// <br/> - Quotes are only required if the path has whitespace.
        /// </param>
        /// <returns>A new <see cref="ParsedSourceDest"/> struct with the results</returns>
        /// <exception cref="RoboCommandParserException"/>
        public static ParsedSourceDest ParseSourceAndDestination(string inputText)
        {
            // Definition (prefix) (Source (quoted version) | (no quotes)) (dest (quoted version) | (no quotes))
            // This should handle all scenarios, including networks paths such as \\MyServer\DriveName$\Apps\
            // Note : if its contained within quotes, it simply accepts tall characters within the quotes.
            // This also includes allowing both source and destination to be empty, as long as both are empty quotes : robocopy "" "" /XF
            //lang=regex 
            const string fullPattern = @"^\s*(?<source> (""\s*"") | (?<sQuote>""(.+?[:$].+?)"") | (?<sNoQuote>[^:*?""<>|\s]+?[:$][^:*?<>|\s]+) ) \s+ (?<dest> (""\s*"") | (?<dQuote>"".+?[:$].+?"") | (?<dNoQuote>[^:*?""<>|\s]+?[:$][^:*?<>|\s]+) ).*$";
            //lang=regex 
            const string fallbackPattern = @"^\s*(?<source>(""\s*"") | (?<sQuote>""(.+?[:$].+?)"") | (?<sNoQuote>[^:*?""<>|\s]+?[:$][^:*?<>|\s]+) ) (?<dest>).+";
            const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;

            var match = Regex.Match(inputText, fullPattern, regexOptions);
            RoboCommandParserException ex;
            if (!match.Success)
            {
                match = Regex.Match(inputText, fallbackPattern, regexOptions);
                if (!match.Success)
                {
                    Debugger.Instance.DebugMessage($"-- > Unable to detect a Source/Destination pattern match \n----> Input text : " + inputText);
                    ex = new RoboCommandParserException(SourceDestinationUnableToParseMessage);
                    ex.AddData("input", inputText);
                    //ex.AddData("Workaround", "One workaround to this is  to submit the input text with the source / destination empty. This can be done with an empty set of quotes at the beginning of the input string.");
                    throw ex;
                }
            }
            
            string rawSource = match.Groups["source"].Value;
            string rawDest = match.Groups["dest"].Value;
            string source = rawSource.Trim('\"');
            string dest = rawDest.Trim('\"');

            // Validate source and destination - both must be empty, or both must be fully qualified
            bool sourceEmpty = string.IsNullOrWhiteSpace(source);
            bool destEmpty = string.IsNullOrWhiteSpace(dest);
            bool sourceQualified = source.IsPathFullyQualified();
            bool destQualified = dest.IsPathFullyQualified();

            if (sourceQualified && destQualified)
            {
                Debugger.Instance.DebugMessage($"--> Source and Destination Pattern Match Success:");
                Debugger.Instance.DebugMessage($"----> Source : " + source);
                Debugger.Instance.DebugMessage($"----> Destination : " + dest);
                return new ParsedSourceDest(source, dest, inputText, inputText.RemoveFirstOccurrence(rawSource).RemoveFirstOccurrence(rawDest));
            }
            else if (sourceEmpty && destEmpty)
            {
                Debugger.Instance.DebugMessage($"--> Source and Destination Pattern Match Success: Neither specified");
                string sanitized = !match.Success ? inputText : inputText.RemoveFirstOccurrence(rawSource).RemoveFirstOccurrence(rawDest);
                return new ParsedSourceDest(string.Empty, string.Empty, inputText, sanitized);
            }
            else
            {
                Debugger.Instance.DebugMessage($"--> Unable to detect a valid Source/Destination pattern match -- Input text : " + inputText);
                Debugger.Instance.DebugMessage($"----> Source : " + source);
                Debugger.Instance.DebugMessage($"----> Destination : " + dest);
                ex = new RoboCommandParserException(message: true switch
                {
                    true when sourceEmpty && destQualified => "Destination is fully qualified, but Source is empty. See exception data.",
                    true when destEmpty && sourceQualified => "Source is fully qualified, but Destination is empty. See exception data.",
                    true when !sourceQualified && !destQualified => "Source and Destination are not fully qualified. See exception data.",
                    true when !sourceQualified => "Source is not fully qualified. See exception data. ",
                    true when !destQualified => "Destination is not fully qualified. See exception data.",
                    _ => "Source / Destination Parsing Error",
                });
                ex.AddData("Input Text", inputText);
                ex.AddData("Parsed Source", rawSource);
                ex.AddData("Parsed Destination", rawDest);
                throw ex;
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

            value = subSection.RemoveFirstOccurrence(prefix).Trim();
            Debugger.Instance.DebugMessage($"--> Switch {prefix} found. Value : {value}");
            modifiedText = inputText.RemoveFirstOccurrence(subSection);
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
            modifiedText = !value ? inputText : inputText.RemoveFirstOccurrence(flag);
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
                filters.Add(value.Trim());
                filterBuilder.Clear();
            }
        }

        //lang=regex
        const string XF_Pattern = @"(?<filter>\/XF\s*( ((?<Quotes>""(\/\/[a-zA-Z]|[A-Z]:|[^/:\s])?[\w\*$\-\/\\.\s]+"") | (?<NoQuotes>(\/\/[a-zA-Z]|[A-Z]:|[^\/\:\s])?[\w*$\-\/\\.]+)) (\s*(?!\/[a-zA-Z])) )+)";
        //lang=regex
        const string XD_Pattern = @"(?<filter>\/XD\s*(( (?<Quotes>""(\/\/[a-zA-Z]|[A-Z]:|[^/:\s])?[\w\*$\-\/\\.\s]+"") | (?<NoQuotes>(\/\/[a-zA-Z]|[A-Z]:|[^\/\:\s])?[\w*$\-\/\\.]+)) (\s*(?!\/[a-zA-Z])) )+)";
        //lang=regex
        const string FileFilter = @"^\s*(?<filter>((?<Quotes>""[^""]+"") | (?<NoQuotes>((?<!\/)[^\/""])+) )+)"; // anything up until the first standalone option 

        /// <summary>
        /// File Filters to INCLUDE - These are always be at the beginning of the input string
        /// </summary>
        /// <param name="input">An input string with the source and destination removed. 
        /// <br/>Valid : *.*  ""text"" /XF  -- Reads up until the first OPTION switch
        /// <br/>Not Valid : robocopy Source destination -- these will be consdidered 3 seperate filters.
        /// <br/>Not Valid : Source/destination -- these will be considered as file filters.
        /// </param>
        /// <param name="modifiedText">Any text that was found after all filters were parsed.</param>
        public static IEnumerable<string> ExtractFileFilters(string input, out string modifiedText)
        {
            const string debugFormat = "--> Found File Filter : {0}";
            Debugger.Instance.DebugMessage($"Parsing Copy Options - Extracting File Filters");

            var match = Regex.Match(input, FileFilter, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
            string foundFilters = match.Groups["filter"].Value;
            modifiedText = input.RemoveFirstOccurrence(foundFilters);

            if (match.Success && !string.IsNullOrWhiteSpace(foundFilters))
            {
                return ParseFilters(foundFilters, debugFormat);
            }
            else
            {
                Debugger.Instance.DebugMessage($"--> No file filters found.");
#if NET452
                return new string[] { };
#else
                return Array.Empty<string>();
#endif
            }
        }

        public static IEnumerable<string> ExtractExclusionFiles(string input, out string modifiedText)
        {
            // Get Excluded Files
            Debugger.Instance.DebugMessage($"Parsing Selection Options - Extracting Excluded Files");
            var matchCollection = Regex.Matches(input, XF_Pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            if (matchCollection.Count == 0) Debugger.Instance.DebugMessage($"--> No File Exclusions found.");
            List<string> result = new List<string>();
            modifiedText = input;
            foreach (Match c in matchCollection)
            {
                string s = c.Groups["filter"].Value;
                modifiedText = modifiedText.RemoveFirstOccurrence(s);
                s = s.TrimStart("/XF").Trim();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    result.AddRange(ParseFilters(s, "---> Excluded File : {0}"));
                }
            }
            return result;
        }

        public static IEnumerable<string> ExtractExclusionDirectories(string input, out string modifiedText)
        {
            // Get Excluded Dirs
            Debugger.Instance.DebugMessage($"Parsing Selection Options - Extracting Excluded Directories");
            var matchCollection = Regex.Matches(input, XD_Pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
            if (matchCollection.Count == 0) Debugger.Instance.DebugMessage($"--> No Directory Exclusions found.");
            List<string> result = new List<string>();
            modifiedText = input;
            foreach (Match c in matchCollection)
            {
                string s = c.Groups["filter"].Value;
                modifiedText = modifiedText.RemoveFirstOccurrence(s);
                s = s.TrimStart("/XD").Trim();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    result.AddRange(ParseFilters(s, "---> Excluded Directory : {0}")); ;
                }
            }
            return result;
        }
    }
}
