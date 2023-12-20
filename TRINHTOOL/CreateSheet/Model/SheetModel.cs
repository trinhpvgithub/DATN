using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TRINHTOOL.CreateSheet.Model
{
   public class SheetModel
   {
      public static void CreateSheet(Document document,ElementId titleblock,Element view,string name)
      {
         ViewSheet SHEET = null;
         Transaction trans = new Transaction(document);
         trans.Start("sheet");
         try
         {
            SHEET = ViewSheet.Create(document, titleblock);
            SHEET.Name = name;
            SHEET.SheetNumber = "Mặt Bằng";
            if (null == SHEET)
            {
               throw new Exception("Failed to create new ViewSheet.");
            }

            // Add passed in view onto the center of the sheet
            UV location = new UV((SHEET.Outline.Max.U - SHEET.Outline.Min.U) / 2,
                                    (SHEET.Outline.Max.V - SHEET.Outline.Min.V) / 2);

            //viewSheet.AddView(view3D, location);
            Viewport.Create(document, SHEET.Id, view.Id, new XYZ(location.U, location.V, 0));
            trans.Commit();
         }
         catch
         {
            trans.RollBack();
         }
      }
   }
}
