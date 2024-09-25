using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using BimSpeedModelFromCad.Properties;
using HcBimUtils.DocumentUtils;
using HcBimUtils.JsonData.ModelFromCadJson;
using HcBimUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TRINHTOOL.Model;
using TRINHTOOL.Pile.Model;
using HcBimUtils.WPFUtils;
using TRINHTOOL.Pile.View;
using TRINHTOOL.Views;
using HcBimUtils.MoreLinq;
using TRINHTOOL.TrinhUtils;

namespace TRINHTOOL.Pile.ViewModel
{
	public class PileViewModel : ViewModelBase
	{
		#region Declare
		public PileView MainView { get; set; }
		public XyzData CadPileOrigin;

		public int Dept { get; set; } = 5000;

		public List<CadRoundPile> CadRoundPile = new();

		private Level _selectedLevel;

		private XYZ _origin;

		private List<Family> _families;

		private List<string> _selectedFamilyParameters;

		private Family _familySelectedsquare;

		public List<Level> Levels { get; set; } = new();

		public RelayCommand SelectFormCadPile { get; set; }

		public RelayCommand Create { get; set; }
		public RelayCommand Cancel { get; set; }
		public RelayCommand PointRevit { get; set; }

		public List<string> Layers { get; set; }

		private string _selectedLayer;

		public string SelectedLayer
		{
			get { return _selectedLayer; }
			set
			{
				_selectedLayer = value;
				OnPropertyChanged();
			}
		}
		public List<Family> Families
		{
			get => _families;

			set
			{
				_families = value;
				OnPropertyChanged();
			}
		}

		public List<string> SelectedFamilyParameters
		{
			get => _selectedFamilyParameters;
			set
			{
				_selectedFamilyParameters = value;
				OnPropertyChanged();
			}
		}


		public string SelectedDeptParameter { get; set; }

		public string SelectedRadiusParameter { get; set; }

		public Family FamilySelectedPile
		{
			get => _familySelectedsquare;
			set
			{
				_familySelectedsquare = value;
				if (FamilySelectedPile != null)
				{
					var first = FamilySelectedPile.GetFamilySymbolIds().FirstOrDefault().ToElement() as FamilySymbol;
					SelectedFamilyParameters = first.GetOrderedParameters().Where(x => x.StorageType == StorageType.Double).Select(x => x.Definition.Name)
						.ToList();

					SelectedDeptParameter = SelectedFamilyParameters.FirstOrDefault(x => x.Contains("Depth"));
					SelectedRadiusParameter = SelectedFamilyParameters.FirstOrDefault(x => x.Contains("Radius"));

					if (string.IsNullOrEmpty(SelectedDeptParameter))
					{
						SelectedDeptParameter = SelectedFamilyParameters.FirstOrDefault();
					}
					if (string.IsNullOrEmpty(SelectedRadiusParameter))
					{
						SelectedRadiusParameter = SelectedFamilyParameters.LastOrDefault();
					}
					OnPropertyChanged();
				}
				OnPropertyChanged(nameof(SelectedDeptParameter));
				OnPropertyChanged(nameof(SelectedRadiusParameter));
			}
		}

		public ObservableCollection<PileInfoCollection> PileInfoCollections { get; set; } = new();

		private readonly List<PileInfoCollection> _pileInfoCollection = new();

		#endregion

