using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace RoboSharp.Results
{
    /// <summary>
    /// Information about number of items Copied, Skipped, Failed, etc.
    /// </summary>
    public class Statistic
    {
        /// <summary> Total Scanned during the run</summary>
        public long Total { get; private set; }
        /// <summary> Total Copied </summary>
        public long Copied { get; private set; }
        /// <summary> Total Skipped </summary>
        public long Skipped { get; private set; }
        /// <summary>  </summary>
        public long Mismatch { get; private set; }
        /// <summary> Total that failed to copy or move </summary>
        public long Failed { get; private set; }
        /// <summary> Total Extra that exist in the Destination (but are missing from the Source)</summary>
        public long Extras { get; private set; }

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

        #region ADD

        /// <summary>
        /// Add the results of the supplied Statistics object to this Statistics object.
        /// </summary>
        /// <param name="stats">Statistics Item to add</param>
        public void AddStatistic(Statistic stats)
        {
            this.Total += stats.Total;
            this.Copied += stats.Copied;
            this.Extras += stats.Extras;
            this.Failed += stats.Failed;
            this.Mismatch += stats.Mismatch;
            this.Skipped += stats.Skipped;
        }

        /// <summary>
        /// Add the results of the supplied Statistics objects to this Statistics object.
        /// </summary>
        /// <param name="stats">Statistics Item to add</param>
        public void AddStatistic(IEnumerable<Statistic> stats)
        {
            foreach (Statistic stat in stats)
                AddStatistic(stat);
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

        #region AVERAGE

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
    }
}
