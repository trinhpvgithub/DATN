using Autodesk.Revit.Attributes;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;
using TRINHTOOL.CreateLevel.View;
using TRINHTOOL.CreateLevel.ViewModel;

namespace TRINHTOOL.CreateLevel
{
   [UsedImplicitly]
   [Transaction(TransactionMode.Manual)]
   public class LevelCmd : ExternalCommand
   {
      public override void Execute()
      {
         AC.GetInformation(UiDocument);
         var viewModel = new CreateLevelViewModel();
         var view = new CreateLevelView
         {
            DataContext = viewModel
         };
         view.ShowDialog();
      }
   }
}
