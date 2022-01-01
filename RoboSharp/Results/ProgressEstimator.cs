using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RoboSharp.Results
{
    /// <summary>
    /// Object that provides <see cref="Statistic"/> objects whose events can be bound to report estimated RoboCommand progress periodically.
    /// </summary>
    public class ProgressEstimator : IDisposable
    {
        private ProgressEstimator() { }

        internal ProgressEstimator(RoboSharpConfiguration config)
        {
            Config = config;
            DirStats = new Statistic(Statistic.StatType.Directories, "Directory Stats Estimate");
            FileStats = new Statistic(Statistic.StatType.Files, "File Stats Estimate");
            ByteStats = new Statistic(Statistic.StatType.Bytes, "Byte Stats Estimate");
        }

        #region < Private Members >

        private bool SkippingFile;
        private bool CopyOpStarted;
        private List<Statistic> SubscribedStats;
        internal ProcessedFileInfo CurrentDir;
        internal ProcessedFileInfo CurrentFile;
        private bool disposedValue;

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

        /// <summary>
        /// Parse this object's stats into a <see cref="RoboCopyExitCodes"/> enum.
        /// </summary>
        /// <returns></returns>
        public RoboCopyExitCodes GetExitCode()
        {
            Results.RoboCopyExitCodes code = 0;

            //Files Copied
            if (FileStats.Copied > 0)
                code |= Results.RoboCopyExitCodes.FilesCopiedSuccessful;

            //Extra
            if (DirStats.Extras > 0 | FileStats.Extras > 0)
                code |= Results.RoboCopyExitCodes.ExtraFilesOrDirectoriesDetected;

            //MisMatch
            if (DirStats.Mismatch > 0 | FileStats.Mismatch > 0)
                code |= Results.RoboCopyExitCodes.MismatchedDirectoriesDetected;

            //Failed
            if (DirStats.Failed > 0 | FileStats.Failed > 0)
                code |= Results.RoboCopyExitCodes.SomeFilesOrDirectoriesCouldNotBeCopied;

            return code;

        }

        #region < Event Binding for Auto-Updates >

        private void BindDirStat(object o, PropertyChangedEventArgs e) => this.DirStats.AddStatistic((StatChangedEventArg)e);
        private void BindFileStat(object o, PropertyChangedEventArgs e) => this.FileStats.AddStatistic((StatChangedEventArg)e);
        private void BindByteStat(object o, PropertyChangedEventArgs e) => this.ByteStats.AddStatistic((StatChangedEventArg)e);

        /// <summary>
        /// Subscribe to the update events of a <see cref="ProgressEstimator"/> object
        /// </summary>
        internal void BindToProgressEstimator(ProgressEstimator estimator)
        {
            BindToStatistic(estimator.ByteStats);
            BindToStatistic(estimator.DirStats);
            BindToStatistic(estimator.FileStats);
        }

        /// <summary>
        /// Subscribe to the update events of a <see cref="Statistic"/> object
        /// </summary>
        internal void BindToStatistic(Statistic StatObject)
        {
            //SubScribe
            if (StatObject.Type == Statistic.StatType.Directories) StatObject.PropertyChanged += BindDirStat; //Directories
            else if (StatObject.Type == Statistic.StatType.Files) StatObject.PropertyChanged += BindFileStat; //Files
            else if (StatObject.Type == Statistic.StatType.Bytes) StatObject.PropertyChanged += BindByteStat; // Bytes
            
            //Add to binding list
            if (StatObject.Type != Statistic.StatType.Unknown)
            {
                if (SubscribedStats == null) SubscribedStats = new List<Statistic>();
                SubscribedStats.Add(StatObject);
            }
        }

        /// <summary>
        /// Unsubscribe from all bound Statistic objects
        /// </summary>
        internal void UnBind()
        {
            if (SubscribedStats != null)
            {
                foreach (Statistic c in SubscribedStats)
                {
                    if (c != null)
                    {
                        c.PropertyChanged -= BindDirStat;
                        c.PropertyChanged -= BindFileStat;
                        c.PropertyChanged -= BindByteStat;
                    }
                }
                SubscribedStats.Clear();
            }
        }

        #endregion

        #region < Disposing >
        
        /// <summary></summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    UnBind(); //Unbind to show that those Statistics objects aren't required anymore
                    SubscribedStats = null;
                }


                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ProgressEstimator()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

}
