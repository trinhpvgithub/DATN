using Autodesk.Revit.DB;
using HcBimUtils;
using HcBimUtils.DocumentUtils;
using HcBimUtils.JsonData.ModelFromCadJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL
{
   public static class ModelFromCadUtils
   {
      public static string CadDataPath = AC.HCSettingPath + "\\CadData.json";

      public static ExportData GetData()
      {
         var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BimSpeedSetting" + "\\CadData.json";
         var data = HcBimUtils.JsonData.JsonUtils.GetSettingFromFile<ExportData>(path);
         if (data == null)
         {
            return new ExportData();
         }
         return data;
      }

      public static void SaveData(ExportData data)
      {
         HcBimUtils.JsonData.JsonUtils.SaveSettingToFile(data, CadDataPath);
      }

      //public static Line ToRevitLine(this LineData lineData)
      //{
      //   //return Line.CreateBound(lineData.SP.ToXyz(), lineData.EP.ToXyz());
      //}

      public static XYZ ToXyz(this XyzData data)
      {
         return new XYZ(data.X.MmToFoot(), data.Y.MmToFoot(), data.Z.MmToFoot());
      }

      public static XYZ ToXyzfit(this XYZ data)
      {
         return new XYZ(data.X.MmToFoot(), data.Y.MmToFoot(), data.Z.MmToFoot());
      }

      private const double _inch = 1.0 / 12.0;
      private const double _sixteenth = _inch / 16.0;

      public static Curve CreateReversedCurve(Curve orig)
      {


         if (orig is Line)
         {
            return Line.CreateBound(
                orig.GetEndPoint(1),
                orig.GetEndPoint(0));
         }
         else
         {
            throw new Exception(
                "CreateReversedCurve - Unreachable");
         }
      }


      public static List<Curve> SortCurvesContiguous(this IList<Curve> list, bool debugOutput = false)
      {

         var curves = new List<Curve>(list);

         int n = curves.Count;

         // Walk through each curve (after the first) 
         // to match up the curves in order

         for (int i = 0; i < n; ++i)
         {
            Curve curve = curves[i];
            XYZ endPoint = curve.GetEndPoint(1);

            if (debugOutput)
            {
               Debug.Print("{0} endPoint {1}", i, endPoint);
            }

            XYZ p;

            // Find curve with start point = end point

            bool found = (i + 1 >= n);

            for (int j = i + 1; j < n; ++j)
            {
               p = curves[j].GetEndPoint(0);

               // If there is a match end->start, 
               // this is the next curve

               if (_sixteenth > p.DistanceTo(endPoint))
               {
                  if (debugOutput)
                  {
                     Debug.Print(
                       "{0} start point, swap with {1}",
                       j, i + 1);
                  }

                  if (i + 1 != j)
                  {
                     Curve tmp = curves[i + 1];
                     curves[i + 1] = curves[j];
                     curves[j] = tmp;
                  }
                  found = true;
                  break;
               }

               p = curves[j].GetEndPoint(1);

               // If there is a match end->end, 
               // reverse the next curve

               if (_sixteenth > p.DistanceTo(endPoint))
               {
                  if (i + 1 == j)
                  {
                     if (debugOutput)
                     {
                        Debug.Print(
                          "{0} end point, reverse {1}",
                          j, i + 1);
                     }

                     curves[i + 1] = CreateReversedCurve(curves[j]);
                  }
                  else
                  {
                     if (debugOutput)
                     {
                        Debug.Print(
                          "{0} end point, swap with reverse {1}",
                          j, i + 1);
                     }

                     Curve tmp = curves[i + 1];
                     curves[i + 1] = CreateReversedCurve(curves[j]);
                     curves[j] = tmp;
                  }
                  found = true;
                  break;
               }
            }


            if (!found)
            {
               throw new Exception("SortCurvesContiguous:"
                 + " non-contiguous input curves");
            }
         }

         return curves;
      }
   }
}
