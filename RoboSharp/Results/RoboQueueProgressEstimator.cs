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
            UpdateTaskStarted = new Lazy<bool>(() =>
            {
                tmpDirs = new Statistic(Statistic.StatType.Directories);
                tmpFiles = new Statistic(Statistic.StatType.Files);
                tmpBytes = new Statistic(Statistic.StatType.Bytes);

                tmpDirs.EnablePropertyChangeEvent = false;
                tmpFiles.EnablePropertyChangeEvent = false;
                tmpBytes.EnablePropertyChangeEvent = false;

                StartUpdateTask(out UpdateTaskCancelSource);
                return true;
            });
        }

        #endregion

        #region < Private Members >

        //ThreadSafe Bags/Queues
        private readonly ConcurrentBag<IStatistic> SubscribedStats = new ConcurrentBag<IStatistic>();

        //Stats
        private readonly Statistic DirStatField = new Statistic(Statistic.StatType.Directories, "Directory Stats Estimate");
        private readonly Statistic FileStatsField = new Statistic(Statistic.StatType.Files, "File Stats Estimate");
        private readonly Statistic ByteStatsField = new Statistic(Statistic.StatType.Bytes, "Byte Stats Estimate");

        //Lazy Bools
        private readonly Lazy<bool> UpdateTaskStarted;

        //Add Tasks
        private int UpdatePeriodInMilliSecond = 250;
        private Statistic tmpDirs;
        private Statistic tmpFiles;
        private Statistic tmpBytes;
        private CancellationTokenSource UpdateTaskCancelSource;
        private bool disposedValue;

        #endregion

        #region < Public Properties > 

        /// <summary>
        /// Estimate of current number of directories processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic DirectoriesStatistic => UpdateTaskStarted.Value ? DirStatField : DirStatField;

        /// <summary>
        /// Estimate of current number of files processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic FilesStatistic => UpdateTaskStarted.Value ? FileStatsField : FileStatsField;

        /// <summary>
        /// Estimate of current number of bytes processed while the job is still running. <br/>
        /// Estimate is provided by parsing of the LogLines produces by RoboCopy.
        /// </summary>
        public IStatistic BytesStatistic => UpdateTaskStarted.Value ? ByteStatsField : ByteStatsField;

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

        /// <summary>
        /// Creates a LongRunning task that is meant to periodically push out Updates to the UI on a thread isolated from the event thread.
        /// </summary>
        /// <param name="CancelSource"></param>
        /// <returns></returns>
        private Task StartUpdateTask(out CancellationTokenSource CancelSource)
        {
            CancelSource = new CancellationTokenSource();
            var CST = CancelSource.Token;
            return Task.Run(async () =>
            {
                while (!CST.IsCancellationRequested)
                {
                    PushUpdate();
                    //Setup a new TaskCompletionSource that can be cancelled or times out
                    var TCS = new TaskCompletionSource<object>();
                    var CS = new CancellationTokenSource(UpdatePeriodInMilliSecond);
                    var RC = CancellationTokenSource.CreateLinkedTokenSource(CS.Token, CST);
                    RC.Token.Register(() => TCS.TrySetResult(null));
                    //Wait for TCS to run - Blocking State since task is LongRunning
                    await TCS.Task;
                    RC.Dispose();
                    CS.Dispose();
                }
                PushUpdate();
            }, CST);
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
            bool DirAdded;
            bool FileAdded;
            lock (tmpDirs)
            {
                DirAdded = tmpDirs.NonZeroValue;
                if (DirAdded)
                {
                    TD = tmpDirs.Clone();
                    tmpDirs.Reset();
                }

            }
            lock (tmpFiles)
            {
                lock (tmpBytes)
                {
                    FileAdded = tmpFiles.NonZeroValue || tmpBytes.NonZeroValue;
                    if (FileAdded)
                    {
                        TF = tmpFiles.Clone();
                        TB = tmpBytes.Clone();
                        tmpFiles.Reset();
                        tmpBytes.Reset();
                    }
                }
            }
            //Push UI update after locks are released, to avoid holding up the other thread for too long
            if (DirAdded) DirStatField.AddStatistic(TD);
            if (FileAdded)
            {
                FileStatsField.AddStatistic(TF);
                ByteStatsField.AddStatistic(TB);
            }
        }

        #endregion

        #region < Event Binding for Auto-Updates ( Internal ) >

        private void BindDirStat(object o, PropertyChangedEventArgs e)
        {
            lock (tmpDirs)
            {
                tmpDirs.AddStatistic(e);
            }
        }
        private void BindFileStat(object o, PropertyChangedEventArgs e)
        {
            lock (tmpFiles)
            {
                tmpFiles.AddStatistic(e);
            }
        }
        private void BindByteStat(object o, PropertyChangedEventArgs e)
        {
            lock (tmpBytes)
            {
                tmpBytes.AddStatistic(e);
            }
        }

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
            lock (SubscribedStats)
            {
                if (SubscribedStats.Contains(StatObject)) return;
                SubscribedStats.Add(StatObject);
            }
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
                lock (SubscribedStats)
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
            UpdateTaskCancelSource?.Cancel();
            UpdateTaskCancelSource?.Dispose();
            UpdateTaskCancelSource = null;
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
