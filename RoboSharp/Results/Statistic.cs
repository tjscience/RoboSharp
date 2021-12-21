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
    /// Information about number of items Copied, Skipped, Failed, etc.
    /// </summary>
    public class Statistic : INotifyPropertyChanged
    {
        #region < Fields, Events, Properties >

        private long TotalField;
        private long CopiedField;
        private long SkippedField;
        private long MismatchField;
        private long FailedField;
        private long ExtrasField;

        /// <summary> This toggle Enables/Disables firing the <see cref="PropertyChanged"/> Event to avoid firing it when doing multiple consecutive changes to the values </summary>
        private bool EnablePropertyChangeEvent = true;

        /// <summary>This event will fire when the value of the statistic is updated via Adding / Subtracting methods </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Total Scanned during the run</summary>
        public long Total { 
            get => TotalField;
            private set
            {
                if (TotalField != value)
                {
                    TotalField = value;
                    if (EnablePropertyChangeEvent) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Total"));
                }
            }
        }

        /// <summary> Total Copied </summary>
        public long Copied { 
            get => CopiedField;
            private set
            {
                if (CopiedField != value)
                {
                    CopiedField = value;
                    if (EnablePropertyChangeEvent) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Copied"));
                }
            }
        }

        /// <summary> Total Skipped </summary>
        public long Skipped { 
            get => SkippedField;
            private set
            {
                if (SkippedField != value)
                {
                    SkippedField = value;
                    if (EnablePropertyChangeEvent) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Skipped"));
                }
            }
        }

        /// <summary>  </summary>
        public long Mismatch { 
            get => MismatchField;
            private set
            {
                if (MismatchField != value)
                {
                    MismatchField = value;
                    if (EnablePropertyChangeEvent) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Mismatch"));
                }
            }
        }

        /// <summary> Total that failed to copy or move </summary>
        public long Failed { 
            get => FailedField; 
            private set
            {
                if (FailedField != value)
                {
                    FailedField = value;
                    if (EnablePropertyChangeEvent) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Failed"));
                }
            }
        }

        /// <summary> Total Extra that exist in the Destination (but are missing from the Source)</summary>
        public long Extras { 
            get => ExtrasField;
            private set 
            {
                if (ExtrasField != value)
                {
                    ExtrasField = value;
                    if (EnablePropertyChangeEvent) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Extras"));
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
            return $"Total: {Total}, Copied: {Copied}, Skipped: {Skipped}, Mismatch: {Mismatch}, Failed: {Failed}, Extras: {Extras}";
        }

        /// <summary>
        /// Parse a string and for the tokens reported by RoboCopy
        /// </summary>
        /// <param name="line"></param>
        /// <returns>New Statistic Object</returns>
        public static Statistic Parse(string line)
        {
            var res = new Statistic();

            var tokenNames = new[] { nameof(Total), nameof(Copied), nameof(Skipped), nameof(Mismatch), nameof(Failed), nameof(Extras) };
            var patternBuilder = new StringBuilder(@"^.*:");

            foreach (var tokenName in tokenNames)
            {
                var tokenPattern = GetTokenPattern(tokenName);
                patternBuilder.Append(@"\s+").Append(tokenPattern);
            }

            var pattern = patternBuilder.ToString();
            var match = Regex.Match(line, pattern);
            if (!match.Success)
                return res;

            var props = res.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var tokenName in tokenNames)
            {
                var prop = props.FirstOrDefault(x => x.Name == tokenName);
                if (prop == null)
                    continue;

                var tokenString = match.Groups[tokenName].Value;
                var tokenValue = ParseTokenString(tokenString);
                prop.SetValue(res, tokenValue, null);
            }

            return res;
        }

        private static string GetTokenPattern(string tokenName)
        {
            return $@"(?<{tokenName}>[\d\.]+(\s\w)?)";
        }

        private static long ParseTokenString(string tokenString)
        {
            if (string.IsNullOrWhiteSpace(tokenString))
                return 0;

            tokenString = tokenString.Trim();
            if (Regex.IsMatch(tokenString, @"^\d+$"))
                return long.Parse(tokenString);

            var match = Regex.Match(tokenString, @"(?<Mains>[\d\.,]+)(\.(?<Fraction>\d+))\s(?<Unit>\w)");
            if (match.Success)
            {
                var mains = match.Groups["Mains"].Value.Replace(".", "").Replace(",", "");
                var fraction = match.Groups["Fraction"].Value;
                var unit = match.Groups["Unit"].Value.ToLower();

                var number = double.Parse($"{mains}.{fraction}", NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                switch (unit)
                {
                    case "k":
                        // Byte = kBytes * 1024
                        number *= Math.Pow(1024, 1);
                        break;
                    case "m":
                        // Byte = MBytes * 1024 * 1024
                        number *= Math.Pow(1024, 2);
                        break;
                    case "g":
                        // Byte = GBytes * 1024 * 1024 * 1024
                        number *= Math.Pow(1024, 3);
                        break;
                    case "t":
                        // Byte = TBytes * 1024 * 1024 * 1024 * 1024
                        number *= Math.Pow(1024, 4);
                        break;
                }

                return Convert.ToInt64(number);
            }

            return 0;
        }

        #endregion

        /// <summary>
        /// Reset all values to Zero ( 0 ) -- Used by <see cref="RoboCopyResultsList"/> for the properties
        /// </summary>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        public void Reset()
        {
            Copied = 0;
            Extras = 0;
            Failed = 0;
            Mismatch = 0;
            Skipped = 0;
            Total = 0;
        }

        /// <summary>
        /// Set the values for this object to 0
        /// </summary>
        internal void Reset(bool enablePropertyChangeEvent)
        {
            EnablePropertyChangeEvent = enablePropertyChangeEvent;
            Reset();
            EnablePropertyChangeEvent = true;
        }

        #region < ADD Methods >

        /// <summary>
        /// Add the results of the supplied Statistics object to this Statistics object.
        /// </summary>
        /// <param name="stats">Statistics Item to add</param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        public void AddStatistic(Statistic stats)
        {
            Total += stats?.Total ?? 0;
            Copied += stats?.Copied ?? 0;
            Extras += stats?.Extras ?? 0;
            Failed += stats?.Failed ?? 0;
            Mismatch += stats?.Mismatch ?? 0;
            Skipped += stats?.Skipped ?? 0;
        }

        
        #pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        /// <param name="enablePropertyChangedEvent"><inheritdoc cref="EnablePropertyChangeEvent" path="*"/></param>
        /// <inheritdoc cref="AddStatistic(Statistic)"/>        
        internal void AddStatistic(Statistic stats, bool enablePropertyChangedEvent)
        {
            EnablePropertyChangeEvent = enablePropertyChangedEvent;
            AddStatistic(stats);
            EnablePropertyChangeEvent = true;

        }
        #pragma warning restore CS1573


        /// <summary>
        /// Add the results of the supplied Statistics objects to this Statistics object.
        /// </summary>
        /// <param name="stats">Statistics Item to add</param>
        public void AddStatistic(IEnumerable<Statistic> stats)
        {
            foreach (Statistic stat in stats)
            {
                EnablePropertyChangeEvent = stat == stats.Last();
                AddStatistic(stat);
            }
        }

        /// <summary>
        /// Combine the results of the supplied statistics objects
        /// </summary>
        /// <param name="stats">Statistics Item to add</param>
        /// <returns>New Statistics Object</returns>
        public static Statistic AddStatistics(IEnumerable<Statistic> stats)
        {
            Statistic ret = new Statistic();
            ret.AddStatistic(stats);
            return ret;
        }

        #endregion ADD

        #region < AVERAGE Methods >

        /// <summary>
        /// Combine the supplied <see cref="Statistic"/> objects, then get the average.
        /// </summary>
        /// <param name="stats">Array of Stats objects</param>
        public void AverageStatistic(IEnumerable<Statistic> stats)
        {
            this.AddStatistic(stats);
            int cnt = stats.Count() + 1;
            Total /= cnt;
            Copied /= cnt;
            Extras /= cnt;
            Failed /= cnt;
            Mismatch /= cnt;
            Skipped /= cnt;

        }

        /// <returns>New Statistics Object</returns>
        /// <inheritdoc cref=" AverageStatistic(IEnumerable{Statistic})"/>
        public static Statistic AverageStatistics(IEnumerable<Statistic> stats)
        {
            Statistic stat = AddStatistics(stats);
            int cnt = stats.Count();
            if (cnt > 1)
            {
                stat.Total /= cnt;
                stat.Copied /= cnt;
                stat.Extras /= cnt;
                stat.Failed /= cnt;
                stat.Mismatch /= cnt;
                stat.Skipped /= cnt;
            }
            return stat;
        }

        #endregion AVERAGE

        #region < Subtract Methods >

        /// <summary>
        /// Subtract Method used by <see cref="RoboCopyResultsList"/>
        /// </summary>
        /// <param name="stat">Statistics Item to subtract</param>
#if !NET40
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
#endif
        public void Subtract(Statistic stat)
        {
            Copied -= stat?.Copied ?? 0;
            Extras -= stat?.Extras ?? 0;
            Failed -= stat?.Failed ?? 0;
            Mismatch -= stat?.Mismatch ?? 0;
            Skipped -= stat?.Skipped ?? 0;
            Total -= stat?.Total ?? 0;
        }

        #pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        /// <param name="enablePropertyChangedEvent"><inheritdoc cref="EnablePropertyChangeEvent" path="*"/></param>
        /// <inheritdoc cref="Subtract(Statistic)"/>        
        internal void Subtract(Statistic stats, bool enablePropertyChangedEvent)
        {
            EnablePropertyChangeEvent = enablePropertyChangedEvent;
            Subtract(stats);
            EnablePropertyChangeEvent = true;
        }
        #pragma warning restore CS1573

        /// <summary>
        /// Add the results of the supplied Statistics objects to this Statistics object.
        /// </summary>
        /// <param name="stats">Statistics Item to add</param>
        public void Subtract(IEnumerable<Statistic> stats)
        {
            foreach (Statistic stat in stats)
            {
                EnablePropertyChangeEvent = stat == stats.Last();
                Subtract(stat);
            }
        }

        #endregion Subtract
    }
}
