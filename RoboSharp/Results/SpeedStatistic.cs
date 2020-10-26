using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RoboSharp.Results
{
    public class SpeedStatistic
    {
        public decimal BytesPerSec { get; private set; }
        public decimal MegaBytesPerMin { get; private set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return $"Speed: {BytesPerSec} Bytes/sec{Environment.NewLine}Speed: {MegaBytesPerMin} MegaBytes/min";
        }

        public static SpeedStatistic Parse(string line1, string line2)
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
    }
}