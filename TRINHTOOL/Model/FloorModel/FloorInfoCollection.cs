using HcBimUtils.WPFUtils;

namespace TRINHTOOL.Model.FloorModel
{
   public class FloorInfoCollection:ViewModelBase
   {
      private double _area;

      public double Area
      {
         get => _area;

         set
         {
            _area = value;
            OnPropertyChanged();
         }
      }
   }
}
