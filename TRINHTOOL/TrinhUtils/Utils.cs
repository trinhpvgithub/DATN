using Autodesk.Revit.DB;
using HcBimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL.TrinhUtils
{
   public class Utils
   {
      public static double DistancePoint2Line2(XYZ p, Line line)
      {
         double value = 1;
         XYZ xYZ = null;
         xYZ = ((!line.IsBound) ? line.Origin : line.GetEndPoint(0));
         XYZ source = line.Direction.Normalize();
         var num = Math.Abs((p - xYZ).DotProduct(source));
         var nm = source.CrossProduct(XYZ.BasisZ);
         var l = p.CreateLine(p.Add(nm * 1.MeterToFoot()));
         if (l.Intersect(line) == SetComparisonResult.Disjoint)
         {
            value = 10000000000;
         }
         else value = Math.Sqrt(p.DistanceTo(xYZ) * p.DistanceTo(xYZ) - num * num);
         return value;
      }
      public static string[] Split(string text)
      {
         var texts= text.Split('(') ;
         return texts;
      }
   }
}
