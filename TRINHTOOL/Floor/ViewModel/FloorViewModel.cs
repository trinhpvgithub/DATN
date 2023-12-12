using Autodesk.Revit.DB;
using BimSpeedModelFromCad.Properties;
using HcBimUtils.DocumentUtils;
using HcBimUtils.JsonData.ModelFromCadJson;
using HcBimUtils;
using HcBimUtils.WPFUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TRINHTOOL.Model;
using TRINHTOOL.ViewModels.SubViewModels;
using TRINHTOOL.Floor.Model;
using TRINHTOOL.Views;
using TRINHTOOL.Floor.View;

namespace TRINHTOOL.Floor.ViewModel
{
   public class FloorViewModel:ViewModelBase
   {
      public FloorView MainView { get; set; }
      public ObservableCollection<FloorInfoCollection> FloorInfoCollections { get; set; } = new();

      public List<FloorInfoCollection> floorInfoCollections = new();

      public XyzData CadFloorOrigin { get; set; }
      public int Offset { get; set; } = -50;

      public List<List<XyzData>> ListPoint = new();

      public XYZ Origin;

      public List<cadFloor> cadFloors = new();

      public FloorType SelectedFloorType { get; set; }
      public List<FloorType> FloorType { get; set; }
      public List<Level> Levels { get; set; } = new();

      public Level SelectedLevel { get; set; }

      public RelayCommand Create { get; set; }

      public RelayCommand SelectFromCad { get; set; }
      //Constructor
      public FloorViewModel()
      {
         SelectFromCad = new RelayCommand(x => SelectFloorFromCad());
         Create = new RelayCommand(ModelFloor);
         GetData();
         SelectedLevel = Levels.FirstOrDefault();
         FloorType = new FilteredElementCollector(AC.Document).OfCategory(BuiltInCategory.OST_Floors)
             .OfClass(typeof(FloorType)).Cast<FloorType>().ToList();

         SelectedFloorType = FloorType.FirstOrDefault();
      }

      //Select Floor From cad
      public void SelectFloorFromCad()
      {
         //_pileInfoCollection.Clear();
         MainView.Hide();
         dynamic a = Marshal.GetActiveObject("AutoCaD.Application");

         dynamic doc
             = a.Documents.Application.ActiveDocument;

         string[] arrPoint = null;
         try
         {
            var pointCad = doc.Utility.GetPoint(Type.Missing, "Select point: ");
            arrPoint = ((IEnumerable)pointCad).Cast<object>()
                .Select(x => x.ToString())
                .ToArray();
         }
         catch (Exception e)
         {
            MessageBox.Show(Resources.COMMON_MESSAGEPICKPOINTCAD, Resources.COMMON_NOTIFY, MessageBoxButton.OKCancel, MessageBoxImage.Error);
         }

         if (arrPoint != null)
         {
            double[] arrPoint1 = new double[3];

            int i = 0;

            foreach (var item in arrPoint)
            {
               arrPoint1[i] = Convert.ToDouble(item);
               i++;
            }

            CadFloorOrigin = new XyzData(arrPoint1[0], arrPoint1[1], arrPoint1[2]);
            var newset = doc.SelectionSets.Add(Guid.NewGuid().ToString());

            newset.SelectOnScreen();

            List<dynamic> listText = new List<dynamic>();

            List<dynamic> listPolylines = new List<dynamic>();

            foreach (dynamic s in newset)
            {
               if (s.EntityName == "AcDbPolyline")
               {
                  listPolylines.Add(s);
               }
               if (s.EntityName == "AcDbText")
               {
                  listText.Add(s);
               }
            }

            foreach (var polyline in listPolylines)
            {
               dynamic c = polyline.Coordinates;
               var ct = Enumerable.Count(c) / 2;
               var slabPoints = new List<XyzData>();
               for (int j = 0; j < ct; j++)
               {
                  dynamic pointarr = polyline.Coordinate[j];
                  var point = new XyzData((double)pointarr[0], (double)pointarr[1], 0);
                  slabPoints.Add(point);
               }
               ListPoint.Add(slabPoints);


               var collection = new FloorInfoCollection
               {
                  Area = Math.Round(polyline.Area / 1000000, 1)
               };

               floorInfoCollections.Add(collection);
            }

            FloorInfoCollections = new ObservableCollection<FloorInfoCollection>(floorInfoCollections);

            OnPropertyChanged(nameof(FloorInfoCollections));
         }
         cadFloors.Add(new cadFloor()
         {
            Points = new List<List<XyzData>>(ListPoint),
            Origin = CadFloorOrigin,
            SelectedLevel = SelectedLevel
         });

         ListPoint.Clear();
         MainView.ShowDialog();
      }

