using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSharp.Results
{
    /// <summary>
    /// Object used to represent reults from multiple <see cref="RoboCommand"/>s. <br/>
    /// As <see cref="RoboCopyResults"/> are added to this object, it will update the Totals and Averages accordingly.
    /// </summary>
    /// <remarks>
    /// This object is derived from <see cref="List{T}"/>, where T = <see cref="RoboCopyResults"/>.
    /// </remarks>
    public sealed class RoboCopyResultsList : ListWithEvents<RoboCopyResults>
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
            Average_SpeedStatsField = new Lazy<SpeedStatistic>(
                () =>
                {
                    if (this.Count == 0)
                    {
                        this.SpeedStatValid = false;
                        return new SpeedStatistic();
                    }
                    else
                    {
                        this.SpeedStatValid = true;
                        return SpeedStatistic.AverageStatistics(this.GetSpeedStatistics());
                    }
                });
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
        private Lazy<SpeedStatistic> Average_SpeedStatsField;
        
        /// <summary> 
        /// Speed Stat can be averaged only if the first value was supplied by an actual result. <br/> 
        /// Set TRUE after first item was added to the list, set FALSE if list is cleared.
        /// </summary>
        private bool SpeedStatValid { get; set; }

        #endregion

        #region < Public Properties >

        /// <summary> Sum of all DirectoryStatistics objects </summary>
        public Statistic Total_DirectoriesStatistic => Total_DirStatsField.Value;

        /// <summary> Sum of all ByteStatistics objects </summary>
        public Statistic Total_BytesStatistic => Total_ByteStatsField.Value;

        /// <summary> Sum of all FileStatistics objects </summary>
        public Statistic Total_FilesStatistic => Total_FileStatsField.Value;

        /// <summary> Average of all SpeedStatistics objects </summary>
        public SpeedStatistic Average_SpeedStatistic => Average_SpeedStatsField.Value;

        #endregion

        #region < Get Array Methods >

        /// <summary>
        /// Get a snapshot of the ByteStatistics objects from this list.
        /// </summary>
        /// <returns>New array of the ByteStatistic objects</returns>
        public Statistic[] GetByteStatistics()
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
        public Statistic[] GetDirectoriesStatistics()
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
        public Statistic[] GetFilesStatistics()
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
        public SpeedStatistic[] GetSpeedStatistics()
        {
            List<SpeedStatistic> tmp = new List<SpeedStatistic> { };
            foreach (RoboCopyResults r in this)
                tmp.Add(r?.SpeedStatistic);
            return tmp.ToArray();
        }

        #endregion

        #region < Methods that handle List Modifications >

        /// <summary>Overrides the Event Listener check since this class always processes added/removed items.</summary>
        /// <returns>True</returns>
        protected override bool HasEventListener_ListModification() => true;

        /// <summary>Process the Added/Removed items, then fire the event</summary>
        /// <inheritdoc cref="ListWithEvents{T}.OnListModification(ListWithEvents{T}.ListModificationEventArgs)"/>
        protected override void OnListModification(ListModificationEventArgs e)
        {
            foreach (RoboCopyResults r in e.ItemsAdded)
                AddItem(r, e.ItemsAdded.Last() == r);
            foreach (RoboCopyResults r in e.ItemsRemoved)
                SubtractItem(r, e.ItemsRemoved.Last() == r);
            base.OnListModification(e);
        }

        /// <summary>
        /// Adds item to the private Statistics objects
        /// </summary>
        /// <param name="item"></param>
        /// <param name="RaiseValueChangeEvent"><inheritdoc cref="Statistic.EnablePropertyChangeEvent" path="*"/></param>
        private void AddItem(RoboCopyResults item, bool RaiseValueChangeEvent)
        {
            //Bytes
            if (Total_ByteStatsField.IsValueCreated)
                Total_ByteStatsField.Value.AddStatistic(item?.BytesStatistic, RaiseValueChangeEvent);
            //Directories
            if (Total_DirStatsField.IsValueCreated)
                Total_DirStatsField.Value.AddStatistic(item?.DirectoriesStatistic, RaiseValueChangeEvent);
            //Files
            if (Total_FileStatsField.IsValueCreated)
                Total_FileStatsField.Value.AddStatistic(item?.FilesStatistic, RaiseValueChangeEvent);
            //Speed
            if (Average_SpeedStatsField.IsValueCreated)
            {
                if (SpeedStatValid)
                    //Average the new value with the previous average
                    Average_SpeedStatsField.Value.AverageStatistic(item?.SpeedStatistic);
                else
                {
                    //The previous value was not valid since it was not based off a RoboCopy Result. 
                    //Set the value starting average to this item's value.
                    Average_SpeedStatsField.Value.SetValues(item?.SpeedStatistic);
                    SpeedStatValid = item?.SpeedStatistic != null;
                }
            }
        }

        /// <summary>
        /// Subtracts item from the private Statistics objects
        /// </summary>
        /// <param name="item"></param>
        /// <param name="ReCalculateSpeedNow">Triggers Recalculating the Average Speed Stats if needed.<para/><inheritdoc cref="Statistic.EnablePropertyChangeEvent" path="*"/></param>
        private void SubtractItem(RoboCopyResults item, bool ReCalculateSpeedNow)
        {
            //Bytes
            if (Total_ByteStatsField.IsValueCreated)
                Total_ByteStatsField.Value.Subtract(item?.BytesStatistic, ReCalculateSpeedNow);
            //Directories
            if (Total_DirStatsField.IsValueCreated)
                Total_DirStatsField.Value.Subtract(item?.DirectoriesStatistic, ReCalculateSpeedNow);
            //Files
            if (Total_FileStatsField.IsValueCreated)
                Total_FileStatsField.Value.Subtract(item?.FilesStatistic, ReCalculateSpeedNow);
            //Speed
            if (Average_SpeedStatsField.IsValueCreated && ReCalculateSpeedNow)
            {
                if (this.Count == 0)
                {
                    Average_SpeedStatsField.Value.Reset();
                    SpeedStatValid = false;
                }
                if (this.Count == 1)
                {
                    Average_SpeedStatsField.Value.SetValues(item?.SpeedStatistic);
                    SpeedStatValid = true;
                }
                else
                {
                    List<SpeedStatistic> tmpList = new List<SpeedStatistic>();
                    foreach (RoboCopyResults r in this)
                        tmpList.Add(r?.SpeedStatistic);
                    SpeedStatistic tmp = SpeedStatistic.AverageStatistics(tmpList);
                    Average_SpeedStatsField.Value.SetValues(tmp);
                    SpeedStatValid = true;
                }
            }
        }



        #endregion

    }
}

