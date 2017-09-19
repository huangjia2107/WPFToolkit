using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace UIResources.Helps
{
    #region IValueConverter

    public class MultipleValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dblVal = (double)value;
            var multiple = parameter == null ? 1d : System.Convert.ToDouble(parameter);

            return dblVal * multiple;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 

    public class Positive_NegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(double))
                return -(double)value;
            else if (value.GetType() == typeof(float))
                return -(float)value;
            else if (value.GetType() == typeof(int))
                return -(int)value;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    #endregion
}
