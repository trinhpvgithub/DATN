using HcBimUtils.WPFUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TRINHTOOL.CreateLevel.Model;

namespace TRINHTOOL.CreateLevel.ViewModel
{
    public class CreateLevelViewModel : ViewModelBase
    {
        private CreateLevelModel _CreateLevelModel;
        public CreateLevelModel CreateLevelModel
        {
            get { return _CreateLevelModel; }
            set { _CreateLevelModel = value; OnPropertyChanged(); }
        }
        public RelayCommand StartCommand1 { get; set; }
        public RelayCommand StartCommand2 { get; set; }
        public RelayCommand CancelCommand { get; }
        public CreateLevelViewModel()
        {
            CreateLevelModel = new CreateLevelModel();
            StartCommand1 = new RelayCommand(Start1);
            StartCommand2 = new RelayCommand(Start2);
            CancelCommand = new RelayCommand(Cancel);
        }
        public void Start1(object obj)
        {
         var w = obj as Window;
            CreateLevelModel.CreateTang(obj);
        }
        public void Start2(object obj)
        {
            CreateLevelModel.CreateCaodo(obj);
        }
        public void Cancel(object obj)
        {
            if (obj is Window window)
            {
                window.Close();
            }
        }
    }
}
