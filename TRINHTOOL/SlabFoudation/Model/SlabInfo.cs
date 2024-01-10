using HcBimUtils.WPFUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL.SlabFoudation.Model
{
   public class SlabInfo : ViewModelBase
   {
      private double _area;

      public double Area
      {
         get => _area;

         set
         {
            _area = value;
            OnPropertyChanged();
         }
      }
      private double _text;

      public double Text
      {
         get => _text;

         set
         {
            _text = value;
            OnPropertyChanged();
         }
      }
   }
}
