using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace CustomControls.WPF.ValueConverters
{
    /// <summary>
    /// Abstract Base Class for <see cref="IValueConverter"/> interface
    /// </summary>
    /// <remarks>
    /// <see href="http://www.wpftutorial.net/ValueConverters.html"/>
    /// </remarks>
    public abstract class BaseConverter : IValueConverter //MarkupExtension, IValueConverter
    {
        /// <remarks> Used when displaying data from the viewmodel ( Convert ViewModelData to UserControl Data ) </remarks>
        /// <inheritdoc cref="IValueConverter.Convert(object, Type, object, CultureInfo)"/>
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        /// <remarks> Used when passing data to the viewmodel ( Convert User Control Data to ViewModel Data )</remarks>
        /// <inheritdoc cref="IValueConverter.ConvertBack(object, Type, object, CultureInfo)"/>
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
        
        ///// <returns>This Object</returns>
        ///// <inheritdoc cref="MarkupExtension.ProvideValue(IServiceProvider)"/>
        //public override object ProvideValue(IServiceProvider serviceProvider)
        //{
        //    return this;
        //}
    }

    /// <summary>
    /// Abstract Base Class for <see cref="IMultiValueConverter"/> interface
    /// </summary>
    /// <remarks>
    /// <see href="http://www.wpftutorial.net/ValueConverters.html"/>
    /// </remarks>
    public abstract class BaseMultiConverter : IMultiValueConverter //MarkupExtension, IValueConverter
    {
        /// <inheritdoc cref="IMultiValueConverter.Convert"/>
        /// <remarks> Used to display the data to user </remarks>
        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        /// <inheritdoc cref="IMultiValueConverter.ConvertBack"/>
        /// <remarks> Used to seperate a result into the multiple bindings </remarks>
        public abstract object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);

        ///// <returns>This Object</returns>
        ///// <inheritdoc cref="MarkupExtension.ProvideValue(IServiceProvider)"/>
        //public override object ProvideValue(IServiceProvider serviceProvider)
        //{
        //    return this;
        //}
    }
}
