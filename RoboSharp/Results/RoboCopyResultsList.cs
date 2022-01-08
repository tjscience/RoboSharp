using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using StatType = RoboSharp.Results.Statistic.StatType;

namespace RoboSharp.Results
{
    /// <summary>
    /// Interface to provide Read-Only access to a <see cref="RoboCopyResultsList"/>
    /// </summary>
    public interface IRoboCopyResultsList : IEnumerable<RoboCopyResults>, ICloneable
    {
        #region < Properties >

        /// <summary> Sum of all DirectoryStatistics objects </summary>
        IStatistic DirectoriesStatistic { get; }

        /// <summary> Sum of all ByteStatistics objects </summary>
        IStatistic BytesStatistic { get; }

        /// <summary> Sum of all FileStatistics objects </summary>
        IStatistic FilesStatistic { get; }

        /// <summary> Average of all SpeedStatistics objects </summary>
        ISpeedStatistic SpeedStatistic { get; }

        /// <summary> Sum of all RoboCopyExitStatus objects </summary>
        IRoboCopyCombinedExitStatus Status { get; }

        /// <inheritdoc cref="List{T}.Count"/>
        int Count { get; }
        
        #endregion

        #region < Methods >

        /// <summary>
        /// Get a snapshot of the ByteStatistics objects from this list.
        /// </summary>
        /// <returns>New array of the ByteStatistic objects</returns>
        IStatistic[] GetByteStatistics();

        /// <summary>
        /// Get a snapshot of the DirectoriesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the DirectoriesStatistic objects</returns>
        IStatistic[] GetDirectoriesStatistics();

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        IStatistic[] GetFilesStatistics();

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        RoboCopyExitStatus[] GetStatuses();

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        ISpeedStatistic[] GetSpeedStatistics();

        /// <summary>
        /// Copy the values within the list to a new object
        /// </summary>
        /// <returns>new <see cref="RoboCopyResultsList"/> object</returns>
        new RoboCopyResultsList Clone();

        #endregion
    }

    /// <summary>
    /// Object used to represent results from multiple <see cref="RoboCommand"/>s. <br/>
    /// As <see cref="RoboCopyResults"/> are added to this object, it will update the Totals and Averages accordingly.
    /// </summary>
    /// <remarks>
    /// This object is derived from <see cref="List{T}"/>, where T = <see cref="RoboCopyResults"/>, and implements <see cref="INotifyCollectionChanged"/>
    /// </remarks>
    public sealed class RoboCopyResultsList : ObservableList<RoboCopyResults>, IRoboCopyResultsList, INotifyCollectionChanged
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

        /// <summary>
        /// Clone a RoboCopyResultsList into a new object
        /// </summary>
        public RoboCopyResultsList(RoboCopyResultsList resultsList) : base(resultsList)
        {            
            Total_DirStatsField = GetLazyStat(resultsList.Total_DirStatsField, GetLazyFunc(GetDirectoriesStatistics, StatType.Directories));
            Total_FileStatsField = GetLazyStat(resultsList.Total_FileStatsField, GetLazyFunc(GetFilesStatistics, StatType.Files));
            Total_ByteStatsField = GetLazyStat(resultsList.Total_ByteStatsField, GetLazyFunc(GetByteStatistics, StatType.Bytes));
            Average_SpeedStatsField = GetLazyStat(resultsList.Average_SpeedStatsField, GetLazyAverageSpeedFunc());
            ExitStatusSummaryField = GetLazyStat(resultsList.ExitStatusSummaryField, GetLazCombinedStatusFunc());
        }

        private Func<Statistic> GetLazyFunc(Func<IStatistic[]> Action, StatType StatType) => new Func<Statistic>(() => Statistic.AddStatistics(Action.Invoke(), StatType));
        private Func<AverageSpeedStatistic> GetLazyAverageSpeedFunc() => new Func<AverageSpeedStatistic>(() => AverageSpeedStatistic.GetAverage(GetSpeedStatistics()));
        private Func<RoboCopyCombinedExitStatus> GetLazCombinedStatusFunc() => new Func<RoboCopyCombinedExitStatus>(() => RoboCopyCombinedExitStatus.CombineStatuses(GetStatuses()));

        private Lazy<T> GetLazyStat<T>(Lazy<T> lazyStat, Func<T> action) where T : ICloneable
        {
            if (lazyStat.IsValueCreated)
            {
                var clone = lazyStat.Value.Clone();
                return new Lazy<T>(() => (T)clone);
            }
            else
            {
                return new Lazy<T>(action);
            }
        }