      //Model Floor
      public void ModelFloor(object obj)
      {
         MainView.Hide();
         if (obj is Window w)
         {
            w.Close();
         }


         try
         {
            Origin = AC.Selection.PickPoint();
         }
         catch (Exception e)
         {
            MessageBox.Show(Resources.COMMON_MESSAGEPICKPOINTREVIT, Resources.COMMON_NOTIFY,
                MessageBoxButton.OKCancel, MessageBoxImage.Error);
         }

         if (Origin != null)
         {
            var max = FloorInfoCollections.Count;
            Origin = new XYZ(Origin.X, Origin.Y, 0);
            var progressView = new progressbar();
            progressView.Show();

            using var tg = new TransactionGroup(AC.Document, "Model Floor");
            tg.Start();

            DeleteWarningSuper waringsuper = new DeleteWarningSuper();

            foreach (var info in cadFloors)
            {
               foreach (var listpoint in info.Points)
               {
                  if (progressView.Flag == false)
                  {
                     break;
                  }

                  using (var tx = new Transaction(AC.Document, "Modeling Column From Cad"))
                  {
                     tx.Start();
                     FailureHandlingOptions failOpt = tx.GetFailureHandlingOptions();

                     failOpt.SetFailuresPreprocessor(waringsuper);

                     tx.SetFailureHandlingOptions(failOpt);

                     CurveArray curvearr = new CurveArray();

                     for (int i = 0; i < listpoint.Count - 1; i++)
                     {
                        var p1 = listpoint[i].ToXyz().Add(Origin - info.Origin.ToXyz());
                        var p2 = listpoint[i + 1].ToXyz().Add(Origin - info.Origin.ToXyz());

                        p1 = p1.EditZ(info.SelectedLevel.Elevation);
                        p2 = p2.EditZ(info.SelectedLevel.Elevation);

                        var l = Line.CreateBound(p1, p2);

                        Curve curve = l;

                        curvearr.Append(curve);
                     }

                     var pe = listpoint[listpoint.Count - 1].ToXyz().Add(Origin - info.Origin.ToXyz()).EditZ(info.SelectedLevel.Elevation);
                     var pt = listpoint[0].ToXyz().Add(Origin - info.Origin.ToXyz()).EditZ(info.SelectedLevel.Elevation);
                     Curve cv = Line.CreateBound(pe, pt);

                     curvearr.Append(cv);
                     try
                     {
                        Autodesk.Revit.DB.Floor floor;
#if R23 || R22
                         var cl = new CurveLoop();
                         curvearr.ToCurves().ForEach(x => cl.Append(x));
                         floor = Floor.Create(AC.Document, new List<CurveLoop>() { cl }, SelectedFloorType.Id, SelectedLevel.Id);
#else
                        floor = AC.Document.Create.NewFloor(curvearr, SelectedFloorType, info.SelectedLevel, true);
#endif

                        var offsetParam = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);

                        offsetParam.Set(Offset.MmToFoot());

                     }
                     catch
                     {

                     }

                     progressView.Create(max, "FloorModel");
                     tx.Commit();
                  }
               }

            }

            tg.Assimilate();
            progressView.Close();
         }

         MainView.ShowDialog();
      }

      //GetInfoCollection
      public void GetFloorInfoCollections()
      {

      }

      //Getdata
      public void GetData()
      {
         //Level
         Levels = new FilteredElementCollector(AC.Document).OfClass(typeof(Level)).Cast<Level>()
             .OrderBy(x => x.Elevation).ToList();
      }
      //End class
   }
   public class cadFloor
   {
      public List<List<XyzData>> Points { get; set; }

      public XyzData Origin { get; set; }

      public Level SelectedLevel { get; set; }
   }
}
