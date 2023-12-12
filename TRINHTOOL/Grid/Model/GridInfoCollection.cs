using Autodesk.Revit.DB;
using HcBimUtils.WPFUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL.Grid.Model
{
   public class GridInfoCollection : ViewModelBase
   {
      private double length;
      private string text;

      public double Length
      {
         get => length;

         set
         {
            length = value;
            OnPropertyChanged();
         }
      }

      public string Text
      {
         get => text;

         set
         {
            text = value;
            OnPropertyChanged();
         }
      }

      public XYZ StartPoint
      {
         get; set;
      }

      public XYZ EndPoint { get; set; }

      public int Number { get; set; }
   }
}
