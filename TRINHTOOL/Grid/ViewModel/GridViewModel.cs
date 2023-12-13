using Autodesk.Revit.DB;
using BimSpeedModelFromCad.Properties;
using HcBimUtils.DocumentUtils;
using HcBimUtils.GeometryUtils;
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
using TRINHTOOL.Grid.Model;
using TRINHTOOL.Model;
using TRINHTOOL.Grid.View;
using TRINHTOOL.Views;

namespace TRINHTOOL.Grid.ViewModel
{
   public class GridViewModel : ViewModelBase
   {
      public GridView MainView { get; set; }
      public XyzData CadGridOrigin { get; set; }

      public List<CadGrids> _cadGrids = new();

      public List<Level> Levels { get; set; } = new();

      public Level SelectedLevel { get; set; }

      public GridType Grid { get; set; }

      public RelayCommand SelectFromCad { get; set; }

      public RelayCommand Create { get; set; }

      public List<GridType> Grids { get; set; } = new();

      public List<GridInfoCollection> gridInfoCollections = new();
      private XYZ _origin;

      public ObservableCollection<GridInfoCollection> GridInfoCollections { get; set; } = new();


      //Constructor
      public GridViewModel()
      {
         SelectFromCad = new RelayCommand(x => SelectGridFromCad());
         Create = new RelayCommand(ModelGrid);
         GetData();
         SelectedLevel = Levels.FirstOrDefault();
         Grid = Grids.FirstOrDefault();
      }

      //SelectFromCad
      public void SelectGridFromCad()
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

            CadGridOrigin = new XyzData(arrPoint1[0], arrPoint1[1], arrPoint1[2]);

            var newset = doc.SelectionSets.Add(Guid.NewGuid().ToString());

            newset.SelectOnScreen();

            if (newset.Count <= 0)
            {
               MessageBox.Show(Resources.COMMON_MESSAGESELECTELEMENTCAD, Resources.COMMON_NOTIFY, MessageBoxButton.OK,
                   MessageBoxImage.Warning);
            }

            List<dynamic> listText = new List<dynamic>();

            List<dynamic> listLine = new List<dynamic>();


            foreach (dynamic s in newset)
            {
               if (s.EntityName == "AcDbLine")
               {
                  listLine.Add(s);
               }

               if (s.EntityName == "AcDbText")
               {
                  listText.Add(s);
               }
            }

            List<TextData> listtext = new List<TextData>();
            if (listText.Count > 0)
            {
               foreach (var text in listText)
               {
                  string[] arrtextpoint = ((IEnumerable)text.InsertionPoint).Cast<object>()
                      .Select(x => x.ToString())
                      .ToArray();

                  double[] arrtextpoint1 = new double[3];
                  int k = 0;
                  foreach (var item in arrtextpoint)
                  {
                     arrtextpoint1[k] = Convert.ToDouble(item);
                     k++;
                  }

                  listtext.Add(new TextData()
                  {
                     point = new XYZ(arrtextpoint1[0], arrtextpoint1[1], arrtextpoint1[2]),
                     text = text.TextString
                  });
               }
            }

            foreach (var line in listLine)
            {
               dynamic startpointarr = line.StartPoint;
               dynamic endpointarr = line.EndPoint;

               var startpoint = new XyzData((double)startpointarr[0], (double)startpointarr[1], 0);
               var endpoint = new XyzData((double)endpointarr[0], (double)endpointarr[1], 0);

               var vector = new XYZ(endpoint.X - startpoint.X, endpoint.Y - startpoint.Y, 0);
               if (listText.Count <= 0)
               {
                  _cadGrids.Add(new CadGrids()
                  {
                     StartPoint = startpoint,
                     EndPoint = endpoint,
                     Length = Math.Round((line.Length), 0)
                  });
               }
               else
               {
                  var normal = vector.CrossProduct(XYZ.BasisZ);
                  var plane = BPlane.CreateByNormalAndOrigin(normal, new XYZ(endpoint.X, endpoint.Y, 0));

                  var item = listtext.MinBy2(x => Math.Abs(plane.SignedDistanceToPlaneReal(x.point)));

                  _cadGrids.Add(new CadGrids()
                  {
                     StartPoint = startpoint,
                     EndPoint = endpoint,
                     Length = Math.Round((line.Length), 0),
                     Text = item.text
                  });
               }

            }
            GetGridInfoCollection();
         }

