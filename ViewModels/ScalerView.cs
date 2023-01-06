using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class ScalerView : ViewModelBase
    {
        private int _number;
        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _minDataSourceTemplateNames;
        public ObservableCollection<string> MinDataSourceTemplateNames
        {
            get { return _minDataSourceTemplateNames; }
            set
            {
                _minDataSourceTemplateNames = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _maxDataSourceTemplateNames;
        public ObservableCollection<string> MaxDataSourceTemplateNames
        {
            get { return _maxDataSourceTemplateNames; }
            set
            {
                _maxDataSourceTemplateNames = value;
                OnPropertyChanged();
            }
        }
    }
}
