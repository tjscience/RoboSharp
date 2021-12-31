using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboSharp.Results
{
    /// <summary>
    /// Helper class to build a <see cref="RoboCopyResults"/> object.
    /// </summary>
    internal class ResultsBuilder
    {
        private ResultsBuilder() { }

        internal ResultsBuilder(RoboSharpConfiguration config) { Config = config; }

        private readonly List<string> outputLines = new List<string>();

        private RoboSharpConfiguration Config { get; }

        #region < Command Options Properties >

        /// <see cref="RoboCommand.CommandOptions"/>
        internal string CommandOptions { get; set; }

        /// <inheritdoc cref="CopyOptions.Source"/>
        internal string Source { get; set; }
        
        /// <inheritdoc cref="CopyOptions.Destination"/>
        internal string Destination { get; set; }

        #endregion

        #region < Counters in case cancellation >

        //Counters used to generate statistics if job is cancelled 

        private long TotalDirs { get; set; } = 0;
        private long TotalDirs_Copied { get; set; } = 0;
        private long TotalDirs_Skipped { get; set; } = 0;
        private long TotalDirs_Extras { get; set; } = 0;

        private long TotalFiles { get; set; } = 0;
        private long TotalFiles_Copied { get; set; } = 0;
        private long TotalFiles_Skipped { get; set; } = 0;
        private long TotalFiles_Extras { get; set; } = 0;
        private long TotalFiles_Mismatch { get; set; } = 0;
        private long TotalFiles_Failed { get; set; } = 0;

        private long TotalBytes { get; set; } = 0;
        private long TotalBytes_Copied { get; set; } = 0;
        private long TotalBytes_Failed { get; set; } = 0;
        private long TotalBytes_Skipped { get; set; } = 0;
        private long TotalBytes_Extra { get; set; } = 0;
        private long TotalBytes_MisMatch { get; set; } = 0;

        private bool SkippingFile;
        private bool CopyOpStarted;
        private ProcessedFileInfo CurrentDir;
        private ProcessedFileInfo CurrentFile;

        // Methods to add to internal counters -> created as methods to allow inline null check since results?.TotalDirs++; won't compile
        /// <summary>Increment <see cref="TotalDirs"/></summary>
        internal void AddDir(ProcessedFileInfo currentDir, bool CopyOperation)
        {
            TotalDirs++;
            CurrentDir = currentDir;
            if (currentDir.FileClass == Config.LogParsing_ExistingDir) { /* No Action */ }
            else if (currentDir.FileClass == Config.LogParsing_NewDir) { if (CopyOperation) TotalDirs_Copied++; }
            else if (currentDir.FileClass == Config.LogParsing_ExtraDir) TotalDirs_Extras++;
            else
            {
                
            }
        }

        /// <summary>Increment <see cref="TotalFiles"/></summary>
        internal void AddFile(ProcessedFileInfo currentFile, bool CopyOperation)
        {
            TotalFiles++;
            if (SkippingFile)
            {
                TotalFiles_Skipped++;
                TotalBytes_Skipped += CurrentFile?.Size ?? 0;
            }
            CurrentFile = currentFile;
            TotalBytes += currentFile.Size;
            SkippingFile = false;
            CopyOpStarted = false;

            // EXTRA FILES
            if (currentFile.FileClass == Config.LogParsing_ExtraFile)
            {
                TotalFiles_Extras++;
                TotalBytes_Extra += CurrentFile.Size;
            }

            //MisMatch
            else if (currentFile.FileClass == Config.LogParsing_MismatchFile)
            {
                TotalFiles_Mismatch++;
                TotalBytes_MisMatch += CurrentFile.Size;
            }

            //Failed Files
            else if (currentFile.FileClass == Config.LogParsing_FailedFile)
            {
                TotalFiles_Failed++;
                TotalBytes_Failed += currentFile.Size;
            }


            //Identical Files
            else if (currentFile.FileClass == Config.LogParsing_SameFile)
            {
                TotalFiles_Skipped++; //File is the same -> It will be skipped
                TotalBytes_Skipped += CurrentFile.Size;
                CurrentFile = null;
            }

            //Files to be Copied/Skipped
            else
            {
                SkippingFile = CopyOperation;//Assume Skipped, adjusted when CopyProgress is updated
                if (currentFile.FileClass == Config.LogParsing_NewFile) { }
                else if (currentFile.FileClass == Config.LogParsing_OlderFile) { }
                else if (currentFile.FileClass == Config.LogParsing_NewerFile) { }
            }
        }

        /// <summary>Catch start copy progress of large files</summary>
        internal void SetCopyOpStarted()
        {
            SkippingFile = false;
            CopyOpStarted = true;
        }

        /// <summary>Increment <see cref="TotalFiles_Copied"/></summary>
        internal void AddFileCopied()
        {
            SkippingFile = false;
            CopyOpStarted = false;
            TotalFiles_Copied++;
            TotalBytes_Copied += CurrentFile?.Size ?? 0;
            CurrentFile = null;
        }

        #endregion

        internal void AddOutput(string output)
        {
            if (output == null)
                return;

            if (Regex.IsMatch(output, @"^\s*[\d\.,]+%\s*$"))
                return;

            outputLines.Add(output);
        }

        internal RoboCopyResults BuildResults(int exitCode)
        {
            var res = new RoboCopyResults();
            res.Status = new RoboCopyExitStatus(exitCode);

            var statisticLines = GetStatisticLines();

            //Dir Stats
            if (exitCode >= 0 && statisticLines.Count >= 1)
                res.DirectoriesStatistic = Statistic.Parse(statisticLines[0]);
            else
                res.DirectoriesStatistic = new Statistic() { Total = TotalDirs, Copied = TotalDirs_Copied, Extras = TotalDirs_Extras};

            //File Stats
            if (exitCode >= 0 && statisticLines.Count >= 2)
                res.FilesStatistic = Statistic.Parse(statisticLines[1]);
            else
            {
                if (CopyOpStarted) TotalFiles_Failed++;
                res.FilesStatistic = new Statistic() { Total = TotalFiles, Copied = TotalFiles_Copied, Failed = TotalFiles_Failed, Extras = TotalFiles_Extras, Skipped = TotalFiles_Skipped, Mismatch = TotalFiles_Mismatch };
            }

            //Bytes
            if (exitCode >= 0 && statisticLines.Count >= 3)
                res.BytesStatistic = Statistic.Parse(statisticLines[2]);
            else
            {
                TotalBytes_Failed += CopyOpStarted ? ( CurrentFile?.Size ?? 0 ) : 0;
                res.BytesStatistic = new Statistic() { Total = TotalBytes, Copied = TotalBytes_Copied, Failed = TotalBytes_Failed, Extras = TotalBytes_Extra, Skipped = TotalBytes_Skipped, Mismatch = TotalBytes_MisMatch };
            }

            //Speed Stats
            if (exitCode >= 0 && statisticLines.Count >= 6)
                res.SpeedStatistic = SpeedStatistic.Parse(statisticLines[4], statisticLines[5]);
            else
                res.SpeedStatistic = new SpeedStatistic();

            res.LogLines = outputLines.ToArray();
            res.Source = this.Source;
            res.Destination = this.Destination;
            res.CommandOptions = this.CommandOptions;
            return res;
        }

        private List<string> GetStatisticLines()
        {
            var res = new List<string>();
            for (var i = outputLines.Count-1; i > 0; i--)
            {
                var line = outputLines[i];
                if (line.StartsWith("-----------------------"))
                    break;

                if (line.Contains(":") && !line.Contains("\\"))
                    res.Add(line);
            }

            res.Reverse();
            return res;
        }
    }
}