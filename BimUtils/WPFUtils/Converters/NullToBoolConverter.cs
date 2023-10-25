﻿using System.Globalization ;
using System.Windows.Data ;

namespace BimUtils.WPFUtils.Converters
{
   public class NullToBoolConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value == null)
         {
            return false;
         }
         return true;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return parameter;
      }
   }
}