using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using RoboSharp.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using RoboSharp.EventArgObjects;

namespace RoboSharp.Results
{
    /// <summary>
    /// Object that provides <see cref="IStatistic"/> objects whose events can be bound to report estimated RoboCommand progress periodically.
    /// <br/>
    /// Note: Only works properly with /V verbose set TRUE.
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

            tmpByte.EnablePropertyChangeEvent = false;
            tmpFile.EnablePropertyChangeEvent = false;
            tmpDir.EnablePropertyChangeEvent = false;
            this.StartUpdateTask(out UpdateTaskCancelSource);
        }

        #endregion

        #region < Private Members >

        private readonly RoboCommand command;
        private bool SkippingFile { get; set; }
        private bool CopyOpStarted { get; set; }
        internal bool FileFailed { get; set; }

        private RoboSharpConfiguration Config => command?.Configuration;

        // Stat Objects that will be publicly visible
        private readonly Statistic DirStatField;
        private readonly Statistic FileStatsField;
        private readonly Statistic ByteStatsField;

        internal enum WhereToAdd { Copied, Skipped, Extra, MisMatch, Failed }

        // Storage for last entered Directory and File objects 
        /// <summary>Used for providing Source Directory in CopyProgressChanged args</summary>
        internal ProcessedFileInfo CurrentDir { get; private set; }
        /// <summary>Used for providing Source Directory in CopyProgressChanged args AND for byte Statistic</summary>
        internal ProcessedFileInfo CurrentFile { get; private set; }
        /// <summary> Marked as TRUE if this is LIST ONLY mode or the file is 0KB  -- Value set during 'AddFile' method </summary>
        private bool CurrentFile_SpecialHandling { get; set; }

        //Stat objects to house the data temporarily before writing to publicly visible stat objects
        readonly Statistic tmpDir =new Statistic(type: Statistic.StatType.Directories);
        readonly Statistic tmpFile = new Statistic(type: Statistic.StatType.Files);
        readonly Statistic tmpByte = new Statistic(type: Statistic.StatType.Bytes);

        //UpdatePeriod
        private const int UpdatePeriod = 150; // Update Period in milliseconds to push Updates to a UI or RoboQueueProgressEstimator
        private readonly object DirLock = new object();     //Thread Lock for tmpDir
        private readonly object FileLock = new object();    //Thread Lock for tmpFile and tmpByte
        private readonly object UpdateLock = new object();  //Thread Lock for NextUpdatePush and UpdateTaskTrgger
        private DateTime NextUpdatePush = DateTime.Now.AddMilliseconds(UpdatePeriod);
        private TaskCompletionSource<object> UpdateTaskTrigger; // TCS that the UpdateTask awaits on
        private CancellationTokenSource UpdateTaskCancelSource; // While !Cancelled, UpdateTask continues looping

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

        /// <summary>  </summary>
        public delegate void UIUpdateEventHandler(IProgressEstimator sender, IProgressEstimatorUpdateEventArgs e);

        /// <inheritdoc cref="IProgressEstimator.ValuesUpdated"/>
        public event UIUpdateEventHandler ValuesUpdated;

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
            //Stop the Update Task
            UpdateTaskCancelSource?.Cancel();
            UpdateTaskTrigger?.TrySetResult(null);
           
            // - if copy operation wasn't completed, register it as failed instead.
            // - if file was to be marked as 'skipped', then register it as skipped.

            ProcessPreviousFile();
            PushUpdate(); // Perform Final calculation before generating the Results Object

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

        #region < Calculate Dirs (Internal) >

        /// <summary>Increment <see cref="DirStatField"/></summary>
        internal void AddDir(ProcessedFileInfo currentDir, bool CopyOperation)
        {
            
            WhereToAdd? whereTo = null;
            bool SetCurrentDir = false;
            if (currentDir.FileClass.Equals(Config.LogParsing_ExistingDir, StringComparison.CurrentCultureIgnoreCase))  // Existing Dir
            { 
                whereTo = WhereToAdd.Skipped;
                SetCurrentDir = true;
            }   
            else if (currentDir.FileClass.Equals(Config.LogParsing_NewDir, StringComparison.CurrentCultureIgnoreCase))  //New Dir
            { 
                whereTo = WhereToAdd.Copied;
                SetCurrentDir = true;
            }    
            else if (currentDir.FileClass.Equals(Config.LogParsing_ExtraDir, StringComparison.CurrentCultureIgnoreCase)) //Extra Dir
            { 
                whereTo = WhereToAdd.Extra;
                SetCurrentDir = false;
            }   
            else if (currentDir.FileClass.Equals(Config.LogParsing_DirectoryExclusion, StringComparison.CurrentCultureIgnoreCase)) //Excluded Dir
            { 
                whereTo = WhereToAdd.Skipped;
                SetCurrentDir = false;
            }
            //Store CurrentDir under various conditions
            if (SetCurrentDir) CurrentDir = currentDir;

            lock (DirLock)
            {
                switch (whereTo)
                {
                    case WhereToAdd.Copied: tmpDir.Total++; tmpDir.Copied++;break;
                    case WhereToAdd.Extra: tmpDir.Extras++; break;  //Extras do not count towards total
                    case WhereToAdd.Failed: tmpDir.Total++; tmpDir.Failed++; break;
                    case WhereToAdd.MisMatch: tmpDir.Total++; tmpDir.Mismatch++; break;
                    case WhereToAdd.Skipped: tmpDir.Total++; tmpDir.Skipped++; break;
                }
            }
            
            
            //Check if the UpdateTask should push an update to the public fields
            if (Monitor.TryEnter(UpdateLock))
            {
                if (NextUpdatePush <= DateTime.Now) 
                    UpdateTaskTrigger?.TrySetResult(null);
                Monitor.Exit(UpdateLock);
            }
        }

        #endregion

        #region < Calculate Files (Internal) >

        /// <summary>
        /// Performs final processing of the previous file if needed
        /// </summary>
        private void ProcessPreviousFile()
        {
            if (CurrentFile != null)
            {
                if (FileFailed)
                {
                    PerformByteCalc(CurrentFile, WhereToAdd.Failed);
                }
                else if (CopyOpStarted && CurrentFile_SpecialHandling)
                {
                    PerformByteCalc(CurrentFile, WhereToAdd.Copied);
                }
                else if (SkippingFile)
                {
                    PerformByteCalc(CurrentFile, WhereToAdd.Skipped);
                }
                else if (UpdateTaskCancelSource?.IsCancellationRequested ?? true)
                {
                    //Default marks as failed - This should only occur during the 'GetResults()' method due to the if statement above.
                    PerformByteCalc(CurrentFile, WhereToAdd.Failed);
                }
            }
        }

        /// <summary>Increment <see cref="FileStatsField"/></summary>
        internal void AddFile(ProcessedFileInfo currentFile, bool CopyOperation)
        {
            ProcessPreviousFile();

            CurrentFile = currentFile;
            SkippingFile = false;
            CopyOpStarted = false;
            FileFailed = false;

            // Flag to perform checks during a ListOnly operation OR for 0kb files (They won't get Progress update, but will be created)
            bool SpecialHandling = !CopyOperation || currentFile.Size == 0;
            CurrentFile_SpecialHandling = SpecialHandling;

            // EXTRA FILES
            if (currentFile.FileClass.Equals(Config.LogParsing_ExtraFile, StringComparison.CurrentCultureIgnoreCase))
            {
                PerformByteCalc(currentFile, WhereToAdd.Extra);
            }
            //MisMatch
            else if (currentFile.FileClass.Equals(Config.LogParsing_MismatchFile, StringComparison.CurrentCultureIgnoreCase))
            {
                PerformByteCalc(currentFile, WhereToAdd.MisMatch);
            }
            //Failed Files
            else if (currentFile.FileClass.Equals(Config.LogParsing_FailedFile, StringComparison.CurrentCultureIgnoreCase))
            {
                PerformByteCalc(currentFile, WhereToAdd.Failed);
            }

            //Files to be Copied/Skipped
            else
            {
                SkippingFile = CopyOperation;//Assume Skipped, adjusted when CopyProgress is updated
                if (currentFile.FileClass.Equals(Config.LogParsing_NewFile, StringComparison.CurrentCultureIgnoreCase)) // New File
                {
                    //Special handling for 0kb files & ListOnly -> They won't get Progress update, but will be created
                    if (SpecialHandling)
                    {
                        SetCopyOpStarted();
                    }
                }
                else if (currentFile.FileClass.Equals(Config.LogParsing_SameFile, StringComparison.CurrentCultureIgnoreCase))    //Identical Files
                {
                    if (command.SelectionOptions.IncludeSame)
                    {
                        if (SpecialHandling) SetCopyOpStarted();   // Only add to Copied if ListOnly / 0-bytes
                    }
                    else
                        PerformByteCalc(currentFile, WhereToAdd.Skipped);
                }
                else if (SpecialHandling) // These checks are always performed during a ListOnly operation
                {

                    switch (true)
                    {
                        //Skipped Or Copied Conditions
                        case true when currentFile.FileClass.Equals(Config.LogParsing_NewerFile, StringComparison.CurrentCultureIgnoreCase):    // ExcludeNewer
                            SkippedOrCopied(currentFile, command.SelectionOptions.ExcludeNewer);
                            break;
                        case true when currentFile.FileClass.Equals(Config.LogParsing_OlderFile, StringComparison.CurrentCultureIgnoreCase):    // ExcludeOlder
                            SkippedOrCopied(currentFile, command.SelectionOptions.ExcludeOlder);
                            break;
                        case true when currentFile.FileClass.Equals(Config.LogParsing_ChangedExclusion, StringComparison.CurrentCultureIgnoreCase):  //ExcludeChanged
                            SkippedOrCopied(currentFile, command.SelectionOptions.ExcludeChanged);
                            break;
                        case true when currentFile.FileClass.Equals(Config.LogParsing_TweakedInclusion, StringComparison.CurrentCultureIgnoreCase):  //IncludeTweaked
                            SkippedOrCopied(currentFile, !command.SelectionOptions.IncludeTweaked);
                            break;

                        //Mark As Skip Conditions
                        case true when currentFile.FileClass.Equals(Config.LogParsing_FileExclusion, StringComparison.CurrentCultureIgnoreCase):    //FileExclusion
                        case true when currentFile.FileClass.Equals(Config.LogParsing_AttribExclusion, StringComparison.CurrentCultureIgnoreCase):  //AttributeExclusion
                        case true when currentFile.FileClass.Equals(Config.LogParsing_MaxFileSizeExclusion, StringComparison.CurrentCultureIgnoreCase):     //MaxFileSizeExclusion
                        case true when currentFile.FileClass.Equals(Config.LogParsing_MinFileSizeExclusion, StringComparison.CurrentCultureIgnoreCase):     //MinFileSizeExclusion
                        case true when currentFile.FileClass.Equals(Config.LogParsing_MaxAgeOrAccessExclusion, StringComparison.CurrentCultureIgnoreCase):  //MaxAgeOrAccessExclusion
                        case true when currentFile.FileClass.Equals(Config.LogParsing_MinAgeOrAccessExclusion, StringComparison.CurrentCultureIgnoreCase):  //MinAgeOrAccessExclusion
                            PerformByteCalc(currentFile, WhereToAdd.Skipped);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Method meant only to be called from AddFile method while SpecialHandling is true - helps normalize code and avoid repetition
        /// </summary>
        private void SkippedOrCopied(ProcessedFileInfo currentFile, bool MarkSkipped)
        {
            if (MarkSkipped)
                PerformByteCalc(currentFile, WhereToAdd.Skipped);
            else
            {
                SetCopyOpStarted();
                //PerformByteCalc(currentFile, WhereToAdd.Copied);
            }
        }

        /// <summary>Catch start copy progress of large files</summary>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal void SetCopyOpStarted()
        {
            SkippingFile = false;
            CopyOpStarted = true;
        }

        /// <summary>Increment <see cref="FileStatsField"/>.Copied ( Triggered when copy progress = 100% ) </summary>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        internal void AddFileCopied(ProcessedFileInfo currentFile)
        {
            PerformByteCalc(currentFile, WhereToAdd.Copied);
        }

        /// <summary>
        /// Perform the calculation for the ByteStatistic
        /// </summary>
        private void PerformByteCalc(ProcessedFileInfo file, WhereToAdd where)
        {
            if (file == null) return;
            
            //Reset Flags
            SkippingFile = false;
            CopyOpStarted = false;
            FileFailed = false;
            CurrentFile = null;
            CurrentFile_SpecialHandling = false;

            //Perform Math
            lock (FileLock)
            {
                //Extra files do not contribute towards Copy Total.
                if (where == WhereToAdd.Extra)
                {
                    tmpFile.Extras++;
                    tmpByte.Extras += file.Size;
                }
                else
                {
                    tmpFile.Total++;
                    tmpByte.Total += file.Size;

                    switch (where)
                    {
                        case WhereToAdd.Copied:
                            tmpFile.Copied++;
                            tmpByte.Copied += file.Size;
                            break;
                        case WhereToAdd.Extra:
                            break;
                        case WhereToAdd.Failed:
                            tmpFile.Failed++;
                            tmpByte.Failed += file.Size;
                            break;
                        case WhereToAdd.MisMatch:
                            tmpFile.Mismatch++;
                            tmpByte.Mismatch += file.Size;
                            break;
                        case WhereToAdd.Skipped:
                            tmpFile.Skipped++;
                            tmpByte.Skipped += file.Size;
                            break;
                    }
                }
            }
            //Check if the UpdateTask should push an update to the public fields
            if (Monitor.TryEnter(UpdateLock))
            {
                if (NextUpdatePush <= DateTime.Now)
                    UpdateTaskTrigger?.TrySetResult(null);
                Monitor.Exit(UpdateLock);
            }
        }

        #endregion

        #region < PushUpdate to Public Stat Objects >

        /// <summary>
        /// Creates a LongRunning task that is meant to periodically push out Updates to the UI on a thread isolated from the event thread.
        /// </summary>
        /// <param name="CancelSource"></param>
        /// <returns></returns>
        private Task StartUpdateTask(out CancellationTokenSource CancelSource)
        {
            CancelSource = new CancellationTokenSource();
            var CS = CancelSource;
            return Task.Run(async () =>
            {
                while (!CS.IsCancellationRequested)
                {
                    lock(UpdateLock)
                    {
                        PushUpdate();
                        UpdateTaskTrigger = new TaskCompletionSource<object>();
                        NextUpdatePush = DateTime.Now.AddMilliseconds(UpdatePeriod);
                    }
                    await UpdateTaskTrigger.Task;
                }
                //Cleanup
                CS?.Dispose();
                UpdateTaskTrigger = null;
                UpdateTaskCancelSource = null;
            }, CS.Token);
        }

        /// <summary>
        /// Push the update to the public Stat Objects
        /// </summary>
        private void PushUpdate()
        {
            //Lock the Stat objects, clone, reset them, then push the update to the UI.
            Statistic TD = null;
            Statistic TB = null;
            Statistic TF = null;
            lock (DirLock)
            {
                if (tmpDir.NonZeroValue)
                {
                    TD = tmpDir.Clone();
                    tmpDir.Reset();
                }
            }
            lock (FileLock)
            {
                if (tmpFile.NonZeroValue)
                {
                    TF = tmpFile.Clone();
                    tmpFile.Reset();
                }
                if (tmpByte.NonZeroValue)
                {
                    TB = tmpByte.Clone();
                    tmpByte.Reset();
                }
            }
            //Push UI update after locks are released, to avoid holding up the other thread for too long
            if (TD != null) DirStatField.AddStatistic(TD);
            if (TB != null) ByteStatsField.AddStatistic(TB);
            if (TF != null) FileStatsField.AddStatistic(TF);
            //Raise the event if any of the values have been updated
            if (TF != null || TD != null || TB != null)
            {
                ValuesUpdated?.Invoke(this, new IProgressEstimatorUpdateEventArgs(this, TB, TF, TD));
            }
        }

        #endregion
    }
}
