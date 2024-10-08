﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using HcBimUtils.DocumentUtils;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRINHTOOL.CreateSheet.View;
using TRINHTOOL.CreateSheet.ViewModel;

namespace TRINHTOOL.CreateSheet
{
   [UsedImplicitly]
   [Transaction(TransactionMode.Manual)]
   public class CreateSheetCmd: ExternalCommand
   {
      public override void Execute()
      {
         AC.GetInformation(UiDocument);

         var viewModel = new SheetViewModel();
         var view = new SheetView() { DataContext = viewModel };
         viewModel.MainView = view;
         view.ShowDialog();
      }
   }
}
