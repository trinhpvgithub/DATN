using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using HcBimUtils.DocumentUtils;
using HcBimUtils.SelectionFilter;
using HcBimUtils;
using HcBimUtils.WPFUtils;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;
using VIEW= Autodesk.Revit.DB.View;
namespace TRINHTOOL.CreateLevel.Model
{
   public class CreateLevelModel : ViewModelBase
   {
      #region khai bao Tang
      private string _NhapsolieuTang = "2500*2";
      public string NhapsolieuTang
      {
         get { return _NhapsolieuTang; }
         set { _NhapsolieuTang = value; OnPropertyChanged(); }
      }
      public string _TuychinhtenTang = "Level";
      public string Tuychinhtentang
      {
         get { return _TuychinhtenTang; }
         set { _TuychinhtenTang = value; OnPropertyChanged(); }
      }
      public int _TuychinhsoTang = 2;
      public int Tuychinhsotang
      {
         get { return _TuychinhsoTang; }
         set { _TuychinhsoTang = value; OnPropertyChanged(); }
      }
      public bool _IsTangmacdinh;
      public bool IsTangmacdinh
      {
         get { return _IsTangmacdinh; }
         set { _IsTangmacdinh = value; OnPropertyChanged(); }
      }
      public bool _IsTangtuychinh;
      public bool IsTangtuychinh
      {
         get { return _IsTangtuychinh; }
         set { _IsTangtuychinh = value; OnPropertyChanged(); }
      }
      #endregion
      #region khai bao cao độ 
      private string _NhapsolieuCaodo = "5000";
      public string NhapsolieuCaodo
      {
         get { return _NhapsolieuCaodo; }
         set { _NhapsolieuCaodo = value; OnPropertyChanged(); }
      }
      public string _TuychinhtenCaodo = "Level";
      public string TuychinhtenCaodo
      {
         get { return _TuychinhtenCaodo; }
         set { _TuychinhtenCaodo = value; OnPropertyChanged(); }
      }
      public int _TuychinhsoCaodo = 2;
      public int TuychinhsoCaodo
      {
         get { return _TuychinhsoCaodo; }
         set { _TuychinhsoCaodo = value; OnPropertyChanged(); }
      }
      public bool _IsCaodomacdinh;
      public bool IsCaodomacdinh
      {
         get { return _IsCaodomacdinh; }
         set { _IsCaodomacdinh = value; OnPropertyChanged(); }
      }
      public bool _IsCaodotuychinh;
      public bool IsCaodotuychinh
      {
         get { return _IsCaodotuychinh; }
         set { _IsCaodotuychinh = value; OnPropertyChanged(); }
      }
      #endregion
      public CreateLevelModel()
      {
         IsTangmacdinh = true;
         IsTangtuychinh = false;

         IsCaodomacdinh = true;
         IsCaodotuychinh = false;
      }
      public void CreateTang(object obj)
      {
         var doc = AC.Document;
         var levels=new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(x=>x.Elevation)
            .ToList();
         var views= new FilteredElementCollector(AC.Document)
            .OfCategory(BuiltInCategory.OST_Views)
            .Cast<VIEW>()
            .Where(x => x is ViewSection)
            .Where(x => !x.IsTemplate)
            .Where(x => x.GetTypeId().ToElement().Name.Contains("Elevation"))
            .ToList();
         var view = doc.ActiveView;
         if(view is not ViewSection)
         {
            view = views.FirstOrDefault();
            AC.UiDoc.RequestViewChange(view);
            AC.UiDoc.RefreshActiveView();
         }   
         var window = obj as Window;
         List<double> solieu = Nhapsolieu(NhapsolieuTang, "chiều cao tầng");
         Transaction trans = new Transaction(doc, "Tạo level");
         try
         {
            if (solieu != null)
            {
               window.Close();
               trans.Start();
               var levelbase = levels.LastOrDefault();
               var curve = levelbase.GetCurvesInView(DatumExtentType.ViewSpecific, view).ElementAt(0);
               var dir = curve.Direction();
               double elevation = 0;
               for (int i = 0; i < solieu.Count; i++)
               {
                  if (i == 0)
                  {
                     elevation = (solieu[i].MmToFoot());
                     Element levelnew = doc.GetElement(levelbase.Copy(dir.X, dir.Y, elevation).First());
                     if (IsTangtuychinh == true) levelnew.Name = Tuychinhtentang + " " + Tuychinhsotang.ToString();
                     elevation = solieu[0].MmToFoot();
                  }
                  else
                  {
                     elevation += (solieu[i]).MmToFoot();
                     Level levelnew = levelbase.Copy(dir.X, dir.Y, elevation) as Level;
                     for (int j = 0; j < solieu.Count - 2; j++)
                     {
                        elevation += solieu[i].MmToFoot();
                        Level levelnew1 = levelbase.Copy(dir.X, dir.Y, elevation) as Level;
                     }
                     break;
                  }
               }
               trans.Commit();
               ExtendLevelsAndGrids(doc, view as ViewSection);
            }
         }
         catch
         {
            trans.RollBack();
         }
      }
      public void CreateCaodo(object obj)
      {
         var doc = AC.Document;
         var view = doc.ActiveView;
         var window = obj as Window;
         List<double> solieu = Nhapsolieu(NhapsolieuCaodo, "cao độ công trình");
         Transaction trans = new Transaction(doc, "Tạo level");
         try
         {
            if (solieu != null)
            {
               trans.Start();
               window.Close();
               for (int i = 0; i < solieu.Count; i++)
               {
                  double elevation = solieu[i].MmToFoot();
                  var levelnew = Level.Create(doc, elevation);
                  if (i == 0 && IsCaodotuychinh == true)
                  {
                     levelnew.Name = TuychinhtenCaodo + "" + TuychinhsoCaodo.ToString();
                  }
               }
               trans.Commit();
               ExtendLevelsAndGrids(doc, view as ViewSection);
            }
         }
         catch (Exception)
         {
            MessageBox.Show("Bạn đã nhập sai số liệu");
         }
      }
      private void ExtendLevelsAndGrids(Document document, ViewSection viewSection)
      {
         var Trans = new Transaction(document, "Căn chỉnh LV");
         Trans.Start();
         var scale = viewSection.Scale;
         Curve gridMin, gridMax;
         List<Level> datums = new FilteredElementCollector(AC.Document, viewSection.Id)
             .WhereElementIsNotElementType()
             .OfClass(typeof(Level)).Cast<Level>()
             .OrderBy(x => x.GetParameterValueByNameAsDouble(BuiltInParameter.LEVEL_ELEV))
             .ToList();
         List<Autodesk.Revit.DB.Grid> grids = new FilteredElementCollector(AC.Document, viewSection.Id)
                 .WhereElementIsNotElementType()
                 .OfClass(typeof(Autodesk.Revit.DB.Grid))
                 .Cast<Autodesk.Revit.DB.Grid>()
                 .ToList();
         if (datums.Count == 0 || grids.Count == 0) return;
         #region thiet lap cho levels
         gridMin = grids.FirstOrDefault().Curve;
         gridMax = grids.LastOrDefault().Curve;
         List<DatumPlane> datumPlanes = new List<DatumPlane>();
         foreach (DatumPlane datum in datums)
         {
            if (datum != null) datumPlanes.Add(datum);
         }
         Curve cur1 = datumPlanes[0].GetCurvesInView(DatumExtentType.ViewSpecific, viewSection).ElementAt(0);
         XYZ tempdirLevel1 = Line.CreateBound(cur1.GetIntersectPoint(gridMin), cur1.GetIntersectPoint(gridMax)).Direction;
         var p1 = cur1.GetIntersectPoint(gridMin).Add(-40.MmToFoot() * scale * tempdirLevel1);
         var p2 = cur1.GetIntersectPoint(gridMax).Add(40.MmToFoot() * scale * tempdirLevel1);
         foreach (DatumPlane datum in datumPlanes)
         {
            var temp = datum.GetCurvesInView(DatumExtentType.ViewSpecific, viewSection).ElementAt(0);
            double z = temp.GetEndPoint(0).Z;
            Curve newcurveLevel = Line.CreateBound(new XYZ(p1.X, p1.Y, z), new XYZ(p2.X, p2.Y, z));
            datum.SetDatumExtentType(DatumEnds.End1, viewSection, DatumExtentType.ViewSpecific);
            datum.SetDatumExtentType(DatumEnds.End0, viewSection, DatumExtentType.ViewSpecific);
            datum.SetCurveInView(DatumExtentType.ViewSpecific, viewSection, newcurveLevel);
            datum.ShowBubbleInView(DatumEnds.End0, viewSection);
            datum.ShowBubbleInView(DatumEnds.End1, viewSection);
         }
         Trans.Commit();
         #endregion
      }
      public List<double> Nhapsolieu(string value, string truc)
      {
         if (string.IsNullOrEmpty(value))
         {
            MessageBox.Show("Bạn chưa nhập số liệu" + " " + truc);
            return null;
         }
         string[] sps = value.Split(',');
         List<double> spaces = new List<double>();
         for (var i = 0; i < sps.Length; i++)
         {
            if (sps[i].IndexOf("*") > 0)
            {
               var a = (sps[i]).Split('*');
               if (a.Length == 2)
               {
                  var b = a[0];
                  var c = a[1];
                  for (var j = 0; j < Convert.ToDouble(c); j++)
                  {
                     spaces.Add(Convert.ToDouble(b));
                  }
               }
            }
            else
            {
               try
               {
                  spaces.Add(Convert.ToDouble(sps[i]));
               }
               catch (Exception)
               {
                  MessageBox.Show("Nhập sai số liệu");
                  return null;
               }
            }
         }
         return spaces;
      }
   }
}
