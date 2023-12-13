using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRINHTOOL.Grid.View;
using TRINHTOOL.Grid.ViewModel;

namespace TRINHTOOL.Grid
{
   [UsedImplicitly]
   [Transaction(TransactionMode.Manual)]
   public class GridCmd : ExternalCommand
   {
      public override void Execute()
      {
         AC.GetInformation(UiDocument);

         var viewModel = new GridViewModel();
         var view = new GridView() { DataContext = viewModel };
         viewModel.MainView = view;
         view.ShowDialog();
      }
   }
}
