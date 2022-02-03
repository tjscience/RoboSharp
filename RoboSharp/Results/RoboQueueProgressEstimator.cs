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
using System.Runtime.CompilerServices;

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
            tmpDirs = new Statistic(Statistic.StatType.Directories);
            tmpFiles = new Statistic(Statistic.StatType.Files);
            tmpBytes = new Statistic(Statistic.StatType.Bytes);

            tmpDirs.EnablePropertyChangeEvent = false;
            tmpFiles.EnablePropertyChangeEvent = false;
            tmpBytes.EnablePropertyChangeEvent = false;
        }

        #endregion

        #region < Private Members >

        //ThreadSafe Bags/Queues
        private readonly ConcurrentBag<IStatistic> SubscribedStats = new ConcurrentBag<IStatistic>();

        //Stats
        private readonly Statistic DirStatField = new Statistic(Statistic.StatType.Directories, "Directory Stats Estimate");
        private readonly Statistic FileStatsField = new Statistic(Statistic.StatType.Files, "File Stats Estimate");
        private readonly Statistic ByteStatsField = new Statistic(Statistic.StatType.Bytes, "Byte Stats Estimate");

        //Add Tasks
        private int UpdatePeriodInMilliSecond = 250;
        private readonly Statistic tmpDirs;
        private readonly Statistic tmpFiles;
        private readonly Statistic tmpBytes;
        private DateTime NextDirUpdate = DateTime.Now;
        private DateTime NextFileUpdate = DateTime.Now;
        private DateTime NextByteUpdate = DateTime.Now;
        private bool disposedValue;

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

        private void BindDirStat(object o, PropertyChangedEventArgs e)
        {
            Statistic tmp = null;
            lock (tmpDirs)
            {
                tmpDirs.AddStatistic(e);
                if (tmpDirs.NonZeroValue && DateTime.Now >= NextDirUpdate)
                {
                    tmp = tmpDirs.Clone();
                    tmpDirs.Reset();
                    NextDirUpdate = DateTime.Now.AddMilliseconds(UpdatePeriodInMilliSecond);
                }
            }
            if (tmp != null)
                lock (DirStatField)
                    DirStatField.AddStatistic(tmp);
        }

        private void BindFileStat(object o, PropertyChangedEventArgs e)
        {
            Statistic tmp = null;
            lock (tmpFiles)
            {
                tmpFiles.AddStatistic(e);
                if (tmpFiles.NonZeroValue && DateTime.Now >= NextFileUpdate)
                {
                    tmp = tmpFiles.Clone();
                    tmpFiles.Reset();
                    NextFileUpdate = DateTime.Now.AddMilliseconds(UpdatePeriodInMilliSecond);
                }
            }
            if (tmp != null) 
                lock (FileStatsField)
                    FileStatsField.AddStatistic(tmp);
        }

        private void BindByteStat(object o, PropertyChangedEventArgs e)
        {
            Statistic tmp = null;
            lock (tmpBytes)
            {
                tmpBytes.AddStatistic(e);
                if (tmpBytes.NonZeroValue && DateTime.Now >= NextByteUpdate)
                {
                    tmp = tmpBytes.Clone();
                    tmpBytes.Reset();
                    NextByteUpdate = DateTime.Now.AddMilliseconds(UpdatePeriodInMilliSecond);
                }
            }
            if (tmp != null) 
                lock (ByteStatsField)
                    ByteStatsField.AddStatistic(tmp);
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
        /// Unbind all the ProgressEstimators
        /// </summary>
        internal void CancelTasks() => Cancel(true);

        private void CancelTasks(bool RunUpdateTask)
        {
            //Preventn any additional events coming through
            UnBind();
            //Push the last update out after a short delay to allow any pending events through
            if (RunUpdateTask)
            {
                Task.Run( async () => { 
                    lock (tmpDirs) 
                        lock (tmpFiles)
                            lock (tmpBytes)
                            {
                                NextDirUpdate = DateTime.Now.AddMilliseconds(124);
                                NextFileUpdate = NextDirUpdate;
                                NextByteUpdate = NextDirUpdate;
                            }
                    await Task.Delay(125);
                    BindDirStat(null, null);
                    BindFileStat(null, null);
                    BindByteStat(null, null);
                });
            }
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
                CancelTasks(false);
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
