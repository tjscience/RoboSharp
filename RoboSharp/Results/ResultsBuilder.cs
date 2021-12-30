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
         
        private readonly List<string> outputLines = new List<string>();

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

        private bool CopyOpStarted;
        private ProcessedFileInfo CurrentDir;
        private ProcessedFileInfo CurrentFile;

        // Methods to add to internal counters -> created as methods to allow inline null check since results?.TotalDirs++; won't compile
        /// <summary>Increment <see cref="TotalDirs"/></summary>
        internal void AddDir(ProcessedFileInfo currentDir, bool CopyOperation)
        {
            TotalDirs++;
            CurrentDir = currentDir;
            if (currentDir.FileClass.Contains("Existing")) { /* No Action */ }
            else if (currentDir.FileClass == "New Dir") { if (CopyOperation) TotalDirs_Copied++; }
            else if (currentDir.FileClass.Contains("EXTRA")) TotalDirs_Extras++;
            else
            {
                
            }
        }

        /// <summary>Increment <see cref="TotalFiles"/></summary>
        internal void AddFile(ProcessedFileInfo currentFile, bool CopyOperation)
        {
            TotalFiles++;
            CopyOpStarted = false;
            CurrentFile = currentFile;
            TotalBytes += currentFile.Size;

            if (currentFile.FileClass.ToLower().Contains("extra")) TotalFiles_Extras++;
            else if (currentFile.FileClass.ToLower().Contains("mismatch")) TotalFiles_Mismatch++;
            else if (currentFile.FileClass.ToLower().Contains("fail"))
            {
                TotalFiles_Failed++;
                TotalBytes_Failed += currentFile.Size;
            }
            else
            {
                if (CopyOperation) TotalFiles_Skipped++; //Assume Skipped, adjusted when CopyProgress is updated

                if (currentFile.FileClass.ToLower() == "new file") { }
                else if (currentFile.FileClass.ToLower().Contains("older")) { }
                else if (currentFile.FileClass.ToLower().Contains("newer")) { }
            }
        }

        /// <summary>Increment <see cref="TotalFiles_Copied"/></summary>
        internal void SetCopyOpStarted()
        {
            if (!CopyOpStarted) TotalFiles_Skipped--; //Catch start copy progress of large files
            CopyOpStarted = true;
        }

        /// <summary>Increment <see cref="TotalFiles_Copied"/></summary>
        internal void AddFileCopied()
        {
            if (!CopyOpStarted) TotalFiles_Skipped--; //Some files are small enough they only get the first report wher it reports 100% -> this catches that scenario
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
                res.BytesStatistic = new Statistic() { Copied = TotalBytes_Copied, Total = TotalBytes, Failed = TotalBytes_Failed };
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

                if (line.Contains(":"))
                    res.Add(line);
            }

            res.Reverse();
            return res;
        }
    }
}