using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RoboSharp.Results
{
    /// <summary>
    /// Contains information regarding average Transfer Speed. <br/>
    /// Note: Runs that do not perform any copy operations or that exited prematurely ( <see cref="RoboCopyExitCodes.Cancelled"/> ) will result in a null <see cref="SpeedStatistic"/> object.
    /// </summary>
    public class SpeedStatistic : INotifyPropertyChanged
    {
        #region < Fields, Events, Properties >

        private decimal BytesPerSecField = 0;
        private decimal MegaBytesPerMinField = 0;

        /// <summary> This toggle Enables/Disables firing the <see cref="PropertyChanged"/> Event to avoid firing it when doing multiple consecutive changes to the values </summary>
        protected bool EnablePropertyChangeEvent { get; set; } = true;

        /// <summary>This event will fire when the value of the SpeedStatistic is updated </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Average Transfer Rate in Bytes/Second </summary>
        public virtual decimal BytesPerSec
        {
            get => BytesPerSecField;
            protected set
            {
                if (BytesPerSecField != value)
                {
                    BytesPerSecField = value;
                    if (EnablePropertyChangeEvent) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MegaBytesPerMin"));
                }
            }
        }

        /// <summary> Average Transfer Rate in MB/Minute</summary>
        public virtual decimal MegaBytesPerMin
        {
            get => MegaBytesPerMinField;
            protected set
            {
                if (MegaBytesPerMinField != value)
                {
                    MegaBytesPerMinField = value;
                    if (EnablePropertyChangeEvent) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MegaBytesPerMin"));
                }
            }
        }

        #endregion

        #region < Methods >

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"Speed: {BytesPerSec} Bytes/sec{Environment.NewLine}Speed: {MegaBytesPerMin} MegaBytes/min";
        }

        internal static SpeedStatistic Parse(string line1, string line2)
        {
            var res = new SpeedStatistic();

            var pattern = new Regex(@"\d+([\.,]\d+)?");
            Match match;

            match = pattern.Match(line1);
            if (match.Success)
            {
                res.BytesPerSec = Convert.ToDecimal(match.Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            }

            match = pattern.Match(line2);
            if (match.Success)
            {
                res.MegaBytesPerMin = Convert.ToDecimal(match.Value.Replace(',', '.'), CultureInfo.InvariantCulture);
            }

            return res;
        }

        #endregion

    }

    /// <summary>
    /// This object represents the Average of several <see cref="SpeedStatistic"/> objects, and contains 
    /// methods to facilitate that functionality.
    /// </summary>
    public sealed class AverageSpeedStatistic : SpeedStatistic
    {
        #region < Constructors >

        /// <summary>
        /// Initialize a new <see cref="AverageSpeedStatistic"/> object with the default values.
        /// </summary>
        public AverageSpeedStatistic() : base() { }

        /// <summary>
        /// Initialize a new <see cref="AverageSpeedStatistic"/> object. <br/>
        /// Values will be set to the return values of <see cref="SpeedStatistic.BytesPerSec"/> and <see cref="SpeedStatistic.MegaBytesPerMin"/> <br/>
        /// </summary>
        /// <param name="speedStat">
        /// Either a <see cref="SpeedStatistic"/> or a <see cref="AverageSpeedStatistic"/> object. <br/>
        /// If a <see cref="AverageSpeedStatistic"/> is passed into this constructor, it wil be treated as the base <see cref="SpeedStatistic"/> instead.
        /// </param>
        public AverageSpeedStatistic(SpeedStatistic speedStat) : base()
        {
            Divisor = 1;
            Combined_BytesPerSec = speedStat.BytesPerSec;
            Combined_MegaBytesPerMin = speedStat.MegaBytesPerMin;
            CalculateAverage();
        }

        /// <summary>
        /// Initialize a new <see cref="AverageSpeedStatistic"/> object using <see cref="AverageSpeedStatistic.Average(IEnumerable{SpeedStatistic})"/>. <br/>
        /// </summary>
        /// <param name="speedStats"><inheritdoc cref="Average(IEnumerable{SpeedStatistic})"/></param>
        /// <inheritdoc cref="Average(IEnumerable{SpeedStatistic})"/>
        public AverageSpeedStatistic(IEnumerable<SpeedStatistic> speedStats) : base()
        {
            Average(speedStats);
        }

        #endregion

        #region < Fields >

        /// <summary> Sum of all <see cref="SpeedStatistic.BytesPerSec"/> </summary>
        private decimal Combined_BytesPerSec = 0;

        /// <summary>  Sum of all <see cref="SpeedStatistic.MegaBytesPerMin"/> </summary>
        private decimal Combined_MegaBytesPerMin = 0;

        /// <summary> Total number of SpeedStats that were combined to produce the Combined_* values </summary>
        private long Divisor = 0;

        #endregion

        #region < Reset Value Methods >

        /// <summary>
        /// Set the values for this object to 0
        /// </summary>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        public void Reset()
        {
            Combined_BytesPerSec = 0;
            Combined_MegaBytesPerMin = 0;
            Divisor = 0;
            BytesPerSec = 0;
            MegaBytesPerMin = 0;
        }

        /// <summary>
        /// Set the values for this object to 0
        /// </summary>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        internal void Reset(bool enablePropertyChangeEvent)
        {
            EnablePropertyChangeEvent = enablePropertyChangeEvent;
            Reset();
            EnablePropertyChangeEvent = true;
        }

        #endregion

        // Add / Subtract methods are internal to allow usage within the RoboCopyResultsList object.
        // The 'Average' Methods will first Add the statistics to the current one, then recalculate the average.
        // Subtraction is only used when an item is removed from a RoboCopyResultsList 
        // As such, public consumers should typically not require the use of subtract methods 

        #region < ADD >

        /// <summary>
        /// Add the results of the supplied SpeedStatistic objects to this object. <br/>
        /// Does not automatically recalculate the average, and triggers no events.
        /// </summary>
        /// <remarks>
        /// If any supplied Speedstat object is actually an <see cref="AverageSpeedStatistic"/> object, default functionality will combine the private fields
        /// used to calculate the average speed instead of using the publicly reported speeds. <br/>
        /// This ensures that combining the average of multiple <see cref="AverageSpeedStatistic"/> objects returns the correct value. <br/>
        /// Ex: One object with 2 runs and one with 3 runs will return the average of all 5 runs instead of the average of two averages.
        /// </remarks>
        /// <param name="stat">SpeedStatistic Item to add</param>
        /// <param name="ForceTreatAsSpeedStat">        
        /// Setting this to TRUE will instead combine the calculated average of the <see cref="AverageSpeedStatistic"/>, treating it as a single <see cref="SpeedStatistic"/> object. <br/>
        /// Ignore the private fields, and instead use the calculated speeds)
        /// </param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        internal void Add(SpeedStatistic stat, bool ForceTreatAsSpeedStat = false)
        {
            if (stat == null) return;
            bool IsAverageStat = !ForceTreatAsSpeedStat && stat.GetType() == typeof(AverageSpeedStatistic);
            AverageSpeedStatistic AvgStat = IsAverageStat ? (AverageSpeedStatistic)stat : null;
            Divisor += IsAverageStat ? AvgStat.Divisor : 1;
            Combined_BytesPerSec += IsAverageStat ? AvgStat.Combined_BytesPerSec : stat.BytesPerSec;
            Combined_MegaBytesPerMin += IsAverageStat ? AvgStat.Combined_MegaBytesPerMin : stat.MegaBytesPerMin;
        }


        /// <summary>
        /// Add the supplied SpeedStatistic collection to this object.
        /// </summary>
        /// <param name="stats">SpeedStatistic collection to add</param>
        /// <param name="ForceTreatAsSpeedStat"><inheritdoc cref="Add(SpeedStatistic, bool)"/></param>
        /// <inheritdoc cref="Add(SpeedStatistic, bool)" path="/remarks"/>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        internal void Add(IEnumerable<SpeedStatistic> stats, bool ForceTreatAsSpeedStat = false)
        {
            foreach (SpeedStatistic stat in stats)
                Add(stat, ForceTreatAsSpeedStat);
        }

        #endregion

        #region < Subtract >

        /// <summary>
        /// Subtract the results of the supplied SpeedStatistic objects from this object.<br/>
        /// </summary>
        /// <param name="stat">Statistics Item to add</param>
        /// <param name="ForceTreatAsSpeedStat"><inheritdoc cref="Add(SpeedStatistic, bool)"/></param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        internal void Subtract(SpeedStatistic stat, bool ForceTreatAsSpeedStat = false)
        {
            if (stat == null) return;
            bool IsAverageStat = !ForceTreatAsSpeedStat && stat.GetType() == typeof(AverageSpeedStatistic);
            AverageSpeedStatistic AvgStat = IsAverageStat ? (AverageSpeedStatistic)stat : null;
            Divisor -= IsAverageStat ? AvgStat.Divisor : 1;
            //Combine the values if Divisor is still valid
            if (Divisor >= 1)
            {
                Combined_BytesPerSec -= IsAverageStat ? AvgStat.Combined_BytesPerSec : stat.BytesPerSec;
                Combined_MegaBytesPerMin -= IsAverageStat ? AvgStat.Combined_MegaBytesPerMin : stat.MegaBytesPerMin;
            }
            //Cannot have negative speeds or divisors -> Reset all values
            if (Divisor < 1 || Combined_BytesPerSec < 0 || Combined_MegaBytesPerMin < 0)
            {
                Combined_BytesPerSec = 0;
                Combined_MegaBytesPerMin = 0;
                Divisor = 0;
            }
        }

        /// <summary>
        /// Subtract the supplied SpeedStatistic collection from this object.
        /// </summary>
        /// <param name="stats">SpeedStatistic collection to subtract</param>
        /// <param name="ForceTreatAsSpeedStat"><inheritdoc cref="Add(SpeedStatistic, bool)"/></param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        internal void Subtract(IEnumerable<SpeedStatistic> stats, bool ForceTreatAsSpeedStat = false)
        {
            foreach (SpeedStatistic stat in stats)
                Subtract(stat, ForceTreatAsSpeedStat);
        }

        #endregion

        #region < AVERAGE >

        /// <summary>
        /// Immediately recalculate the BytesPerSec and MegaBytesPerMin values
        /// </summary>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        internal void CalculateAverage()
        {
            BytesPerSec = Divisor < 1 ? 0 : Math.Round(Combined_BytesPerSec / Divisor, 3);
            MegaBytesPerMin = Divisor < 1 ? 0 : Math.Round(Combined_MegaBytesPerMin / Divisor, 3);
        }

        /// <summary>
        /// Combine the supplied <see cref="SpeedStatistic"/> objects, then get the average.
        /// </summary>
        /// <param name="stat">Stats object</param>
        /// <inheritdoc cref="Add(SpeedStatistic, bool)" path="/remarks"/>
        public void Average(SpeedStatistic stat)
        {
            Add(stat);
            CalculateAverage();
        }

        /// <summary>
        /// Combine the supplied <see cref="SpeedStatistic"/> objects, then get the average.
        /// </summary>
        /// <param name="stats">Collection of <see cref="SpeedStatistic"/> objects</param>
        /// <inheritdoc cref="Add(SpeedStatistic, bool)" path="/remarks"/>
        public void Average(IEnumerable<SpeedStatistic> stats)
        {
            Add(stats);
            CalculateAverage();
        }

        /// <returns>New Statistics Object</returns>
        /// <inheritdoc cref=" Average(IEnumerable{SpeedStatistic})"/>
        public static AverageSpeedStatistic GetAverage(IEnumerable<SpeedStatistic> stats)
        {
            AverageSpeedStatistic stat = new AverageSpeedStatistic();
            stat.Average(stats);
            return stat;
        }

        #endregion 

    }
}
