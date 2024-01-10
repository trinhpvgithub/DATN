using Autodesk.Revit.Attributes;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRINHTOOL.Pile.ViewModel;
using TRINHTOOL.Pile.View;

namespace TRINHTOOL.Pile
{
   [UsedImplicitly]
   [Transaction(TransactionMode.Manual)]
   public class PileCmd:ExternalCommand
    {
      public override void Execute()
      {
         AC.GetInformation(UiDocument);

         var viewModel = new PileViewModel();
         var view = new PileView { DataContext = viewModel };
         viewModel.MainView = view;
         view.ShowDialog();
      }
   }
}
