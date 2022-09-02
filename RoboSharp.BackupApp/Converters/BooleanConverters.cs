using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CustomControls.WPF.ValueConverters
{
    
    /// <summary>
    /// 
    /// </summary>
    [ValueConversion(typeof(bool),typeof(bool))]
    public sealed class InverseBooleanConverter : BaseConverter
    {
        public static InverseBooleanConverter Converter { get; } = new InverseBooleanConverter();
        
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? false : !(bool)value;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? false : !(bool)value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public BooleanToVisibilityConverter()
        {
            converter = new System.Windows.Controls.BooleanToVisibilityConverter();
        }

        private System.Windows.Controls.BooleanToVisibilityConverter converter {get;}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IValueConverter)converter).Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IValueConverter)converter).ConvertBack(value, targetType, parameter, culture);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToHiddenConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public BooleanToHiddenConverter()
        {
            converter = new System.Windows.Controls.BooleanToVisibilityConverter();
        }

        private System.Windows.Controls.BooleanToVisibilityConverter converter { get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility ret = (Visibility)((IValueConverter)converter).Convert(value, targetType, parameter, culture);
            if (ret == Visibility.Collapsed) return Visibility.Hidden;
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Hidden) value = Visibility.Collapsed;
            return ((IValueConverter)converter).ConvertBack(value, targetType, parameter, culture);
        }
    }

    /// <summary>
    /// Always returns the string representation of the input value
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public sealed class ToStringConverter : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Always returns the string representation of the input value
    /// </summary>
    [ValueConversion(typeof(int), typeof(string))]
    public sealed class StringToInt : BaseConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), out int result))
                return result;
            else
                return 0;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }
    }

}
