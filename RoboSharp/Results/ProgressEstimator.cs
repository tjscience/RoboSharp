using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RoboSharp.Results
{
    /// <summary>
    /// Object that provides <see cref="IStatistic"/> objects whose events can be bound to report estimated RoboCommand / RoboQueue progress periodically.
    /// </summary>
    public interface IProgressEstimator
    {

        /// <summary>
        /// Estimate of current number of directories processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        IStatistic DirectoriesStatistic { get; }

        /// <summary>
        /// Estimate of current number of files processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        IStatistic FilesStatistic { get; }

        /// <summary>
        /// Estimate of current number of bytes processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        IStatistic BytesStatistic { get; }

        /// <summary>
        /// Parse this object's stats into a <see cref="RoboCopyExitCodes"/> enum.
        /// </summary>
        RoboCopyExitCodes GetExitCode();

    }

    /// <summary>
    /// Object that provides <see cref="IStatistic"/> objects whose events can be bound to report estimated RoboCommand progress periodically.
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
    public class ProgressEstimator : IProgressEstimator, IResults
    {
        #region < Constructors >

        private ProgressEstimator() { }

        internal ProgressEstimator(RoboCommand cmd)
        {
            command = cmd;
            DirStatField = new Statistic(Statistic.StatType.Directories, "Directory Stats Estimate");
            FileStatsField = new Statistic(Statistic.StatType.Files, "File Stats Estimate");
            ByteStatsField = new Statistic(Statistic.StatType.Bytes, "Byte Stats Estimate");

            BytesToAdd = new System.Collections.Concurrent.ConcurrentQueue<Tuple<ProcessedFileInfo, WhereToAdd>>();
            ByteCalcRequested = new Lazy<bool>(() => { DeQueueByteCalc(); return true; });
        }

        #endregion

        #region < Private Members >

        private RoboCommand command;
        private bool SkippingFile;
        private bool CopyOpStarted;

        private RoboSharpConfiguration Config => command?.Configuration;
        private readonly Statistic DirStatField;
        private readonly Statistic FileStatsField;
        private readonly Statistic ByteStatsField;

        private readonly Lazy<bool> ByteCalcRequested; //Byte Calc can be very large, so calculation only occurs after first request. Dirs and FileCounts are incremented 1 at a time, so no optimization is needed.
        private readonly System.Collections.Concurrent.ConcurrentQueue<Tuple<ProcessedFileInfo, WhereToAdd>> BytesToAdd;    //Store Files in queue for calculation since bytes can be large
        internal enum WhereToAdd { Copied, Skipped, Extra, MisMatch, Failed }

        internal ProcessedFileInfo CurrentDir;
        internal ProcessedFileInfo CurrentFile;

        #endregion

        #region < Public Properties > 

        /// <summary>
        /// Estimate of current number of directories processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic DirectoriesStatistic => DirStatField;

        /// <summary>
        /// Estimate of current number of files processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic FilesStatistic => FileStatsField;

        /// <summary>
        /// Estimate of current number of bytes processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic BytesStatistic => ByteCalcRequested.Value ? ByteStatsField : ByteStatsField;

        RoboCopyExitStatus IResults.Status => new RoboCopyExitStatus((int)GetExitCode());

        #endregion

        #region < Public Methods >

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

        #endregion

        #region < Get RoboCopyResults Object ( Internal ) >

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
                BytesStatistic = (Statistic)BytesStatistic,
                DirectoriesStatistic = (Statistic)DirectoriesStatistic,
                FilesStatistic = (Statistic)FilesStatistic,
                SpeedStatistic = new SpeedStatistic(),
            };
        }

        #endregion

        #region < Counting Methods ( Internal ) >

        /// <summary>Increment <see cref="DirStatField"/></summary>
        internal void AddDir(ProcessedFileInfo currentDir, bool CopyOperation)
        {
            CurrentDir = currentDir;
            DirStatField.Total++;
            if (currentDir.FileClass == Config.LogParsing_ExistingDir) { DirStatField.Skipped++; }
            else if (currentDir.FileClass == Config.LogParsing_NewDir) { DirStatField.Copied++; }
            else if (currentDir.FileClass == Config.LogParsing_ExtraDir) DirStatField.Extras++;
            else
            {

            }
        }

        #region < AddFile >

        /// <summary>Increment <see cref="FileStatsField"/></summary>
        internal void AddFile(ProcessedFileInfo currentFile, bool CopyOperation)
        {
            FileStatsField.Total++;
            if (SkippingFile)
            {
                FileStatsField.Skipped++;
                QueueByteCalc(currentFile, WhereToAdd.Skipped);
            }

            CurrentFile = currentFile;
            SkippingFile = false;
            CopyOpStarted = false;

            // EXTRA FILES
            if (currentFile.FileClass == Config.LogParsing_ExtraFile)
            {
                FileStatsField.Extras++;
                QueueByteCalc(currentFile, WhereToAdd.Extra);
            }
            //MisMatch
            else if (currentFile.FileClass == Config.LogParsing_MismatchFile)
            {
                FileStatsField.Mismatch++;
                QueueByteCalc(currentFile, WhereToAdd.MisMatch);
            }
            //Failed Files
            else if (currentFile.FileClass == Config.LogParsing_FailedFile)
            {
                FileStatsField.Failed++;
                QueueByteCalc(currentFile, WhereToAdd.Failed);
            }
            //Identical Files
            else if (currentFile.FileClass == Config.LogParsing_SameFile)
            {
                FileStatsField.Skipped++;
                QueueByteCalc(currentFile, WhereToAdd.Skipped);
                CurrentFile = null;
            }

            //Files to be Copied/Skipped
            else
            {
                SkippingFile = CopyOperation;//Assume Skipped, adjusted when CopyProgress is updated
                if (currentFile.FileClass == Config.LogParsing_NewFile)
                {
                    //Special handling for 0kb files -> They won't get Progress update, but will be created
                    if (currentFile.Size == 0)
                    {
                        FileStatsField.Copied++;
                        SkippingFile = false;
                    }
                    else if (!CopyOperation)
                    {
                        FileStatsField.Copied++;
                        QueueByteCalc(currentFile, WhereToAdd.Copied);
                    }
                }
                else if (currentFile.FileClass == Config.LogParsing_OlderFile)
                {
                    if (!CopyOperation && !command.SelectionOptions.ExcludeNewer)
                    {
                        FileStatsField.Copied++;
                        QueueByteCalc(currentFile, WhereToAdd.Copied);
                    }
                }
                else if (currentFile.FileClass == Config.LogParsing_NewerFile)
                {
                    if (!CopyOperation && !command.SelectionOptions.ExcludeOlder)
                    {
                        FileStatsField.Copied++;
                        QueueByteCalc(currentFile, WhereToAdd.Copied);
                    }
                }
            }
        }

        #region < Byte Stat Optimization >

        /// <summary>
        /// Stage / perform the calculation for the ByteStatistic
        /// </summary>
        private void QueueByteCalc(ProcessedFileInfo file, WhereToAdd whereTo)
        {
            if (ByteCalcRequested.IsValueCreated)
            {
                if (!BytesToAdd.IsEmpty) DeQueueByteCalc(); // Process the queue first
                CalculateByteStat(file, whereTo);
            }
            else
                BytesToAdd.Enqueue(new Tuple<ProcessedFileInfo, WhereToAdd>(file, whereTo));
        }

        /// <summary>
        /// Allows Defering calculation of DirStat until the Lazy Object is requested
        /// </summary>
        private void DeQueueByteCalc()
        {
            while (!BytesToAdd.IsEmpty)
            {
                if (BytesToAdd.TryDequeue(out var TP))
                {
                    ByteStatsField.EnablePropertyChangeEvent = BytesToAdd.IsEmpty; //Disable PropertyChangeEvent until last item in list
                    CalculateByteStat(TP.Item1, TP.Item2);
                }
            }
        }

        private void CalculateByteStat(ProcessedFileInfo currentFile, WhereToAdd whereTo)
        {
            ByteStatsField.Total += currentFile.Size;

            switch (whereTo)
            {
                case WhereToAdd.Copied:
                    ByteStatsField.Copied += currentFile.Size;
                    break;

                case WhereToAdd.Skipped:
                    ByteStatsField.Skipped += currentFile.Size;
                    break;

                case WhereToAdd.Extra:
                    ByteStatsField.Extras += currentFile.Size;
                    break;

                case WhereToAdd.MisMatch:
                    ByteStatsField.Mismatch += currentFile.Size;
                    break;

                case WhereToAdd.Failed:
                    ByteStatsField.Failed += currentFile.Size;
                    break;
            }
        }

        #endregion

        /// <summary>Catch start copy progress of large files</summary>
        internal void SetCopyOpStarted()
        {
            SkippingFile = false;
            CopyOpStarted = true;
        }

        /// <summary>Increment <see cref="FileStatsField"/>.Copied ( Triggered when copy progress = 100% ) </summary>
        internal void AddFileCopied(ProcessedFileInfo currentFile)
        {
            SkippingFile = false;
            CopyOpStarted = false;
            FileStatsField.Copied++;
            QueueByteCalc(currentFile, WhereToAdd.Copied);
            CurrentFile = null;
        }

        #endregion

        #endregion

    }

}
