using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoboSharp.Interfaces;

namespace RoboSharp.Results
{
    /// <summary>
    /// Object returned by RoboQueue when a run has completed.
    /// </summary>
    public sealed class RoboQueueResults : IRoboQueueResults, IRoboCopyResultsList, ITimeSpan
    {
        internal RoboQueueResults() 
        {
            collection = new RoboCopyResultsList();
            StartTime = DateTime.Now;
            QueueProcessRunning = true;
        }

        private RoboCopyResultsList collection { get; }
        private DateTime EndTimeField;
        private TimeSpan TimeSpanField;
        private bool QueueProcessRunning;

        /// <summary>
        /// Add a result to the collection
        /// </summary>
        internal void Add(RoboCopyResults result) => collection.Add(result);

        #region < IRoboQueueResults >

        /// <summary> Time the RoboQueue task was started </summary>
        public DateTime StartTime { get; }

        /// <summary> Time the RoboQueue task was completed / cancelled. </summary>
        /// <remarks> Should Only considered valid if <see cref="QueueComplete"/> = true.</remarks>
        public DateTime EndTime 
        { 
            get => EndTimeField;
            internal set 
            {
                EndTimeField = value;
                TimeSpanField = value.Subtract(StartTime);
                QueueProcessRunning = false;
            }
        }

        /// <summary> Length of Time RoboQueue was running </summary>
        /// <remarks> Should Only considered valid if <see cref="QueueComplete"/> = true.</remarks>
        public TimeSpan TimeSpan => TimeSpanField;

        /// <summary> TRUE if the RoboQueue object that created this results set has not finished running yet. </summary>
        public bool QueueRunning => QueueProcessRunning;

        /// <summary> TRUE if the RoboQueue object that created this results has completed running, or has been cancelled. </summary>
        public bool QueueComplete => !QueueProcessRunning;

        #endregion

        #region < IRoboCopyResultsList Implementation >

        /// <inheritdoc cref="IRoboCopyResultsList.DirectoriesStatistic"/>
        public IStatistic DirectoriesStatistic => ((IRoboCopyResultsList)collection).DirectoriesStatistic;

        /// <inheritdoc cref="IRoboCopyResultsList.BytesStatistic"/>
        public IStatistic BytesStatistic => ((IRoboCopyResultsList)collection).BytesStatistic;

        /// <inheritdoc cref="IRoboCopyResultsList.FilesStatistic"/>
        public IStatistic FilesStatistic => ((IRoboCopyResultsList)collection).FilesStatistic;

        /// <inheritdoc cref="IRoboCopyResultsList.SpeedStatistic"/>
        public ISpeedStatistic SpeedStatistic => ((IRoboCopyResultsList)collection).SpeedStatistic;

        /// <inheritdoc cref="IRoboCopyResultsList.Status"/>
        public IRoboCopyCombinedExitStatus Status => ((IRoboCopyResultsList)collection).Status;

        /// <inheritdoc cref="IRoboCopyResultsList.Collection"/>
        public IReadOnlyList<RoboCopyResults> Collection => ((IRoboCopyResultsList)collection).Collection;

        /// <inheritdoc cref="IRoboCopyResultsList.Count"/>
        public int Count => ((IRoboCopyResultsList)collection).Count;

        public RoboCopyResults this[int i] => ((IRoboCopyResultsList)collection)[i];

        /// <inheritdoc cref="RoboCopyResultsList.CollectionChanged"/>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                ((INotifyCollectionChanged)collection).CollectionChanged += value;
            }

            remove
            {
                ((INotifyCollectionChanged)collection).CollectionChanged -= value;
            }
        }

        /// <inheritdoc cref="IRoboCopyResultsList.GetByteStatistics"/>
        public IStatistic[] GetByteStatistics()
        {
            return ((IRoboCopyResultsList)collection).GetByteStatistics();
        }

        /// <inheritdoc cref="IRoboCopyResultsList.GetDirectoriesStatistics"/>
        public IStatistic[] GetDirectoriesStatistics()
        {
            return ((IRoboCopyResultsList)collection).GetDirectoriesStatistics();
        }

        /// <inheritdoc cref="RoboCopyResultsList.GetEnumerator"/>
        public IEnumerator<RoboCopyResults> GetEnumerator()
        {
            return ((IEnumerable<RoboCopyResults>)collection).GetEnumerator();
        }

        /// <inheritdoc cref="IRoboCopyResultsList.GetFilesStatistics"/>
        public IStatistic[] GetFilesStatistics()
        {
            return ((IRoboCopyResultsList)collection).GetFilesStatistics();
        }

        /// <inheritdoc cref="IRoboCopyResultsList.GetSpeedStatistics"/>
        public ISpeedStatistic[] GetSpeedStatistics()
        {
            return ((IRoboCopyResultsList)collection).GetSpeedStatistics();
        }

        /// <inheritdoc cref="IRoboCopyResultsList.GetStatuses"/>
        public RoboCopyExitStatus[] GetStatuses()
        {
            return ((IRoboCopyResultsList)collection).GetStatuses();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)collection).GetEnumerator();
        }

        /// <inheritdoc cref="IRoboCopyResultsList.GetErrors"/>
        public ErrorEventArgs[] GetErrors()
        {
            return ((IRoboCopyResultsList)collection).GetErrors();
        }
        #endregion
    }
}
