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
         var layers = GetLayer();
         var viewModel = new BeamViewModel(layers);
         var view = new BeamView() { DataContext = viewModel };
         viewModel.  MainView = view;
         view.ShowDialog();
      }
      public List<string> GetLayer()
      {
         dynamic a = Marshal.GetActiveObject("AutoCaD.Application");

         dynamic doc
             = a.Documents.Application.ActiveDocument;

         string[] arrPoint = null;
         try
         {
            var pointCad = doc.Utility.GetPoint(Type.Missing, "Select point: ");
            arrPoint = ((IEnumerable)pointCad).Cast<object>()
                .Select(x => x.ToString())
                .ToArray();
         }
         catch (Exception e)
         {
            MessageBox.Show(Resources.COMMON_MESSAGEPICKPOINTCAD, Resources.COMMON_NOTIFY, MessageBoxButton.OKCancel, MessageBoxImage.Error);
         }

         if (arrPoint != null)
         {
            double[] arrPoint1 = new double[3];

            int i = 0;

            foreach (var item in arrPoint)
            {
               arrPoint1[i] = Convert.ToDouble(item);
               i++;
            }

            var newset = doc.SelectionSets.Add(Guid.NewGuid().ToString());
            newset.SelectOnScreen();
            if (newset.Count <= 0)
            {
               MessageBox.Show(Resources.COMMON_MESSAGESELECTELEMENTCAD, Resources.COMMON_NOTIFY, MessageBoxButton.OK,
                   MessageBoxImage.Warning);
            }
            List<dynamic> listText = new List<dynamic>();

            List<dynamic> listLine = new List<dynamic>();

            List<CadData> cadDatas = new List<CadData>();
            foreach (var l in newset)
            {
               cadDatas.Add(new CadData() { CadObject = l, LayerName = l.Layer });
            }
            var groupCadData = cadDatas.GroupBy(x => x.LayerName);
            var listLayer = new List<string>();
            foreach (var item in groupCadData)
            {
               listLayer.Add(item.Key);
            }
            return listLayer;
         }
         else { return null; }
      }
   }
}
