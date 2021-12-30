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
            {"en", new RoboSharpConfiguration { ErrorToken = "ERROR"} }, //en uses Defaults for LogParsing properties
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

        #region < Tokens for Log Parsing >

        #region < File Tokens >

        /// <summary>
        /// Log Lines starting with this string indicate : New File -> Souce FILE Exists, Destination does not
        /// </summary>
        public string LogParsing_NewFile
        {
            get { return newFileToken ?? "New File"; }
            set { newFileToken = value; }
        }
        private string newFileToken;

        /// <summary>
        /// Log Lines starting with this string indicate : Destination File newer than Source
        /// </summary>
        public string LogParsing_OlderFile
        {
            get { return olderToken ?? "Older"; }
            set { olderToken = value; }
        }
        private string olderToken;

        /// <summary>
        /// Log Lines starting with this string indicate : Source File newer than Destination
        /// </summary>
        public string LogParsing_NewerFile
        {
            get { return newerToken ?? "Newer"; }
            set { newerToken = value; }
        }
        private string newerToken;

        /// <summary>
        /// Log Lines starting with this string indicate : Source FILE is identical to Destination File
        /// </summary>
        public string LogParsing_SameFile
        {
            get { return sameToken ?? "same"; }
            set { sameToken = value; }
        }
        private string sameToken;

        /// <summary>
        /// Log Lines starting with this string indicate : EXTRA FILE -> Destination Exists, but Source does not
        /// </summary>
        public string LogParsing_ExtraFile
        {
            get { return extraToken ?? "*EXTRA File"; }
            set { extraToken = value; }
        }
        private string extraToken;

        /// <summary>
        /// Log Lines starting with this string indicate : MISMATCH FILE
        /// </summary>
        public string LogParsing_MismatchFile
        {
            get { return mismatchToken ?? "*Mismatch"; } // TO DO: Needs Verification
            set { mismatchToken = value; }
        }
        private string mismatchToken;

        /// <summary>
        /// Log Lines starting with this string indicate : File Failed to Copy
        /// </summary>
        public string LogParsing_FailedFile
        {
            get { return failedToken ?? "*Failed"; } // TO DO: Needs Verification
            set { failedToken = value; }
        }
        private string failedToken;

        #endregion

        #region < Directory Tokens >

        /// <summary>
        /// Log Lines starting with this string indicate : New Dir -> Directory will be copied to Destination
        /// </summary>
        public string LogParsing_NewDir
        {
            get { return newerDirToken ?? "New Dir"; } 
            set { newerDirToken = value; }
        }
        private string newerDirToken;

        /// <summary>
        /// Log Lines starting with this string indicate : Extra Dir -> Does not exist in source
        /// </summary>
        public string LogParsing_ExtraDir
        {
            get { return extraDirToken ?? "*EXTRA Dir"; }
            set { extraDirToken = value; }
        }
        private string extraDirToken;

        /// <summary>
        /// Existing Dirs do not have an identifier on the line. Instead, this string will be used when creating the <see cref="ProcessedFileInfo"/> object to indicate an Existing Directory.
        /// </summary>
        public string LogParsing_ExistingDir
        {
            get { return existingDirToken ?? "Existing Dir"; }
            set { existingDirToken = value; }
        }
        private string existingDirToken;

        #endregion

        #endregion </ Tokens for Log Parsing >

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
