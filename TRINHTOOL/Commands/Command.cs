using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;
using TRINHTOOL.ViewModels;
using TRINHTOOL.Views;

namespace TRINHTOOL.Commands
{
   [UsedImplicitly]
   [Transaction(TransactionMode.Manual)]
   public class Command : ExternalCommand
   {
      public override void Execute()
      {
         AC.GetInformation(UiDocument);

         var viewModel = new TRINHTOOLViewModel();
         var view = new TRINHTOOLView() { DataContext=viewModel};
         viewModel.TRINHTOOLView=view;
         view.ShowDialog();
      }
   }
}