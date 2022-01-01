using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp.Results
{
    /// <summary>
    /// Object that provides <see cref="Statistic"/> objects that can be bound to report estimated RoboCommand progress periodically.
    /// </summary>
    public class ProgressEstimator
    {
        private ProgressEstimator() { }

        internal ProgressEstimator(RoboSharpConfiguration config)
        {
            Config = config;
            DirStats = new Statistic();
            FileStats = new Statistic();
            ByteStats = new Statistic();
        }

        #region < Private Members >

        private bool SkippingFile;
        private bool CopyOpStarted;
        private ProcessedFileInfo CurrentDir;
        private ProcessedFileInfo CurrentFile;

        #endregion

        #region < Public Properties > 

        /// <summary>
        /// Estimate of current number of directories processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public Statistic DirStats { get; }

        /// <summary>
        /// Estimate of current number of files processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public Statistic FileStats { get; }

        /// <summary>
        /// Estimate of current number of bytes processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public Statistic ByteStats { get; }

        /// <summary>
        /// <inheritdoc cref="RoboSharpConfiguration"/>
        /// </summary>
        public RoboSharpConfiguration Config { get; }

        #endregion

        #region < Counting Methods >

        /// <summary>Increment <see cref="DirStats"/></summary>
        internal void AddDir(ProcessedFileInfo currentDir, bool CopyOperation)
        {
            DirStats.Total++;
            CurrentDir = currentDir;
            if (currentDir.FileClass == Config.LogParsing_ExistingDir) { /* No Action */ }
            else if (currentDir.FileClass == Config.LogParsing_NewDir) { if (CopyOperation) DirStats.Copied++; }
            else if (currentDir.FileClass == Config.LogParsing_ExtraDir) DirStats.Extras++;
            else
            {

            }
        }

        /// <summary>Increment <see cref="FileStats"/></summary>
        internal void AddFile(ProcessedFileInfo currentFile, bool CopyOperation)
        {
            FileStats.Total++;
            if (SkippingFile)
            {
                FileStats.Skipped++;
                ByteStats.Skipped += CurrentFile?.Size ?? 0;
            }
            CurrentFile = currentFile;
            ByteStats.Total += currentFile.Size;
            SkippingFile = false;
            CopyOpStarted = false;

            // EXTRA FILES
            if (currentFile.FileClass == Config.LogParsing_ExtraFile)
            {
                FileStats.Extras++;
                ByteStats.Extras += CurrentFile.Size;
            }

            //MisMatch
            else if (currentFile.FileClass == Config.LogParsing_MismatchFile)
            {
                FileStats.Mismatch++;
                ByteStats.Mismatch += CurrentFile.Size;
            }

            //Failed Files
            else if (currentFile.FileClass == Config.LogParsing_FailedFile)
            {
                FileStats.Failed++;
                ByteStats.Failed += currentFile.Size;
            }


            //Identical Files
            else if (currentFile.FileClass == Config.LogParsing_SameFile)
            {
                FileStats.Skipped++; //File is the same -> It will be skipped
                ByteStats.Skipped += CurrentFile.Size;
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

        /// <summary>Increment <see cref="FileStats"/>.Copied</summary>
        internal void AddFileCopied()
        {
            SkippingFile = false;
            CopyOpStarted = false;
            FileStats.Copied++;
            ByteStats.Copied += CurrentFile?.Size ?? 0;
            CurrentFile = null;
        }

        #endregion

        /// <summary>
        /// Repackage the statistics into a new <see cref="RoboCopyResults"/> object
        /// </summary>
        /// <remarks>Used by ResultsBuilder as starting point for the results.</remarks>
        /// <returns></returns>
        internal RoboCopyResults GetResults()
        {
            //ResultsBuilder calls this at end of run:
            // - if copy operation wasn't completed, register it as failed instead.
            // - if file was to be marked as 'skipped', then register it as skipped.
            if (CopyOpStarted && CurrentFile != null)
            {
                FileStats.Failed++;
                ByteStats.Failed += CurrentFile.Size;
            }
            else if (SkippingFile && CurrentFile != null)
            {
                FileStats.Skipped++;
                ByteStats.Skipped += CurrentFile.Size;
            }

            // Package up
            return new RoboCopyResults()
            {
                BytesStatistic = this.ByteStats,
                DirectoriesStatistic = DirStats,
                FilesStatistic = FileStats,
                SpeedStatistic = new SpeedStatistic(),
            };
        }
    }

}
