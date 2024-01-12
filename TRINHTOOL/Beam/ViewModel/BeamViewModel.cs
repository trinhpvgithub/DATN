using Autodesk.Revit.DB.Structure;
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
using TRINHTOOL.TrinhUtils;
using TRINHTOOL.ViewModels;
using TRINHTOOL.Beam.Model;
using HcBimUtils.MoreLinq;
using TRINHTOOL.Views;
using TRINHTOOL.Beam.View;
using System.Security.AccessControl;

namespace TRINHTOOL.Beam.ViewModel
{
   public class BeamViewModel : ViewModelBase
   {
      public BeamView MainView { get; set; }
      public XyzData CadBeamOrigin;

      private List<Family> _families;

      private Family _selectedFamily;

      public XYZ Origin;

      private readonly List<CadBeams> _cadBeams = new();

      private readonly List<BeamInfoCollection> _beamInfoCollections = new();

      public ObservableCollection<BeamInfoCollection> BeamInfoCollections { get; set; } = new();

      private List<string> _selectedFamilyParameters;
      private Level _selectedLevel;

      public List<Level> Levels { get; set; } = new();

      public List<string> SelectedFamilyParameters
      {
         get => _selectedFamilyParameters;
         set
         {
            _selectedFamilyParameters = value;
            OnPropertyChanged();
         }
      }
      public string SelectedWidthParameter { get; set; }
      public string SelectedHeightParameter { get; set; }

      public List<Family> Families
      {
         get => _families;

         set
         {
            _families = value;
            OnPropertyChanged();
         }

      }
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
      public Family FamilySelected
      {
         get => _selectedFamily;
         set
         {
            _selectedFamily = value;
            if (FamilySelected != null)
            {
               var first = FamilySelected.GetFamilySymbolIds().FirstOrDefault().ToElement() as FamilySymbol;
               SelectedFamilyParameters = first.GetOrderedParameters().Where(x => x.StorageType == StorageType.Double).Select(x => x.Definition.Name)
                   .ToList();

               SelectedWidthParameter = SelectedFamilyParameters.FirstOrDefault(x => x.Contains("b"));
               SelectedHeightParameter = SelectedFamilyParameters.FirstOrDefault(x => x.Contains("h"));

               if (string.IsNullOrEmpty(SelectedWidthParameter))
               {
                  SelectedWidthParameter = SelectedFamilyParameters.FirstOrDefault();
               }
               if (string.IsNullOrEmpty(SelectedHeightParameter))
               {
                  SelectedHeightParameter = SelectedFamilyParameters.LastOrDefault();
               }
            }

            OnPropertyChanged(nameof(SelectedWidthParameter));
            OnPropertyChanged(nameof(SelectedHeightParameter));
         }
      }

      public Level SelectedLevel
      {
         get => _selectedLevel;
         set
         {
            _selectedLevel = value;
            OnPropertyChanged();
         }
      }

      public RelayCommand SelectFromCad { get; set; }

      public RelayCommand Create { get; set; }
      public RelayCommand CanCel{ get; set; }
      public RelayCommand PointRevit{ get; set; }

      //Constructor
      public BeamViewModel()
      {
         GetLayer();
         GetData();
         SelectFromCad = new RelayCommand(x => SelectBeam());
         Create = new RelayCommand(x=>ModelBeams(AC.Selection.PickPoint()));
         CanCel = new RelayCommand(x=>CC());
         PointRevit = new RelayCommand(x => ModelBeams(new XYZ()));
         SelectedLevel = Levels.FirstOrDefault();
         FamilySelected = _families.FirstOrDefault();
         SelectedLayer=Layers.FirstOrDefault();
      }

      //Select BeamFromCad

