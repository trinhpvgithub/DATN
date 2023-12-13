using Autodesk.Revit.DB;
using HcBimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TRINHTOOL.Beam.Model
{
   public class BeamInfo
   {
      public XYZ StartPoint { get; set; }
      public XYZ OriginPoint { get; set; }

      public XYZ EndPoint { get; set; }

      public double Width { get; set; }

      public double Height { get; set; }

      public string Mark { get; set; }

      public BeamInfo(XYZ origin, XYZ start, XYZ end, string text)
      {
         OriginPoint = origin;
         StartPoint = start;
         EndPoint = end;
         GetBeam(text);
      }

      public void GetBeam(string Text)
      {
         var numbers = Regex.Split(Text, @"\D+").Where(x => string.IsNullOrEmpty(x) == false).ToList();

         if (numbers.Count >= 2)
         {
            var last = numbers[numbers.Count - 1];
            var last1 = numbers[numbers.Count - 2];
            Width = Convert.ToDouble(last1);
            Height = Convert.ToDouble(last);
         }

      }

      public class BeamInfoComparerByPoint : IEqualityComparer<BeamInfo>
      {
         public bool Equals(BeamInfo x, BeamInfo y)
         {
            if (x == null && y == null)
            {
               return true;
            }

            if (x == null || y == null)
            {
               return false;
            }

            if (x.StartPoint.X.IsEqual(y.StartPoint.X) && x.EndPoint.X.IsEqual(y.EndPoint.X) && x.StartPoint.Y.IsEqual(y.StartPoint.Y) && x.EndPoint.Y.IsEqual(y.EndPoint.Y))
            {
               return true;
            }

            return false;
         }

         public int GetHashCode(BeamInfo obj)
         {
            return 0;
         }
      }
   }
}
