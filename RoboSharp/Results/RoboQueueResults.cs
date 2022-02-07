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
    public class RoboQueueResults : IRoboQueueResults, IRoboCopyResultsList, ITimeSpan
    {
        internal RoboQueueResults() 
        {
            collection = new RoboCopyResultsList();
        }

        private RoboCopyResultsList collection { get; }

        /// <summary>
        /// Add a result to the collection
        /// </summary>
        internal void Add(RoboCopyResults result) => collection.Add(result);

        #region < IRoboQueueResults >

        /// <summary> Time the RoboQueue task was started </summary>
        public DateTime StartTime { get; internal set; }

        /// <summary> Time the RoboQueue task was completed / cancelled. </summary>
        public DateTime EndTime { get; internal set; }

        /// <summary> Length of Time RoboQueue was running </summary>
        public TimeSpan TimeSpan { get; internal set; }

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
        #endregion
    }
}