		public Level SelectedLevel
		{
			set
			{
				_selectedLevel = value;
				OnPropertyChanged();
			}
			get => _selectedLevel;
		}
		public List<string> FileCads { get; set; }
		private string _selectedFileCad;
		public string SelectedFileCad
		{
			get
			{
				return _selectedFileCad;
			}
			set
			{
				_selectedFileCad = value;
				Layers = (List<string>)CadExtend.GetAllLayers(SelectedFileCad);
				if (Layers == null) return;
				SelectedLayer = Layers.FirstOrDefault();
				OnPropertyChanged(nameof(Layers));
				OnPropertyChanged();
			}
		}
		public void Cancell()
		{
			MainView?.Close();
		}
		//Constructor
		public PileViewModel()
		{
			Create = new RelayCommand(x => ModelPile(AC.Selection.PickPoint()));
			PointRevit = new RelayCommand(x => ModelPile(new XYZ()));
			Cancel = new RelayCommand(x => Cancell());
			SelectFormCadPile = new RelayCommand(x => SelectPile());
			GetData();
			FamilySelectedPile = Families.FirstOrDefault();
			SelectedLevel = Levels.FirstOrDefault();
			//GetLayer();
			SelectedLayer = Layers.FirstOrDefault();
		}
		public void GetLayer()
		{
			
			Layers = (List<string>)CadExtend.GetAllLayers(SelectedFileCad);
		}
		//Select Form Cad
		public void SelectPile()
		{
			_pileInfoCollection.Clear();
			MainView.Hide();
			var cadWD = CadExtend.OpenWindowCad(AC.UiApplication.MainWindowHandle, SelectedFileCad);
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

				CadPileOrigin = new XyzData(arrPoint1[0], arrPoint1[1], arrPoint1[2]);
				var newset = doc.SelectionSets.Add(Guid.NewGuid().ToString());

				newset.SelectOnScreen();

				if (newset.Count <= 0)
				{
					MessageBox.Show(Resources.COMMON_MESSAGESELECTELEMENTCAD, Resources.COMMON_NOTIFY, MessageBoxButton.OK,
						MessageBoxImage.Warning);
				}

				List<dynamic> listText = new List<dynamic>();

				List<dynamic> listCirle = new List<dynamic>();
#if true //Tét
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
				Layers = listLayer;
				var list = new List<CadData>();
				foreach (var item in groupCadData)
				{
					if (item.Key == SelectedLayer)
					{
						list = item.ToList();
					}
				}
				var ob = new List<dynamic>();
				foreach (var o in list)
				{
					ob.Add(o.CadObject);
				}
#endif
				//cot tron
				foreach (dynamic s in ob)
				{
					if (s.EntityName == "AcDbMText")
					{
						listText.Add(s);
					}

					if (s.EntityName == "AcDbCircle")
					{
						listCirle.Add(s);
					}
				}

				List<TextDataPile> listpoint = new List<TextDataPile>();

				foreach (var text in listText)
				{
					string[] arrtextpoint = ((IEnumerable)text.InsertionPoint).Cast<object>()
						.Select(x => x.ToString())
						.ToArray();

					double[] arrtextpoint1 = new double[3];
					int k = 0;
					foreach (var item in arrtextpoint)
					{
						arrtextpoint1[k] = Convert.ToDouble(item);
						k++;
					}

					listpoint.Add(new TextDataPile()
					{
						PointPile = new XYZ(arrtextpoint1[0], arrtextpoint1[1], arrtextpoint1[2]),
						TextPile = text.TextString
					});
				}

				foreach (var circle in listCirle)
				{
					dynamic center = circle.Center;

					XYZ centerpoint = new XYZ((double)center[0], (double)center[1], 0);

					if (listText.Count > 0)
					{
						TextDataPile textData = listpoint.MinBy2(x => x.PointPile.DistanceTo(centerpoint));

						CadRoundPile.Add(new CadRoundPile()
						{
							Center = new XyzData(centerpoint.X, centerpoint.Y, 0),
							Radius = Math.Round(circle.Radius, 0),
							MarkPile = textData.TextPile
						});
					}
					else
					{
						CadRoundPile.Add(new CadRoundPile()
						{
							Center = new XyzData(centerpoint.X, centerpoint.Y, 0),
							Radius = Math.Round(circle.Radius, 0),
							MarkPile = ""
						});
					}
				}
				GetPileInfoCollections();

				OnPropertyChanged(nameof(PileInfoCollections));
			}

			MainView.ShowDialog();

		}

