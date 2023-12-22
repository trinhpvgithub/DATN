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
using TRINHTOOL.CreateLevel;
using TRINHTOOL.CreateSheet;

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
         var panelSet = Application.CreatePanel("Setup Ban Đầu", TAB_NAME);
         var level = panelSet.AddPushButton<LevelCmd>("CreateLevel");
         var panelCad = Application.CreatePanel("Cad To Revit", TAB_NAME);
         var grid = panelCad.AddPushButton<GridCmd>("CreateGrid");
         grid.SetImage("/DaiwaLease2;component/Resources2/Icons/arc2_16.png");
         grid.SetLargeImage("/TRINHTOOL;component/Resources/icon/grid_32.png");
         var column = panelCad.AddPushButton<ColumnCmd>("CreateColumn");
         column.SetImage("/TRINHTOOL;component/Resources/icon/column_16.png");
         column.SetLargeImage("/TRINHTOOL;component/Resources/icon/column_32.png");
         var beam = panelCad.AddPushButton<BeamCmd>("CreateBeam");
         beam.SetImage("/TRINHTOOL;component/Resources/icon/Beam_16.png");
         beam.SetLargeImage("/TRINHTOOL;component/Resources/icon/Beam_32.png");
         var floor = panelCad.AddPushButton<FloorCmd>("CreateFloor");
         floor.SetImage("/TRINHTOOL;component/Resources/icon/floor_16.png");
         floor.SetLargeImage("/TRINHTOOL;component/Resources/icon/floor_32.png");
         var panelBanve = Application.CreatePanel("Xuất bản vẽ", TAB_NAME);
         var sheet = panelBanve.AddPushButton<CreateSheetCmd>("Create Sheet");
      }

   }
}
