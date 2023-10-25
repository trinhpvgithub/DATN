using System.Windows ;
using System.Windows.Controls ;
using BimUtils.DocumentUtils ;

namespace BimUtils.WPFUtils
{
   /// <summary>
   /// Interaction logic for FooterLeft.xaml
   /// </summary>
   public partial class FooterLeft : UserControl
   {
      public FooterLeft()
      {
         InitializeComponent();
      }

      private void BtnFeedBack_OnClick(object sender, RoutedEventArgs e)
      {

         System.Diagnostics.Process.Start("");
      }

      private void BtnYoutube_OnClick(object sender, RoutedEventArgs e)
      {
         if (AC.DicCommandYoutubeLink != null && AC.DicCommandYoutubeLink.ContainsKey(AC.CurrentCommand))
         {
            var link = AC.DicCommandYoutubeLink[AC.CurrentCommand];
            System.Diagnostics.Process.Start(link);
         }
         else
         {
            System.Diagnostics.Process.Start("");
         }
      }

      private void BtnHomePage_OnClick(object sender, RoutedEventArgs e)
      {
         System.Diagnostics.Process.Start("");
      }
   }
}
