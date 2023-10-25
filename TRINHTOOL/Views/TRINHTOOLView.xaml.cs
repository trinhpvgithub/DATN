using TRINHTOOL.ViewModels;

namespace TRINHTOOL.Views
{
   public partial class TRINHTOOLView
   {
      public TRINHTOOLView(TRINHTOOLViewModel viewModel)
      {
         InitializeComponent();
         DataContext = viewModel;
      }
   }
}