      public void GetLayer()
      {
         dynamic a = Marshal.GetActiveObject("AutoCaD.Application");
         dynamic doc = a.Documents.Application.ActiveDocument;
         var layers = doc.Layers;
         List<string> layerss= new List<string>();
         for (int i = 0; i < layers.Count; i++)
         {
            var item = layers[i];
            layerss.Add(item.Name) ;
         }
         Layers=layerss;
      }
      public void SelectBeam()
      {
         //beamInfoCollections.Clear();
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
         catch (Exception )
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

            CadBeamOrigin = new XyzData(arrPoint1[0], arrPoint1[1], arrPoint1[2]);
            var newset = doc.SelectionSets.Add(Guid.NewGuid().ToString());
            newset.SelectOnScreen();
            if (newset.Count <= 0)
            {
               MessageBox.Show(Resources.COMMON_MESSAGESELECTELEMENTCAD, Resources.COMMON_NOTIFY, MessageBoxButton.OK,
                   MessageBoxImage.Warning);
            }
            List<dynamic> listText = new List<dynamic>();

            List<dynamic> listLine = new List<dynamic>();

#if true //Tét
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
#endif
            //cot tron
            foreach (dynamic s in ob)
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

            List<TextData> listpoint = new List<TextData>();
            if (listText.Count > 0)
            {
               foreach (var text in listText)
               { // get text coordinate
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

                  listpoint.Add(new TextData()
                  {
                     point = new XYZ(arrtextpoint1[0], arrtextpoint1[1], arrtextpoint1[2]),
                     text = text.TextString
                  });
               }
            }

            if (listLine.Count > 0)
            {
               foreach (var line in listLine)
               {
                  dynamic startpointarr = line.StartPoint;
                  dynamic endpointarr = line.EndPoint;
                  // get the start point and end of each line beam in cad
                  var startpoint = new XyzData((double)startpointarr[0], (double)startpointarr[1], 0);
                  var endpoint = new XyzData((double)endpointarr[0], (double)endpointarr[1], 0);
                  var line1 = startpoint.ToXyz().CreateLine(endpoint.ToXyz());
                  //TextData textData = listpoint.MinBy2(x => x.point.ToXyzfit().DistanceTo(startpoint.ToXyz()));
                  TextData textData = listpoint.MinBy2(x => Utils.DistancePoint2Line2(x.point.ToXyzfit(), line1));
                  var text = Utils.Split(textData.text)[1];
                  var mark = Utils.Split(textData.text)[0];
                  _cadBeams.Add(new CadBeams()
                  {
                     StartPoint = startpoint,
                     EndPoint = endpoint,
                     Text = textData.text,
                     Mark = mark,
                     OriginPoint = CadBeamOrigin
                  });
               }
            }
            GetBeamInfoCollection();
            OnPropertyChanged(nameof(BeamInfoCollections));
         }
         MainView.ShowDialog();
         MainView.Focus();
      }
      public void CC()
      {
         MainView?.Close();
      }

      //Model Beam
      public void ModelBeams(XYZ Point)
      {
         MainView.Hide();
         try
         {
            Origin = Point;
         }
         catch (Exception )
         {
            MessageBox.Show(Resources.COMMON_MESSAGEPICKPOINTREVIT, Resources.COMMON_NOTIFY, MessageBoxButton.OKCancel, MessageBoxImage.Error);
         }

         if (Origin != null)
         {
            Origin = new XYZ(Origin.X, Origin.Y, 0);
            var max = BeamInfoCollections.Select(x => x.BeamInfos.Count).Sum();
            var progressView = new progressbar();
            progressView.Show();

            using var tg = new TransactionGroup(AC.Document, "Model Beams");
            tg.Start();
            DeleteWarningSuper waringsuper = new DeleteWarningSuper();
            foreach (var beamInfoCollection in BeamInfoCollections)
            {
               if (progressView.Flag == false)
               {
                  break;
               }
               var height = Convert.ToInt32(beamInfoCollection.Height);
               var width = Convert.ToInt32(beamInfoCollection.Width);

               var elementType = beamInfoCollection.ElementType = GetElementType(width, height);

               if (elementType == null)
               {
                  continue;
               }

               foreach (var beamInfo in beamInfoCollection.BeamInfos)
               {
                  using var tx = new Transaction(AC.Document, "Modeling Beam From Cad");
                  tx.Start();

                  FailureHandlingOptions failOpt = tx.GetFailureHandlingOptions();

                  failOpt.SetFailuresPreprocessor(waringsuper);

                  tx.SetFailureHandlingOptions(failOpt);

                  var fs = beamInfoCollection.ElementType as FamilySymbol;
                  if (fs.IsActive == false)
                  {
                     fs.Activate();
                  }
                  var p1 = beamInfo.StartPoint.Add(Origin - beamInfo.OriginPoint);
                  var p2 = beamInfo.EndPoint.Add(Origin - beamInfo.OriginPoint);
                  var line = Line.CreateBound(p1, p2);
                  var a = line.SP();
                  var b = line.EP();

                  a = a.EditZ(SelectedLevel.Elevation);
                  b = b.EditZ(SelectedLevel.Elevation);

                  var l = Line.CreateBound(a, b);
                  try
                  {
                     var beam = AC.Document.Create.NewFamilyInstance(l, fs, SelectedLevel,
                         StructuralType.Beam);

                     var mark = beam.SetParameterValueByName(BuiltInParameter.ALL_MODEL_MARK, beamInfoCollection.Mark);
                  }
                  catch
                  {

                  }

                  AC.Document.Regenerate();
                  progressView.Create(max, "BeamModel");

                  tx.Commit();
               }
            }
            tg.Assimilate();
            progressView.Close();
         }
      }

