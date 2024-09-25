using Autodesk.AutoCAD.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL.TrinhUtils
{
	public class CadExtend
	{
		#region Methods from Win32 API
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		// thu nho windows
		public const int SW_MAXIMIZE = 3;
		// phong to windows
		public const int SW_MINIMIZE = 6;
		[DllImport("ole32.dll")]
		private static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable prot);

		[DllImport("ole32.dll")]
		private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);
		#endregion

		public static IntPtr FindAndActiveCad(UIApplication UiApplication)
		{
			IntPtr cadWindow = IntPtr.Zero;
			Process[] processes = Process.GetProcesses();
			foreach (Process p in processes)
			{
				if (!String.IsNullOrEmpty(p.MainWindowTitle))
				{
					if (p.MainWindowTitle.Contains("Autodesk AutoCAD"))
					{
						cadWindow = p.MainWindowHandle;
						ShowWindow(cadWindow, UiApplication.MainWindowHandle);
						break;
					}
				}
			}
			return cadWindow;
		}
		public static IList<IntPtr> FindAllCadProcess()
		{
			List<IntPtr> list = [];
			Process[] processes = Process.GetProcesses();
			foreach (Process p in processes)
			{
				if (!String.IsNullOrEmpty(p.MainWindowTitle))
				{
					if (p.MainWindowTitle.Contains("Autodesk AutoCAD"))
					{
						list.Add(p.MainWindowHandle);
					}
				}
			}
			return list;
		}
		public static void ShowWindow(IntPtr hWndShow, IntPtr hWndHide)
		{
			ShowWindow(hWndShow, SW_MAXIMIZE);
			ShowWindow(hWndHide, SW_MINIMIZE);
			SetForegroundWindow(hWndShow);
		}
		public static dynamic SelectObjects(Int16[] FilterType, object[] FilterData)
		{
			dynamic acadApp;
			dynamic selectionSets;
			try
			{
				acadApp = Marshal.GetActiveObject("AutoCAD.Application");
			}
			catch (Exception)
			{
				return null;
			}
			dynamic activeDoc = acadApp.ActiveDocument;
			selectionSets = activeDoc.SelectionSets;
			dynamic selectionSet;
			string name = "Filter block by Layer";
			try
			{
				selectionSet = selectionSets.Item(name);
				selectionSet.Clear();
			}
			catch
			{
				selectionSet = selectionSets.Add(name);
			}
			selectionSet.SelectOnScreen(FilterType, FilterData);
			return selectionSets;
		}
		public static AcadSelectionSets SelectObjects(object FilterType, object[] FilterData)
		{
			dynamic acadApp = Marshal.GetActiveObject("AutoCaD.Application");

			AcadApplication acadAppTyped = acadApp as AcadApplication;

			AcadDocument activeDoc = acadAppTyped.ActiveDocument;

			AcadSelectionSets selectionSets = activeDoc.SelectionSets;
			AcadSelectionSet selectionSet;
			string name = "Filter block by Layer";
			try
			{
				selectionSet = selectionSets.Item(name);
				selectionSet.Clear();
			}
			catch
			{
				selectionSet = selectionSets.Add(name);
			}

			selectionSet.SelectOnScreen(FilterType, FilterData);

			return selectionSets;
		}
		public static dynamic SelectObjects()
		{
			dynamic a = Marshal.GetActiveObject("AutoCaD.Application");
			dynamic doc = a.Documents.Application.ActiveDocument;
			dynamic newset = doc.SelectionSets.Add(Guid.NewGuid().ToString());
			newset.SelectOnScreen();
			return newset;
		}
		public static XYZ PickPoint()
		{
			try
			{
				string[] arr;
				dynamic a = Marshal.GetActiveObject("AutoCaD.Application");
				dynamic doc = a.Documents.Application.ActiveDocument;
				var pointCad = doc.Utility.GetPoint(Type.Missing, "Select point: ");
				arr = ((IEnumerable)pointCad).Cast<object>()
						.Select(x => x.ToString())
						.ToArray();
				return new XYZ(double.Parse(arr[0]), double.Parse(arr[1]), double.Parse(arr[2]));
			}
			catch
			{
				return null;
			}
		}
		public static IList<string> GetAllFilesCad()
		{
			IList<string> FileCads = [];
			dynamic acadApp;
			try
			{
				acadApp = Marshal.GetActiveObject("AutoCAD.Application");
			}
			catch (Exception)
			{
				return [];
			}
			dynamic acadDocs = acadApp.Documents;
			foreach (dynamic doc in acadDocs)
			{
				string name = doc.Name;
				FileCads.Add(name);
			}
			return FileCads;
		}
		public static IList<string> GetAllLayers(string acadDoc)
		{
			IList<string> layers = [];
			dynamic acadApp;
			try
			{
				acadApp = Marshal.GetActiveObject("AutoCAD.Application");
			}
			catch (Exception)
			{
				return [];
			}
			dynamic acadDocs = acadApp.Documents;
			foreach (dynamic doc in acadDocs)
			{
				string name = doc.Name;
				if (name == acadDoc)
				{
					foreach (dynamic layer in doc.Layers)
					{
						string layerName = layer.Name;
						layers.Add(layerName);
					}
				}
			}
			return layers;
		}
		public static void SetActiveCad(string docName)
		{
			dynamic acadApp = Marshal.GetActiveObject("AutoCAD.Application");

			dynamic acadDocs = acadApp.Documents;
			foreach (dynamic doc in acadDocs)
			{
				string name = doc.Name;
				if (name == docName)
				{
					doc.Activate();
					return;
				}
			}
		}
		public static IntPtr OpenWindowCad(IntPtr revitWD, string documentName)
		{
			dynamic acadApp = Marshal.GetActiveObject("AutoCAD.Application");
			dynamic acadDocs = acadApp.Documents;
			// Tìm tài liệu với tên được chỉ định
			foreach (dynamic doc in acadDocs)
			{
				string name = doc.Name;
				if (name.Equals(documentName, StringComparison.OrdinalIgnoreCase))
				{
					// Kích hoạt tài liệu
					doc.Activate();
					// Lấy handle của cửa sổ AutoCAD
					IntPtr acadHandle = new(acadApp.HWND);
					// Đưa cửa sổ AutoCAD lên phía trước
					ShowWindow(acadHandle, revitWD);
					return acadHandle;
				}
			}
			return IntPtr.Zero;
		}
	}
}