         MainView.ShowDialog();
      }

      //ModelGrid
      public void ModelGrid(object obj)
      {


         if (obj is Window w)
         {
            w.Close();
         }

         MainView.Hide();

         try
         {
            _origin = AC.Selection.PickPoint();
            _origin = new XYZ(_origin.X, _origin.Y, 0);
         }
         catch (Exception e)
         {
            MessageBox.Show(Resources.COMMON_MESSAGEPICKPOINTREVIT, Resources.COMMON_NOTIFY, MessageBoxButton.OKCancel, MessageBoxImage.Error);
         }

         if (_origin != null)
         {
            var max = _cadGrids.Count;
            var progressView = new progressbar();
            progressView.Show();

            using var tg = new TransactionGroup(AC.Document, "Model Grid");
            tg.Start();
            DeleteWarningSuper waringsuper = new DeleteWarningSuper();

            foreach (var gridInfoCollection in GridInfoCollections)
            {
               if (progressView.Flag == false)
               {
                  break;
               }


               using var tx = new Transaction(AC.Document, "Modeling Beam From Cad");
               tx.Start();

               FailureHandlingOptions failOpt = tx.GetFailureHandlingOptions();

               failOpt.SetFailuresPreprocessor(waringsuper);

               tx.SetFailureHandlingOptions(failOpt);

               var p1 = gridInfoCollection.StartPoint.Add(_origin - CadGridOrigin.ToXyz());
               var p2 = gridInfoCollection.EndPoint.Add(_origin - CadGridOrigin.ToXyz());
               var line = Line.CreateBound(p1, p2);
               var a = line.SP();
               var b = line.EP();

               a = a.EditZ(SelectedLevel.Elevation);
               b = b.EditZ(SelectedLevel.Elevation);

               var l = Line.CreateBound(a, b);
               try
               {
                  var grid = Autodesk.Revit.DB.Grid.Create(AC.Document, l);
                  var nameparam = grid.get_Parameter(BuiltInParameter.DATUM_TEXT);
                  if (gridInfoCollection.Text != null)
                  {
                     nameparam.Set(gridInfoCollection.Text);
                  }
               }
               catch
               {

               }

               AC.Document.Regenerate();
               progressView.Create(max, "BeamModel");

               tx.Commit();
            }
            tg.Assimilate();
            progressView.Close();
         }

         MainView.Show();

      }

      //GetInfoCollections
      public void GetGridInfoCollection()
      {
         gridInfoCollections.Clear();

         if (_cadGrids != null)
         {
            foreach (var grid in _cadGrids)
            {

               var collection = new GridInfoCollection
               {
                  Length = grid.Length,
                  Text = grid.Text,
                  StartPoint = grid.StartPoint.ToXyz(),
                  EndPoint = grid.EndPoint.ToXyz()
               };

               gridInfoCollections.Add(collection);
            }

            GridInfoCollections = new ObservableCollection<GridInfoCollection>(gridInfoCollections);
         }
         OnPropertyChanged(nameof(GridInfoCollections));
         OnPropertyChanged(nameof(gridInfoCollections));

      }
      //Get Data
      public void GetData()
      {
         Levels = new FilteredElementCollector(AC.Document).OfClass(typeof(Level)).Cast<Level>()
             .OrderBy(x => x.Elevation).ToList();

         Grids = new FilteredElementCollector(AC.Document).OfClass(typeof(GridType))
             .OfCategory(BuiltInCategory.OST_Grids).Cast<GridType>().ToList();
      }
      public class CadGrids
      {
         public XyzData StartPoint { get; set; }

         public XyzData EndPoint { get; set; }

         public double Length { get; set; }

         public string Text { get; set; }
      }
      //End class
   }
   public class TextData
   {
      public XYZ point;
      public string text;
   }
}
