using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRINHTOOL.Floor.View;
using TRINHTOOL.Floor.ViewModel;

namespace TRINHTOOL.Floor
{
   [UsedImplicitly]
   [Transaction(TransactionMode.Manual)]
   public class FloorCmd : ExternalCommand
   {
      public override void Execute()
      {
         AC.GetInformation(UiDocument);

         var viewModel = new FloorViewModel();
         var view = new FloorView() { DataContext = viewModel };
         viewModel.MainView = view;
         view.ShowDialog();
      }
   }
}
