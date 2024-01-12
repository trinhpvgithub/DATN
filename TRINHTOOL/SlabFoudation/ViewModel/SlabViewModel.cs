using Autodesk.Revit.DB;
using BimSpeedModelFromCad.Properties;
using HcBimUtils.DocumentUtils;
using HcBimUtils;
using HcBimUtils.JsonData.ModelFromCadJson;
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
using TRINHTOOL.SlabFoudation.View;
using TRINHTOOL.SlabFoudation.Model;
using TRINHTOOL.Views;

namespace TRINHTOOL.SlabFoudation.ViewModel
{
   public class SlabViewModel : ViewModelBase
   {
      public SlabView MainView { get; set; }

      public XyzData CadSlabOrigin { get; set; }
      public int Offset { get; set; } = -50;

      public List<List<XyzData>> ListPoint = new();

      public IList<Curve> ListCurve = new List<Curve>();

      public XYZ Origin;

      public ObservableCollection<SlabInfo> SlabInfoCollections { get; set; } = new();

      public List<SlabInfo> slabInfoCollections = new();
      public const double _inch = 1.0 / 12.0;
      public const double _sixteenth = _inch / 16.0;

      public List<string> Layers { get; set; }

      private string _selectedLayer;

      public string SelectedLayer
      {
         get { return _selectedLayer; }
         set
         {
            _selectedLayer = value;
            OnPropertyChanged();
         }
      }
      public FloorType SelectedSlabType { get; set; }
      public List<FloorType> SlabType { get; set; }
      public List<Level> Levels { get; set; } = new();

      public Level SelectedLevel { get; set; }

      public RelayCommand Create { get; set; }
      public RelayCommand Cancel { get; set; }
      public RelayCommand PointRevit { get; set; }

      public RelayCommand SelectFromCad { get; set; }

      //Constructor
      public SlabViewModel()
      {
         SelectFromCad = new RelayCommand(x => SelectFloorFromCad());
         Create = new RelayCommand(x => ModelSlab(AC.Selection.PickPoint()));
         PointRevit = new RelayCommand(x => ModelSlab(new XYZ()));
         Cancel = new RelayCommand(x => Cancell());
         GetData();
         SelectedLevel = Levels.FirstOrDefault();
         GetLayer();
         SelectedLayer = Layers.FirstOrDefault();
         SelectedSlabType = SlabType.FirstOrDefault();
      }
      public void Cancell()
      {
         MainView?.Close();
      }
      public void GetLayer()
      {
         dynamic a = Marshal.GetActiveObject("AutoCaD.Application");
         dynamic doc = a.Documents.Application.ActiveDocument;
         var layers = doc.Layers;
         List<string> layerss = new List<string>();
         for (int i = 0; i < layers.Count; i++)
         {
            var item = layers[i];
            layerss.Add(item.Name);
         }
         Layers = layerss;
      }

      //Select Floor From cad
      public void SelectFloorFromCad()
      {
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
            MainView.ShowDialog();
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

            CadSlabOrigin = new XyzData(arrPoint1[0], arrPoint1[1], arrPoint1[2]);
            var newset = doc.SelectionSets.Add(Guid.NewGuid().ToString());

            newset.SelectOnScreen();

            if (newset.Count <= 0)
            {
               MessageBox.Show(Resources.COMMON_MESSAGESELECTELEMENTCAD, Resources.COMMON_NOTIFY, MessageBoxButton.OK,
                   MessageBoxImage.Warning);
            }

            List<dynamic> listPolylines = new List<dynamic>();
            List<CadData> cadDatas = new List<CadData>();
            foreach (var l in newset)
            {
               cadDatas.Add(new CadData() { CadObject = l, LayerName = l.Layer });
            }
            var groupCadData = cadDatas.GroupBy(x => x.LayerName);
            var listLayer = new List<string>();
            foreach (var item in groupCadData)
            {
               listLayer.Add(item.Key);
            }
            Layers = listLayer;
            var list = new List<CadData>();
            foreach (var item in groupCadData)
            {
               if (item.Key == SelectedLayer)
               {
                  list = item.ToList();
               }
            }
            var ob = new List<dynamic>();
            foreach (var o in list)
            {
               ob.Add(o.CadObject);
            }
            foreach (dynamic s in newset)
            {
               if (s.EntityName == "AcDbPolyline")
               {
                  listPolylines.Add(s);
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

               var collection = new SlabInfo
               {
                  Area = Math.Round(polyline.Area / 1000000, 1)
               };

               slabInfoCollections.Add(collection);
            }

            SlabInfoCollections = new ObservableCollection<SlabInfo>(slabInfoCollections);

            OnPropertyChanged(nameof(SlabInfoCollections));
         }

         MainView.ShowDialog();
      }

