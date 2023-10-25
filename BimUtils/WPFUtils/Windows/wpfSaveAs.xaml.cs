using System.Windows ;
using Autodesk.Revit.UI ;

namespace BimUtils.WPFUtils.Windows
{
   /// <summary>
   /// Interaction logic for wpfSaveAs.xaml
   /// </summary>
   public partial class wpfSaveAs : Window
   {
      public string NewName { get; set; }

      public wpfSaveAs()
      {
         InitializeComponent();
         DataContext = this;
         myTextBox.Focus();
      }

      protected override void OnSourceInitialized(EventArgs e)
      {
         IconHelper.RemoveIcon(this);
      }

      private void CmSave(object sender, RoutedEventArgs e)
      {
         if (StringUtils.IsValidFilename(NewName))
         {
            DialogResult = true;
         }
         else
         {
            TaskDialog.Show("Warning", "A file name can't contain any of the following characters: \\ / : * ? \" < > |");
         }
      }
   }
}