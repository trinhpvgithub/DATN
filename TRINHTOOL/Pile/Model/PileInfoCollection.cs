using Autodesk.Revit.DB;
using HcBimUtils.WPFUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL.Pile.Model
{
   public class PileInfoCollection : ViewModelBase
   {
      public List<PileInfo> PileInfos { get; set; } = new List<PileInfo>();

      public double Radius
      {
         get => radius;
         set
         {
            radius = value;
            OnPropertyChanged();
         }
      }

      private string text;
      private double radius;

      public string Text
      {
         get => text;
         set
         {
            text = value;
            OnPropertyChanged();
         }
      }

      public XYZ Center { get; set; }


      public ElementType ElementType { get; set; }

      public int Number { get; set; }
   }
}
