﻿using System.Windows.Data ;

namespace BimUtils.WPFUtils.Converters
{
   public class RadioButtonCheckedConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter,
          System.Globalization.CultureInfo culture)
      {
         return value.Equals(parameter);
      }

      public object ConvertBack(object value, Type targetType, object parameter,
          System.Globalization.CultureInfo culture)
      {
         return value.Equals(true) ? parameter : Binding.DoNothing;
      }
   }
}