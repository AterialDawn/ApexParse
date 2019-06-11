using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ApexParse.Views.Converters
{
    public class GridLengthConverter : IValueConverter
    {
        public GridUnitType UnitType { get; set; } = GridUnitType.Star;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is double)) throw new InvalidCastException($"Cannot convert from {value.GetType()} to {targetType}");
            double val = (double)value;

            return new GridLength(val, UnitType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLength val = (GridLength)value;

            return val.Value;
        }
    }
}
