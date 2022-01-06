using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RoboSharp.Results
{
    /// <summary>
    /// Provde Read-Only Access to the a <see cref="ProgressEstimator"/> object
    /// </summary>
    public interface IProgressEstimator
    {
        /// <inheritdoc cref="ProgressEstimator.DirStats"/>
        IStatistic DirStats { get; }

        /// <inheritdoc cref="ProgressEstimator.FileStats"/>
        IStatistic FileStats { get; }

        /// <inheritdoc cref="ProgressEstimator.ByteStats"/>
        IStatistic ByteStats { get; }
    }
    
    /// <summary>
    /// Object that provides <see cref="Statistic"/> objects whose events can be bound to report estimated RoboCommand progress periodically.
    /// </summary>
    /// <remarks>
    /// Subscribe to <see cref="RoboCommand.OnProgressEstimatorCreated"/> or <see cref="RoboQueue.OnProgressEstimatorCreated"/> to be notified when the ProgressEstimator becomes available for binding <br/>
    /// Create event handler to subscribe to the Events you want to handle: <para/>
    /// <code>
    /// private void OnProgressEstimatorCreated(object sender, Results.ProgressEstimatorCreatedEventArgs e) { <br/>
    /// e.ResultsEstimate.ByteStats.PropertyChanged += ByteStats_PropertyChanged;<br/>
    /// e.ResultsEstimate.DirStats.PropertyChanged += DirStats_PropertyChanged;<br/>
    /// e.ResultsEstimate.FileStats.PropertyChanged += FileStats_PropertyChanged;<br/>
    /// }<br/>
    /// </code>
    /// </remarks>
    public class ProgressEstimator : IDisposable, IProgressEstimator
    {
        private ProgressEstimator() { }

        internal ProgressEstimator(RoboSharpConfiguration config)
        {
            Config = config;
            DirStatField = new Statistic(Statistic.StatType.Directories, "Directory Stats Estimate");
            FileStatsField = new Statistic(Statistic.StatType.Files, "File Stats Estimate");
            ByteStatsField = new Statistic(Statistic.StatType.Bytes, "Byte Stats Estimate");
        }

        #region < Private Members >

        private bool SkippingFile;
        private bool CopyOpStarted;
        private List<IStatistic> SubscribedStats;
        internal ProcessedFileInfo CurrentDir;
        internal ProcessedFileInfo CurrentFile;
        private bool disposedValue;

        private readonly Statistic DirStatField;
        private readonly Statistic FileStatsField;
        private readonly Statistic ByteStatsField;

        #endregion

        #region < Public Properties > 

        /// <summary>
        /// Estimate of current number of directories processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic DirStats => DirStatField;

        /// <summary>
        /// Estimate of current number of files processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic FileStats => FileStatsField;

        /// <summary>
        /// Estimate of current number of bytes processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic ByteStats => ByteStatsField;

        /// <summary>
        /// <inheritdoc cref="RoboSharpConfiguration"/>
        /// </summary>
        public RoboSharpConfiguration Config { get; }

        #endregion

        #region < Counting Methods >

        /// <summary>Increment <see cref="DirStatField"/></summary>
        internal void AddDir(ProcessedFileInfo currentDir, bool CopyOperation)
        {
            DirStatField.Total++;
            CurrentDir = currentDir;
            if (currentDir.FileClass == Config.LogParsing_ExistingDir) { DirStatField.Skipped++; }
            else if (currentDir.FileClass == Config.LogParsing_NewDir) { if (CopyOperation) DirStatField.Copied++; }
            else if (currentDir.FileClass == Config.LogParsing_ExtraDir) DirStatField.Extras++;
            else
            {

            }
        }

        /// <summary>Increment <see cref="FileStatsField"/></summary>
        internal void AddFile(ProcessedFileInfo currentFile, bool CopyOperation)
        {
            FileStatsField.Total++;
            if (SkippingFile)
            {
                FileStatsField.Skipped++;
                ByteStatsField.Skipped += CurrentFile?.Size ?? 0;
            }
            CurrentFile = currentFile;
            ByteStatsField.Total += currentFile.Size;
            SkippingFile = false;
            CopyOpStarted = false;

            // EXTRA FILES
            if (currentFile.FileClass == Config.LogParsing_ExtraFile)
            {
                FileStatsField.Extras++;
                ByteStatsField.Extras += currentFile.Size;
            }

            //MisMatch
            else if (currentFile.FileClass == Config.LogParsing_MismatchFile)
            {
                FileStatsField.Mismatch++;
                ByteStatsField.Mismatch += currentFile.Size;
            }

            //Failed Files
            else if (currentFile.FileClass == Config.LogParsing_FailedFile)
            {
                FileStatsField.Failed++;
                ByteStatsField.Failed += currentFile.Size;
            }


            //Identical Files
            else if (currentFile.FileClass == Config.LogParsing_SameFile)
            {
                FileStatsField.Skipped++; //File is the same -> It will be skipped
                ByteStatsField.Skipped += currentFile.Size;
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

        /// <summary>Increment <see cref="FileStatsField"/>.Copied</summary>
        internal void AddFileCopied()
        {
            SkippingFile = false;
            CopyOpStarted = false;
            FileStatsField.Copied++;
            ByteStatsField.Copied += CurrentFile?.Size ?? 0;
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
                FileStatsField.Failed++;
                ByteStatsField.Failed += CurrentFile.Size;
            }
            else if (SkippingFile && CurrentFile != null)
            {
                FileStatsField.Skipped++;
                ByteStatsField.Skipped += CurrentFile.Size;
            }

            // Package up
            return new RoboCopyResults()
            {
                BytesStatistic = this.ByteStatsField,
                DirectoriesStatistic = DirStatField,
                FilesStatistic = FileStatsField,
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
            if (FileStatsField.Copied > 0)
                code |= Results.RoboCopyExitCodes.FilesCopiedSuccessful;

            //Extra
            if (DirStatField.Extras > 0 | FileStatsField.Extras > 0)
                code |= Results.RoboCopyExitCodes.ExtraFilesOrDirectoriesDetected;

            //MisMatch
            if (DirStatField.Mismatch > 0 | FileStatsField.Mismatch > 0)
                code |= Results.RoboCopyExitCodes.MismatchedDirectoriesDetected;

            //Failed
            if (DirStatField.Failed > 0 | FileStatsField.Failed > 0)
                code |= Results.RoboCopyExitCodes.SomeFilesOrDirectoriesCouldNotBeCopied;

            return code;

        }

        #region < Event Binding for Auto-Updates >

        private void BindDirStat(object o, PropertyChangedEventArgs e) => this.DirStatField.AddStatistic((StatChangedEventArg)e);
        private void BindFileStat(object o, PropertyChangedEventArgs e) => this.FileStatsField.AddStatistic((StatChangedEventArg)e);
        private void BindByteStat(object o, PropertyChangedEventArgs e) => this.ByteStatsField.AddStatistic((StatChangedEventArg)e);

        /// <summary>
        /// Subscribe to the update events of a <see cref="ProgressEstimator"/> object
        /// </summary>
        internal void BindToProgressEstimator(IProgressEstimator estimator)
        {
            BindToStatistic(estimator.ByteStats);
            BindToStatistic(estimator.DirStats);
            BindToStatistic(estimator.FileStats);
        }

        /// <summary>
        /// Subscribe to the update events of a <see cref="Statistic"/> object
        /// </summary>
        internal void BindToStatistic(IStatistic StatObject)
        {
            //SubScribe
            if (StatObject.Type == Statistic.StatType.Directories) StatObject.PropertyChanged += BindDirStat; //Directories
            else if (StatObject.Type == Statistic.StatType.Files) StatObject.PropertyChanged += BindFileStat; //Files
            else if (StatObject.Type == Statistic.StatType.Bytes) StatObject.PropertyChanged += BindByteStat; // Bytes
        }

        /// <summary>
        /// Unsubscribe from all bound Statistic objects
        /// </summary>
        internal void UnBind()
        {
            if (SubscribedStats != null)
            {
                foreach (IStatistic c in SubscribedStats)
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
