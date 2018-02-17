using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RoboSharp
{
    public class RoboSharpConfiguration
    {
        private static readonly IDictionary<string, RoboSharpConfiguration> 
            defaultConfigurations = new Dictionary<string, RoboSharpConfiguration>()
        {
            {"en", new RoboSharpConfiguration { ErrorToken = "ERROR"} },
            {"de", new RoboSharpConfiguration { ErrorToken = "FEHLER"} },
        };

        public string ErrorToken
        {
            get { return errorToken ?? GetDefaultConfiguration().ErrorToken; }
            set { errorToken = value; }
        }
        private string errorToken = null;

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
