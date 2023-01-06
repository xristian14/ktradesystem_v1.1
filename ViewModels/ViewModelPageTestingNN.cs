using ktradesystem.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTestingNN : ViewModelBase
    {
        private static ViewModelPageTestingNN _instance;
        public static ViewModelPageTestingNN getInstance()
        {
            if (_instance == null)
            {
                _instance = new ViewModelPageTestingNN();
            }
            return _instance;
        }

        private ViewmodelData _viewmodelData;
        public ViewmodelData viewmodelData
        {
            get
            {
                if (_viewmodelData == null)
                {
                    _viewmodelData = ViewmodelData.getInstance();
                }
                return _viewmodelData;
            }
        }

        public void AdditionalWindow_Closing(object sender, CancelEventArgs e)
        {
            viewmodelData.IsPagesAndMainMenuButtonsEnabled = true;
            CloseAdditionalWindowAction = null; //сбрасываем Action, чтобы при инициализации нового окна в него поместился метод его закрытия
        }
        public Action CloseAdditionalWindowAction { get; set; }

        private ObservableCollection<string> _buttonsTooltip = new ObservableCollection<string>();
        public ObservableCollection<string> ButtonsTooltip //подсказка, показываемая при наведении на кнопку
        {
            get { return _buttonsTooltip; }
            set
            {
                _buttonsTooltip = value;
                OnPropertyChanged();
            }
        }



        #region add delete DataSourceTemplates
        private ObservableCollection<DataSourceTemplateNnView> _dataSourceTemplatesNnView = new ObservableCollection<DataSourceTemplateNnView>();
        public ObservableCollection<DataSourceTemplateNnView> DataSourceTemplatesNnView
        {
            get { return _dataSourceTemplatesNnView; }
            private set
            {
                _dataSourceTemplatesNnView = value;
                OnPropertyChanged();
            }
        }
        private DataSourceTemplateNnView _selectedDataSourceTemplatesNnView;
        public DataSourceTemplateNnView SelectedDataSourceTemplatesNnView
        {
            get { return _selectedDataSourceTemplatesNnView; }
            set
            {
                _selectedDataSourceTemplatesNnView = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    DataSourceTemplateNnView dataSourceTemplateNnView = new DataSourceTemplateNnView { Name = "template" + (DataSourceTemplatesNnView.Count + 1).ToString(), InputLayerCandleCount = new NumericUpDown(1, false), LastCandleOffset = new NumericUpDown(0, false), IsOpenCandleNeuron = true, IsMaxMinCandleNeuron = true, IsCloseCandleNeuron = true, IsVolumeCandleNeuron = true, Scalers = ScalersView, IsScaleShowingNeurons = false };
                    DataSourceTemplatesNnView.Add(dataSourceTemplateNnView);
                }, (obj) => true);
            }
        }
        public ICommand DeleteDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    DataSourceTemplatesNnView.Remove(SelectedDataSourceTemplatesNnView);
                    for(int i = 0; i < DataSourceTemplatesNnView.Count; i++)
                    {
                        DataSourceTemplatesNnView[i].Name = "template" + (i + 1).ToString();
                    }
                    ScalersView.Clear();
                }, (obj) => SelectedDataSourceTemplatesNnView != null);
            }
        }
        #endregion



        #region add edit delete Scalers
        private ObservableCollection<ScalerView> _scalersView = new ObservableCollection<ScalerView>();
        public ObservableCollection<ScalerView> ScalersView
        {
            get { return _scalersView; }
            private set
            {
                _scalersView = value;
                OnPropertyChanged();
            }
        }
        private ScalerView _selectedScalerView;
        public ScalerView SelectedScalerView
        {
            get { return _selectedScalerView; }
            set
            {
                _selectedScalerView = value;
                OnPropertyChanged();
            }
        }
        private const string ZERO_SCALER = "Zero";
        private bool _isAddScaler = false; //добавляется или редактируется Scaler
        private int _scalerNumber;
        public int ScalerNumber
        {
            get { return _scalerNumber; }
            set
            {
                _scalerNumber = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _minScalerDataSourceTemplates = new ObservableCollection<string>();
        public ObservableCollection<string> MinScalerDataSourceTemplates //список с доступными для выбора шаблонами источников данных в minScaler
        {
            get { return _minScalerDataSourceTemplates; }
            private set
            {
                _minScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private string _selectedMinScalerDataSourceTemplates;
        public string SelectedMinScalerDataSourceTemplates
        {
            get { return _selectedMinScalerDataSourceTemplates; }
            set
            {
                _selectedMinScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _maxScalerDataSourceTemplates = new ObservableCollection<string>();
        public ObservableCollection<string> MaxScalerDataSourceTemplates //список с доступными для выбора шаблонами источников данных в maxScaler
        {
            get { return _maxScalerDataSourceTemplates; }
            private set
            {
                _maxScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private string _selectedMaxScalerDataSourceTemplates;
        public string SelectedMaxScalerDataSourceTemplates
        {
            get { return _selectedMaxScalerDataSourceTemplates; }
            set
            {
                _selectedMaxScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _listSelectedMinScalerDataSourceTemplates = new ObservableCollection<string>();
        public ObservableCollection<string> ListSelectedMinScalerDataSourceTemplates //список с выбранными шаблонами источников данных в minScaler
        {
            get { return _listSelectedMinScalerDataSourceTemplates; }
            private set
            {
                _listSelectedMinScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private string _selectedListSelectedMinScalerDataSourceTemplates;
        public string SelectedListSelectedMinScalerDataSourceTemplates
        {
            get { return _selectedListSelectedMinScalerDataSourceTemplates; }
            set
            {
                _selectedListSelectedMinScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _listSelectedMaxScalerDataSourceTemplates = new ObservableCollection<string>();
        public ObservableCollection<string> ListSelectedMaxScalerDataSourceTemplates //список с выбранными шаблонами источников данных в maxScaler
        {
            get { return _listSelectedMaxScalerDataSourceTemplates; }
            private set
            {
                _listSelectedMaxScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private string _selectedListSelectedMaxScalerDataSourceTemplates;
        public string SelectedListSelectedMaxScalerDataSourceTemplates
        {
            get { return _selectedListSelectedMaxScalerDataSourceTemplates; }
            set
            {
                _selectedListSelectedMaxScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private void UpdateMinMaxScalerDataSourceTemplates()
        {
            ListSelectedMinScalerDataSourceTemplates.Clear();
            ListSelectedMaxScalerDataSourceTemplates.Clear();
            SelectedListSelectedMinScalerDataSourceTemplates = "";
            SelectedListSelectedMaxScalerDataSourceTemplates = "";
            MinScalerDataSourceTemplates.Clear();
            MaxScalerDataSourceTemplates.Clear();
            MinScalerDataSourceTemplates.Add(ZERO_SCALER);
            for(int i = 0; i < DataSourceTemplatesNnView.Count; i++)
            {
                MinScalerDataSourceTemplates.Add(DataSourceTemplatesNnView[i].Name);
                MaxScalerDataSourceTemplates.Add(DataSourceTemplatesNnView[i].Name);
            }
            SelectedMinScalerDataSourceTemplates = MinScalerDataSourceTemplates[0];
            SelectedMaxScalerDataSourceTemplates = MaxScalerDataSourceTemplates.Count > 0 ? MaxScalerDataSourceTemplates[0] : "";
        }
        public ICommand AddScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _isAddScaler = true;
                    UpdateMinMaxScalerDataSourceTemplates();
                    ScalerNumber = ScalersView.Count + 1;
                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewAddEditScaler viewAddEditScaler = new ViewAddEditScaler();
                    viewAddEditScaler.Show();
                }, (obj) => true);
            }
        }
        public ICommand EditScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _isAddScaler = false;
                    UpdateMinMaxScalerDataSourceTemplates();
                    ScalerNumber = SelectedScalerView.Number;
                    ListSelectedMinScalerDataSourceTemplates = new ObservableCollection<string>(SelectedScalerView.MinDataSourceTemplateNames);
                    ListSelectedMaxScalerDataSourceTemplates = new ObservableCollection<string>(SelectedScalerView.MaxDataSourceTemplateNames);
                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewAddEditScaler viewAddEditScaler = new ViewAddEditScaler();
                    viewAddEditScaler.Show();
                }, (obj) => SelectedScalerView != null);
            }
        }
        public ICommand DeleteScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ScalersView.Remove(SelectedScalerView);
                    for(int i = 0; i < ScalersView.Count; i++)
                    {
                        ScalersView[i].Number = i + 1;
                    }
                }, (obj) => SelectedScalerView != null);
            }
        }
        private bool CheckAddEditScalerFields()
        {
            ButtonsTooltip.Clear();
            bool res = true;
            if(ListSelectedMinScalerDataSourceTemplates.Count == 0)
            {
                res = false;
                ButtonsTooltip.Add("Список с минимальными значениями не должен быть пустым.");
            }
            if (ListSelectedMaxScalerDataSourceTemplates.Count == 0)
            {
                res = false;
                ButtonsTooltip.Add("Список с максимальными значениями не должен быть пустым.");
            }
            return res;
        }
        public ICommand SaveScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (_isAddScaler)
                    {
                        ScalerView scalerView = new ScalerView { Number = ScalerNumber, MinDataSourceTemplateNames = new ObservableCollection<string>(ListSelectedMinScalerDataSourceTemplates), MaxDataSourceTemplateNames = new ObservableCollection<string>(ListSelectedMaxScalerDataSourceTemplates) };
                        ScalersView.Add(scalerView);
                    }
                    else
                    {
                        SelectedScalerView.MinDataSourceTemplateNames = new ObservableCollection<string>(ListSelectedMinScalerDataSourceTemplates);
                        SelectedScalerView.MaxDataSourceTemplateNames = new ObservableCollection<string>(ListSelectedMaxScalerDataSourceTemplates);
                    }
                    CloseAdditionalWindowAction?.Invoke();
                }, (obj) => CheckAddEditScalerFields());
            }
        }
        public ICommand CancelScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CloseAdditionalWindowAction?.Invoke();
                }, (obj) => true);
            }
        }
        public ICommand AddMinScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (!ListSelectedMinScalerDataSourceTemplates.Contains(SelectedMinScalerDataSourceTemplates))
                    {
                        ListSelectedMinScalerDataSourceTemplates.Add(SelectedMinScalerDataSourceTemplates);
                    }
                }, (obj) => MinScalerDataSourceTemplates.Contains(SelectedMinScalerDataSourceTemplates));
            }
        }
        public ICommand DeleteMinScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ListSelectedMinScalerDataSourceTemplates.Remove(SelectedListSelectedMinScalerDataSourceTemplates);
                }, (obj) => ListSelectedMinScalerDataSourceTemplates.Contains(SelectedListSelectedMinScalerDataSourceTemplates));
            }
        }
        public ICommand AddMaxScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (!ListSelectedMaxScalerDataSourceTemplates.Contains(SelectedMaxScalerDataSourceTemplates))
                    {
                        ListSelectedMaxScalerDataSourceTemplates.Add(SelectedMaxScalerDataSourceTemplates);
                    }
                }, (obj) => MaxScalerDataSourceTemplates.Contains(SelectedMaxScalerDataSourceTemplates));
            }
        }
        public ICommand DeleteMaxScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ListSelectedMaxScalerDataSourceTemplates.Remove(SelectedListSelectedMaxScalerDataSourceTemplates);
                }, (obj) => ListSelectedMaxScalerDataSourceTemplates.Contains(SelectedListSelectedMaxScalerDataSourceTemplates));
            }
        }
        #endregion
    }
}
