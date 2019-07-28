using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ApexParse.Views.Converters
{
    public class BoolToVisConverter : MarkupExtension, IValueConverter
    {
        public BoolToVisConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Visibility.Visible.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = System.Convert.ToBoolean(value);
            Visibility retVal = val ? Visibility.Visible : Visibility.Collapsed;
            return retVal;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
