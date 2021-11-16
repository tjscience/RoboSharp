using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace RoboSharp.Results
{
    /// <summary> Contains information regarding average Transfer Speed </summary>
    public class SpeedStatistic
    {
        /// <summary> Average Transfer Rate in Bytes/Second </summary>
        public decimal BytesPerSec { get; private set; }
        /// <summary> Average Transfer Rate in MB/Minute</summary>
        public decimal MegaBytesPerMin { get; private set; }

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

        #region ADD

        //The Adding methods exists solely to facilitate the averaging method. Adding speeds together over consecutive runs makes little sense. 

        /// <summary>
        /// Add the results of the supplied SpeedStatistic objects to this Statistics object.
        /// </summary>
        /// <param name="stats">Statistics Items to add</param>
        private void AddStatistic(IEnumerable<SpeedStatistic> stats)
        {
            foreach (SpeedStatistic stat in stats)
            {
                this.BytesPerSec += stat.BytesPerSec;
                this.MegaBytesPerMin += stat.MegaBytesPerMin;
            }
        }

        /// <summary>
        /// Combine the results of the supplied SpeedStatistic objects
        /// </summary>
        /// <param name="stats">SpeedStatistic Items to add</param>
        /// <returns>New Statistics Object</returns>
        private static SpeedStatistic AddStatistics(IEnumerable<SpeedStatistic> stats)
        {
            SpeedStatistic ret = new SpeedStatistic();
            ret.AddStatistic(stats);
            return ret;
        }

        #endregion ADD

        #region AVERAGE

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
            SpeedStatistic stat = AddStatistics(stats);
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
