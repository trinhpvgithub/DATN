using Autodesk.Revit.Attributes;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRINHTOOL.Beam.View;
using TRINHTOOL.Beam.ViewModel;
using TRINHTOOL.ViewModels;
using TRINHTOOL.Views;

namespace TRINHTOOL.Beam
{
   [UsedImplicitly]
   [Transaction(TransactionMode.Manual)]
   public class BeamCmd: ExternalCommand
   {
      public override void Execute()
      {
         AC.GetInformation(UiDocument);

         var viewModel = new BeamViewModel();
         var view = new BeamView() { DataContext = viewModel };
         viewModel.  MainView = view;
         view.ShowDialog();
      }
   }
}