		//Model Pile
		public void ModelPile(XYZ point)
		{
			MainView.Hide();
			try
			{
				_origin = point;
				_origin = new XYZ(_origin.X, _origin.Y, 0);
			}
			catch (Exception e)
			{
				MessageBox.Show(Resources.COMMON_MESSAGEPICKPOINTREVIT, Resources.COMMON_NOTIFY, MessageBoxButton.OKCancel, MessageBoxImage.Error);
			}

			if (_origin != null)
			{
				var max = PileInfoCollections.Select(x => x.PileInfos.Count).Sum();
				var progressView = new progressbar();
				progressView.Show();

				using var tg = new TransactionGroup(AC.Document, "Model Pile");
				tg.Start();
				DeleteWarningSuper waringsuper = new DeleteWarningSuper();
				foreach (var pileInfoCollection in PileInfoCollections)
				{
					if (progressView.Flag == false)
					{
						break;
					}
					var radius = Convert.ToDouble(pileInfoCollection.Radius);

					var elementType = pileInfoCollection.ElementType = GetElementType(radius, Dept);

					if (elementType == null)
					{
						continue;
					}

					foreach (var pileInfo in pileInfoCollection.PileInfos)
					{
						using (var tx = new Transaction(AC.Document, "Modeling Pile From Cad"))
						{
							tx.Start();

							FailureHandlingOptions failOpt = tx.GetFailureHandlingOptions();

							failOpt.SetFailuresPreprocessor(waringsuper);

							tx.SetFailureHandlingOptions(failOpt);

							var center = pileInfo.Center.Add(_origin - CadPileOrigin.ToXyz());

							var fs = pileInfoCollection.ElementType as FamilySymbol;
							if (fs.IsActive == false)
							{
								fs.Activate();
							}

							try
							{
								var pile = AC.Document.Create.NewFamilyInstance(center, fs,
									SelectedLevel, StructuralType.Footing);
								pile.GetParameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(-200.MmToFoot());
								var mark = pile.SetParameterValueByName(BuiltInParameter.ALL_MODEL_MARK, pileInfoCollection.Text);
							}
							catch
							{

							}
							AC.Document.Regenerate();
							progressView.Create(max, "PileModel");

							tx.Commit();
						}
					}
				}
				tg.Assimilate();
				progressView.Close();
			}
			MainView.ShowDialog();

		}
		//GetType
		private ElementType GetElementType(double radius, double dept)
		{
			ElementType elementType = null;

			using (var tx = new Transaction(AC.Document, "Duplicate Type"))
			{
				tx.Start();
				//update element in revit
				AC.Document.Regenerate();

				var pileTypes = FamilySelectedPile.GetFamilySymbolIds().Select(x => x.ToElement())
					.Cast<FamilySymbol>().ToList();

				foreach (var familySymbol in pileTypes)
				{

					var rParam = familySymbol.LookupParameter(SelectedRadiusParameter);

					var rInMM = Convert.ToDouble(rParam.AsDouble().FootToMm());

					if (radius == rInMM)
					{
						elementType = familySymbol;
					}
				}

				if (elementType == null)
				{
					//Duplicate Column Type
					var type = pileTypes.FirstOrDefault();

					var newTypeName = "Rounded_Pile" + "_" + Math.Round(radius, 0);

					var i = 1;
					if (pileTypes.Select(x => x.Name).Contains(newTypeName))
					{
						newTypeName = $"{newTypeName} (1)";
					}

					while (true)
					{
						try
						{
							elementType = type?.Duplicate(newTypeName);
							break;
						}
						catch
						{
							i++;
							newTypeName = $"{newTypeName} ({i})";
						}
					}

					if (elementType != null)
					{
						elementType.LookupParameter(SelectedRadiusParameter).Set(radius.MmToFoot());
						elementType.LookupParameter(SelectedDeptParameter).Set(dept.MmToFoot());
					}
				}

				tx.Commit();
			}
			return elementType;
		}

		//get Pile Info
		public void GetPileInfoCollections()
		{

			var dic = new Dictionary<PileInfo, List<PileInfo>>();

			foreach (var pile in CadRoundPile)
			{
				var pileInfo = new PileInfo(pile.Radius, pile.Center.ToXyz(), pile.MarkPile);
				var radius = pile.Radius;
				if (dic.ContainsKey(pileInfo))
				{
					dic[pileInfo].Add(pileInfo);
				}
				else
				{
					dic.Add(pileInfo, new List<PileInfo> { pileInfo });
				}
			}

			foreach (var pair in dic)
			{

				var collection = new PileInfoCollection
				{
					Radius = pair.Key.Radius,
					Number = pair.Value.Count,
					Text = pair.Key.Text
				};

				var b = pair.Value.ToList().Distinct(new PileInfoComparerByPoint()).ToList(); ;
				collection.PileInfos = b;
				_pileInfoCollection.Add(collection);

			}


			PileInfoCollections = new ObservableCollection<PileInfoCollection>(_pileInfoCollection);
		}
		//Get data
		public void GetData()
		{
			FileCads = [.. CadExtend.GetAllFilesCad()];
			SelectedFileCad = FileCads.FirstOrDefault();
			//Level
			Levels = new FilteredElementCollector(AC.Document).OfClass(typeof(Level)).Cast<Level>()
				.OrderBy(x => x.Elevation).ToList();

			//Get family
			Families = new FilteredElementCollector(AC.Document).OfCategory(BuiltInCategory.OST_StructuralFoundation).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Select(x => x.Family).DistinctBy(x => x.Id).OrderBy(x => x.Name).ToList();

		}
	}
	public class CadRoundPile
	{
		public XyzData Center { get; set; }

		public double Radius { get; set; }

		public string MarkPile { get; set; }
	}

	public class TextDataPile
	{
		public XYZ PointPile;
		public string TextPile;
	}

}
