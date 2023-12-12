using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRINHTOOL.Beam;
using TRINHTOOL.Column;
using TRINHTOOL.Floor;
using Autodesk.Revit.UI.Events;
using HcBimUtils.DocumentUtils;
using Newtonsoft.Json;
using TRINHTOOL.Grid;

namespace TRINHTOOL
{
   [UsedImplicitly]
   public class Application :ExternalApplication
   {
      public override void OnStartup()
      {
         CreateRibbon();

      }
      private static readonly string TAB_NAME = "TRINH-TOOL";
      public void CreateRibbon()
      {
         var panelCad = Application.CreatePanel("Cad To Revit", TAB_NAME);
         var grid = panelCad.AddPushButton<GridCmd>("CreateGrid");
         var beam = panelCad.AddPushButton<BeamCmd>("CreateBeam");
         var column= panelCad.AddPushButton<ColumnCmd>("CreateColumn");
         var floor = panelCad.AddPushButton<FloorCmd>("CreateFloor");
      }

   }
}
