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
            SelectedActivationFunction = ActivationFunctions[0];
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
                    DataSourceTemplateNnView dataSourceTemplateNnView = new DataSourceTemplateNnView { Name = "template" + (DataSourceTemplatesNnView.Count + 1).ToString(), LimitPrognosisCandles = new NumericUpDown(1, true, 1), IsOpenCandleNeuron = true, IsMaxMinCandleNeuron = true, IsCloseCandleNeuron = true, IsVolumeCandleNeuron = true };
                    DataSourceTemplatesNnView.Add(dataSourceTemplateNnView);
                    DataSourceGroupsView.Clear();
                    CreateDsPrognosisFiles();
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
                    DataSourceGroupsView.Clear();
                    SelectedTradingDataSourceTemplatesNnView = null;
                    CreateDsPrognosisFiles();
                }, (obj) => SelectedDataSourceTemplatesNnView != null);
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
                    CreateDsPrognosisFiles();
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
                    CreateDsPrognosisFiles();
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



        #region add delete NnSettingsVeiw
        private ObservableCollection<NnSettingsView> _nnSettingsViews = new ObservableCollection<NnSettingsView>();
        public ObservableCollection<NnSettingsView> NnSettingsViews
        {
            get { return _nnSettingsViews; }
            private set
            {
                _nnSettingsViews = value;
                OnPropertyChanged();
            }
        }
        private NnSettingsView _selectedNnSettingsView;
        public NnSettingsView SelectedNnSettingsView
        {
            get { return _selectedNnSettingsView; }
            set
            {
                _selectedNnSettingsView = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddNnSettings_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    NnSettingsView nnSettingsView = new NnSettingsView { Number = NnSettingsViews.Count + 1, LearningOffsetY = new NumericUpDown(0, true, 0), LearningOffsetM = new NumericUpDown(0, true, 0), LearningOffsetD = new NumericUpDown(0, true, 0), LearningPeriodsCount = new NumericUpDown(1, true, 1), LearningDurationY = new NumericUpDown(0, true, 0), LearningDurationM = new NumericUpDown(0, true, 0), LearningDurationD = new NumericUpDown(0, true, 0), LearningDistanceY = new NumericUpDown(0, true, 0), LearningDistanceM = new NumericUpDown(0, true, 0), LearningDistanceD = new NumericUpDown(0, true, 0), IsForwardTesting = true, ForwardOffsetY = new NumericUpDown(0, true, 0), ForwardOffsetM = new NumericUpDown(0, true, 0), ForwardOffsetD = new NumericUpDown(0, true, 0), ForwardPeriodsCount = new NumericUpDown(1, true, 1), ForwardDurationY = new NumericUpDown(0, true, 0), ForwardDurationM = new NumericUpDown(0, true, 0), ForwardDurationD = new NumericUpDown(0, true, 0), ForwardDistanceY = new NumericUpDown(0, true, 0), ForwardDistanceM = new NumericUpDown(0, true, 0), ForwardDistanceD = new NumericUpDown(0, true, 0) };
                    NnSettingsViews.Add(nnSettingsView);
                }, (obj) => true);
            }
        }
        public ICommand DeleteNnSettings_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    NnSettingsViews.Remove(SelectedNnSettingsView);
                    for (int i = 0; i < NnSettingsViews.Count; i++)
                    {
                        NnSettingsViews[i].Number = i + 1;
                    }
                }, (obj) => SelectedNnSettingsView != null);
            }
        }
        #endregion



        #region view add delete DsPrognosisFileView
        private ObservableCollection<DsPrognosisFileView> _dsPrognosisFilesView = new ObservableCollection<DsPrognosisFileView>();
        public ObservableCollection<DsPrognosisFileView> DsPrognosisFilesView
        {
            get { return _dsPrognosisFilesView; }
            private set
            {
                _dsPrognosisFilesView = value;
                OnPropertyChanged();
            }
        }
        private void CreateDsPrognosisFiles()
        {
            DsPrognosisFilesView.Clear();
            List<DataSource> dataSources = new List<DataSource>();
            for(int i = 0; i < DataSourceGroupsView.Count; i++) //формируем список со всеми источниками данных групп источников данных
            {
                for(int k = 0; k < DataSourceGroupsView[i].DataSourcesAccordances.Count; k++)
                {
                    if (!dataSources.Any(a => a.Id == DataSourceGroupsView[i].DataSourcesAccordances[k].DataSource.Id))
                    {
                        dataSources.Add(DataSourceGroupsView[i].DataSourcesAccordances[k].DataSource);
                    }
                }
            }
            for(int i = 0; i < dataSources.Count; i++)
            {
                DsPrognosisFilesView.Add(new DsPrognosisFileView { DataSource = dataSources[i] });
            }
        }
        #endregion



        #region Genetic Algorithm

        private int _inputLayerSize;
        public int InputLayerSize
        {
            get { return _inputLayerSize; }
            set
            {
                _inputLayerSize = value;
                OnPropertyChanged();
            }
        }
        private int _hiddenLayersCount = 2;
        public int HiddenLayersCount
        {
            get { return _hiddenLayersCount; }
            set
            {
                _hiddenLayersCount = value;
                OnPropertyChanged();
            }
        }
        private string _firstHiddenLayerCountNeurons = "1";
        public string FirstHiddenLayerCountNeurons
        {
            get { return _firstHiddenLayerCountNeurons; }
            set
            {
                if (int.TryParse(value, out int res))
                {
                    _firstHiddenLayerCountNeurons = value;
                }
                OnPropertyChanged();
            }
        }
        private string _secondHiddenLayerCountNeurons = "1";
        public string SecondHiddenLayerCountNeurons
        {
            get { return _secondHiddenLayerCountNeurons; }
            set
            {
                if (int.TryParse(value, out int res))
                {
                    _secondHiddenLayerCountNeurons = value;
                }
                OnPropertyChanged();
            }
        }
        private string _thirdHiddenLayerCountNeurons = "1";
        public string ThirdHiddenLayerCountNeurons
        {
            get { return _thirdHiddenLayerCountNeurons; }
            set
            {
                if (int.TryParse(value, out int res))
                {
                    _thirdHiddenLayerCountNeurons = value;
                }
                OnPropertyChanged();
            }
        }
        private string _fourthHiddenLayerCountNeurons = "1";
        public string FourthHiddenLayerCountNeurons
        {
            get { return _fourthHiddenLayerCountNeurons; }
            set
            {
                if (int.TryParse(value, out int res))
                {
                    _fourthHiddenLayerCountNeurons = value;
                }
                OnPropertyChanged();
            }
        }
        private string _fifthHiddenLayerCountNeurons = "1";
        public string FifthHiddenLayerCountNeurons
        {
            get { return _fifthHiddenLayerCountNeurons; }
            set
            {
                if (int.TryParse(value, out int res))
                {
                    _fifthHiddenLayerCountNeurons = value;
                }
                OnPropertyChanged();
            }
        }
        private string _weightsMin = "-5";
        public string WeightsMin
        {
            get { return _weightsMin; }
            set
            {
                if (int.TryParse(value, out int res))
                {
                    _weightsMin = value;
                }
                OnPropertyChanged();
            }
        }
        private string _weightsMax = "5";
        public string WeightsMax
        {
            get { return _weightsMax; }
            set
            {
                if (int.TryParse(value, out int res))
                {
                    _weightsMax = value;
                }
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _activationFunctions = new ObservableCollection<string>() { "relu", "leaky relu", "sigmoid" };
        public ObservableCollection<string> ActivationFunctions
        {
            get { return _activationFunctions; }
            set
            {
                _activationFunctions = value;
                OnPropertyChanged();
            }
        }
        private string _selectedActivationFunction;
        public string SelectedActivationFunction
        {
            get { return _selectedActivationFunction; }
            set
            {
                _selectedActivationFunction = value;
                OnPropertyChanged();
            }
        }
        private int _genomeLength;
        public int GenomeLength
        {
            get { return _genomeLength; }
            set
            {
                _genomeLength = value;
                OnPropertyChanged();
            }
        }
        private string _populationSize = "100";
        public string PopulationSize
        {
            get { return _populationSize; }
            set
            {
                if (int.TryParse(value, out int res))
                {
                    _populationSize = value;
                }
                OnPropertyChanged();
            }
        }

        private ObservableCollection<StepsSettingsView> _stepsSettingsViews = new ObservableCollection<StepsSettingsView>();
        public ObservableCollection<StepsSettingsView> StepsSettingsViews
        {
            get { return _stepsSettingsViews; }
            private set
            {
                _stepsSettingsViews = value;
                OnPropertyChanged();
            }
        }
        private StepsSettingsView _selectedStepsSettingsViews;
        public StepsSettingsView SelectedStepsSettingsViews
        {
            get { return _selectedStepsSettingsViews; }
            set
            {
                _selectedStepsSettingsViews = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddStepsSettingsView_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    StepsSettingsView stepsSettingsView = new StepsSettingsView { GenerationsDuration = "100", MutationProbability = "0,01", FitnessScaleRate = "0" };
                    StepsSettingsViews.Add(stepsSettingsView);
                }, (obj) => true);
            }
        }
        public ICommand DeleteStepsSettingsView_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    StepsSettingsViews.Remove(SelectedStepsSettingsViews);
                }, (obj) => SelectedStepsSettingsViews != null);
            }
        }
        private bool _useFitnessReduce = false;
        public bool UseFitnessReduce
        {
            get { return _useFitnessReduce; }
            set
            {
                _useFitnessReduce = value;
                OnPropertyChanged();
            }
        }
        private string _intervalsCount = "1";
        public string IntervalsCount
        {
            get { return _intervalsCount; }
            set
            {
                if (int.TryParse(value, out int res))
                {
                    _intervalsCount = value;
                }
                OnPropertyChanged();
            }
        }
        private string _volatilityMultiply = "0,38";
        public string VolatilityMultiply
        {
            get { return _volatilityMultiply; }
            set
            {
                if (double.TryParse(value, out double res))
                {
                    _volatilityMultiply = value;
                }
                OnPropertyChanged();
            }
        }
        #endregion
    }
}