      //Model Floor
      public void ModelSlab(XYZ point)
      {
         MainView.Hide();
         var max = SlabInfoCollections.Count;
         try
         {
            Origin = point;
         }
         catch (Exception e)
         {
            MessageBox.Show(Resources.COMMON_MESSAGEPICKPOINTREVIT, Resources.COMMON_NOTIFY, MessageBoxButton.OKCancel, MessageBoxImage.Error);
         }

         if (Origin != null)
         {
            Origin = new XYZ(Origin.X, Origin.Y, 0);
            var progressView = new progressbar();
            progressView.Show();

            using var tg = new TransactionGroup(AC.Document, "Model Slab");
            tg.Start();

            DeleteWarningSuper waringsuper = new DeleteWarningSuper();

            foreach (var listpoint in ListPoint)
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

                  CurveArray curveArray = new CurveArray();

                  for (int i = 0; i < listpoint.Count - 1; i++)
                  {
                     var p1 = listpoint[i].ToXyz().Add(Origin - CadSlabOrigin.ToXyz());
                     var p2 = listpoint[i + 1].ToXyz().Add(Origin - CadSlabOrigin.ToXyz());

                     p1 = p1.EditZ(SelectedLevel.Elevation);
                     p2 = p2.EditZ(SelectedLevel.Elevation);

                     var l = Line.CreateBound(p1, p2);

                     Curve curve = l;

                     curveArray.Append(curve);
                  }

                  var pe = listpoint[listpoint.Count - 1].ToXyz().Add(Origin - CadSlabOrigin.ToXyz()).EditZ(SelectedLevel.Elevation);
                  var pt = listpoint[0].ToXyz().Add(Origin - CadSlabOrigin.ToXyz()).EditZ(SelectedLevel.Elevation);
                  Curve cv = Line.CreateBound(pe, pt);

                  curveArray.Append(cv);
                  try
                  {
                     if (AC.Document != null)
                     {
                        

#if R23 || R22||R24
                        var cl = new CurveLoop();
                        curveArray.ToCurves().ForEach(x => cl.Append(x));
                        var floor =Autodesk.Revit.DB.Floor.Create(AC.Document, new List<CurveLoop>() { cl }, SelectedSlabType.Id, SelectedLevel.Id);
#else
                        var floor = AC.Document.Create.NewFoundationSlab(curveArray, SelectedSlabType, SelectedLevel,
                                                       true, XYZ.BasisZ);
#endif


                        var offsetParam = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);

                        offsetParam.Set(Offset.MmToFoot());
                     }
                  }
                  catch
                  {

                  }

                  progressView.Create(max, "FloorModel");
                  tx.Commit();
               }
            }

            tg.Assimilate();
            progressView.Close();
         }
         //ParentViewModel.ModelFromCadMainView.ShowDialog();
      }

      //Getdata
      public void GetData()
      {
         //Level
         Levels = new FilteredElementCollector(AC.Document).OfClass(typeof(Level)).Cast<Level>()
             .OrderBy(x => x.Elevation).ToList();

         SlabType = new FilteredElementCollector(AC.Document).OfClass(typeof(FloorType)).OfCategory(BuiltInCategory.OST_StructuralFoundation)
             .Cast<FloorType>().ToList();
      }
      //End class
   }
   public class cadFloor
   {
      public List<List<XyzData>> Points { get; set; }

      public XyzData Origin { get; set; }

      public Level SelectedLevel { get; set; }
   }
   public class TextDataSlab
   {
      public XYZ PointSlab;
      public string TextSlab;
   }
}
