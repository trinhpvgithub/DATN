using CommunityToolkit.Mvvm.ComponentModel;
using HcBimUtils.MoreLinq;
using HcBimUtils.WPFUtils;
using System.Windows.Controls;
using System.Windows.Media;
using TRINHTOOL.ViewModels.SubViewModels;
using TRINHTOOL.Views;

namespace TRINHTOOL.ViewModels
{
   public sealed class TRINHTOOLViewModel : ObservableObject
   {
      public TRINHTOOLView TRINHTOOLView { get; set; }
      private object _selectedViewModel;
      public Object SelectedViewModel
      {
         get => _selectedViewModel;
         set
         {
            _selectedViewModel = value;
            OnPropertyChanged();
         }
      }
      public ColumnViewModel ColumnViewModel { get; set; }
      public bool _columnCreatable;
      public bool ColumnCreatable
      {
         get => _columnCreatable;
         set
         {
            _columnCreatable = value;
            OnPropertyChanged();
         }
      }
      public RelayCommand UpdateViewCommand { get; set; }
      public TRINHTOOLViewModel()
      {
         ColumnViewModel= new ColumnViewModel() { ParentViewModel = this };
         UpdateViewCommand = new RelayCommand(ChangeView);
         SelectedViewModel = ColumnViewModel;
      }
      public void ChangeView(object obi)
      {
         var buttons = TRINHTOOLView.TabContainer.Children.Flatten().Where(x => x is Button).Cast<Button>().Where(x => !x.Name.Contains("Special"));

         buttons.ForEach(x =>
         {
            x.Background = null;
         });

         if (obi.ToString() == "CreateColumn")
         {
            SelectedViewModel = ColumnViewModel;
            TRINHTOOLView.ButtonCol.Background = Brushes.LightBlue;

         }
      }
   }
}