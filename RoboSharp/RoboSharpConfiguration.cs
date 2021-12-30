using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RoboSharp
{
    /// <summary>
    /// Setup the ErrorToken and the path to RoboCopy.exe.
    /// </summary>
    public class RoboSharpConfiguration
    {
        private static readonly IDictionary<string, RoboSharpConfiguration> 
            defaultConfigurations = new Dictionary<string, RoboSharpConfiguration>()
        {
            {"en", new RoboSharpConfiguration { ErrorToken = "ERROR"} },
            {"de", new RoboSharpConfiguration { ErrorToken = "FEHLER"} },
        };

        /// <summary>
        /// Error Token Identifier -- EN = "ERROR", DE = "FEHLER", etc <br/>
        /// Leave as / Set to null to use system default.
        /// </summary>
        public string ErrorToken
        {
            get { return errorToken ?? GetDefaultConfiguration().ErrorToken; }
            set {
                if (value != errorToken) ErrRegexInitRequired = true;
                errorToken = value; 
            }
        }
        private string errorToken = null;

        /// <summary>
        /// Regex to identify Error Tokens with during LogLine parsing
        /// </summary>
        internal Regex ErrorTokenRegex
        {
            get
            {
                if (ErrRegexInitRequired | errorTokenRegex == null)
                    errorTokenRegex = new Regex($" {this.ErrorToken} " + @"(\d{1,3}) \(0x\d{8}\) ");
                ErrRegexInitRequired = false;
                return errorTokenRegex;
            }
        }
        private Regex errorTokenRegex;
        private bool ErrRegexInitRequired;

        /// <summary>
        /// Specify the path to RoboCopy.exe here. If not set, use the default copy.
        /// </summary>
        public string RoboCopyExe
        {
            get { return roboCopyExe ?? "Robocopy.exe"; }
            set { roboCopyExe = value; }
        }
        private string roboCopyExe = null;

        private RoboSharpConfiguration GetDefaultConfiguration()
        {
            // check for default with language Tag xx-YY (e.g. en-US)
            var currentLanguageTag = System.Globalization.CultureInfo.CurrentUICulture.IetfLanguageTag;
            if (defaultConfigurations.ContainsKey(currentLanguageTag))
                return defaultConfigurations[currentLanguageTag];

            // check for default with language Tag xx (e.g. en)
            var match = Regex.Match(currentLanguageTag, @"^\w+");
            if (match.Success)
            {
                var currentMainLanguageTag = match.Value;
                if (defaultConfigurations.ContainsKey(currentMainLanguageTag))
                    return defaultConfigurations[currentMainLanguageTag];
            }

            // no match, fallback to en
            return defaultConfigurations["en"];
        }
    }
}
