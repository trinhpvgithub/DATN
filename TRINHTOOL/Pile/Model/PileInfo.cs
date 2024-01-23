using Autodesk.Revit.DB;
using HcBimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL.Pile.Model
{
   public class PileInfo
   {
      public XYZ Center { get; set; }

      public double Radius { get; set; }

      public string Text { get; set; }

      public PileInfo(double radius, XYZ center, string text)
      {
         Radius = radius;
         Text = text;
         Center = center;
      }

      public override int GetHashCode()
      {
         return 0;
      }

      public override bool Equals(object obj)
      {
         if (obj == null)
            return false;
         if (GetType() != obj.GetType()) return false;
         return obj is PileInfo pileInfo && Radius.IsEqual(pileInfo.Radius, 0.1) && Text.IsEqual(pileInfo.Text);
      }

   }
   public class PileInfoComparerByPoint : IEqualityComparer<PileInfo>
   {
      public bool Equals(PileInfo x, PileInfo y)
      {
         if (x == null && y == null)
         {
            return true;
         }

         if (x == null || y == null)
         {
            return false;
         }

         if (x.Center.X.IsEqual(y.Center.X) &&
             x.Center.Y.IsEqual(y.Center.Y) && x.Center.Z.IsEqual(y.Center.Z))
         {
            return true;
         }

         return false;
      }

      public int GetHashCode(PileInfo obj)
      {
         return 0;
      }
   }
}
