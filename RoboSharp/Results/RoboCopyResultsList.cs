using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace RoboSharp.Results
{
    /// <summary>
    /// Object used to represent results from multiple <see cref="RoboCommand"/>s. <br/>
    /// As <see cref="RoboCopyResults"/> are added to this object, it will update the Totals and Averages accordingly.
    /// </summary>
    /// <remarks>
    /// This object is derived from <see cref="List{T}"/>, where T = <see cref="RoboCopyResults"/>, and implements <see cref="INotifyCollectionChanged"/>
    /// </remarks>
    public sealed class RoboCopyResultsList : ObservableList<RoboCopyResults>, IDisposable
    {
        #region < Constructors >

        /// <inheritdoc cref="List{T}.List()"/>
        public RoboCopyResultsList() : base() { Init(); }

        /// <param name="result">Populate the new List object with this result as the first item.</param>
        /// <inheritdoc cref="List{T}.List(IEnumerable{T})"/>
        public RoboCopyResultsList(RoboCopyResults result) :base(collection: new RoboCopyResults[] { result } ) { Init(); }

        /// <inheritdoc cref="List{T}.List(int)"/>
        public RoboCopyResultsList(int capacity): base(capacity: capacity) { Init(); }

        /// <inheritdoc cref="List{T}.List(IEnumerable{T})"/>
        public RoboCopyResultsList(List<RoboCopyResults> collection) : base(collection) { Init(); }

        /// <inheritdoc cref="List{T}.List(IEnumerable{T})"/>
        public RoboCopyResultsList(IEnumerable<RoboCopyResults> collection) :base(collection) { Init(); }

        private void Init()
        {
            Total_DirStatsField = new Lazy<Statistic>(() => Statistic.AddStatistics(this.GetDirectoriesStatistics()));
            Total_ByteStatsField = new Lazy<Statistic>(() => Statistic.AddStatistics(this.GetByteStatistics()));
            Total_FileStatsField = new Lazy<Statistic>(() => Statistic.AddStatistics(this.GetFilesStatistics()));
            Average_SpeedStatsField = new Lazy<AverageSpeedStatistic>( () => AverageSpeedStatistic.GetAverage(this.GetSpeedStatistics()));
            ExitStatusSummaryField = new Lazy<RoboCopyCombinedExitStatus>(() => RoboCopyCombinedExitStatus.CombineStatuses(this.GetStatuses()));
        }

        #endregion

        #region < Fields >

        //These objects are the underlying Objects that may be bound to by consumers.
        //The values are updated upon request of the associated property. 
        //This is so that properties are not returning new objects every request (which would break bindings)
        //If the statistic is never requested, then Lazy<> allows the list to skip performing the math against that statistic.

        private Lazy<Statistic> Total_DirStatsField;
        private Lazy<Statistic> Total_ByteStatsField;
        private Lazy<Statistic> Total_FileStatsField;
        private Lazy<AverageSpeedStatistic> Average_SpeedStatsField;
        private Lazy<RoboCopyCombinedExitStatus> ExitStatusSummaryField;
        private bool disposedValue;
        private bool startedDisposing;
        private bool Disposed => disposedValue || startedDisposing;

        #endregion

        #region < Public Properties >

        /// <summary> Sum of all DirectoryStatistics objects </summary>
        public Statistic DirectoriesStatistic => Total_DirStatsField?.Value;

        /// <summary> Sum of all ByteStatistics objects </summary>
        public Statistic BytesStatistic => Total_ByteStatsField?.Value;

        /// <summary> Sum of all FileStatistics objects </summary>
        public Statistic FilesStatistic => Total_FileStatsField?.Value;

        /// <summary> Average of all SpeedStatistics objects </summary>
        public SpeedStatistic SpeedStatistic => Average_SpeedStatsField?.Value;

        /// <summary> Sum of all RoboCopyExitStatus objects </summary>
        public RoboCopyExitStatus Status => ExitStatusSummaryField?.Value;

        #endregion

        #region < Get Array Methods >

        /// <summary>
        /// Get a snapshot of the ByteStatistics objects from this list.
        /// </summary>
        /// <returns>New array of the ByteStatistic objects</returns>
        public Statistic[] GetByteStatistics()
        {
            if (Disposed) return null;
            List<Statistic> tmp = new List<Statistic>{ };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.BytesStatistic);
            return tmp.ToArray();
        }

        /// <summary>
        /// Get a snapshot of the DirectoriesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the DirectoriesStatistic objects</returns>
        public Statistic[] GetDirectoriesStatistics()
        {
            if (Disposed) return null;
            List<Statistic> tmp = new List<Statistic> { };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.DirectoriesStatistic);
            return tmp.ToArray();
        }

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        public Statistic[] GetFilesStatistics()
        {
            if (Disposed) return null;
            List<Statistic> tmp = new List<Statistic> { };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.FilesStatistic);
            return tmp.ToArray();
        }

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        public RoboCopyExitStatus[] GetStatuses()
        {
            if (Disposed) return null;
            List<RoboCopyExitStatus> tmp = new List<RoboCopyExitStatus> { };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.Status);
            return tmp.ToArray();
        }

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        public SpeedStatistic[] GetSpeedStatistics()
        {
            if (Disposed) return null;
            List<SpeedStatistic> tmp = new List<SpeedStatistic> { };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.SpeedStatistic);
            return tmp.ToArray();
        }

        #endregion

        #region < Methods that handle List Modifications >

        /// <summary>Overrides the Event Listener check since this class always processes added/removed items.</summary>
        /// <returns>True</returns>
        protected override bool HasEventListener_CollectionChanged() => true;

        /// <summary>Process the Added/Removed items, then fire the event</summary>
        /// <inheritdoc cref="ObservableList{T}.OnCollectionChanged(NotifyCollectionChangedEventArgs)"/>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (Disposed) return;
            if (e.Action == NotifyCollectionChangedAction.Move) goto RaiseEvent; // Sorting causes no change in math -> Simply raise the event

            //Reset means a drastic change -> Recalculate everything, then goto RaiseEvent
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //Bytes
                if (Total_ByteStatsField.IsValueCreated)
                {
                    Total_ByteStatsField.Value.Reset(false);
                    Total_ByteStatsField.Value.AddStatistic(GetByteStatistics());
                }
                //Directories
                if (Total_DirStatsField.IsValueCreated)
                {
                    Total_DirStatsField.Value.Reset(false);
                    Total_DirStatsField.Value.AddStatistic(GetDirectoriesStatistics());
                }
                //Files
                if (Total_FileStatsField.IsValueCreated)
                {
                    Total_FileStatsField.Value.Reset(false);
                    Total_FileStatsField.Value.AddStatistic(GetFilesStatistics());
                }
                //Exit Status
                if (ExitStatusSummaryField.IsValueCreated)
                {
                    ExitStatusSummaryField.Value.Reset(false);
                    ExitStatusSummaryField.Value.CombineStatus(GetStatuses());
                }
                //Speed
                if (Average_SpeedStatsField.IsValueCreated)
                {
                    Average_SpeedStatsField.Value.Reset(false);
                    Average_SpeedStatsField.Value.Average(GetSpeedStatistics());
                }

                goto RaiseEvent;
            }
                
            //Process New Items
            if (e.NewItems != null)
            {
                int i = 0;
                int i2 = e.NewItems.Count;
                foreach (RoboCopyResults r in e?.NewItems)
                {
                    i++;
                    bool RaiseValueChangeEvent = (e.OldItems == null || e.OldItems.Count == 0 ) && ( i == i2 ); //Prevent raising the event if more calculation needs to be performed either from NewItems or from OldItems
                    //Bytes
                    if (Total_ByteStatsField.IsValueCreated)
                        Total_ByteStatsField.Value.AddStatistic(r?.BytesStatistic, RaiseValueChangeEvent);
                    //Directories
                    if (Total_DirStatsField.IsValueCreated)
                        Total_DirStatsField.Value.AddStatistic(r?.DirectoriesStatistic, RaiseValueChangeEvent);
                    //Files
                    if (Total_FileStatsField.IsValueCreated)
                        Total_FileStatsField.Value.AddStatistic(r?.FilesStatistic, RaiseValueChangeEvent);
                    //Exit Status
                    if (ExitStatusSummaryField.IsValueCreated)
                        ExitStatusSummaryField.Value.CombineStatus(r?.Status, RaiseValueChangeEvent);
                    //Speed
                    if (Average_SpeedStatsField.IsValueCreated)
                    {
                        Average_SpeedStatsField.Value.Add(r?.SpeedStatistic);
                        if (RaiseValueChangeEvent) Average_SpeedStatsField.Value.CalculateAverage();
                    }
                }
            }

            //Process Removed Items
            if (e.OldItems != null)
            {
                int i = 0;
                int i2 = e.OldItems.Count;
                foreach (RoboCopyResults r in e?.OldItems)
                {
                    i++;
                    bool RaiseValueChangeEvent = i == i2;
                    //Bytes
                    if (Total_ByteStatsField.IsValueCreated)
                        Total_ByteStatsField.Value.Subtract(r?.BytesStatistic, RaiseValueChangeEvent);
                    //Directories
                    if (Total_DirStatsField.IsValueCreated)
                        Total_DirStatsField.Value.Subtract(r?.DirectoriesStatistic, RaiseValueChangeEvent);
                    //Files
                    if (Total_FileStatsField.IsValueCreated)
                        Total_FileStatsField.Value.Subtract(r?.FilesStatistic, RaiseValueChangeEvent);
                    //Exit Status
                    if (ExitStatusSummaryField.IsValueCreated && RaiseValueChangeEvent)
                    {
                        ExitStatusSummaryField.Value.Reset(false);
                        ExitStatusSummaryField.Value.CombineStatus(GetStatuses());
                    }
                    //Speed
                    if (Average_SpeedStatsField.IsValueCreated)
                    {
                        if (this.Count == 0)
                            Average_SpeedStatsField.Value.Reset();
                        else
                            Average_SpeedStatsField.Value.Subtract(r.SpeedStatistic);
                        if (RaiseValueChangeEvent) Average_SpeedStatsField.Value.CalculateAverage();
                    }
                }
            }

            RaiseEvent:
            //Raise the CollectionChanged event
            base.OnCollectionChanged(e);
        }

        #endregion

        #region < IDisposable >

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                startedDisposing = true;

                if (disposing)
                {
                    this.Clear();
                    Total_ByteStatsField = null;
                    Total_DirStatsField = null;
                    Total_FileStatsField = null;
                    Average_SpeedStatsField = null;
                    ExitStatusSummaryField = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RoboCopyResultsList()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Clear the list and Set all the calculated statistics objects to null <br/>
        /// This object uses no 'Unmanaged' resources, so this is not strictly required to be called.
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

