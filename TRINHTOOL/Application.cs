﻿using Autodesk.Revit.DB.Events;
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
using TRINHTOOL.Information;
using TRINHTOOL.Help;
using RestSharp;
using TRINHTOOL.Pile;
using TRINHTOOL.SlabFoudation;

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
         var panelSet = Application.CreatePanel("Da Tum", TAB_NAME);
         var grid = panelSet.AddPushButton<GridCmd>("CreateGrid");
         grid.SetImage("/DaiwaLease2;component/Resources2/Icons/arc2_16.png");
         grid.SetLargeImage("/TRINHTOOL;component/Resources/icon/grid_32.png");
         var level = panelSet.AddPushButton<LevelCmd>("CreateLevel");
         level.SetImage("/DaiwaLease2;component/Resources2/Icons/Level_16.png");
         level.SetLargeImage("/TRINHTOOL;component/Resources/icon/Level_32.png");
         var panelCad = Application.CreatePanel("Cad To Revit", TAB_NAME);
         var pile = panelCad.AddPushButton<PileCmd>("CreatePile");
         pile.SetImage("/TRINHTOOL;component/Resources/icon/pile_16.png");
         pile.SetLargeImage("/TRINHTOOL;component/Resources/icon/pile_32.png");
         var slab = panelCad.AddPushButton<SlabCmd>("CreateSlabFoundation");
         slab.SetImage("/TRINHTOOL;component/Resources/icon/slab_16.png");
         slab.SetLargeImage("/TRINHTOOL;component/Resources/icon/slab_32.png");
         var column = panelCad.AddPushButton<ColumnCmd>("CreateColumn");
         column.SetImage("/TRINHTOOL;component/Resources/icon/column_16.png");
         column.SetLargeImage("/TRINHTOOL;component/Resources/icon/column_32.png");
         var beam = panelCad.AddPushButton<BeamCmd>("CreateBeam");
         beam.SetImage("/TRINHTOOL;component/Resources/icon/Beam_16.png");
         beam.SetLargeImage("/TRINHTOOL;component/Resources/icon/Beam_32.png");
         var floor = panelCad.AddPushButton<FloorCmd>("CreateFloor");
         floor.SetImage("/TRINHTOOL;component/Resources/icon/floor_16.png");
         floor.SetLargeImage("/TRINHTOOL;component/Resources/icon/floor_32.png");
         var panelBanve = Application.CreatePanel("Sheet", TAB_NAME);
         var sheet = panelBanve.AddPushButton<CreateSheetCmd>("Create Sheet");
         sheet.SetImage("/TRINHTOOL;component/Resources/icon/Sheet_16.png");
         sheet.SetLargeImage("/TRINHTOOL;component/Resources/icon/Sheet_32.png");
         var panelinf = Application.CreatePanel("Information", TAB_NAME);
         //var inf = panelinf.AddPushButton<InformationCmd>("Information");
         //inf.SetImage("/TRINHTOOL;component/Resources/icon/inf_16.png");
         //inf.SetLargeImage("/TRINHTOOL;component/Resources/icon/inf_32.png");
         //var help = panelinf.AddPushButton<HelpCmd>("Help");
         //help.SetImage("/TRINHTOOL;component/Resources/icon/help_16.png");
         //help.SetLargeImage("/TRINHTOOL;component/Resources/icon/help_32.png");
      }

   }
}
