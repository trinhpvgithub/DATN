using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HcBimUtils.DocumentUtils;
using HcBimUtils.WPFUtils;
using System.Security.AccessControl;
using TRINHTOOL.CreateSheet.Model;
using TRINHTOOL.CreateSheet.View;
using VIEW = Autodesk.Revit.DB.View;
using GRID = Autodesk.Revit.DB.Grid;
using HcBimUtils;
using Autodesk.Revit.Creation;
using System.Data.Common;

namespace TRINHTOOL.CreateSheet.ViewModel
{
   public class SheetViewModel : ViewModelBase
   {
      public SheetView MainView { get; set; }
      public List<Level> Levels { get; set; }
      private Level _selectedLevel;
      public Level SelectedLevel
      {
         get => _selectedLevel;
         set
         {
               _selectedLevel = value;
               OnPropertyChanged();
         }
      }
      public List<FamilySymbol> TitleBlocks { get; set; }
      private FamilySymbol _selectedTitleBlock;
      public FamilySymbol SelectedTitleBlock
      {
         get => _selectedTitleBlock;
         set
         {
               _selectedTitleBlock = value;
               OnPropertyChanged();
         }
      }
      public List<VIEW> ViewTemplate { get; set; }
      private VIEW _selectedViewTemplate;
      public VIEW SelectedViewTemplate
      {
         get => _selectedViewTemplate;
         set
         {
               _selectedViewTemplate = value;
               OnPropertyChanged();
         }
      }
      public List<string> Scale { get; set; }
      private string _selectedScale;
      public string SelectedScale
      {
         get => _selectedScale;
         set
         {
            _selectedScale = value;
            OnPropertyChanged();
         }
      }
      public List<VIEW> Viewplan { get; set; }
      public RelayCommand Ok { get; set; }
      public RelayCommand Cancel { get; set; }
      public SheetViewModel()
      {
         GetData();
         Ok = new RelayCommand(x => Create());
         Cancel = new RelayCommand(x => Cancell());
      }
      public void Cancell()
      {
         MainView?.Close();
      }
      public void Create()
      {
         Viewplan = new FilteredElementCollector(AC.Document)
            .OfClass(typeof(VIEW))
            .Cast<VIEW>()
            .Where(x => x is ViewPlan).ToList();
         if (MainView.cb_levels.IsChecked == true)
         {
            Viewplan.ForEach(v =>
            {
               var view = DuplicateView(v);
               Dim(view);
               SheetModel.CreateSheet(AC.Document, SelectedTitleBlock.Id, view, view.Name);
            });
         }
         else
         {
            var v=Viewplan.FirstOrDefault(x=>x.Name.Contains(SelectedLevel.Name));
            var view = DuplicateView(v);
            Dim(view);
            SheetModel.CreateSheet(AC.Document, SelectedTitleBlock.Id, view, view.Name);
         }
      }
      public void GetData()
      {
         Levels = new FilteredElementCollector(AC.Document)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(x => x.Elevation)
            .ToList();
         SelectedLevel = Levels.FirstOrDefault();

         TitleBlocks = new FilteredElementCollector(AC.Document)
            .WhereElementIsElementType()
            .OfCategory(BuiltInCategory.OST_TitleBlocks)
            .Cast<FamilySymbol>()
            .ToList();

         SelectedTitleBlock = TitleBlocks.FirstOrDefault();
         ViewTemplate = new FilteredElementCollector(AC.Document)
            .OfClass(typeof(VIEW))
            .Cast<VIEW>()
            .Where(x => x.IsTemplate).ToList();
         SelectedViewTemplate = ViewTemplate.FirstOrDefault();
         Scale = new List<string> { "1:50", "1:100", "1:150", "1:200" };
         SelectedScale = Scale.FirstOrDefault();
      }
      public VIEW DuplicateView(VIEW view)
      {
         if (view != null)
         {
            VIEW dependentView = null;
            ElementId newViewId = ElementId.InvalidElementId;
            if (view.CanViewBeDuplicated(ViewDuplicateOption.AsDependent))
            {
               var tran = new Transaction(AC.Document);
               tran.Start("dup");
               newViewId = view.Duplicate(ViewDuplicateOption.WithDetailing);
               dependentView = view.Document.GetElement(newViewId) as VIEW;
               //dependentView.Name="MB " + view.Name;
               if (SelectedViewTemplate != null)
               {
                  dependentView.GetParameter(BuiltInParameter.VIEW_TEMPLATE).Set(SelectedViewTemplate.Id);
               }
               dependentView.Scale = ViewScale(SelectedScale);
               tran.Commit();
               return dependentView;
            }
         }
         return null;
      }
      private int ViewScale(string s)
      {
         string[] chuoi = null;
         chuoi= s.Split(':');
         return int.Parse(chuoi.LastOrDefault());
      }
      public void Dim(VIEW view)
      {
         var grids = new FilteredElementCollector(AC.Document)
            .OfClass(typeof(GRID))
            .Cast<GRID>()
            .ToList();
         var gridX = grids.FirstOrDefault();
         List<GRID> grid = new List<GRID>();
         List<GRID> gridY = new List<GRID>();
         grids.ForEach(x=>
         {
            if(x.Curve.Direction().IsParallel(gridX.Curve.Direction()))
            {
               grid.Add(x);
            }   
            else gridY.Add(x);
         });
         var a1 = new ReferenceArray();
         var a2 = new ReferenceArray();
         var a3 = new ReferenceArray();
         var a4 = new ReferenceArray();
         grid.ForEach(x=>
         {
            a1.Append(new Reference(x));
         });
         gridY.ForEach(x =>
         {
            a2.Append(new Reference(x));
         });
         a3.Append(new Reference(grid.FirstOrDefault()));
         a3.Append(new Reference(grid.LastOrDefault()));
         a4.Append(new Reference(gridY.FirstOrDefault()));
         a4.Append(new Reference(gridY.LastOrDefault()));
         var line = offsetLine(grid.LastOrDefault().Curve.GetEndPoint(0).CreateLine(grid.FirstOrDefault().Curve.GetEndPoint(0)),grid.FirstOrDefault().Curve.Direction());
         var line1 = offsetLine(gridY.LastOrDefault().Curve.GetEndPoint(0).CreateLine(gridY.FirstOrDefault().Curve.GetEndPoint(0)),gridY.FirstOrDefault().Curve.Direction());
         var line2 = offsetLine(line, grid.FirstOrDefault().Curve.Direction(), 2);
         var line3 = offsetLine(line1, gridY.FirstOrDefault().Curve.Direction(), 2);
         var tran = new Transaction(AC.Document);
         tran.Start("dim");
         AC.Document.Create.NewDimension(view, line, a3);
         AC.Document.Create.NewDimension(view, line1, a4);
         AC.Document.Create.NewDimension(view, line2, a1);
         AC.Document.Create.NewDimension(view, line3, a2);
         DimColumn(view);
         tran.Commit();
      }
      public void DimColumn(VIEW view)
      {
         var columns = new FilteredElementCollector(AC.Document, view.Id)
            .WhereElementIsNotElementType()
            .OfCategory(BuiltInCategory.OST_StructuralColumns)
            .ToList();
         var grids = new FilteredElementCollector(AC.Document)
            .OfClass(typeof(GRID))
            .Cast<GRID>()
            .ToList();
         var grid = grids.FirstOrDefault();
         List<GRID> gridX = new List<GRID>();
         List<GRID> gridY = new List<GRID>();
         grids.ForEach(x =>
         {
            if (x.Curve.Direction().IsParallel(grid.Curve.Direction()))
            {
               gridX.Add(x);
            }
            else gridY.Add(x);
         });
         columns.ForEach(c =>
         {
            var Lines = GetPointCol(c);
            var gridnearX=gridX.MinBy2(g => g.Curve.Distance((c.Location as LocationPoint).Point));
            var gridnearY=gridY.MinBy2(g => g.Curve.Distance((c.Location as LocationPoint).Point));
            var x = new Reference(gridnearX);
            var y = new Reference(gridnearY);
            if(Lines.FirstOrDefault().IsParallelTo(gridnearX.Curve as Line))
            {
               var line = Lines.FirstOrDefault();
               var p1 = line.GetEndPoint(0);
               var p2 = line.GetEndPoint(1);
               var normal = line.Direction.CrossProduct(XYZ.BasisZ);
               line = offsetLine(line, normal);
               var detailLine1 = AC.Document.Create.NewDetailCurve(view, Line.CreateBound(p1, p1 + normal * 1.MmToFoot()));
               var detailLine2 = AC.Document.Create.NewDetailCurve(view, Line.CreateBound(p2, p2 + normal * 1.MmToFoot()));
               ReferenceArray references = new ReferenceArray();
               references.Append(new Reference(detailLine1));
               references.Append(new Reference(gridnearY));
               references.Append(new Reference(detailLine2));

               // create the new dimension
               Dimension dimension = AC.Document.Create.NewDimension(view,
                                                                   line, references);
               var line1 = Lines.LastOrDefault();
               var p3 = line1.GetEndPoint(0);
               var p4 = line1.GetEndPoint(1);
               var normal1 = line1.Direction.CrossProduct(XYZ.BasisZ);
               line1 = offsetLine(line1, -normal1);
               var detailLine3 = AC.Document.Create.NewDetailCurve(view, Line.CreateBound(p3, p3 + normal1 * 1.MmToFoot()));
               var detailLine4 = AC.Document.Create.NewDetailCurve(view, Line.CreateBound(p4, p4 + normal1 * 1.MmToFoot()));
               ReferenceArray references1 = new ReferenceArray();
               references1.Append(new Reference(detailLine3));
               references1.Append(new Reference(gridnearX));
               references1.Append(new Reference(detailLine4));

               // create the new dimension
               Dimension dimension1 = AC.Document.Create.NewDimension(view,
                                                                   line1, references1);

            }
            else
            {
               var line = Lines.FirstOrDefault();
               var p1 = line.GetEndPoint(0);
               var p2 = line.GetEndPoint(1);
               var normal = line.Direction.CrossProduct(XYZ.BasisZ);
               line = offsetLine(line, normal);
               var detailLine1 = AC.Document.Create.NewDetailCurve(view, Line.CreateBound(p1, p1 + normal *1.MmToFoot()));
               var detailLine2 = AC.Document.Create.NewDetailCurve(view, Line.CreateBound(p2, p2 + normal * 1.MmToFoot()));
               ReferenceArray references = new ReferenceArray();
               references.Append(new Reference(detailLine1));
               references.Append(new Reference(gridnearX));
               references.Append(new Reference(detailLine2));

               // create the new dimension
               Dimension dimension = AC.Document.Create.NewDimension(view,
                                                                   line, references);
               var line1 = Lines.LastOrDefault();
               var p3 = line1.GetEndPoint(0);
               var p4 = line1.GetEndPoint(1);
               var normal1 = line1.Direction.CrossProduct(XYZ.BasisZ);
               line1 = offsetLine(line1, -normal1);
               var detailLine3 = AC.Document.Create.NewDetailCurve(view, Line.CreateBound(p3, p3 + normal1 * 1.MmToFoot()));
               var detailLine4 = AC.Document.Create.NewDetailCurve(view, Line.CreateBound(p4, p4 + normal1 * 1.MmToFoot()));
               ReferenceArray references1 = new ReferenceArray();
               references1.Append(new Reference(detailLine3));
               references1.Append(new Reference(gridnearY));
               references1.Append(new Reference(detailLine4));

               // create the new dimension
               Dimension dimension1 = AC.Document.Create.NewDimension(view,
                                                                   line1, references1);


            }
         });
      }
      public Line offsetLine(Line line,XYZ dir,double a=1)
      {
         var scale = ViewScale(SelectedScale);
         var f=line.GetEndPoint(0);
         var l=line.GetEndPoint(1);
         var ff = f.Add(a*8.MmToFoot() * scale * dir);
         var ll = l.Add(a*8.MmToFoot() * scale * dir);
         return ff.CreateLine(ll);
      }
      public static List<Line> GetPointCol(Element coll)
      {
         var col = coll as FamilyInstance;
         var oriPoint = (col.Location as LocationPoint).Point;
         var type = AC.Document.GetElement(col.GetTypeId()) as ElementType;
         var w = type.LookupParameter("b").AsDouble();
         var l = type.LookupParameter("h").AsDouble();
         var facing = col.FacingOrientation.Normalize();
         var hand = col.HandOrientation.Normalize();
         var p1 = oriPoint.Add(-hand * w / 2 + -facing * l / 2);
         var p2 = oriPoint.Add(hand * w / 2 + -facing * l / 2);
         var p3 = p2.Add(facing * l);
         var p4 = p1.Add(facing * l);
         List<XYZ> xYZs = new List<XYZ>() { p1, p2, p3, p4 };
         var Lines=new List<Line>();
         Lines.Add(p1.CreateLine(p2));
         Lines.Add(p1.CreateLine(p4));
         return Lines;
      }
      private IndependentTag CreateIndependentTag(VIEW view, List<Element> farm)
      {

         // define tag mode and tag orientation for new tag
         TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
         TagOrientation tagorn = TagOrientation.Horizontal;
         var farmm = farm.FirstOrDefault();
         // Add the tag to the middle of the wall
         LocationCurve wallLoc = farmm.Location as LocationCurve;
         XYZ wallStart = wallLoc.Curve.GetEndPoint(0);
         XYZ wallEnd = wallLoc.Curve.GetEndPoint(1);
         XYZ wallMid = wallLoc.Curve.Evaluate(0.5, true);
         Reference wallRef = new Reference(farmm);

         IndependentTag newTag = IndependentTag.Create(AC.Document, view.Id, wallRef, true, tagMode, tagorn, wallMid);
         if (null == newTag)
         {
            throw new Exception("Create IndependentTag Failed.");
         }

         // newTag.TagText is read-only, so we change the Type Mark type parameter to 
         // set the tag text.  The label parameter for the tag family determines
         // what type parameter is used for the tag text.


         // set leader mode free
         // otherwise leader end point move with elbow point

         newTag.LeaderEndCondition = LeaderEndCondition.Free;
         XYZ elbowPnt = wallMid + new XYZ(5.0, 5.0, 0.0);
         newTag.LeaderElbow = elbowPnt;
         XYZ headerPnt = wallMid + new XYZ(10.0, 10.0, 0.0);
         newTag.TagHeadPosition = headerPnt;

         return newTag;
      }

   }
}
