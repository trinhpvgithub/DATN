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
using TRINHTOOL.Column.Model;
using TRINHTOOL.Views;
using TRINHTOOL.Model;
using HcBimUtils.MoreLinq;
using TRINHTOOL.Column.View;

namespace TRINHTOOL.Column.ViewModel
{
   public class ColumnViewModel: ViewModelBase
   {
      public ColumnView MainView {  get; set; }
      public XyzData cadOrigin;

      public XYZ TextPoint;

      public List<CadRectangle> cadRectangles = new();

      private Level _selectedBaseLevel;

      private Level _selectedTopLevel;

      public XYZ Origin;
      public XYZ _origin;

      private List<Family> _families;

      private readonly List<ColumnInfoCollection> columnInfoCollections = new();
      private List<string> _selectedFamilyParameters;
      private Family _familySelected;

      public ObservableCollection<ColumnInfoCollection> ColumnInfoCollections { get; set; } = new();

      public List<Level> Levels { get; set; } = new();

      public List<Family> Families
      {
         get => _families;

         set
         {
            _families = value;
            OnPropertyChanged();
         }

      }

      public Family FamilySelected
      {
         get => _familySelected;
         set
         {
            _familySelected = value;
            if (FamilySelected != null)
            {
               var first = FamilySelected.GetFamilySymbolIds().FirstOrDefault().ToElement() as FamilySymbol;
               SelectedFamilyParameters = first.GetOrderedParameters().Where(x => x.StorageType == StorageType.Double).Select(x => x.Definition.Name)
                   .ToList();

               SelectedWidthParameter = SelectedFamilyParameters.FirstOrDefault(x => x.Contains("b") || x.Contains("width"));
               SelectedHeightParameter = SelectedFamilyParameters.FirstOrDefault(x => x.Contains("h") || x.Contains("height"));

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

      public string SelectedWidthParameter { get; set; }
      public string SelectedHeightParameter { get; set; }

      public List<string> SelectedFamilyParameters
      {
         get => _selectedFamilyParameters;
         set
         {
            _selectedFamilyParameters = value;
            OnPropertyChanged();
         }
      }

      public Level SelectedBaseLevel
      {
         set
         {
            _selectedBaseLevel = value;
            //var index = Levels.IndexOf(_selectedBaseLevel);
            //_selectedTopLevel = Levels.Count > 1 ? Levels[index + 1] : Levels[index];
            OnPropertyChanged(nameof(SelectedBaseLevel));
         }
         get => _selectedBaseLevel;
      }

      public Level SelectedTopLevel
      {
         set
         {
            _selectedTopLevel = value;
            //var index = Levels.IndexOf(_selectedTopLevel);
            //_selectedBaseLevel = Levels.Count > 1 ? Levels[index - 1] : Levels[index];
            OnPropertyChanged(nameof(SelectedTopLevel));
         }
         get => _selectedTopLevel;
      }

      public int BaseOffset { get; set; } = -50;

      public int TopOffset { get; set; } = -50;

      public RelayCommand SelectFromCad { get; set; }

      public RelayCommand Create { get; set; }

      public ColumnViewModel()
      {
         SelectFromCad = new RelayCommand(x => SelectFormCad());
         Create = new RelayCommand(x => ModelColumn());
         GetData();
         FamilySelected = _families.FirstOrDefault();
      }

      public void SelectFormCad()
      {
         MainView.Hide();
         dynamic a = Marshal.GetActiveObject("AutoCaD.Application");

         dynamic doc
             = a.Documents.Application.ActiveDocument;

         string[] arr = null;
         try
         {
            var pointCad = doc.Utility.GetPoint(Type.Missing, "Select point: ");
            arr = ((IEnumerable)pointCad).Cast<object>()
                .Select(x => x.ToString())
                .ToArray();
         }
         catch (Exception e)
         {
            MessageBox.Show(Resources.COMMON_MESSAGEPICKPOINTCAD, Resources.COMMON_NOTIFY, MessageBoxButton.OKCancel, MessageBoxImage.Error);
         }

         if (arr != null)
         {
            double[] arr1 = new double[3];
            int i = 0;
            foreach (var item in arr)
            {
               arr1[i] = Convert.ToDouble(item);
               i++;
            }

            cadOrigin = new XyzData(arr1[0], arr1[1], arr1[2]);
            var newset = doc.SelectionSets.Add(Guid.NewGuid().ToString());

            newset.SelectOnScreen();

            if (newset.Count <= 0)
            {
               MessageBox.Show(Resources.COMMON_MESSAGESELECTELEMENTCAD, Resources.COMMON_NOTIFY, MessageBoxButton.OK,
                   MessageBoxImage.Warning);
            }

            List<dynamic> listText = new List<dynamic>();

            List<dynamic> listPolyline = new List<dynamic>();

            foreach (dynamic s in newset)
            {
               if (s.EntityName == "AcDbPolyline")
               {
                  listPolyline.Add(s);
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

                  listpoint.Add(new TextData()
                  {
                     point = new XYZ(arrtextpoint1[0], arrtextpoint1[1], arrtextpoint1[2]),
                     text = text.TextString
                  });
               }
            }
            else
            {
               listpoint = null;
            }

            foreach (var polyline in listPolyline)
            {

               dynamic c = polyline.Coordinates;
               if (c.Length == 8)
               {
                  dynamic pointArray1 = polyline.Coordinate[0];
                  dynamic pointArray2 = polyline.Coordinate[1];
                  dynamic pointArray3 = polyline.Coordinate[2];
                  dynamic pointArray4 = polyline.Coordinate[3];
                  var point1 = new XyzData((double)pointArray1[0], (double)pointArray1[1], 0);
                  var point2 = new XyzData((double)pointArray2[0], (double)pointArray2[1], 0);
                  var point3 = new XyzData((double)pointArray3[0], (double)pointArray3[1], 0);
                  var point4 = new XyzData((double)pointArray4[0], (double)pointArray4[1], 0);
                  if (listText.Count > 0)
                  {
                     TextData textData = listpoint.MinBy2(x => x.point.ToXyzfit().DistanceTo(point1.ToXyz()));

                     cadRectangles.Add(new CadRectangle()
                     {
                        P1 = point1,
                        P2 = point2,
                        P3 = point3,
                        P4 = point4,
                        Mask = textData.text
                     });
                  }
                  else
                  {
                     cadRectangles.Add(new CadRectangle()
                     {
                        P1 = point1,
                        P2 = point2,
                        P3 = point3,
                        P4 = point4,
                        Mask = ""
                     });
                  }
               }
            }
            GetColumnInfoCollections();
            OnPropertyChanged(nameof(ColumnInfoCollections));
         }
         MainView.ShowDialog();
      }

      public void GetData()
      {
         //Level
         Levels = new FilteredElementCollector(AC.Document)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(x => x.Elevation)
            .ToList();

         if (Levels.Count > 1)
         {
            _selectedBaseLevel = Levels[0];
            _selectedTopLevel = Levels[1];
         }
         else
         {
            _selectedBaseLevel = Levels.FirstOrDefault();
            _selectedTopLevel = Levels.FirstOrDefault();
         }


         //Get family
         Families = new FilteredElementCollector(AC.Document).OfCategory(BuiltInCategory.OST_StructuralColumns).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Select(x => x.Family).Where(x => x.StructuralMaterialType != StructuralMaterialType.Steel).DistinctBy(x => x.Id).OrderBy(x => x.Name).ToList();
      }

      public void GetColumnInfoCollections()
      {
         columnInfoCollections.Clear();

         var dic = new Dictionary<ColumnInfo, List<ColumnInfo>>();

         if (cadRectangles != null)
         {

            foreach (var cadRectangle in cadRectangles)
            {
               var points = cadRectangle.Points.Select(x => x.ToXyz()).ToList();
               if (points.Count == 4)
               {
                  var columnInfo = new ColumnInfo(points, cadRectangle.Mask);
                  if (dic.ContainsKey(columnInfo))
                  {
                     dic[columnInfo].Add(columnInfo);
                  }
                  else
                  {
                     dic.Add(columnInfo, new List<ColumnInfo> { columnInfo });
                  }
               }
            }

            foreach (var pair in dic)
            {
               if ((pair.Key.Height / pair.Key.Width) < 5)
               {
                  var collection = new ColumnInfoCollection
                  {
                     Width = pair.Key.Width,
                     Height = pair.Key.Height,
                     Number = pair.Value.Count,
                     Text = pair.Key.Text
                  };

                  var b = pair.Value.ToList().Distinct(new ColumnInfoComparerByPoint()).ToList();
                  collection.ColumnInfos = b;
                  columnInfoCollections.Add(collection);
               }
            }
         }
         ColumnInfoCollections = new ObservableCollection<ColumnInfoCollection>(columnInfoCollections);
      }

      public void ModelColumn()
      {
         MainView.Hide();
         try
         {
            _origin = AC.Selection.PickPoint();
         }
         catch (Exception e)
         {
            MessageBox.Show(Resources.COMMON_MESSAGEPICKPOINTREVIT, Resources.COMMON_NOTIFY, MessageBoxButton.OKCancel, MessageBoxImage.Error);
         }

         if (_origin != null)
         {
            _origin = new XYZ(_origin.X, _origin.Y, 0);
            var max = ColumnInfoCollections.Select(x => x.ColumnInfos.Count).Sum();
            var progressView = new progressbar();
            progressView.Show();
            using (var tg = new TransactionGroup(AC.Document, "Model Columns"))
            {
               tg.Start();

               DeleteWarningSuper waringsuper = new DeleteWarningSuper();

               foreach (var columnInfoCollection in ColumnInfoCollections)
               {
                  if (progressView.Flag == false)
                  {
                     break;
                  }
                  var width = Convert.ToInt32(columnInfoCollection.Width);
                  var height = Convert.ToInt32(columnInfoCollection.Height);


                  var elementType = columnInfoCollection.ElementType = GetElementType(width, height);

                  if (elementType == null)
                  {
                     continue;
                  }

                  foreach (var columnInfo in columnInfoCollection.ColumnInfos)
                  {
                     using var tx = new Transaction(AC.Document, "Modeling Column From Cad");
                     tx.Start();
                     FailureHandlingOptions failOpt = tx.GetFailureHandlingOptions();

                     failOpt.SetFailuresPreprocessor(waringsuper);

                     tx.SetFailureHandlingOptions(failOpt);

                     var center = columnInfo.Center.Add(_origin - cadOrigin.ToXyz());

                     var fs = columnInfoCollection.ElementType as FamilySymbol;
                     if (fs.IsActive == false)
                     {
                        fs.Activate();
                     }

                     try
                     {
                        var column = AC.Document.Create.NewFamilyInstance(center, fs,
                            SelectedBaseLevel, StructuralType.Column);

                        AC.Document.Regenerate();
                        var topLevelParam = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
                        topLevelParam.Set(SelectedTopLevel.Id);
                        var topOffsetParam = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
                        var baseOffsetParam = column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
                        var mark = column.SetParameterValueByName(BuiltInParameter.ALL_MODEL_MARK, columnInfoCollection.Text);

                        topOffsetParam.Set(TopOffset.MmToFoot());
                        baseOffsetParam.Set(BaseOffset.MmToFoot());

                        var rotateAxis = center.CreateLineByPointAndDirection(XYZ.BasisZ);
                        ElementTransformUtils.RotateElement(AC.Document, column.Id, rotateAxis, columnInfo.Rotation);
                        progressView.Create(max, "ColumnModel");
                     }
                     catch
                     {

                     }

                     tx.Commit();
                  }
               }
               tg.Assimilate();
               progressView.Close();
            }
         }
         //ParentViewModel.ModelFromCadMainView.ShowDialog();
      }

      private ElementType GetElementType(int width, int height)
      {
         ElementType elementType = null;

         using (var tx = new Transaction(AC.Document, "Duplicate Type"))
         {
            tx.Start();
            //update element in revit
            AC.Document.Regenerate();

            var columnTypes = FamilySelected.GetFamilySymbolIds().Select(x => x.ToElement())
                .Cast<FamilySymbol>().ToList();

            foreach (var familySymbol in columnTypes)
            {
               var bParam = familySymbol.LookupParameter(SelectedWidthParameter);
               var hParam = familySymbol.LookupParameter(SelectedHeightParameter);
               var bInMM = Convert.ToInt32(bParam.AsDouble().FootToMm());
               var hInMM = Convert.ToInt32(hParam.AsDouble().FootToMm());

               if (width == bInMM && height == hInMM)
               {
                  elementType = familySymbol;
               }
            }

            if (elementType == null)
            {
               //Duplicate Column Type
               var type = columnTypes.FirstOrDefault();

               var newTypeName = "Column" + "_" + width + "x" + height;

               if (columnTypes.Select(x => x.Name).Contains(newTypeName))
               {
                  newTypeName = newTypeName + " Ignore existed name";
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
                     newTypeName += ".";
                  }
               }
               if (elementType != null)
               {
                  elementType.LookupParameter(SelectedWidthParameter).Set(width.MmToFoot());
                  elementType.LookupParameter(SelectedHeightParameter).Set(height.MmToFoot());
               }
            }

            tx.Commit();
         }
         return elementType;
      }
   }
   public class CadRectangle
   {
      public XyzData P1 { get; set; }
      public XyzData P2 { get; set; }
      public XyzData P3 { get; set; }
      public XyzData P4 { get; set; }

      public string Mask;

      public List<XyzData> Points => new() { P1, P2, P3, P4 };

   }

   public class TextData
   {
      public XYZ point;
      public string text;
   }

}
