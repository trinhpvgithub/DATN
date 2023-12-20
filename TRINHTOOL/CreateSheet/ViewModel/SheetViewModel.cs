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
               SheetModel.CreateSheet(AC.Document, SelectedTitleBlock.Id, view, view.Name);
            });
         }
         else
         {
            var v=Viewplan.FirstOrDefault(x=>x.Name.Contains(SelectedLevel.Name));
            var view = DuplicateView(v);
            //Dim(view);
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
         //Scale = new List<string> { "1:50", "1:100", "1:150", "1:200" };
         //SelectedScale = Scale.FirstOrDefault();
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
               //dependentView.Scale=ViewScale(SelectedScale);
               if (SelectedViewTemplate != null)
               {
                  dependentView.GetParameter(BuiltInParameter.VIEW_TEMPLATE).Set(SelectedViewTemplate.Id);
               }
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
         grid.Add(gridX);
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
         grid.ForEach(x=>
         {
            a1.Append(new Reference(x));
         });
         var line = grid.LastOrDefault().Curve.GetEndPoint(0).CreateLine(grid.FirstOrDefault().Curve.GetEndPoint(0));
         var tran = new Transaction(AC.Document);
         tran.Start("dim");
         AC.Document.Create.NewDimension(view, line, a1);
         tran.Commit();
      }
   }
}
