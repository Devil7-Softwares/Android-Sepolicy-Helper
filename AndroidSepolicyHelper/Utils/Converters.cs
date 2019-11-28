﻿using AndroidSepolicyHelper.ViewModels;
using Avalonia.Data.Converters;
using System;

namespace AndroidSepolicyHelper.Utils
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
}