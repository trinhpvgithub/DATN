using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRINHTOOL.Column.View;
using TRINHTOOL.Column.ViewModel;

namespace TRINHTOOL.Column
{
   [UsedImplicitly]
   [Transaction(TransactionMode.Manual)]
   public class ColumnCmd : ExternalCommand
   {
      public override void Execute()
      {
         AC.GetInformation(UiDocument);

         var viewModel = new ColumnViewModel();
         var view = new ColumnView() { DataContext = viewModel };
         viewModel.MainView = view;
         view.ShowDialog();
      }
   }
}
