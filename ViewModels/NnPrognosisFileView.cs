using ktradesystem.Models.Datatables;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class NnPrognosisFileView : ViewModelBase
    {
        private string _nnNumber = "";
        public string NnNumber
        {
            get { return _nnNumber; }
            set
            {
                _nnNumber = value;
                OnPropertyChanged();
            }
        }
        private string _dsGroupNumber = "";
        public string DsGroupNumber
        {
            get { return _dsGroupNumber; }
            set
            {
                _dsGroupNumber = value;
                OnPropertyChanged();
            }
        }
        private DataSourceTemplateNnView _dataSourceTemplateNnView = new DataSourceTemplateNnView();
        public DataSourceTemplateNnView DataSourceTemplateNnView
        {
            get { return _dataSourceTemplateNnView; }
            set
            {
                _dataSourceTemplateNnView = value;
                OnPropertyChanged();
            }
        }
        private DataSource _dataSource = new DataSource();
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
