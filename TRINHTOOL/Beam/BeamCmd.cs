using Autodesk.Revit.Attributes;
using BimSpeedModelFromCad.Properties;
using HcBimUtils.DocumentUtils;
using HcBimUtils.JsonData.ModelFromCadJson;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TRINHTOOL.Beam.Model;
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
