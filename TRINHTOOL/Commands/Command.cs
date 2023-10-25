using Autodesk.Revit.Attributes;
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
         var viewModel = new TRINHTOOLViewModel();
         var view = new TRINHTOOLView(viewModel);
         view.ShowDialog();
      }
   }
}