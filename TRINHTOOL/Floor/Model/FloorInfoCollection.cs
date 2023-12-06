using HcBimUtils.WPFUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL.Floor.Model
{
   public class FloorInfoCollection:ViewModelBase
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
   }
}
