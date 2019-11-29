using Devil7.Android.SepolicyHelper.ViewModels;
using Avalonia.Data.Converters;
using System;

namespace Devil7.Android.SepolicyHelper.Utils
{
    public class EnumToBoolConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter.ToString().Equals(Enum.GetName(value.GetType(), value)))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Enum.Parse(targetType, parameter.ToString());
        }
        #endregion

    }

    public class InverseBoolConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
        #endregion

    }
}