        private void Init()
        {
            Total_DirStatsField = new Lazy<Statistic>(GetLazyFunc(GetDirectoriesStatistics, StatType.Directories));
            Total_FileStatsField = new Lazy<Statistic>(GetLazyFunc(GetFilesStatistics, StatType.Files));
            Total_ByteStatsField = new Lazy<Statistic>(GetLazyFunc(GetByteStatistics, StatType.Bytes));
            Average_SpeedStatsField = new Lazy<AverageSpeedStatistic>(GetLazyAverageSpeedFunc());
            ExitStatusSummaryField = new Lazy<RoboCopyCombinedExitStatus>(GetLazCombinedStatusFunc());
        }

        /// <inheritdoc cref="IRoboCopyResultsList.Clone"/>
        public RoboCopyResultsList Clone() => new RoboCopyResultsList(this);

        object ICloneable.Clone() => Clone();

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

        #endregion

        #region < Events >

        /// <summary>
        /// Delegate for objects to send notification that the list behind an <see cref="IRoboCopyResultsList"/> interface has been updated
        /// </summary>
        public delegate void ResultsListUpdated(object sender, ResultListUpdatedEventArgs e);

        #endregion

        #region < Public Properties >

        /// <summary> Sum of all DirectoryStatistics objects </summary>
        public IStatistic DirectoriesStatistic => Total_DirStatsField?.Value;

        /// <summary> Sum of all ByteStatistics objects </summary>
        public IStatistic BytesStatistic => Total_ByteStatsField?.Value;

        /// <summary> Sum of all FileStatistics objects </summary>
        public IStatistic FilesStatistic => Total_FileStatsField?.Value;

        /// <summary> Average of all SpeedStatistics objects </summary>
        public ISpeedStatistic SpeedStatistic => Average_SpeedStatsField?.Value;

        /// <summary> Sum of all RoboCopyExitStatus objects </summary>
        public IRoboCopyCombinedExitStatus Status => ExitStatusSummaryField?.Value;

        #endregion

        #region < Get Array Methods ( Public ) >

        /// <summary>
        /// Get a snapshot of the ByteStatistics objects from this list.
        /// </summary>
        /// <returns>New array of the ByteStatistic objects</returns>
        public IStatistic[] GetByteStatistics()
        {
            List<Statistic> tmp = new List<Statistic>{ };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.BytesStatistic);
            return tmp.ToArray();
        }

        /// <summary>
        /// Get a snapshot of the DirectoriesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the DirectoriesStatistic objects</returns>
        public IStatistic[] GetDirectoriesStatistics()
        {
            List<Statistic> tmp = new List<Statistic> { };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.DirectoriesStatistic);
            return tmp.ToArray();
        }

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        public IStatistic[] GetFilesStatistics()
        {
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
            List<RoboCopyExitStatus> tmp = new List<RoboCopyExitStatus> { };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.Status);
            return tmp.ToArray();
        }

        /// <summary>
        /// Get a snapshot of the FilesStatistic objects from this list.
        /// </summary>
        /// <returns>New array of the FilesStatistic objects</returns>
        public ISpeedStatistic[] GetSpeedStatistics()
        {
            List<SpeedStatistic> tmp = new List<SpeedStatistic> { };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.SpeedStatistic);
            return tmp.ToArray();
        }

        #endregion

        #region < Methods that handle List Modifications >

        /// <summary>Process the Added/Removed items, then fire the event</summary>
        /// <inheritdoc cref="ObservableList{T}.OnCollectionChanged(NotifyCollectionChangedEventArgs)"/>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
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

    }

    /// <summary> EventArgs for the <see cref="RoboCopyResultsList.ResultsListUpdated"/> delegate </summary>
    public class ResultListUpdatedEventArgs : EventArgs
    {
        private ResultListUpdatedEventArgs() { }

        /// <summary> Create the EventArgs for the <see cref="RoboCopyResultsList.ResultsListUpdated"/> delegate </summary>
        /// <param name="list">Results list to present as an interface</param>
        public ResultListUpdatedEventArgs(IRoboCopyResultsList list)
        {
            ResultsList = list;
        }

        /// <summary>
        /// Read-Only interface to the List that has been updated.
        /// </summary>
        public IRoboCopyResultsList ResultsList { get; }
    }

}

