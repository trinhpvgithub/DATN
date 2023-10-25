using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Input ;

namespace BimUtils.WPFUtils.Windows
{
   public class vmProgressWithDetail : GalaSoft.MvvmLight.ViewModelBase
   {
      #region Static

      public static bool IsStop;

      #endregion Static

      #region Field
      private double _maxValue;
      private double _currentValue = 0;
      private string _status;
      private string _mainContent;
      private Window _window;
      private TextBox _txbStatus;

      #endregion Field

      #region Property

      public string Status
      {
         get => _status;
         set
         {
            _status = value;
            _txbStatus.ScrollToEnd();
            RaisePropertyChanged(nameof(Status));
         }
      }

      public string MainContent
      {
         get => _mainContent;
         set
         {
            _mainContent = value;
            RaisePropertyChanged("MainContent");
         }
      }

      public string Text
      {
         get
         {
            return string.Format("Total {0} / {1} completed.", CurrentValue, MaxValue);
         }
         set => RaisePropertyChanged("Text");
      }

      public double MaxValue
      {
         get
         {
            return _maxValue;
         }
         set
         {
            _maxValue = value;
            RaisePropertyChanged("MaxValue");
            RaisePropertyChanged("Text");
         }
      }

      public double CurrentValue
      {
         get
         {
            return _currentValue;
         }
         set
         {
            _currentValue = value;
            RaisePropertyChanged("CurrentValue");
            RaisePropertyChanged("Text");
         }
      }

      #endregion Property

      #region Commands

      public ICommand CmLoadedWindow => new GalaSoft.MvvmLight.CommandWpf.RelayCommand<object[]>((p) =>
      {
         _window = p[0] as Window;
         _txbStatus = p[1] as TextBox;
      });

      public ICommand CmStop => new GalaSoft.MvvmLight.CommandWpf.RelayCommand<string>((p) =>
      {
         IsStop = true;
         _window.Close();
      });

      #endregion Commands

      #region Functions

      public void Close()
      {
         _window.Close();
      }

      #endregion Functions
   }
}
