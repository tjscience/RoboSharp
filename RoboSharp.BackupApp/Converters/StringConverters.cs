using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CustomControls.WPF.ValueConverters
{

    public static class StringConverters
    {

        public static BulletPointPrefixConverter GetBulletPointPrefixConverter { get; } = new BulletPointPrefixConverter();

        public static StringArrayConverter StringArrayToString { get; } = new StringArrayConverter();

        /// <summary>
        /// Adds a bullet point to the specified string
        /// </summary>
        [ValueConversion(typeof(string), typeof(string))]

        public class BulletPointPrefixConverter : BaseConverter
        {
            const string BP = "☻";

            public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var v = value as string;
                if (String.IsNullOrEmpty(v)) return BP;
                return BP + " " + v.Trim();
            }

            public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var v = (value as string)?.Trim();
                if (String.IsNullOrEmpty(v)) return string.Empty;
                if (v.StartsWith(BP) && v.Length > 1) return v.Substring(1);
                return v;
            }
        }


        /// <summary>
        /// Converts a string array to a single long string
        /// </summary>
        [ValueConversion(typeof(string[]), typeof(string))]

        public class StringArrayConverter : BaseConverter
        {


            public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var v = value as string[];
                return String.Concat(v.Select(s => s + Environment.NewLine));
            }

            public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var v = (value as string)?.Trim();
                if (String.IsNullOrEmpty(v)) return new string[] { };
                return v.Split(Environment.NewLine.ToCharArray());
            }
        }
    }
}
