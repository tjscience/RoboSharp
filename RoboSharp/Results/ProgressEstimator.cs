using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using RoboSharp.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSharp.Results
{
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
    /// <para/>
    /// <see href="https://github.com/tjscience/RoboSharp/wiki/ProgressEstimator"/>
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
            DirsToAdd = new System.Collections.Concurrent.ConcurrentQueue<WhereToAdd>();
            CalculationTask = StartCalculationTask(out CalculationTaskCancelSource);
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

        private readonly Task CalculationTask;
        private readonly CancellationTokenSource CalculationTaskCancelSource;
        private readonly System.Collections.Concurrent.ConcurrentQueue<Tuple<ProcessedFileInfo, WhereToAdd>> BytesToAdd;    //Store Files in queue for calculation since bytes can be large
        private readonly System.Collections.Concurrent.ConcurrentQueue<WhereToAdd> DirsToAdd;

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
        public IStatistic BytesStatistic => ByteStatsField;

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
        /// <remarks>
        /// Used by ResultsBuilder as starting point for the results. 
        /// Should not be used anywhere else, as it kills the worker thread that calculates the Statistics objects.
        /// </remarks>
        /// <returns></returns>
        internal RoboCopyResults GetResults()
        {
            //ResultsBuilder calls this at end of run:
            // - if copy operation wasn't completed, register it as failed instead.
            // - if file was to be marked as 'skipped', then register it as skipped.
            CalculationTaskCancelSource.Cancel();
            CalculationTask.Wait();
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

        #region < Queue Files and Dirs (Internal) >

        /// <summary>Increment <see cref="DirStatField"/></summary>
        internal void AddDir(ProcessedFileInfo currentDir, bool CopyOperation)
        {
            CurrentDir = currentDir;
            if (currentDir.FileClass == Config.LogParsing_ExistingDir) { DirsToAdd.Enqueue(WhereToAdd.Skipped); }
            else if (currentDir.FileClass == Config.LogParsing_NewDir) { DirsToAdd.Enqueue(WhereToAdd.Copied); }
            else if (currentDir.FileClass == Config.LogParsing_ExtraDir) { DirsToAdd.Enqueue(WhereToAdd.Extra); }
            else
            {

            }
        }

        /// <summary>Increment <see cref="FileStatsField"/></summary>
        internal void AddFile(ProcessedFileInfo currentFile, bool CopyOperation)
        {
            if (SkippingFile)
            {
                QueueByteCalc(currentFile, WhereToAdd.Skipped);
            }

            CurrentFile = currentFile;
            SkippingFile = false;
            CopyOpStarted = false;

            // EXTRA FILES
            if (currentFile.FileClass == Config.LogParsing_ExtraFile)
            {
                QueueByteCalc(currentFile, WhereToAdd.Extra);
            }
            //MisMatch
            else if (currentFile.FileClass == Config.LogParsing_MismatchFile)
            {
                QueueByteCalc(currentFile, WhereToAdd.MisMatch);
            }
            //Failed Files
            else if (currentFile.FileClass == Config.LogParsing_FailedFile)
            {
                QueueByteCalc(currentFile, WhereToAdd.Failed);
            }
            //Identical Files
            else if (currentFile.FileClass == Config.LogParsing_SameFile)
            {
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
                        QueueByteCalc(currentFile, WhereToAdd.Copied);
                        SkippingFile = false;
                    }
                    else if (!CopyOperation)
                    {
                        QueueByteCalc(currentFile, WhereToAdd.Copied);
                    }
                }
                else if (currentFile.FileClass == Config.LogParsing_OlderFile)
                {
                    if (!CopyOperation && !command.SelectionOptions.ExcludeNewer)
                        QueueByteCalc(currentFile, WhereToAdd.Copied);
                }
                else if (currentFile.FileClass == Config.LogParsing_NewerFile)
                {
                    if (!CopyOperation && !command.SelectionOptions.ExcludeOlder)
                        QueueByteCalc(currentFile, WhereToAdd.Copied);
                }
            }
        }

        /// <summary>
        /// Stage / perform the calculation for the ByteStatistic
        /// </summary>
        private void QueueByteCalc(ProcessedFileInfo file, WhereToAdd whereTo)
        {
            BytesToAdd.Enqueue(new Tuple<ProcessedFileInfo, WhereToAdd>(file, whereTo));
        }

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
            QueueByteCalc(currentFile, WhereToAdd.Copied);
            CurrentFile = null;
        }

        #endregion

        #region < Calculation Task >

        private Task StartCalculationTask(out CancellationTokenSource CancelSource)
        {
            
            CancelSource = new CancellationTokenSource();
            var CS = CancelSource;
            return Task.Factory.StartNew(async () => 
                {
                    DateTime LastUpdate = DateTime.Now;
                    TimeSpan UpdatePeriod = new TimeSpan(0, 0, 0, 0, 125);
                    bool DirAdded = false;
                    bool FileAdded = false;

                    var tmpDir = new Statistic(type: Statistic.StatType.Directories);
                    var tmpFile = new Statistic(type: Statistic.StatType.Files);
                    var tmpByte = new Statistic(type: Statistic.StatType.Bytes);

                    while (!CS.IsCancellationRequested)
                    {
                        DirAdded = ClearOutDirs(LastUpdate, UpdatePeriod, tmpDir);
                        FileAdded = ClearOutBytes(LastUpdate, UpdatePeriod, tmpByte, tmpFile);

                        if (DateTime.Now.Subtract(LastUpdate) >= UpdatePeriod && FileAdded | DirAdded)
                        {
                            PushUpdate(ref DirAdded, ref FileAdded, tmpDir, tmpByte, tmpFile);
                            LastUpdate = DateTime.Now;
                        }
                        else
                            await ThreadEx.CancellableSleep(15, CS.Token);
                    }
                    await Task.Delay(250); // Provide 250ms for bags to fill up
                    UpdatePeriod = new TimeSpan(days: 5, 0, 0, 0); //Set excessively long timespan to stay in loop
                    while (!BytesToAdd.IsEmpty || !DirsToAdd.IsEmpty)   //Clean out the bags
                    {
                        DirAdded = ClearOutDirs(LastUpdate, UpdatePeriod, tmpDir);
                        FileAdded = ClearOutBytes(LastUpdate, UpdatePeriod, tmpByte, tmpFile);
                        await Task.Delay(15);
                    }
                    PushUpdate(ref DirAdded, ref FileAdded, tmpDir, tmpByte, tmpFile);

                }, CancelSource.Token, TaskCreationOptions.LongRunning, PriorityScheduler.BelowNormal).Unwrap();
            
        }

        private bool ClearOutDirs(DateTime LastUpdate, TimeSpan UpdatePeriod, Statistic tmpDir)
        {
            //Calculate Dirs
            bool DirAdded = false;
            while (DateTime.Now.Subtract(LastUpdate) < UpdatePeriod && !DirsToAdd.IsEmpty)
            {
                if (DirsToAdd.TryDequeue(out var whereToAdd))
                {
                    tmpDir.Total++;
                    switch (whereToAdd)
                    {
                        case WhereToAdd.Copied: tmpDir.Copied++; break;
                        case WhereToAdd.Extra: tmpDir.Extras++; break;
                        case WhereToAdd.Failed: tmpDir.Failed++; break;
                        case WhereToAdd.MisMatch: tmpDir.Mismatch++; break;
                        case WhereToAdd.Skipped: tmpDir.Skipped++; break;
                    }
                    DirAdded = true;
                }
            }
            return DirAdded;
        }

        private bool ClearOutBytes(DateTime LastUpdate, TimeSpan UpdatePeriod, Statistic tmpByte, Statistic tmpFile)
        {
            //Calculate Files and Bytes
            bool FileAdded = false;
            while (DateTime.Now.Subtract(LastUpdate) < UpdatePeriod && !BytesToAdd.IsEmpty)
            {
                if (BytesToAdd.TryDequeue(out var tuple))
                {
                    tmpFile.Total++;
                    tmpByte.Total += tuple.Item1.Size;
                    switch (tuple.Item2)
                    {
                        case WhereToAdd.Copied:
                            tmpFile.Copied++;
                            tmpByte.Copied += tuple.Item1.Size;
                            break;
                        case WhereToAdd.Extra:
                            tmpFile.Extras++;
                            tmpByte.Extras += tuple.Item1.Size;
                            break;
                        case WhereToAdd.Failed:
                            tmpFile.Failed++;
                            tmpByte.Failed += tuple.Item1.Size;
                            break;
                        case WhereToAdd.MisMatch:
                            tmpFile.Mismatch++;
                            tmpByte.Mismatch += tuple.Item1.Size;
                            break;
                        case WhereToAdd.Skipped:
                            tmpFile.Skipped++;
                            tmpByte.Skipped += tuple.Item1.Size;
                            break;
                    }
                    FileAdded = true;
                }
            }
            return FileAdded;
        }

        private void PushUpdate(ref bool DirAdded,ref bool FileAdded, Statistic tmpDir, Statistic tmpByte, Statistic tmpFile)
        {
            if (DirAdded)
            {
                DirStatField.AddStatistic(tmpDir);
                tmpDir.Reset();
                DirAdded = false;
            }
            if (FileAdded)
            {
                FileStatsField.AddStatistic(tmpFile);
                ByteStatsField.AddStatistic(tmpByte);
                tmpByte.Reset();
                tmpFile.Reset();
                FileAdded = false;
            }
        }

        #endregion

    }

}