      //GetBeamInfoCollections
      public void GetBeamInfoCollection()
      {
         _beamInfoCollections.Clear();
         var dic = new Dictionary<string, List<BeamInfo>>();

         if (_cadBeams != null)
         {

            foreach (var cadbeam in _cadBeams)
            {
               var text = cadbeam.Text;

               var beamInfo = new BeamInfo(cadbeam.OriginPoint.ToXyz(), cadbeam.StartPoint.ToXyz(), cadbeam.EndPoint.ToXyz(), cadbeam.Text);
               if (dic.ContainsKey(text))
               {
                  dic[text].Add(beamInfo);
               }
               else
               {
                  dic.Add(text, new List<BeamInfo> { beamInfo });
               }
            }

            foreach (var pair in dic)
            {
               var collection = new BeamInfoCollection
               {
                  Text = pair.Key,
                  Width = pair.Value.Select(x => x.Width).FirstOrDefault(),
                  Height = pair.Value.Select(x => x.Height).FirstOrDefault(),
                  Number = pair.Value.Count,
                  Mark = Utils.Split(pair.Key).Count() > 1 ? Utils.Split(pair.Key).First() : "",
                  Level = SelectedLevel
               };
               var b = pair.Value.ToList().Distinct(new BeamInfo.BeamInfoComparerByPoint()).ToList();
               collection.BeamInfos = b;
               _beamInfoCollections.Add(collection);
            }
         }
         BeamInfoCollections.Clear();
         OnPropertyChanged(nameof(BeamInfoCollections));
         BeamInfoCollections = new ObservableCollection<BeamInfoCollection>(_beamInfoCollections);
      }

      //GetElementType
      private ElementType GetElementType(int width, int heigt)
      {
         ElementType elementType = null;

         using var tx = new Transaction(AC.Document, "Duplicate Type");
         tx.Start();
         //update element in revit
         AC.Document.Regenerate();

         var beamTypes = FamilySelected.GetFamilySymbolIds().Select(x => x.ToElement())
             .Cast<FamilySymbol>().ToList();

         foreach (var familySymbol in beamTypes)
         {

            var bParameter = familySymbol.LookupParameter(SelectedWidthParameter);

            var binMm = Convert.ToInt32(bParameter.AsDouble().FootToMm());

            var hParameter = familySymbol.LookupParameter(SelectedHeightParameter);

            var hinMm = Convert.ToInt32(hParameter.AsDouble().FootToMm());

            if (heigt == hinMm && width == binMm)
            {
               elementType = familySymbol;
            }
         }

         if (elementType == null)
         {
            //Duplicate Beam Type
            var type = beamTypes.FirstOrDefault();

            var newTypeName = width + "x" + heigt + "mm";

            var i = 1;
            if (beamTypes.Select(x => x.Name).Contains(newTypeName))
            {
               newTypeName = $"{newTypeName} (1)";
            }

            while (true)
            {
               try
               {
                  elementType = type?.Duplicate(newTypeName);
                  break;
               }
               catch
               {
                  i++;
                  newTypeName = $"{newTypeName} ({i})";
               }
            }

            if (elementType != null)
            {
               elementType.LookupParameter(SelectedWidthParameter).Set(width.MmToFoot());
               elementType.LookupParameter(SelectedHeightParameter).Set(heigt.MmToFoot());
            }
         }

         tx.Commit();
         return elementType;
      }

      //GetData
      public void GetData()
      {
         //Level
         Levels = new FilteredElementCollector(AC.Document).OfClass(typeof(Level)).Cast<Level>()
             .OrderBy(x => x.Elevation).ToList();
         //Family
         Families = new FilteredElementCollector(AC.Document).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Select(x => x.Family).Where(x => x.StructuralMaterialType != StructuralMaterialType.Steel).DistinctBy(x => x.Id).OrderBy(x => x.Name).ToList();
      }
      //end class
   }
   public class CadBeams
   {
      public XyzData StartPoint { get; set; }
      public XyzData OriginPoint { get; set; }

      public XyzData EndPoint { get; set; }

      public string Text { get; set; }
      public string Mark { get; set; }
   }
   public class TextData
   {
      public XYZ point;
      public string text;
   }
}

