using Autodesk.Revit.DB;
using HcBimUtils.WPFUtils;

namespace TRINHTOOL.Beam.Model
{
   public class BeamInfoCollection : ViewModelBase
   {
      public List<BeamInfo> BeamInfos { get; set; } = new List<BeamInfo>();

      private double _width;

      public double Width
      {
         get => _width;
         set
         {
            _width = value;
            OnPropertyChanged();
         }
      }

      private double _height;

      public double Height
      {
         get => _height;
         set
         {
            _height = value;
            OnPropertyChanged();
         }
      }

      private string _text;
      private string _mark;

      public string Text
      {
         get => _text;
         set
         {
            _text = value;
            OnPropertyChanged();
         }
      }

      public string Mark
      {
         get => _mark;
         set
         {
            _mark = value;
            OnPropertyChanged();
         }
      }

      private Level _level;

      public Level Level
      {
         get => _level;
         set
         {
            _level = value;
            OnPropertyChanged();
         }
      }

      public ElementType ElementType { get; set; }

      public int Number { get; set; }
   }
}
