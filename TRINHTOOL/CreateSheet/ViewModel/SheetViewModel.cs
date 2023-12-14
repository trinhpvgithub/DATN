using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HcBimUtils.DocumentUtils;
using HcBimUtils.WPFUtils;
using TRINHTOOL.CreateSheet.Model;
using TRINHTOOL.CreateSheet.View;
using VIEW = Autodesk.Revit.DB.View;

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
      }
      public VIEW DuplicateView(VIEW view)
      {
         if (view != null)
         {
            VIEW dependentView = null;
            ElementId newViewId = ElementId.InvalidElementId;
            if (view.CanViewBeDuplicated(ViewDuplicateOption.AsDependent))
            {
               newViewId = view.Duplicate(ViewDuplicateOption.AsDependent);
               dependentView = view.Document.GetElement(newViewId) as VIEW;
               dependentView.Name = "MB  " + view.Name;
               if (SelectedViewTemplate != null)
               {
                  dependentView.GetParameter(BuiltInParameter.VIEW_TEMPLATE).Set(SelectedViewTemplate.Id);
               }
               if (null != dependentView)
               {
                  if (dependentView.GetPrimaryViewId() == view.Id)
                  {
                     TaskDialog.Show("Dependent View", "Dependent view created successfully!");
                  }
               }
               return dependentView;
            }
         }
         return null;
      }
   }
}
