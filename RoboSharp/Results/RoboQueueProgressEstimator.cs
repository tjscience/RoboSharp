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
        private readonly ConcurrentDictionary<IProgressEstimator, object> SubscribedStats = new ConcurrentDictionary<IProgressEstimator, object>();

        //Stats
        private readonly Statistic DirStatField = new Statistic(Statistic.StatType.Directories, "Directory Stats Estimate");
        private readonly Statistic FileStatsField = new Statistic(Statistic.StatType.Files, "File Stats Estimate");
        private readonly Statistic ByteStatsField = new Statistic(Statistic.StatType.Bytes, "Byte Stats Estimate");

        //Add Tasks
        private int UpdatePeriodInMilliSecond = 250;
        private readonly Statistic tmpDirs;
        private readonly Statistic tmpFiles;
        private readonly Statistic tmpBytes;
        private DateTime NextUpdate = DateTime.Now;
        private object StatLock = new object();
        private object UpdateLock = new object();
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

        /// <inheritdoc cref="IProgressEstimator.ValuesUpdated"/>
        public event ProgressEstimator.UIUpdateEventHandler ValuesUpdated;

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
            if (FilesStatistic.Copied > 0)
                code |= Results.RoboCopyExitCodes.FilesCopiedSuccessful;

            //Extra
            if (DirectoriesStatistic.Extras > 0 || FilesStatistic.Extras > 0)
                code |= Results.RoboCopyExitCodes.ExtraFilesOrDirectoriesDetected;

            //MisMatch
            if (DirectoriesStatistic.Mismatch > 0 || FilesStatistic.Mismatch > 0)
                code |= Results.RoboCopyExitCodes.MismatchedDirectoriesDetected;

            //Failed
            if (DirectoriesStatistic.Failed > 0 || FilesStatistic.Failed > 0)
                code |= Results.RoboCopyExitCodes.SomeFilesOrDirectoriesCouldNotBeCopied;

            return code;

        }

        #endregion

        #region < Counting Methods ( private ) >

        /// <summary>
        /// Subscribe to the update events of a <see cref="ProgressEstimator"/> object
        /// </summary>
        internal void BindToProgressEstimator(IProgressEstimator estimator)
        {
            if (!SubscribedStats.ContainsKey(estimator))
            {
                SubscribedStats.TryAdd(estimator, null);
                estimator.ValuesUpdated += Estimator_ValuesUpdated;
            }
        }

        private void Estimator_ValuesUpdated(IProgressEstimator sender, IProgressEstimatorUpdateEventArgs e)
        {
            Statistic bytes = null;
            Statistic files = null;
            Statistic dirs = null;
            
            lock (StatLock)
            {
                //Update the Temp Stats
                tmpBytes.AddStatistic(e.ValueChange_Bytes);
                tmpFiles.AddStatistic(e.ValueChange_Files);
                tmpDirs.AddStatistic(e.ValueChange_Directories);

                //Check if Update Required
                //Check if the UpdateTask should push an update to the public fields
                if (Monitor.TryEnter(UpdateLock))
                {
                    if (DateTime.Now >= NextUpdate)
                    {
                        bytes = tmpBytes.Clone();
                        tmpBytes.Reset();

                        files = tmpFiles.Clone();
                        tmpFiles.Reset();

                        dirs = tmpDirs.Clone();
                        tmpDirs.Reset();
                    }

                    // Perform the Add Events
                    ByteStatsField.AddStatistic(bytes);
                    FileStatsField.AddStatistic(files);
                    DirStatField.AddStatistic(dirs);

                    ValuesUpdated?.Invoke(this, new IProgressEstimatorUpdateEventArgs(this, bytes, files, dirs));
                    NextUpdate = DateTime.Now.AddMilliseconds(UpdatePeriodInMilliSecond);
                    Monitor.Exit(UpdateLock);
                }
            }//End StatLock
        }

        /// <summary>
        /// Unsubscribe from all bound Statistic objects
        /// </summary>
        internal void UnBind()
        {
            if (SubscribedStats != null)
            {
                foreach (var est in SubscribedStats.Keys)
                {
                    est.ValuesUpdated -= Estimator_ValuesUpdated;
                }
            }
        }

        #endregion

        #region < CancelTasks & DisposePattern >

        /// <summary>
        /// Unbind all the ProgressEstimators
        /// </summary>
        internal void CancelTasks() => CancelTasks(true);

        private void CancelTasks(bool RunUpdateTask)
        {
            //Preventn any additional events coming through
            UnBind();
            //Push the last update out after a short delay to allow any pending events through
            if (RunUpdateTask)
            {
                Task.Run( async () => { 
                    lock (UpdateLock)
                        NextUpdate = DateTime.Now.AddMilliseconds(124);
                    await Task.Delay(125);
                    Estimator_ValuesUpdated(null, IProgressEstimatorUpdateEventArgs.DummyArgs);
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
