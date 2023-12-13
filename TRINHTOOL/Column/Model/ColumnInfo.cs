using Autodesk.Revit.DB;
using HcBimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL.Column.Model
{
   public class ColumnInfo
   {
      public XYZ Center { get; set; }
      public Line WidthLine { get; set; }
      public Line HeightLine { get; set; }
      public double Width { get; set; }
      public double Height { get; set; }
      public double Rotation { get; set; }



      public string Text { get; set; }

      public ColumnInfo(List<XYZ> points, string text)
      {
         GetInfo(points);
         Text = text;
      }
      public ColumnInfo()
      {

      }
      private void GetInfo(List<XYZ> points)
      {
         //Center
         Center = new XYZ(points.Average(x => x.X), points.Average(x => x.Y), points.Average(x => x.Z));
         var p1 = points[0];
         var p2 = points[1];
         var p3 = points[2];
         var p4 = points[3];
         var l1 = Line.CreateBound(p1, p2);
         var l2 = Line.CreateBound(p2, p3);
         if (l1.Length >= l2.Length)
         {
            HeightLine = l1;
            WidthLine = l2;
         }
         else
         {
            HeightLine = l2;
            WidthLine = l1;
         }

         var direction = HeightLine.Direction;

         if (direction.Y < 0)
         {
            direction = -direction;
         }

         Width = Math.Round(WidthLine.Length.FootToMm(), 1);

         Height = Math.Round(HeightLine.Length.FootToMm(), 1);

         Rotation = new XYZ(direction.X, direction.Y, 0).AngleTo(XYZ.BasisY);
      }

      public override bool Equals(object obj)
      {
         if (obj == null)
            return false;
         if (GetType() != obj.GetType()) return false;
         return obj is ColumnInfo columnInfo && Width.IsEqual(columnInfo.Width, 0.1) && Height.IsEqual(columnInfo.Height, 0.1);
      }

      public override int GetHashCode()
      {
         return 0;
      }
   }
   public class ColumnInfoComparerByPoint : IEqualityComparer<ColumnInfo>
   {
      public bool Equals(ColumnInfo x, ColumnInfo y)
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

      public int GetHashCode(ColumnInfo obj)
      {
         return 0;
      }
   }
}
