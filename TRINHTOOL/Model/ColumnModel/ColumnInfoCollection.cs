using Autodesk.Revit.DB;
using HcBimUtils.WPFUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRINHTOOL.Model.ColumnModel
{
    public class ColumnInfoCollection : ViewModelBase
    {
        public List<ColumnInfo> ColumnInfos { get; set; } = new List<ColumnInfo>();

        private double width;

        public double Width
        {
            get => width;
            set
            {
                width = value;
                OnPropertyChanged();
            }
        }

        private double height;

        public double Height
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged();
            }
        }

        private string text;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged();
            }
        }

        public ElementType ElementType { get; set; }

        public int Number { get; set; }



        public ColumnInfoCollection()
        {

        }
    }
}
