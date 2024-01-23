using Autodesk.Revit.Attributes;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRINHTOOL.SlabFoudation.ViewModel;
using TRINHTOOL.SlabFoudation.View;

namespace TRINHTOOL.SlabFoudation
{
   [UsedImplicitly]
   [Transaction(TransactionMode.Manual)]
   public class SlabCmd : ExternalCommand
   {
      public override void Execute()
      {
         AC.GetInformation(UiDocument);

         var viewModel = new SlabViewModel();
         var view = new SlabView { DataContext = viewModel };
         viewModel.MainView = view;
         view.ShowDialog();
      }
   }
}
