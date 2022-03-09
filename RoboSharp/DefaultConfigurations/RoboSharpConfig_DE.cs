using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboSharp.DefaultConfigurations
{
    internal class RoboSharpConfig_DE : RoboSharpConfiguration
    {
        public RoboSharpConfig_DE() : base()
        {
            errorToken = "FEHLER";
            errorTokenRegex = RoboSharpConfiguration.ErrorTokenRegexGenerator(errorToken);
            //errorTokenRegex = new Regex($" FEHLER " + @"(\d{1,3}) \(0x\d{8}\) ", RegexOptions.Compiled);

            // < File Tokens >

            //LogParsing_NewFile = "New File";
            //LogParsing_OlderFile = "Older";
            //LogParsing_NewerFile = "Newer";
            //LogParsing_SameFile = "same";
            //LogParsing_ExtraFile = "*EXTRA File";
            //LogParsing_MismatchFile = "*Mismatch";
            //LogParsing_FailedFile = "*Failed";
            //LogParsing_FileExclusion = "named";

            // < Directory Tokens >

            //LogParsing_NewDir = "New Dir";
            //LogParsing_ExtraDir = "*EXTRA Dir";
            //LogParsing_ExistingDir = "Existing Dir";
            //LogParsing_DirectoryExclusion = "named";

        }
    }
}