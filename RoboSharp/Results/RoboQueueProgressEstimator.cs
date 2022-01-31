using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using RoboSharp.Interfaces;
using RoboSharp.EventArgObjects;
using RoboSharp.Results;
using WhereToAdd = RoboSharp.Results.ProgressEstimator.WhereToAdd;

namespace RoboSharp.Results
{
    /// <summary>
    /// Updates the Statistics every 250ms
    /// </summary>
    /// <remarks>
    /// <see href="https://github.com/tjscience/RoboSharp/wiki/RoboQueueProgressEstimator"/>
    /// </remarks>
    internal class RoboQueueProgressEstimator : IProgressEstimator, IResults, IDisposable
    {
        #region < Constructors >
        internal RoboQueueProgressEstimator()
        {
            ByteCalcRequested = new Lazy<bool>(() => { StartAddingBytes(); return true; });
            DirCalcRequested = new Lazy<bool>(() => { StartAddingDirs(); return true; });
            FileCalcRequested = new Lazy<bool>(() => { StartAddingFiles(); return true; });
        }

        #endregion

        #region < Private Members >

        //ThreadSafe Bags/Queues
        private readonly ConcurrentBag<IStatistic> SubscribedStats = new ConcurrentBag<IStatistic>();
        private readonly ConcurrentBag<PropertyChangedEventArgs> FileBag = new ConcurrentBag<PropertyChangedEventArgs>();
        private readonly ConcurrentBag<PropertyChangedEventArgs> DirBag = new ConcurrentBag<PropertyChangedEventArgs>();
        private readonly ConcurrentBag<PropertyChangedEventArgs> ByteBag = new ConcurrentBag<PropertyChangedEventArgs>();

        //Stats
        private readonly Statistic DirStatField = new Statistic(Statistic.StatType.Directories, "Directory Stats Estimate");
        private readonly Statistic FileStatsField = new Statistic(Statistic.StatType.Files, "File Stats Estimate");
        private readonly Statistic ByteStatsField = new Statistic(Statistic.StatType.Bytes, "Byte Stats Estimate");

        //Lazy Bools
        private readonly Lazy<bool> ByteCalcRequested;
        private readonly Lazy<bool> DirCalcRequested;
        private readonly Lazy<bool> FileCalcRequested;

        //Add Tasks
        private int UpdatePeriodInMilliSecond = 250;
        private Task AddDirs;
        private Task AddFiles;
        private Task AddBytes;

        private CancellationTokenSource AddFilesCancelSource;
        private CancellationTokenSource AddDirsCancelSource;
        private CancellationTokenSource AddBytesCancelSource;
        private bool disposedValue;

        #endregion

        #region < Public Properties > 

        /// <summary>
        /// Estimate of current number of directories processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic DirectoriesStatistic => DirCalcRequested.Value ? DirStatField : DirStatField;

        /// <summary>
        /// Estimate of current number of files processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic FilesStatistic => FileCalcRequested.Value ? FileStatsField : FileStatsField;

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
            if (DirStatField.Extras > 0 || FileStatsField.Extras > 0)
                code |= Results.RoboCopyExitCodes.ExtraFilesOrDirectoriesDetected;

            //MisMatch
            if (DirStatField.Mismatch > 0 || FileStatsField.Mismatch > 0)
                code |= Results.RoboCopyExitCodes.MismatchedDirectoriesDetected;

            //Failed
            if (DirStatField.Failed > 0 || FileStatsField.Failed > 0)
                code |= Results.RoboCopyExitCodes.SomeFilesOrDirectoriesCouldNotBeCopied;

            return code;

        }

        #endregion

        #region < Counting Methods ( private ) >

        private void StartAddingFiles() => AddStatTask(ref AddFiles, FileStatsField, FileBag, ref AddFilesCancelSource);
        private void StartAddingDirs() => AddStatTask(ref AddDirs, DirStatField, DirBag, ref AddDirsCancelSource);
        private void StartAddingBytes() =>  AddStatTask(ref AddBytes, ByteStatsField, ByteBag, ref AddBytesCancelSource);


