using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;
using Ookii.Dialogs.Wpf;

namespace ktradesystem.ViewModels
{
    class DsPrognosisFileView : ViewModelBase
    {
        private DataSource _dataSource;
        public DataSource DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                OnPropertyChanged();
            }
        }
        private bool _isSelectFileClick = false;
        public bool IsSelectFileClick
        {
            get { return _isSelectFileClick; }
            set
            {
                if (value)
                {
                    VistaOpenFileDialog vistaOpenFileDialog = new VistaOpenFileDialog();
                    vistaOpenFileDialog.Multiselect = false;
                    bool? isSuccess = vistaOpenFileDialog.ShowDialog();
                    if (isSuccess == true)
                    {
                        FilePath = vistaOpenFileDialog.FileName;
                    }
                }
                OnPropertyChanged();
            }
        }
        private string _filePath = "";
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }
    }
}
