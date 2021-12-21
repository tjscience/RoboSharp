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
    /// <summary> Contains information regarding average Transfer Speed </summary>
    public class SpeedStatistic : INotifyPropertyChanged
    {
        #region < Fields, Events, Properties >

        private decimal BytesPerSecField;
        private decimal MegaBytesPerMinField;

        /// <summary> This toggle Enables/Disables firing the <see cref="PropertyChanged"/> Event to avoid firing it when doing multiple consecutive changes to the values </summary>
        private bool EnablePropertyChangeEvent { get; set; } = true;

        /// <summary>This event will fire when the value of the SpeedStatistic is updated </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Average Transfer Rate in Bytes/Second </summary>
        public decimal BytesPerSec {
            get => BytesPerSecField;
            private set
            {
                if (BytesPerSecField != value)
                {
                    BytesPerSecField = value;
                    if (EnablePropertyChangeEvent) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MegaBytesPerMin"));
                }
            }
        }

        /// <summary> Average Transfer Rate in MB/Minute</summary>
        public decimal MegaBytesPerMin {
            get => MegaBytesPerMinField;
            private set
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

        #region < Set Value Methods >

        /// <summary>
        /// Set the values for this object to 0
        /// </summary>
        public void Reset()
        {
            BytesPerSec = 0;
            MegaBytesPerMin = 0;
        }

        /// <summary>
        /// Set the values for this object to 0
        /// </summary>
        internal void Reset(bool enablePropertyChangeEvent)
        {
            EnablePropertyChangeEvent = enablePropertyChangeEvent;
            BytesPerSec = 0;
            MegaBytesPerMin = 0;
            EnablePropertyChangeEvent = true;
        }

        /// <inheritdoc cref="SetValues(SpeedStatistic, bool)"/>
        public void SetValues(SpeedStatistic speedStat) 
        {
            MegaBytesPerMin = speedStat?.MegaBytesPerMin ?? 0;
            BytesPerSec = speedStat?.BytesPerSec ?? 0;
        }

        /// <summary>
        /// Set the values of this object to the values of another <see cref="SpeedStatistic"/> object.
        /// </summary>
        /// <param name="speedStat">SpeedStatistic to copy the values from</param>
        /// <param name="enablePropertyChangeEvent"><inheritdoc cref="EnablePropertyChangeEvent" path="*"/><para/> After updating the values, this property will be set back to TRUE.</param>
        internal void SetValues(SpeedStatistic speedStat, bool enablePropertyChangeEvent)
        {
            EnablePropertyChangeEvent = enablePropertyChangeEvent;
            MegaBytesPerMin = speedStat.MegaBytesPerMin;
            BytesPerSec = speedStat.BytesPerSec;
            EnablePropertyChangeEvent = true;
        }

        #endregion

        #region ADD

        //The Adding methods exists solely to facilitate the averaging method. Adding speeds together over consecutive runs makes little sense. 

        /// <summary>
        /// Add the results of the supplied SpeedStatistic objects to this Statistics object.
        /// </summary>
        /// <param name="stat">Statistics Item to add</param>
        /// 
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        private void AddStatistic(SpeedStatistic stat)
        {
            EnablePropertyChangeEvent = false;
            BytesPerSec += stat?.BytesPerSec ?? 0;
            MegaBytesPerMin += stat?.MegaBytesPerMin ?? 0;
            EnablePropertyChangeEvent = true;
        }

        /// <summary>
        /// Add the results of the supplied SpeedStatistic objects to this Statistics object.
        /// </summary>
        /// <param name="stats">Statistics Items to add</param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        private void AddStatistic(IEnumerable<SpeedStatistic> stats)
        {
            EnablePropertyChangeEvent = false;
            foreach (SpeedStatistic stat in stats)
            {
                BytesPerSec += stat?.BytesPerSec ?? 0;
                MegaBytesPerMin += stat?.MegaBytesPerMin ?? 0;
            }
            EnablePropertyChangeEvent = true;
        }

        #endregion ADD

        #region AVERAGE

        /// <summary>
        /// Combine the supplied <see cref="SpeedStatistic"/> objects, then get the average.
        /// </summary>
        /// <param name="stat">Stats object</param>
        public void AverageStatistic(SpeedStatistic stat)
        {
            this.AddStatistic(stat);
            BytesPerSec /= 2;
            MegaBytesPerMin /= 2;
        }

        /// <summary>
        /// Combine the supplied <see cref="SpeedStatistic"/> objects, then get the average.
        /// </summary>
        /// <param name="stats">Array of Stats objects</param>
        public void AverageStatistic(IEnumerable<SpeedStatistic> stats)
        {
            this.AddStatistic(stats);
            int cnt = stats.Count() + 1;
            BytesPerSec /= cnt;
            MegaBytesPerMin /= cnt;
        }

        /// <returns>New Statistics Object</returns>
        /// <inheritdoc cref=" AverageStatistic(IEnumerable{SpeedStatistic})"/>
        public static SpeedStatistic AverageStatistics(IEnumerable<SpeedStatistic> stats)
        {
            SpeedStatistic stat = new SpeedStatistic();
            stat.AddStatistic(stats);
            int cnt = stats.Count();
            if (cnt > 1)
            {
                stat.BytesPerSec /= cnt;
                stat.MegaBytesPerMin /= cnt;
            }
            return stat;
        }

        #endregion AVERAGE

    }
}