        private Task AddStatTask(ref Task TaskRef, Statistic StatToAddTo, ConcurrentBag<PropertyChangedEventArgs> EventBag, ref CancellationTokenSource CancelSource)
        {
            if (TaskRef != null && TaskRef.Status <= TaskStatus.Running) return TaskRef; //Don't run if already running

            CancelSource = new CancellationTokenSource();
            var CS = CancelSource; //Compiler required abstracting since this is a CancelSource is marked as ref

            TaskRef = Task.Factory.StartNew( async () =>
            {
                Statistic tmp = new Statistic(type: StatToAddTo.Type);
                tmp.EnablePropertyChangeEvent = false;
                while (!CS.IsCancellationRequested)
                {
                    BagClearOut(tmp, StatToAddTo, EventBag);
                    await Task.Delay(UpdatePeriodInMilliSecond);
                }
                await Task.Delay(250); //Sleep for a bit to let the bag fill up
                //After cancellation is requested, ensure the bag is emptied
                BagClearOut(tmp, StatToAddTo, EventBag);

            }, CS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current).Unwrap();
            
            return TaskRef;
        }

        private void BagClearOut(Statistic Tmp, Statistic StatToAddTo, ConcurrentBag<PropertyChangedEventArgs> EventBag)
        {
            DateTime incrementTimer = DateTime.Now;
            bool itemAdded = false;
            while (!EventBag.IsEmpty)
            {
                if (EventBag.TryTake(out var e))
                {
                    Tmp.AddStatistic(e);
                    itemAdded = true;
                }
                //Update every X ms
                TimeSpan diff = DateTime.Now.Subtract(incrementTimer);
                if (diff.TotalMilliseconds > UpdatePeriodInMilliSecond)
                {
                    StatToAddTo.AddStatistic(Tmp);
                    incrementTimer = DateTime.Now;
                    Tmp.Reset();
                    itemAdded = false;
                }
            }
            if (itemAdded)
            {
                StatToAddTo.AddStatistic(Tmp);
                Tmp.Reset();
            }
        }

        #endregion

        #region < Event Binding for Auto-Updates ( Internal ) >

        private void BindDirStat(object o, PropertyChangedEventArgs e) => DirBag.Add(e);
        private void BindFileStat(object o, PropertyChangedEventArgs e) => FileBag.Add(e);
        private void BindByteStat(object o, PropertyChangedEventArgs e) => ByteBag.Add(e);

        /// <summary>
        /// Subscribe to the update events of a <see cref="ProgressEstimator"/> object
        /// </summary>
        internal void BindToProgressEstimator(IProgressEstimator estimator)
        {
            BindToStatistic(estimator.BytesStatistic);
            BindToStatistic(estimator.DirectoriesStatistic);
            BindToStatistic(estimator.FilesStatistic);
        }

        /// <summary>
        /// Subscribe to the update events of a <see cref="Statistic"/> object
        /// </summary>
        internal void BindToStatistic(IStatistic StatObject)
        {
            SubscribedStats.Add(StatObject);
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
            }
        }

        #endregion

        #region < CancelTasks & DisposePattern >

        /// <summary>
        /// Unbind and cancel the Add Tasks
        /// </summary>
        internal void CancelTasks()
        {
            //Cancel the tasks
            UnBind();
            AddFilesCancelSource?.Cancel();
            AddDirsCancelSource?.Cancel();
            AddBytesCancelSource?.Cancel();

            AddFilesCancelSource?.Dispose();
            AddDirsCancelSource?.Dispose();
            AddBytesCancelSource?.Dispose();

            AddFilesCancelSource = null;
            AddDirsCancelSource = null;
            AddBytesCancelSource = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                //Cancel the tasks
                CancelTasks();
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~RoboQueueProgressEstimator()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
