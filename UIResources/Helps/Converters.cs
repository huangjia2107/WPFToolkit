using System;
using System.Windows;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using UIResources.Controls;

namespace UIResources.Helps
{
    #region IValueConverter



    public class BoolToVisibilityConverter : IValueConverter
    {
        public BoolToVisibilityConverter()
        {
            TrueVisibility = Visibility.Visible;
            FalseVisibility = Visibility.Collapsed;
        }

        public Visibility TrueVisibility { get; set; }
        public Visibility FalseVisibility { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = (bool)value;
            return isTrue ? TrueVisibility : FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {  
            var oriValue = (double)value;
            var compareValue = parameter == null ? 0d : double.Parse(parameter.ToString());

            return DoubleUtil.GreaterThan(oriValue, compareValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


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

    public class ZoomBoxRulerShiftMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Contains(null) || values.Contains(DependencyProperty.UnsetValue))
                return 0d;

            var scrollbarOffset = (double)values[0];
            var originShift = (double)values[1];
            var scale = (double)values[2];
            var unit = (RulerUnit)values[3];

            return (decimal)((originShift - scrollbarOffset) / (double)DpiUtil.GetPixelPerUnit(unit) / scale);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
