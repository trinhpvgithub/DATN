using System.Windows ;
using System.Windows.Input ;

namespace BimUtils.WPFUtils.Windows
{
   public class vmProgress : GalaSoft.MvvmLight.ViewModelBase
   {
      #region Static

      public static bool IsStop;
      private Window _window;

      #endregion Static

      #region Field

      private double _MaxValue;
      private double _CurrentValue;
      private string _Text;
      private string _MainContent;

      #endregion Field

      #region Property

      public string MainContent
      {
         get => _MainContent;
         set
         {
            _MainContent = value;
            RaisePropertyChanged(nameof(MainContent));
         }
      }

      public string Text
      {
         get
         {
            return string.Format("Total {0} / {1} Completed!", CurrentValue, MaxValue);
         }
         set => RaisePropertyChanged(nameof(Text));
      }

      public double MaxValue
      {
         get
         {
            return _MaxValue;
         }
         set
         {
            _MaxValue = value;
            RaisePropertyChanged(nameof(MaxValue));
            RaisePropertyChanged(nameof(Text));
         }
      }

      public double CurrentValue
      {
         get
         {
            return _CurrentValue;
         }
         set
         {
            _CurrentValue = value;
            RaisePropertyChanged(nameof(CurrentValue));
            RaisePropertyChanged(nameof(Text));
         }
      }

      #endregion Property

      #region Commands

      public ICommand CmStop => new GalaSoft.MvvmLight.CommandWpf.RelayCommand<object[]>((p) =>
      {
         IsStop = true;
         _window.Close();
      });

      public ICommand CmLoadedWindow => new GalaSoft.MvvmLight.CommandWpf.RelayCommand<object[]>((p) =>
      {
         _window = p[0] as Window;
      });

      #endregion Commands

      #region Constructor

      public vmProgress()
      {
         CurrentValue = 0;
      }

      #endregion Constructor
   }
}