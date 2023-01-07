using ktradesystem.Models;
using ktradesystem.Models.Datatables;
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
        private ViewModelPageTestingNN()
        {
            _modelData = ModelData.getInstance();
            _viewModelPageDataSource = ViewModelPageDataSource.getInstance();
            Currencies = _modelData.Currencies;
            SelectedCurrency = Currencies[0];
        }
        private ViewModelPageDataSource _viewModelPageDataSource;
        private ModelData _modelData;
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
        private DataSourceTemplateNnView _selectedTradingDataSourceTemplatesNnView;
        public DataSourceTemplateNnView SelectedTradingDataSourceTemplatesNnView //шаблон источника данных, по которому вести торговлю
        {
            get { return _selectedTradingDataSourceTemplatesNnView; }
            set
            {
                _selectedTradingDataSourceTemplatesNnView = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    DataSourceTemplateNnView dataSourceTemplateNnView = new DataSourceTemplateNnView { Name = "template" + (DataSourceTemplatesNnView.Count + 1).ToString(), InputLayerCandleCount = new NumericUpDown(1, true, 1), LastCandleOffset = new NumericUpDown(0, true, 0), IsOpenCandleNeuron = true, IsMaxMinCandleNeuron = true, IsCloseCandleNeuron = true, IsVolumeCandleNeuron = true, Scalers = ScalersView, IsScaleShowingNeurons = false };
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
                    DataSourceGroupsView.Clear();
                    SelectedTradingDataSourceTemplatesNnView = null;
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



        #region view add delete DataSourceGroupsView
        private ObservableCollection<Currency> _currencies;
        public ObservableCollection<Currency> Currencies
        {
            get { return _currencies; }
            set
            {
                _currencies = value;
                OnPropertyChanged();
            }
        }
        private Currency _selectedCurrency;
        public Currency SelectedCurrency
        {
            get { return _selectedCurrency; }
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<DataSourceGroupView> _dataSourceGroupsView = new ObservableCollection<DataSourceGroupView>();
        public ObservableCollection<DataSourceGroupView> DataSourceGroupsView
        {
            get { return _dataSourceGroupsView; }
            private set
            {
                _dataSourceGroupsView = value;
                OnPropertyChanged();
            }
        }
        private DataSourceGroupView _selectedDataSourceGroupView;
        public DataSourceGroupView SelectedDataSourceGroupView //выбранная группа источников данных
        {
            get { return _selectedDataSourceGroupView; }
            set
            {
                _selectedDataSourceGroupView = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DataSource> DataSources { get; set; } //список источников данных для combobox
        private void CreateDataSourceGroupView()
        {
            DataSources = new ObservableCollection<DataSource>(_viewModelPageDataSource.DataSourcesForSubscribers);
        }
        private ObservableCollection<DataSourcesForAddingDsGroupView> _dataSourcesForAddingDsGroupsView = new ObservableCollection<DataSourcesForAddingDsGroupView>();
        public ObservableCollection<DataSourcesForAddingDsGroupView> DataSourcesForAddingDsGroupsView //список с элементами для окна добавления группы источников данных
        {
            get { return _dataSourcesForAddingDsGroupsView; }
            set
            {
                _dataSourcesForAddingDsGroupsView = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddDataSourceGroupView_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CreateDataSourceGroupView();
                    DataSourcesForAddingDsGroupsView.Clear();
                    foreach (DataSourceTemplateNnView dataSourceTemplateNnView in DataSourceTemplatesNnView)
                    {
                        DataSourcesForAddingDsGroupsView.Add(new DataSourcesForAddingDsGroupView { DataSources = _viewModelPageDataSource.DataSourcesForSubscribers, DataSourceTemplate = new DataSourceTemplate { Name = dataSourceTemplateNnView.Name } });
                    }
                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewAddDataSourceGroupNn viewAddDataSourceGroupNn = new ViewAddDataSourceGroupNn();
                    viewAddDataSourceGroupNn.Show();
                }, (obj) => DataSourceTemplatesNnView.Count > 0);
            }
        }
        public ICommand DeleteDataSourceGroupView_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    DataSourceGroupsView.Remove(SelectedDataSourceGroupView);
                    //заново пронумеровывем элементы списка
                    for (int i = 0; i < DataSourceGroupsView.Count; i++)
                    {
                        DataSourceGroupsView[i].Number = i + 1;
                    }
                }, (obj) => SelectedDataSourceGroupView != null);
            }
        }
        private bool CheckAddDataSourceGroupFields()
        {
            bool result = true;

            ButtonsTooltip.Clear(); //очищаем подсказку кнопки добавить
            //проверяем на заполненность полей
            bool isAllDataSourceSelected = true;
            foreach (DataSourcesForAddingDsGroupView dataSourcesForAddingDsGroupView in DataSourcesForAddingDsGroupsView)
            {
                if (dataSourcesForAddingDsGroupView.SelectedDataSource == null)
                {
                    isAllDataSourceSelected = false;
                }
            }
            if (isAllDataSourceSelected)
            {
                //удостоверяемся в том что заполненные поля не содержат одинаковых источников данных
                bool isFindEqual = false;
                for (int i = 0; i < DataSourcesForAddingDsGroupsView.Count; i++)
                {
                    for (int k = 0; k < DataSourcesForAddingDsGroupsView.Count; k++)
                    {
                        if (i != k)
                        {
                            if (DataSourcesForAddingDsGroupsView[i].SelectedDataSource == DataSourcesForAddingDsGroupsView[k].SelectedDataSource)
                            {
                                isFindEqual = true;
                            }
                        }
                    }
                }
                if (isFindEqual == false)
                {
                    //проверяем, имеют ли все выбранные источники данных общие даты (находим саму последнюю дату начала и самую раннюю дату окончания, если начало больше окончания, значит общих дат нет)
                    DateTime startDate = DataSourcesForAddingDsGroupsView[0].SelectedDataSource.StartDate;
                    DateTime endDate = DataSourcesForAddingDsGroupsView[0].SelectedDataSource.EndDate;
                    for (int i = 1; i < DataSourcesForAddingDsGroupsView.Count; i++)
                    {
                        if (DateTime.Compare(startDate, DataSourcesForAddingDsGroupsView[i].SelectedDataSource.StartDate) < 0)
                        {
                            startDate = DataSourcesForAddingDsGroupsView[i].SelectedDataSource.StartDate;
                        }
                        if (DateTime.Compare(endDate, DataSourcesForAddingDsGroupsView[i].SelectedDataSource.EndDate) > 0)
                        {
                            endDate = DataSourcesForAddingDsGroupsView[i].SelectedDataSource.EndDate;
                        }
                    }
                    if (DateTime.Compare(startDate, endDate) < 0) //если true, то общие даты найдены
                    {
                        //проверяем на уникальность комбинации источников данных
                        bool isUnuque = true;
                        foreach (DataSourceGroupView dataSourceGroupView in DataSourceGroupsView)
                        {
                            bool isFind = true;
                            //если хоть один DataSourcesAccordance не имеет полного совпадения, значи все в порядке
                            for (int i = 0; i < dataSourceGroupView.DataSourcesAccordances.Count; i++)
                            {
                                if ((dataSourceGroupView.DataSourcesAccordances[i].DataSourceTemplate.Name == DataSourcesForAddingDsGroupsView[i].DataSourceTemplate.Name && dataSourceGroupView.DataSourcesAccordances[i].DataSource == DataSourcesForAddingDsGroupsView[i].SelectedDataSource) == false) //данный DataSourcesAccordance не имеет полного совпадения
                                {
                                    isFind = false;
                                }
                            }
                            if (isFind)
                            {
                                isUnuque = false;
                            }
                        }
                        if (isUnuque == false)
                        {
                            ButtonsTooltip.Add("Данная комбинация источников данных уже добавлена, выберите другую.");
                            result = false;
                        }
                    }
                }
                else
                {
                    ButtonsTooltip.Add("Выбран один источник данных для нескольких шаблонов.");
                    result = false;
                }
            }
            else
            {
                ButtonsTooltip.Add("Не выбраны источники данных.");
                result = false;
            }

            return result;
        }
        public ICommand DataSourceGroupViewSave_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //определяем доступные даты тестирования для данной группы источников данных. Доступная дата начала - самая поздняя из дат начала, доступная дата окончания - самая поздняя дата окончания
                    DateTime startDateTime = new DateTime();
                    DateTime endDateTime = new DateTime();
                    bool isFirstIteration = true;
                    List<DataSourceAccordanceView> dataSourcesAccordances = new List<DataSourceAccordanceView>();
                    foreach (DataSourcesForAddingDsGroupView dataSourcesForAddingDsGroupView in DataSourcesForAddingDsGroupsView)
                    {
                        dataSourcesAccordances.Add(new DataSourceAccordanceView { DataSourceTemplate = dataSourcesForAddingDsGroupView.DataSourceTemplate, DataSource = dataSourcesForAddingDsGroupView.SelectedDataSource });
                        if (isFirstIteration)
                        {
                            startDateTime = dataSourcesForAddingDsGroupView.SelectedDataSource.StartDate;
                            endDateTime = dataSourcesForAddingDsGroupView.SelectedDataSource.EndDate;
                        }
                        else
                        {
                            if (DateTime.Compare(startDateTime, dataSourcesForAddingDsGroupView.SelectedDataSource.StartDate) < 0)
                            {
                                startDateTime = dataSourcesForAddingDsGroupView.SelectedDataSource.StartDate;
                            }
                            if (DateTime.Compare(endDateTime, dataSourcesForAddingDsGroupView.SelectedDataSource.EndDate) < 0)
                            {
                                endDateTime = dataSourcesForAddingDsGroupView.SelectedDataSource.EndDate;
                            }
                        }
                    }
                    DataSourceGroupsView.Add(new DataSourceGroupView(DataSourceGroupsView.Count + 1, dataSourcesAccordances, startDateTime, endDateTime));
                    SelectedCurrency = Currencies.Where(a => a.Id == dataSourcesAccordances[0].DataSource.Currency.Id).First(); //выбираем валюту как у добавленного источника данных
                    CloseAdditionalWindowAction?.Invoke();
                }, (obj) => CheckAddDataSourceGroupFields());
            }
        }
        public ICommand DataSourceGroupViewCancel_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CloseAdditionalWindowAction?.Invoke();
                }, (obj) => true);
            }
        }
        #endregion
    }
}
