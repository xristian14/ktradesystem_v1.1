using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using ktradesystem.Views;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;
using System.Collections.Specialized;
using System.Diagnostics;


namespace ktradesystem.ViewModels
{
    class ViewModelPageTesting : ViewModelBase
    {
        private static ViewModelPageTesting _instance;

        private ViewModelPageTesting()
        {
            _modelData = ModelData.getInstance();
            _modelData.PropertyChanged += Model_PropertyChanged;

            _modelData.Indicators.CollectionChanged += modelData_IndicatorsCollectionChanged;
            Indicators = _modelData.Indicators;

            _modelData.IndicatorParameterTemplates.CollectionChanged += modelData_IndicatorParameterTemplatesCollectionChanged;
            IndicatorParameterTemplates = _modelData.IndicatorParameterTemplates;

            _modelData.ParameterValueTypes.CollectionChanged += modelData_ParameterValueTypesCollectionChanged;
            ParameterValueTypes = _modelData.ParameterValueTypes;

            _modelData.DataSourceTemplates.CollectionChanged += modelData_DataSourceTemplatesCollectionChanged;
            DataSourceTemplates = _modelData.DataSourceTemplates;

            _modelData.AlgorithmIndicators.CollectionChanged += modelData_AlgorithmIndicatorsCollectionChanged;
            AlgorithmIndicators = _modelData.AlgorithmIndicators;

            _modelData.Algorithms.CollectionChanged += modelData_AlgorithmsCollectionChanged;
            Algorithms = _modelData.Algorithms;

            _modelData.EvaluationCriterias.CollectionChanged += modelData_EvaluationCriteriasChanged;
            EvaluationCriterias = _modelData.EvaluationCriterias;

            _modelTesting = ModelTesting.getInstance();
            _viewModelPageDataSource = ViewModelPageDataSource.getInstance();

            Currencies = _modelData.Currencies;
            SelectedCurrency = Currencies[0];
        }

        public static ViewModelPageTesting getInstance()
        {
            if (_instance == null)
            {
                _instance = new ViewModelPageTesting();
            }
            return _instance;
        }

        private ModelData _modelData;
        private ModelTesting _modelTesting;
        private ViewModelPageDataSource _viewModelPageDataSource;

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var fieldViewModel = this.GetType().GetProperty(e.PropertyName);
            var fieldModel = sender.GetType().GetProperty(e.PropertyName);
            fieldViewModel?.SetValue(this, fieldModel.GetValue(sender));
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










        #region view add edit delete Indicators

        public System.Windows.Controls.TextBox IndicatorScriptTextBox; //ссылка на textBox с текстом скрипта, для обращения к свойству CaretIndex

        private ObservableCollection<Indicator> _indicators = new ObservableCollection<Indicator>(); //индикаторы
        public ObservableCollection<Indicator> Indicators
        {
            get { return _indicators; }
            private set
            {
                _indicators = value;
                OnPropertyChanged();
            }
        }

        private void modelData_IndicatorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Indicators = (ObservableCollection<Indicator>)sender;
        }

        private Indicator _selectedIndicator;
        public Indicator SelectedIndicator //выбранный индикатор
        {
            get { return _selectedIndicator; }
            set
            {
                _selectedIndicator = value;
                OnPropertyChanged();
                SelectedIndicatorChanged(); //помещяет в переменные редактора значения выбранного индикатора для просмотра данного индиктора
            }
        }

        private bool _isIndicatorAdded = false;
        public bool IsIndicatorAdded //флаг того что в настоящий момент создается новый индикатор
        {
            get { return _isIndicatorAdded; }
            set
            {
                _isIndicatorAdded = value;
                OnPropertyChanged();
            }
        }

        private bool _isIndicatorEdited = false;
        public bool IsIndicatorEdited //флаг того что в настоящий момент редактируется индикатор
        {
            get { return _isIndicatorEdited; }
            set
            {
                _isIndicatorEdited = value;
                OnPropertyChanged();
            }
        }

        private bool _isIndicatorReadOnly = true;
        public bool IsIndicatorReadOnly //на эту переменную привязаны свойства readonly полей редактирования индикатора
        {
            get { return _isIndicatorReadOnly; }
            set
            {
                _isIndicatorReadOnly = value;
                OnPropertyChanged();
            }
        }

        private void SelectedIndicatorChanged() //помещяет в переменные редактора значения выбранного индикатора для просмотра данного индиктора
        {
            if(SelectedIndicator != null)
            {
                _indicatorId = SelectedIndicator.Id;
                IndicatorName = SelectedIndicator.Name;
                IndicatorDescription = SelectedIndicator.Description;
                IndicatorParameterTemplatesView.Clear();
                foreach(IndicatorParameterTemplate parameterTemplate in SelectedIndicator.IndicatorParameterTemplates)
                {
                    IndicatorParameterTemplateView indicatorParameterTemplateView = new IndicatorParameterTemplateView { Id = parameterTemplate.Id, Name = parameterTemplate.Name, Description = parameterTemplate.Description, IdIndicator = parameterTemplate.IdIndicator, ParameterValueType = parameterTemplate.ParameterValueType, Indicator = parameterTemplate.Indicator };
                    IndicatorParameterTemplatesView.Add(indicatorParameterTemplateView);

                }
                IndicatorScript = SelectedIndicator.Script;
            }
            else
            {
                IndicatorName = "";
                IndicatorDescription = "";
                IndicatorParameterTemplatesView.Clear();
                IndicatorScript = "";
            }
        }

        private string _indicatorScript = "";
        public string IndicatorScript //скрипт индикатора
        {
            get { return _indicatorScript; }
            set
            {
                _indicatorScript = value;
                OnPropertyChanged();
            }
        }

        private string _indicatorStatusText = "Просмотр";
        public string IndicatorStatusText //статус работы с выбранным индикатором
        {
            get { return _indicatorStatusText; }
            set
            {
                _indicatorStatusText = value;
                OnPropertyChanged();
            }
        }

        int _indicatorId;

        private string _indicatorName;
        public string IndicatorName //название индикатора
        {
            get { return _indicatorName; }
            set
            {
                _indicatorName = value;
                OnPropertyChanged();
            }
        }

        private string _indicatorDescription;
        public string IndicatorDescription //описание индикатора
        {
            get { return _indicatorDescription; }
            set
            {
                _indicatorDescription = value;
                OnPropertyChanged();
            }
        }

        private void UpdateIndicatorStatusText()
        {
            if (IsIndicatorAdded == true)
            {
                IndicatorStatusText = "Добавление";
                IsIndicatorReadOnly = false;
            }
            else if (IsIndicatorEdited == true)
            {
                IndicatorStatusText = "Редактирование";
                IsIndicatorReadOnly = false;
            }
            else
            {
                IndicatorStatusText = "Просмотр";
                IsIndicatorReadOnly = true;
            }
        }

        private bool IsAddOrEditIndicator() //добавляется ли новый индикатор в данный момент, млм редактируется ли имеющийся
        {
            return IsIndicatorAdded || IsIndicatorEdited;
        }

        public ICommand IndicatorsAdd_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IsIndicatorAdded = true;
                    UpdateIndicatorStatusText();
                    SelectedIndicator = null;
                    SelectedIndicatorChanged();
                }, (obj) => !IsAddOrEditIndicator() && !IsAddOrEditAlgorithm());
            }
        }

        public ICommand IndicatorsEdit_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IsIndicatorEdited = true;
                    UpdateIndicatorStatusText();
                }, (obj) => !IsAddOrEditIndicator() && !IsAddOrEditAlgorithm() && SelectedIndicator != null );
            }
        }

        public ICommand IndicatorsDelete_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    string msg = " Id: " + SelectedIndicator.Id + "  Название: " + SelectedIndicator.Name;
                    string caption = "Удалить индикатор?";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == MessageBoxResult.Yes)
                    {
                        _modelTesting.IndicatorDelete(SelectedIndicator.Id);
                    }
                }, (obj) => !IsAddOrEditIndicator() && !IsAddOrEditAlgorithm() && SelectedIndicator != null );
            }
        }

        public ICommand IndicatorResetToTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = Environment.NewLine + Environment.NewLine + "Indicator = 0;";
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteCandles_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "Candles[0]");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteSelectedParameter_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "Parameter_" + SelectedIndicatorParameterTemplateView.Name);
                }, (obj) => SelectedIndicatorParameterTemplateView != null && IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteReturn_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "Indicator = ");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteCondition_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "if (  ) {" + Environment.NewLine + Environment.NewLine + "} else {" + Environment.NewLine + Environment.NewLine + "}");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteWhile_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "int i = 0;" + Environment.NewLine + "while (  ) {" + Environment.NewLine + Environment.NewLine + "i++;" + Environment.NewLine + "}");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteLogicMore_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, ">");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteLogicLess_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "<");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteLogicMoreOrEqual_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, ">=");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteLogicLessOrEqual_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "<=");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteLogicEqual_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "==");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteLogicUnequal_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "!=");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteLogicAnd_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "&&");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteLogicOr_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "||");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteVarInt_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "int var1 = 0;");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteVarDouble_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "double var1 = 0;");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        private ObservableCollection<string> _tooltipAddAddIndicator = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipAddAddIndicator //подсказка, показываемая при наведении на кнопку добавить
        {
            get { return _tooltipAddAddIndicator; }
            set
            {
                _tooltipAddAddIndicator = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsAddIndicatorCorrect()
        {
            bool result = true;
            TooltipAddAddIndicator.Clear(); //очищаем подсказку кнопки добавить

            string name = "";
            if (IndicatorName != null)
            {
                name = IndicatorName.Replace(" ", "");
            }

            //проверка на пустое значение
            if (name == "")
            {
                result = false;
                TooltipAddAddIndicator.Add("Не заполнены все поля.");
            }

            //проверка на допустимые символы
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool isNotFind = false;
            for(int i = 0; i < name.Length; i++)
            {
                if(letters.IndexOf(name[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipAddAddIndicator.Add("Допустимо использование только английского алфавита.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            if (IsIndicatorEdited)
            {
                foreach (Indicator item in Indicators)
                {
                    if (name == item.Name && item.Id != SelectedIndicator.Id) //проверяем имя на уникальность среди всех записей кроме редактируемой
                    {
                        isUnique = false;
                    }
                }
            }
            else
            {
                foreach (Indicator item in Indicators)
                {
                    if (name == item.Name) //проверяем имя на уникальность среди всех записей
                    {
                        isUnique = false;
                    }
                }
            }
            
            if (isUnique == false)
            {
                result = false;
                TooltipAddAddIndicator.Add("Данное название уже используется.");
            }

            return result;
        }

        public ICommand IndicatorSave_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    List<IndicatorParameterTemplate> InsertIndicatorParameterTemplates = new List<IndicatorParameterTemplate>();
                    foreach(IndicatorParameterTemplateView item in IndicatorParameterTemplatesView)
                    {
                        InsertIndicatorParameterTemplates.Add(new IndicatorParameterTemplate { Id = item.Id, Name = item.Name, Description = item.Description, IdIndicator = item.IdIndicator, ParameterValueType = item.ParameterValueType, Indicator = item.Indicator });
                    }

                    if (IsIndicatorAdded)
                    {
                        _modelTesting.IndicatorInsertUpdate(IndicatorName, IndicatorDescription, InsertIndicatorParameterTemplates, IndicatorScript);

                        //выбираем добавленный индикатор
                        SelectedIndicator = Indicators[Indicators.Count - 1];
                    }
                    else if(IsIndicatorEdited)
                    {
                        _modelTesting.IndicatorInsertUpdate(IndicatorName, IndicatorDescription, InsertIndicatorParameterTemplates, IndicatorScript, SelectedIndicator.Id);

                        //выбираем измененный индикатор
                        int index = -1;
                        foreach(Indicator indicator in Indicators)
                        {
                            if(indicator.Id == _indicatorId)
                            {
                                index = Indicators.IndexOf(indicator);
                            }
                        }
                        SelectedIndicator = Indicators[index];
                    }

                    IsIndicatorAdded = false;
                    IsIndicatorEdited = false;
                    UpdateIndicatorStatusText();

                }, (obj) => IsAddOrEditIndicator() && IsFieldsAddIndicatorCorrect() );
            }
        }

        public ICommand IndicatorCancel_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IsIndicatorAdded = false;
                    IsIndicatorEdited = false;
                    UpdateIndicatorStatusText();
                    SelectedIndicatorChanged();
                }, (obj) => IsAddOrEditIndicator() );
            }
        }

        public ICommand IndicatorCheckScript_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {

                }, (obj) => true );
            }
        }

        #endregion










        #region view add edit delete IndicatorParameterTemplate

        private ObservableCollection<ParameterValueType> _parameterValueTypes = new ObservableCollection<ParameterValueType>(); //типы значений параметров
        public ObservableCollection<ParameterValueType> ParameterValueTypes
        {
            get { return _parameterValueTypes; }
            private set
            {
                _parameterValueTypes = value;
                OnPropertyChanged();
            }
        }

        private void modelData_ParameterValueTypesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ParameterValueTypes = (ObservableCollection<ParameterValueType>)sender;
        }

        private ObservableCollection<IndicatorParameterTemplateView> _indicatorParameterTemplatesView = new ObservableCollection<IndicatorParameterTemplateView>();
        public ObservableCollection<IndicatorParameterTemplateView> IndicatorParameterTemplatesView //шаблоны параметров индикатора
        {
            get { return _indicatorParameterTemplatesView; }
            private set
            {
                _indicatorParameterTemplatesView = value;
                OnPropertyChanged();
            }
        }

        private IndicatorParameterTemplateView _selectedIndicatorParameterTemplateView;
        public IndicatorParameterTemplateView SelectedIndicatorParameterTemplateView //выбранный шаблон параметра индикатора
        {
            get { return _selectedIndicatorParameterTemplateView; }
            set
            {
                _selectedIndicatorParameterTemplateView = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddIndicatorParameterTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AddIndicatorParameterTemplateSelectedParameterValueType = ParameterValueTypes[0];

                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewAddIndicatorParameterTemplate viewAddIndicatorParameterTemplate = new ViewAddIndicatorParameterTemplate();
                    viewAddIndicatorParameterTemplate.Show();
                }, (obj) => IsAddOrEditIndicator() );
            }
        }
        
        private string _addIndicatorParameterTemplateName;
        public string AddIndicatorParameterTemplateName //название добавляемого параметра
        {
            get { return _addIndicatorParameterTemplateName; }
            set
            {
                _addIndicatorParameterTemplateName = value;
                OnPropertyChanged();
            }
        }
        
        private string _addIndicatorParameterTemplateDescription;
        public string AddIndicatorParameterTemplateDescription //описание добавляемого параметра
        {
            get { return _addIndicatorParameterTemplateDescription; }
            set
            {
                _addIndicatorParameterTemplateDescription = value;
                OnPropertyChanged();
            }
        }
        
        private ParameterValueType _addIndicatorParameterTemplateSelectedParameterValueType;
        public ParameterValueType AddIndicatorParameterTemplateSelectedParameterValueType //выбранный тип параметра добавляемого параметра
        {
            get { return _addIndicatorParameterTemplateSelectedParameterValueType; }
            set
            {
                _addIndicatorParameterTemplateSelectedParameterValueType = value;
                OnPropertyChanged();
            }
        }

        public void AddIndicatorParameterTemplate_Closing(object sender, CancelEventArgs e)
        {
            viewmodelData.IsPagesAndMainMenuButtonsEnabled = true;
            CloseAddIndicatorParameterTemplateAction = null; //сбрасываем Action, чтобы при инициализации нового окна в него поместился метод его закрытия
        }

        public Action CloseAddIndicatorParameterTemplateAction { get; set; }

        public ICommand CloseAddIndicatorParameterTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CloseAddIndicatorParameterTemplateAction?.Invoke();
                }, (obj) => IsAddOrEditIndicator() );
            }
        }

        private ObservableCollection<string> _tooltipAddAddIndicatorParameterTemplate = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipAddAddIndicatorParameterTemplate //подсказка, показываемая при наведении на кнопку добавить
        {
            get { return _tooltipAddAddIndicatorParameterTemplate; }
            set
            {
                _tooltipAddAddIndicatorParameterTemplate = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsAddIndicatorParameterTemplateCorrect()
        {
            bool result = true;
            TooltipAddAddIndicatorParameterTemplate.Clear(); //очищаем подсказку кнопки добавить

            string name = AddIndicatorParameterTemplateName != null? AddIndicatorParameterTemplateName.Replace(" ", "") : "";

            //проверка на пустое значение
            if(name == "")
            {
                result = false;
                TooltipAddAddIndicatorParameterTemplate.Add("Не заполнены все поля.");
            }

            //проверка на допустимые символы
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool isNotFind = false;
            for (int i = 0; i < name.Length; i++)
            {
                if (letters.IndexOf(name[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipAddAddIndicatorParameterTemplate.Add("Допустимо использование только английского алфавита.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            foreach (IndicatorParameterTemplateView item in IndicatorParameterTemplatesView)
            {
                if (name == item.Name) //проверяем имя на уникальность среди всех записей
                {
                    isUnique = false;
                }
            }
            if (isUnique == false)
            {
                result = false;
                TooltipAddAddIndicatorParameterTemplate.Add("Данное название уже используется.");
            }

            return result;
        }

        public ICommand AddAddIndicatorParameterTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    string name = AddIndicatorParameterTemplateName.Replace(" ", "");
                    IndicatorParameterTemplateView indicatorParameterTemplateView = new IndicatorParameterTemplateView { Name = name, Description = AddIndicatorParameterTemplateDescription, ParameterValueType = AddIndicatorParameterTemplateSelectedParameterValueType };
                    IndicatorParameterTemplatesView.Add(indicatorParameterTemplateView);

                    CloseAddIndicatorParameterTemplateAction?.Invoke();
                }, (obj) => IsFieldsAddIndicatorParameterTemplateCorrect() );
            }
        }

        public ICommand EditIndicatorParameterTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AddIndicatorParameterTemplateName = SelectedIndicatorParameterTemplateView.Name;
                    AddIndicatorParameterTemplateDescription = SelectedIndicatorParameterTemplateView.Description;
                    AddIndicatorParameterTemplateSelectedParameterValueType = SelectedIndicatorParameterTemplateView.ParameterValueType;

                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewEditIndicatorParameterTemplate viewEditIndicatorParameterTemplate = new ViewEditIndicatorParameterTemplate();
                    viewEditIndicatorParameterTemplate.Show();
                }, (obj) => SelectedIndicatorParameterTemplateView != null && IsAddOrEditIndicator());
            }
        }

        private bool IsFieldsEditIndicatorParameterTemplateCorrect()
        {
            bool result = true;
            TooltipAddAddIndicatorParameterTemplate.Clear(); //очищаем подсказку кнопки добавить

            string name = AddIndicatorParameterTemplateName.Replace(" ", "");

            //проверка на пустое значение
            if (name == "")
            {
                result = false;
                TooltipAddAddIndicatorParameterTemplate.Add("Не заполнены все поля.");
            }
            
            //проверка на допустимые символы
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool isNotFind = false;
            for (int i = 0; i < name.Length; i++)
            {
                if (letters.IndexOf(name[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipAddAddIndicatorParameterTemplate.Add("Допустимо использование только английского алфавита.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            foreach (IndicatorParameterTemplateView item in IndicatorParameterTemplatesView)
            {
                if(SelectedIndicatorParameterTemplateView != null) //без этой проверки ошибка на обращение null полю, после сохранения
                {
                    if (name == item.Name && name != SelectedIndicatorParameterTemplateView.Name) //проверяем имя на уникальность среди всех записей кроме редактируемой
                    {
                        isUnique = false;
                    }
                }
            }
            if (isUnique == false)
            {
                result = false;
                TooltipAddAddIndicatorParameterTemplate.Add("Данное название уже используется.");
            }

            return result;
        }

        public ICommand EditSaveIndicatorParameterTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    string name = AddIndicatorParameterTemplateName.Replace(" ", "");
                    int index = IndicatorParameterTemplatesView.IndexOf(SelectedIndicatorParameterTemplateView);
                    IndicatorParameterTemplateView indicatorParameterTemplateView = new IndicatorParameterTemplateView { Name = name, Description = AddIndicatorParameterTemplateDescription, ParameterValueType = AddIndicatorParameterTemplateSelectedParameterValueType };
                    IndicatorParameterTemplatesView.RemoveAt(index);
                    IndicatorParameterTemplatesView.Insert(index, indicatorParameterTemplateView);

                    CloseAddIndicatorParameterTemplateAction?.Invoke();
                }, (obj) => IsFieldsEditIndicatorParameterTemplateCorrect());
            }
        }

        public ICommand DeleteIndicatorParameterTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    int index = IndicatorParameterTemplatesView.IndexOf(SelectedIndicatorParameterTemplateView); //находим индекс выбранного элемента
                    string msg = "Название: " + SelectedIndicatorParameterTemplateView.Name;
                    string caption = "Удалить?";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == MessageBoxResult.Yes)
                    {
                        IndicatorParameterTemplatesView.RemoveAt(index);
                    }
                }, (obj) => SelectedIndicatorParameterTemplateView != null && IsAddOrEditIndicator() );
            }
        }

        #endregion










        #region view add edit delete Algorithms

        public System.Windows.Controls.TextBox AlgorithmScriptTextBox; //ссылка на textBox с текстом скрипта, для обращения к свойству CaretIndex

        private ObservableCollection<Algorithm> _algorithms = new ObservableCollection<Algorithm>(); //алгоритмы
        public ObservableCollection<Algorithm> Algorithms
        {
            get { return _algorithms; }
            private set
            {
                _algorithms = value;
                OnPropertyChanged();
            }
        }

        private void modelData_AlgorithmsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Algorithms = (ObservableCollection<Algorithm>)sender;
        }

        private Algorithm _selectedAlgorithm;
        public Algorithm SelectedAlgorithm //выбранный алгоритм
        {
            get { return _selectedAlgorithm; }
            set
            {
                _selectedAlgorithm = value;
                OnPropertyChanged();
                SelectedAlgorithmChanged(); //помещяет в переменные редактора значения выбранного алгоритма для просмотра данного алгоритма
            }
        }

        private bool _isAlgorithmAdded = false;
        public bool IsAlgorithmAdded //флаг того что в настоящий момент создается новый алгоритм
        {
            get { return _isAlgorithmAdded; }
            set
            {
                _isAlgorithmAdded = value;
                OnPropertyChanged();
            }
        }

        private bool _isAlgorithmEdited = false;
        public bool IsAlgorithmEdited //флаг того что в настоящий момент редактируется алгоритм
        {
            get { return _isAlgorithmEdited; }
            set
            {
                _isAlgorithmEdited = value;
                OnPropertyChanged();
            }
        }

        private bool _isAlgorithmReadOnly = true;
        public bool IsAlgorithmReadOnly //на эту переменную привязаны свойства readonly полей редактирования алгоритма
        {
            get { return _isAlgorithmReadOnly; }
            set
            {
                _isAlgorithmReadOnly = value;
                OnPropertyChanged();
                IsAlgorithmEnabled = !value;
            }
        }

        private bool _isAlgorithmEnabled = false;
        public bool IsAlgorithmEnabled //на эту переменную привязаны свойства enabled полей редактирования алгоритма
        {
            get { return _isAlgorithmEnabled; }
            set
            {
                _isAlgorithmEnabled = value;
                OnPropertyChanged();
            }
        }

        private void SelectedAlgorithmChanged() //помещяет в переменные редактора значения выбранного алгоритма для просмотра данного алгоритма
        {
            if (SelectedAlgorithm != null)
            {
                _algorithmId = SelectedAlgorithm.Id;
                AlgorithmName = SelectedAlgorithm.Name;
                AlgorithmDescription = SelectedAlgorithm.Description;
                CreateDataSourceTemplatesView();
                CreateAlgorithmParametersView();
                CreateAlgorithmParametersViewIntDouble();
                CreateAlgorithmIndicators();
                AlgorithmScript = SelectedAlgorithm.Script;
            }
            else
            {
                AlgorithmName = "";
                AlgorithmDescription = "";
                DataSourceTemplatesView.Clear();
                IndicatorParameterRangesView.Clear();
                AlgorithmIndicatorsView.Clear();
                AlgorithmParametersView.Clear();
                CreateAlgorithmParametersViewIntDouble();
                AlgorithmScript = "";
            }
        }

        private string _algorithmScript = "";
        public string AlgorithmScript //скрипт алгоритма
        {
            get { return _algorithmScript; }
            set
            {
                _algorithmScript = value;
                OnPropertyChanged();
            }
        }

        private string _algorithmStatusText = "Просмотр";
        public string AlgorithmStatusText //статус работы с выбранным алгоритмом
        {
            get { return _algorithmStatusText; }
            set
            {
                _algorithmStatusText = value;
                OnPropertyChanged();
            }
        }

        private int _algorithmId; //id алгоритма

        private string _algorithmName;
        public string AlgorithmName //название алгоритма
        {
            get { return _algorithmName; }
            set
            {
                _algorithmName = value;
                OnPropertyChanged();
            }
        }

        private string _algorithmDescription;
        public string AlgorithmDescription //описание алгоритма
        {
            get { return _algorithmDescription; }
            set
            {
                _algorithmDescription = value;
                OnPropertyChanged();
            }
        }

        private void UpdateAlgorithmStatusText()
        {
            if (IsAlgorithmAdded == true)
            {
                AlgorithmStatusText = "Добавление";
                IsAlgorithmReadOnly = false;
            }
            else if (IsAlgorithmEdited == true)
            {
                AlgorithmStatusText = "Редактирование";
                IsAlgorithmReadOnly = false;
            }
            else
            {
                AlgorithmStatusText = "Просмотр";
                IsAlgorithmReadOnly = true;
            }
        }

        private bool IsAddOrEditAlgorithm() //добавляется ли новый алгоритм в данный момент, или редактируется ли имеющийся
        {
            return IsAlgorithmAdded || IsAlgorithmEdited;
        }

        public ICommand AlgorithmsAdd_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IsAlgorithmAdded = true;
                    UpdateAlgorithmStatusText();
                    SelectedAlgorithm = null;
                    SelectedAlgorithmChanged();
                }, (obj) => !IsAddOrEditAlgorithm() && !IsAddOrEditIndicator());
            }
        }

        public ICommand AlgorithmsEdit_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IsAlgorithmEdited = true;
                    UpdateAlgorithmStatusText();
                }, (obj) => !IsAddOrEditAlgorithm() && !IsAddOrEditIndicator() && SelectedAlgorithm != null);
            }
        }

        public ICommand AlgorithmsDelete_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    string msg = " Id: " + SelectedAlgorithm.Id + "  Название: " + SelectedAlgorithm.Name;
                    string caption = "Удалить индикатор?";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == MessageBoxResult.Yes)
                    {
                        _modelTesting.AlgorithmDelete(SelectedAlgorithm.Id);
                    }
                }, (obj) => !IsAddOrEditAlgorithm() && !IsAddOrEditIndicator() && SelectedAlgorithm != null);
            }
        }

        public ICommand AlgorithmPasteAccountFreeMoney_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.FreeMoney");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountTakenMoney_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.TakenMoney");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountIsForwardDepositTrading_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.IsForwardDepositTrading");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountIntVar1_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.IntVar1");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountIntVar2_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.IntVar2");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountIntVar3_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.IntVar3");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountIntVar4_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.IntVar4");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountIntVar5_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.IntVar5");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountDoubleVar1_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.DoubleVar1");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountDoubleVar2_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.DoubleVar2");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountDoubleVar3_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.DoubleVar3");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountDoubleVar4_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.DoubleVar4");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAccountDoubleVar5_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "account.AccountVariables.DoubleVar5");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteOrderMarketSell_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Order_MarketSell(источникДанных, цена, количество);");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteOrderMarketBuy_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Order_MarketBuy(источникДанных, цена, количество);");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteOrderLimitSell_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Order_LimitSell(источникДанных, цена, количество);");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteOrderLimitBuy_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Order_LimitBuy(источникДанных, цена, количество);");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteOrderStopSell_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Order_StopSell(источникДанных, цена, количество);");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteOrderStopBuy_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Order_StopBuy(источникДанных, цена, количество);");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteOrderStopTakeBuy_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Order_StopTakeBuy(источникДанных, стопЦена, тейкЦена, количество);");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteOrderStopTakeSell_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Order_StopTakeSell(источникДанных, стопЦена, тейкЦена, количество);");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDatasourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name);
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDatasourceCandles_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".Candles[0]");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDatasourcePriceStep_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".PriceStep");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDatasourceCostPriceStep_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".CostPriceStep");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDatasourceOneLotCost_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".MinLotsCost");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDatasourceCountLotsBuy_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".CountBuy");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDatasourceCountLotsSell_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".CountSell");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDataSourcePriceOpenPosition_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".Price");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDatasourceIsCurrencyRuble_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".IsCurrencyRuble");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDatasourceMinLotCount_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".MinLotCount");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDataSourceTimeInCandle_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".TimeInCandle");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDataSourceTradingStartTimeOfDay_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".TradingStartTimeOfDay");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDataSourceTradingEndTimeOfDay_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + ".TradingEndTimeOfDay");
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDataSourceIndicator_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Datasource_" + SelectedDataSourceTemplateView.Name + "_Indicator_" + SelectedAlgorithmIndicatorView.Indicator.Name + "_" + SelectedAlgorithmIndicatorView.Ending);
                }, (obj) => SelectedDataSourceTemplateView != null && SelectedAlgorithmIndicatorView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteAlgorithmParameter_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "Parameter_" + SelectedAlgorithmParameterView.Name);
                }, (obj) => SelectedAlgorithmParameterView != null && IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteCondition_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "if (  ) {" + Environment.NewLine + Environment.NewLine + "} else {" + Environment.NewLine + Environment.NewLine + "}");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteWhile_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "int i = 0;" + Environment.NewLine + "while (  ) {" + Environment.NewLine + Environment.NewLine + "i++;" + Environment.NewLine + "}");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteDateTimeCompare_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "DateTime.Compare(дата1, дата2) > 0");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        public ICommand AlgorithmPasteLogicMore_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, ">");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        public ICommand AlgorithmPasteLogicLess_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "<");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        public ICommand AlgorithmPasteLogicMoreOrEqual_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, ">=");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        public ICommand AlgorithmPasteLogicLessOrEqual_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "<=");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        public ICommand AlgorithmPasteLogicEqual_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "==");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        public ICommand AlgorithmPasteLogicUnequal_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "!=");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        public ICommand AlgorithmPasteLogicAnd_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "&&");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        public ICommand AlgorithmPasteLogicOr_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "||");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteVarInt_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "int var1 = 0;");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }
        public ICommand AlgorithmPasteVarDouble_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "double var1 = 0;");
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        private ObservableCollection<string> _tooltipSaveAlgorithm = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipSaveAlgorithm //подсказка, показываемая при наведении на кнопку добавить
        {
            get { return _tooltipSaveAlgorithm; }
            set
            {
                _tooltipSaveAlgorithm = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsSaveAlgorithmCorrect()
        {
            bool result = true;
            TooltipSaveAlgorithm.Clear(); //очищаем подсказку кнопки добавить

            string name = "";
            if (AlgorithmName != null)
            {
                name = AlgorithmName;
            }

            //проверка на пустое значение
            if (name == "")
            {
                result = false;
                TooltipSaveAlgorithm.Add("Не заполнены все поля.");
            }

            //проверка на допустимые символы
            string letters = "абвгдеёзжийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            bool isNotFind = false;
            for (int i = 0; i < name.Length; i++)
            {
                if (letters.IndexOf(name[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipSaveAlgorithm.Add("Допустимо использование для названия только русского, английского алфавитов, и цифр.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            if (IsAlgorithmEdited)
            {
                foreach (Algorithm item in Algorithms)
                {
                    if (name == item.Name && item.Id != SelectedAlgorithm.Id) //проверяем имя на уникальность среди всех записей кроме редактируемой
                    {
                        isUnique = false;
                    }
                }
            }
            else
            {
                foreach (Algorithm item in Algorithms)
                {
                    if (name == item.Name) //проверяем имя на уникальность среди всех записей
                    {
                        isUnique = false;
                    }
                }
            }
            if (isUnique == false)
            {
                result = false;
                TooltipSaveAlgorithm.Add("Данное название уже используется.");
            }

            //проверка на наличие минимум одного шаблона источника данных
            if(DataSourceTemplatesView.Count == 0)
            {
                result = false;
                TooltipSaveAlgorithm.Add("Необходимо добавить хотя бы один макет источников данных.");
            }

            //проверка на выбранность параметра алгоритма для параметров индикаторов
            bool isChosenAlgorithmParameter = true;
            foreach(IndicatorParameterRangeView indicatorParameterRangeView in IndicatorParameterRangesView)
            {
                if(indicatorParameterRangeView.SelectedAlgorithmParameterView == null)
                {
                    isChosenAlgorithmParameter = false;
                }
            }
            if(isChosenAlgorithmParameter == false)
            {
                result = false;
                TooltipSaveAlgorithm.Add("Не выбраны параметры алгоритма для параметров индикаторов.");
            }

            return result;
        }

        public ICommand AlgorithmSave_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    List<DataSourceTemplate> dataSourceTemplates = new List<DataSourceTemplate>();
                    foreach(DataSourceTemplate item in DataSourceTemplatesView)
                    {
                        DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Id = item.Id, Name = item.Name, Description = item.Description, IdAlgorithm = item.IdAlgorithm };
                        dataSourceTemplates.Add(dataSourceTemplate);
                    }

                    List<AlgorithmParameter> algorithmParameters = new List<AlgorithmParameter>();
                    foreach(AlgorithmParameterView algorithmParameterView in AlgorithmParametersView)
                    {
                        AlgorithmParameter algorithmParameter = new AlgorithmParameter { Id = algorithmParameterView.Id, Name = algorithmParameterView.Name, Description = algorithmParameterView.Description, ParameterValueType = algorithmParameterView.ParameterValueType, MinValue = double.Parse(algorithmParameterView.MinValue), MaxValue = double.Parse(algorithmParameterView.MaxValue), Step = double.Parse(algorithmParameterView.Step), IsStepPercent = algorithmParameterView.IsStepPercent, IdAlgorithm = algorithmParameterView.IdAlgorithm };
                        algorithmParameters.Add(algorithmParameter);
                    }

                    List<AlgorithmIndicator> algorithmIndicators = new List<AlgorithmIndicator>();
                    foreach(AlgorithmIndicatorView algorithmIndicatorView in AlgorithmIndicatorsView)
                    {
                        AlgorithmIndicator algorithmIndicator = new AlgorithmIndicator { Id = algorithmIndicatorView.Id, Algorithm = algorithmIndicatorView.Algorithm, Indicator = algorithmIndicatorView.Indicator, IndicatorParameterRanges = new List<IndicatorParameterRange>(), Ending = algorithmIndicatorView.Ending }; //после добавитьindicatorparameterranges
                        algorithmIndicators.Add(algorithmIndicator);
                    }

                    List<IndicatorParameterRange> indicatorParameterRanges = new List<IndicatorParameterRange>();
                    foreach(IndicatorParameterRangeView indicatorParameterRangeView in IndicatorParameterRangesView)
                    {
                        IndicatorParameterRange indicatorParameterRange = new IndicatorParameterRange { Id = indicatorParameterRangeView.Id, IndicatorParameterTemplate = indicatorParameterRangeView.IndicatorParameterTemplate, AlgorithmParameter = algorithmParameters.Where(j => j.Name == indicatorParameterRangeView.SelectedAlgorithmParameterView.Name).First(), AlgorithmIndicator = algorithmIndicators.Where(j => j.Indicator == indicatorParameterRangeView.AlgorithmIndicatorView.Indicator && j.Ending == indicatorParameterRangeView.AlgorithmIndicatorView.Ending).First() };
                        indicatorParameterRanges.Add(indicatorParameterRange);
                    }

                    //добавляем indicatorParameterRanges в algorithmIndicators
                    foreach(AlgorithmIndicator algorithmIndicator1 in algorithmIndicators)
                    {
                        foreach(IndicatorParameterRange indicatorParameterRange in indicatorParameterRanges)
                        {
                            if(indicatorParameterRange.AlgorithmIndicator == algorithmIndicator1)
                            {
                                algorithmIndicator1.IndicatorParameterRanges.Add(indicatorParameterRange);
                            }
                        }
                    }

                    if (IsAlgorithmAdded)
                    {
                        _modelTesting.AlgorithmInsertUpdate(AlgorithmName, AlgorithmDescription, dataSourceTemplates, algorithmIndicators, algorithmParameters, AlgorithmScript);

                        //выбираем добавленный алгоритм
                        SelectedAlgorithm = Algorithms.Last();
                    }
                    else if (IsAlgorithmEdited)
                    {
                        _modelTesting.AlgorithmInsertUpdate(AlgorithmName, AlgorithmDescription, dataSourceTemplates, algorithmIndicators, algorithmParameters, AlgorithmScript, _algorithmId);

                        //выбираем измененный алгоритм
                        SelectedAlgorithm = Algorithms.Where(j => j.Id == _algorithmId).First();
                    }

                    IsAlgorithmAdded = false;
                    IsAlgorithmEdited = false;
                    UpdateAlgorithmStatusText();

                }, (obj) => IsAddOrEditAlgorithm() && IsFieldsSaveAlgorithmCorrect());
            }
        }

        public ICommand AlgorithmCancel_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IsAlgorithmAdded = false;
                    IsAlgorithmEdited = false;
                    UpdateAlgorithmStatusText();
                    SelectedAlgorithmChanged();
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        #endregion










        #region view add edit delete DataSourceTemplate

        private ObservableCollection<DataSourceTemplate> _dataSourceTemplates = new ObservableCollection<DataSourceTemplate>();
        public ObservableCollection<DataSourceTemplate> DataSourceTemplates //шаблоны источников данных
        {
            get { return _dataSourceTemplates; }
            private set
            {
                _dataSourceTemplates = value;
                OnPropertyChanged();
            }
        }

        private void CreateDataSourceTemplatesView()
        {
            DataSourceTemplatesView.Clear();
            foreach(DataSourceTemplate dataSourceTemplate in DataSourceTemplates)
            {
                if(dataSourceTemplate.IdAlgorithm == _algorithmId)
                {
                    DataSourceTemplatesView.Add(dataSourceTemplate);
                }
            }
        }

        private void modelData_DataSourceTemplatesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DataSourceTemplates = (ObservableCollection<DataSourceTemplate>)sender;
        }

        private ObservableCollection<DataSourceTemplate> _dataSourceTemplatesView = new ObservableCollection<DataSourceTemplate>();
        public ObservableCollection<DataSourceTemplate> DataSourceTemplatesView //шаблоны источников данных для выбранного алгоритма
        {
            get { return _dataSourceTemplatesView; }
            private set
            {
                _dataSourceTemplatesView = value;
                OnPropertyChanged();
            }
        }

        private DataSourceTemplate _SelectedDataSourceTemplateView;
        public DataSourceTemplate SelectedDataSourceTemplateView //выбранный шаблон источника данных для выбранного алгоритма
        {
            get { return _SelectedDataSourceTemplateView; }
            set
            {
                _SelectedDataSourceTemplateView = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewAddDataSourceTemplate viewAddDataSourceTemplate = new ViewAddDataSourceTemplate();
                    viewAddDataSourceTemplate.Show();
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        private string _addDataSourceTemplateName;
        public string AddDataSourceTemplateName //название шаблона источника данных
        {
            get { return _addDataSourceTemplateName; }
            set
            {
                _addDataSourceTemplateName = value;
                OnPropertyChanged();
            }
        }

        private string _addDataSourceTemplateDescription;
        public string AddDataSourceTemplateDescription //описание шаблона источника данных
        {
            get { return _addDataSourceTemplateDescription; }
            set
            {
                _addDataSourceTemplateDescription = value;
                OnPropertyChanged();
            }
        }

        public void AddDataSourceTemplate_Closing(object sender, CancelEventArgs e)
        {
            viewmodelData.IsPagesAndMainMenuButtonsEnabled = true;
            CloseAddDataSourceTemplateAction = null; //сбрасываем Action, чтобы при инициализации нового окна в него поместился метод его закрытия
        }

        public Action CloseAddDataSourceTemplateAction { get; set; }

        public ICommand CloseAddDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => true);
            }
        }

        private ObservableCollection<string> _tooltipAddAddDataSourceTemplate = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipAddAddDataSourceTemplate //подсказка, показываемая при наведении на кнопку добавить
        {
            get { return _tooltipAddAddDataSourceTemplate; }
            set
            {
                _tooltipAddAddDataSourceTemplate = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsAddDataSourceTemplateCorrect()
        {
            bool result = true;
            TooltipAddAddDataSourceTemplate.Clear(); //очищаем подсказку кнопки добавить

            string name = AddDataSourceTemplateName != null ? AddDataSourceTemplateName.Replace(" ", "") : "";

            //проверка на пустое значение
            if (name == "")
            {
                result = false;
                TooltipAddAddDataSourceTemplate.Add("Не заполнены все поля.");
            }

            //проверка на допустимые символы
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool isNotFind = false;
            for (int i = 0; i < name.Length; i++)
            {
                if (letters.IndexOf(name[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipAddAddDataSourceTemplate.Add("Допустимо использование только английского алфавита.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            foreach (DataSourceTemplate item in DataSourceTemplatesView)
            {
                if (name == item.Name) //проверяем имя на уникальность среди всех записей
                {
                    isUnique = false;
                }
            }
            if (isUnique == false)
            {
                result = false;
                TooltipAddAddDataSourceTemplate.Add("Данное название уже используется.");
            }

            return result;
        }

        public ICommand AddAddDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    string name = AddDataSourceTemplateName != null ? AddDataSourceTemplateName.Replace(" ", "") : "";
                    DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Name = name, Description = AddDataSourceTemplateDescription };
                    DataSourceTemplatesView.Add(dataSourceTemplate);

                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => IsFieldsAddDataSourceTemplateCorrect());
            }
        }

        public ICommand EditDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AddDataSourceTemplateName = SelectedDataSourceTemplateView.Name;
                    AddDataSourceTemplateDescription = SelectedDataSourceTemplateView.Description;

                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewEditDataSourceTemplate viewEditDataSourceTemplate = new ViewEditDataSourceTemplate();
                    viewEditDataSourceTemplate.Show();
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }

        private bool IsFieldsEditDataSourceTemplateCorrect()
        {
            bool result = true;
            TooltipAddAddDataSourceTemplate.Clear(); //очищаем подсказку кнопки добавить

            string name = AddDataSourceTemplateName.Replace(" ", "");

            //проверка на пустое значение
            if (name == "")
            {
                result = false;
                TooltipAddAddDataSourceTemplate.Add("Не заполнены все поля.");
            }

            //проверка на допустимые символы
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool isNotFind = false;
            for (int i = 0; i < name.Length; i++)
            {
                if (letters.IndexOf(name[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipAddAddDataSourceTemplate.Add("Допустимо использование только английского алфавита.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            foreach (DataSourceTemplate item in DataSourceTemplatesView)
            {
                if (SelectedDataSourceTemplateView != null) //без этой проверки ошибка на обращение null полю, после сохранения
                {
                    if (name == item.Name && name != SelectedDataSourceTemplateView.Name) //проверяем имя на уникальность среди всех записей кроме редактируемой
                    {
                        isUnique = false;
                    }
                }
            }
            if (isUnique == false)
            {
                result = false;
                TooltipAddAddDataSourceTemplate.Add("Данное название уже используется.");
            }

            return result;
        }

        public ICommand EditSaveDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    string name = AddDataSourceTemplateName.Replace(" ", "");
                    int index = DataSourceTemplatesView.IndexOf(SelectedDataSourceTemplateView);
                    DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Name = name, Description = AddDataSourceTemplateDescription };
                    DataSourceTemplatesView.RemoveAt(index);
                    DataSourceTemplatesView.Insert(index, dataSourceTemplate);

                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => IsFieldsEditDataSourceTemplateCorrect());
            }
        }

        public ICommand DeleteDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    int index = DataSourceTemplatesView.IndexOf(SelectedDataSourceTemplateView); //находим индекс выбранного элемента
                    string msg = "Название: " + SelectedDataSourceTemplateView.Name;
                    string caption = "Удалить?";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == MessageBoxResult.Yes)
                    {
                        DataSourceTemplatesView.RemoveAt(index);
                    }
                }, (obj) => SelectedDataSourceTemplateView != null && IsAddOrEditAlgorithm());
            }
        }

        #endregion










        #region view edit IndicatorParameterRange    view add delete AlgorithmIndicators

        private ObservableCollection<IndicatorParameterTemplate> _indicatorParameterTemplates = new ObservableCollection<IndicatorParameterTemplate>();
        public ObservableCollection<IndicatorParameterTemplate> IndicatorParameterTemplates //шаблоны параметров индикатора
        {
            get { return _indicatorParameterTemplates; }
            private set
            {
                _indicatorParameterTemplates = value;
                OnPropertyChanged();
            }
        }

        private void modelData_IndicatorParameterTemplatesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IndicatorParameterTemplates = (ObservableCollection<IndicatorParameterTemplate>)sender;
        }

        private ObservableCollection<IndicatorParameterRangeView> _indicatorParameterRangesView = new ObservableCollection<IndicatorParameterRangeView>();
        public ObservableCollection<IndicatorParameterRangeView> IndicatorParameterRangesView //диапазоны значений параметров индикаторов
        {
            get { return _indicatorParameterRangesView; }
            private set
            {
                _indicatorParameterRangesView = value;
                OnPropertyChanged();
            }
        }

        private IndicatorParameterRangeView _selectedIndicatorParameterRangeView;
        public IndicatorParameterRangeView SelectedIndicatorParameterRangeView //выбранный диапазон значений параметра индикатора
        {
            get { return _selectedIndicatorParameterRangeView; }
            set
            {
                _selectedIndicatorParameterRangeView = value;
                OnPropertyChanged();
            }
        }

        public void UpdateIndicatorParameterRangeViewAlgorithmParameters() //обновляет списки для параметров индикаторов со значениями, для параметра типа int - устанавливается список с int параметрами алгоритма, для double - список с double параметрами алгоритма
        {
            foreach (IndicatorParameterRangeView indicatorParameterRangeView in IndicatorParameterRangesView)
            {
                indicatorParameterRangeView.AlgorithmParametersView = indicatorParameterRangeView.IndicatorParameterTemplate.ParameterValueType.Id == 1 ? AlgorithmParametersViewInt : AlgorithmParametersViewDouble;
                if (indicatorParameterRangeView.AlgorithmParametersView.IndexOf(indicatorParameterRangeView.SelectedAlgorithmParameterView) == -1) //если выбранный параметр алгоритма отсутствует в списке с параметрами, очищаем выбранный параметр
                {
                    indicatorParameterRangeView.SelectedAlgorithmParameterView = null;
                }
            }
        }

        private ObservableCollection<AlgorithmIndicator> _algorithmIndicators = new ObservableCollection<AlgorithmIndicator>();
        public ObservableCollection<AlgorithmIndicator> AlgorithmIndicators //индикаторы алгоритма
        {
            get { return _algorithmIndicators; }
            private set
            {
                _algorithmIndicators = value;
                OnPropertyChanged();
            }
        }

        private void modelData_AlgorithmIndicatorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AlgorithmIndicators = (ObservableCollection<AlgorithmIndicator>)sender;
        }

        private ObservableCollection<AlgorithmIndicatorView> _algorithmIndicatorsView = new ObservableCollection<AlgorithmIndicatorView>();
        public ObservableCollection<AlgorithmIndicatorView> AlgorithmIndicatorsView //индикаторы алгоритма для представления
        {
            get { return _algorithmIndicatorsView; }
            private set
            {
                _algorithmIndicatorsView = value;
                OnPropertyChanged();
            }
        }
        
        private AlgorithmIndicatorView _selectedAlgorithmIndicatorView;
        public AlgorithmIndicatorView SelectedAlgorithmIndicatorView //выбранный индикатор алгоритма
        {
            get { return _selectedAlgorithmIndicatorView; }
            set
            {
                _selectedAlgorithmIndicatorView = value;
                OnPropertyChanged();
            }
        }

        private void CreateAlgorithmIndicators() //формирует список индикаторов выбранного алгоритма для представления, а так же список параметров индикаторов выбранного алгоритма
        {
            AlgorithmIndicatorsView.Clear();
            IndicatorParameterRangesView.Clear();
            foreach(AlgorithmIndicator algorithmIndicator in SelectedAlgorithm.AlgorithmIndicators)
            {
                AlgorithmIndicatorView algorithmIndicatorView = new AlgorithmIndicatorView { Id = algorithmIndicator.Id, Algorithm = algorithmIndicator.Algorithm, Indicator = algorithmIndicator.Indicator, IndicatorParameterRangesView = new List<IndicatorParameterRangeView>(), Ending = algorithmIndicator.Ending };
                foreach(IndicatorParameterRange indicatorParameterRange in algorithmIndicator.IndicatorParameterRanges)
                {
                    string nameAlgorithmindicator = indicatorParameterRange.AlgorithmIndicator.Indicator.Name + "_" + indicatorParameterRange.AlgorithmIndicator.Ending;
                    IndicatorParameterRangeView indicatorParameterRangeView = new IndicatorParameterRangeView { Id = indicatorParameterRange.Id, IndicatorParameterTemplate = indicatorParameterRange.IndicatorParameterTemplate, AlgorithmIndicatorView = algorithmIndicatorView, NameAlgorithmIndicator = nameAlgorithmindicator };
                    if (indicatorParameterRange.IndicatorParameterTemplate.ParameterValueType.Id == 1)
                    {
                        indicatorParameterRangeView.AlgorithmParametersView = AlgorithmParametersViewInt;
                    }
                    else
                    {
                        indicatorParameterRangeView.AlgorithmParametersView = AlgorithmParametersViewDouble;
                    }
                    indicatorParameterRangeView.SelectedAlgorithmParameterView = indicatorParameterRangeView.AlgorithmParametersView.Where(j => j.Name == indicatorParameterRange.AlgorithmParameter.Name).First();
                    IndicatorParameterRangesView.Add(indicatorParameterRangeView); //добавляем параметр индикатора со значением во все параметры с выбираемым значением
                    algorithmIndicatorView.IndicatorParameterRangesView.Add(indicatorParameterRangeView); //добавляем параметр индикатора со значением в параметры конкретного индикатора алгоритма
                }
                AlgorithmIndicatorsView.Add(algorithmIndicatorView);
            }
        }

        public ICommand AddAlgorithmIndicator_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AddAlgorithmIndicatorSelectedIndicator = null;
                    AddAlgorithmIndicatorEnding = "";
                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewAddAlgorithmIndicator viewAddAlgorithmIndicator = new ViewAddAlgorithmIndicator();
                    viewAddAlgorithmIndicator.Show();

                }, (obj) => IsAddOrEditAlgorithm() );
            }
        }

        public ICommand EditAlgorithmIndicator_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _editedAlgorithmIndicatorView = SelectedAlgorithmIndicatorView;
                    AddAlgorithmIndicatorSelectedIndicator = SelectedAlgorithmIndicatorView.Indicator;
                    AddAlgorithmIndicatorEnding = SelectedAlgorithmIndicatorView.Ending;
                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewEditAlgorithmIndicator viewEditAlgorithmIndicator = new ViewEditAlgorithmIndicator();
                    viewEditAlgorithmIndicator.Show();

                }, (obj) => IsAddOrEditAlgorithm() && SelectedAlgorithmIndicatorView != null);
            }
        }

        private AlgorithmIndicatorView _editedAlgorithmIndicatorView; //редактируемый индикатор алгоритма

        private Indicator _addAlgorithmIndicatorSelectedIndicator;
        public Indicator AddAlgorithmIndicatorSelectedIndicator //выбранный индикатор при добавлении индикатора алгоритму
        {
            get { return _addAlgorithmIndicatorSelectedIndicator; }
            set
            {
                _addAlgorithmIndicatorSelectedIndicator = value;
                OnPropertyChanged();
            }
        }

        private string _addAlgorithmIndicatorEnding;
        public string AddAlgorithmIndicatorEnding //окончание названия добавляемого индикатора алгоритма
        {
            get { return _addAlgorithmIndicatorEnding; }
            set
            {
                _addAlgorithmIndicatorEnding = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _tooltipAddAddAlgorithmIndicator = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipAddAddAlgorithmIndicator //подсказка, показываемая при наведении на кнопку добавить
        {
            get { return _tooltipAddAddAlgorithmIndicator; }
            set
            {
                _tooltipAddAddAlgorithmIndicator = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsAddAlgorithmIndicatorCorrect(bool isAdd) //isAdd: true - добавление индикатора алгоритму, false - редактирование индикатора алгоритма
        {
            bool result = true;
            TooltipAddAddAlgorithmIndicator.Clear(); //очищаем подсказку кнопки добавить

            //проверка на выбранность индикатора
            if(AddAlgorithmIndicatorSelectedIndicator == null)
            {
                result = false;
                TooltipAddAddAlgorithmIndicator.Add("Не выбран индикатор.");
            }
            else //если индикатор выбран, проверяем уникальность названия
            {
                bool isUnique = true;
                foreach(AlgorithmIndicatorView algorithmIndicatorView in AlgorithmIndicatorsView)
                {
                    if(algorithmIndicatorView.Indicator.Name + algorithmIndicatorView.Ending == AddAlgorithmIndicatorSelectedIndicator.Name + AddAlgorithmIndicatorEnding)
                    {
                        if(isAdd == false) //редактирование индикатора алгоритма
                        {
                            if((AddAlgorithmIndicatorSelectedIndicator == _editedAlgorithmIndicatorView.Indicator && AddAlgorithmIndicatorEnding == _editedAlgorithmIndicatorView.Ending) == false) //если текущая комбинация индикатора и окончания названия не совпадает с редактируемым индикатором алгоритма, отмечаем что данный индикатор и окончание названия уже используется. Если совпадает -> все в порядке, можно сохранять прежнее значение индикатора
                            {
                                isUnique = false;
                            }
                        }
                        else //добавление индикатора
                        {
                            isUnique = false;
                        }
                    }
                }
                if(isUnique == false)
                {
                    result = false;
                    TooltipAddAddAlgorithmIndicator.Add("Данный индикатор с таким окончанием названия уже существует.");
                }
            }

            //проверка на пустое значение
            if (AddAlgorithmIndicatorEnding == "")
            {
                result = false;
                TooltipAddAddAlgorithmIndicator.Add("Не заполнены все поля.");
            }

            //проверка на допустимые символы
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            bool isNotFind = false;
            for (int i = 0; i < AddAlgorithmIndicatorEnding.Length; i++)
            {
                if (letters.IndexOf(AddAlgorithmIndicatorEnding[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipAddAddAlgorithmIndicator.Add("Допустимо использование только английского алфавита и цифр.");
            }

            return result;
        }

        public ICommand AddAddAlgorithmIndicator_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmIndicatorView algorithmIndicatorView = new AlgorithmIndicatorView { Indicator = AddAlgorithmIndicatorSelectedIndicator, Ending = AddAlgorithmIndicatorEnding };
                    List<IndicatorParameterRangeView> indicatorParameterRangesView = new List<IndicatorParameterRangeView>();
                    for (int i = 0; i < AddAlgorithmIndicatorSelectedIndicator.IndicatorParameterTemplates.Count; i++)
                    {
                        IndicatorParameterRangeView indicatorParameterRangeView = new IndicatorParameterRangeView { IndicatorParameterTemplate = AddAlgorithmIndicatorSelectedIndicator.IndicatorParameterTemplates[i], AlgorithmIndicatorView = algorithmIndicatorView, NameAlgorithmIndicator = AddAlgorithmIndicatorSelectedIndicator.Name + "_" + AddAlgorithmIndicatorEnding };
                        indicatorParameterRangesView.Add(indicatorParameterRangeView);
                    }
                    algorithmIndicatorView.IndicatorParameterRangesView = indicatorParameterRangesView;
                    //вставляем в порядке следования индикаторов
                    int nextIndex = -1; //индкс элемента с id индикатора больше созданного
                    foreach(AlgorithmIndicatorView algorithmIndicatorView1 in AlgorithmIndicatorsView)
                    {
                        if(algorithmIndicatorView1.Indicator.Id > algorithmIndicatorView.Indicator.Id)
                        {
                            if(nextIndex == -1)
                            {
                                nextIndex = AlgorithmIndicatorsView.IndexOf(algorithmIndicatorView1);
                            }
                        }
                    }
                    if(nextIndex == -1)
                    {
                        AlgorithmIndicatorsView.Add(algorithmIndicatorView);
                    }
                    else
                    {
                        AlgorithmIndicatorsView.Insert(nextIndex, algorithmIndicatorView);
                    }

                    //вставляем algorithmIndicatorView.IndicatorParameterRangesView в IndicatorParameterRangesView, в порядке следования индикаторов
                    nextIndex = -1; //индкс элемента с id индикатора больше созданного
                    foreach (IndicatorParameterRangeView indicatorParameterRangeView in IndicatorParameterRangesView)
                    {
                        if (indicatorParameterRangeView.IndicatorParameterTemplate.IdIndicator > algorithmIndicatorView.Indicator.Id)
                        {
                            if (nextIndex == -1)
                            {
                                nextIndex = IndicatorParameterRangesView.IndexOf(indicatorParameterRangeView);
                            }
                        }
                    }
                    if (nextIndex == -1) //вставляем в конец если не было индикатора с id больше текущего
                    {
                        foreach(IndicatorParameterRangeView indicatorParameterRangeView in algorithmIndicatorView.IndicatorParameterRangesView)
                        {
                            IndicatorParameterRangesView.Add(indicatorParameterRangeView);
                        }
                    }
                    else //вставляем на место индикатора с id больше текущего
                    {
                        for(int i = 0; i < algorithmIndicatorView.IndicatorParameterRangesView.Count; i++)
                        {
                            IndicatorParameterRangesView.Insert(nextIndex, algorithmIndicatorView.IndicatorParameterRangesView[i]);
                        }
                    }
                    UpdateIndicatorParameterRangeViewAlgorithmParameters(); //обновляем списки с парараметрами алгоритма для выбора параметра алгоритма параметру индикатора алгоритма

                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => IsFieldsAddAlgorithmIndicatorCorrect(true));
            }
        }

        public ICommand EditSaveAlgorithmIndicator_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //формируем новый индикатор алгоритма, удаляем старый и вставляем новый. Если индикатор не изменился, сохраняем параметрам индикатору алгоритма выбранный параметр алгоритма
                    bool isIndicatorChanged = AddAlgorithmIndicatorSelectedIndicator != SelectedAlgorithmIndicatorView.Indicator; //изменился ли индикатор
                    //удаляем редактируемый индикатор алгоритма
                    AlgorithmIndicatorsView.Remove(SelectedAlgorithmIndicatorView);
                    //добавляем новый индикатор алгоритма
                    AlgorithmIndicatorView algorithmIndicatorView = new AlgorithmIndicatorView { Indicator = AddAlgorithmIndicatorSelectedIndicator, Ending = AddAlgorithmIndicatorEnding };
                    List<IndicatorParameterRangeView> indicatorParameterRangesView = new List<IndicatorParameterRangeView>();
                    for(int i = 0; i < AddAlgorithmIndicatorSelectedIndicator.IndicatorParameterTemplates.Count; i++)
                    {
                        IndicatorParameterRangeView indicatorParameterRangeView = new IndicatorParameterRangeView { IndicatorParameterTemplate = AddAlgorithmIndicatorSelectedIndicator.IndicatorParameterTemplates[i], AlgorithmIndicatorView = algorithmIndicatorView, NameAlgorithmIndicator = AddAlgorithmIndicatorSelectedIndicator.Name + "_" + AddAlgorithmIndicatorEnding };
                        if (isIndicatorChanged == false) //если индикатор не изменился, сохраняем выбранный параметр алгоритма
                        {
                            indicatorParameterRangeView.SelectedAlgorithmParameterView = _editedAlgorithmIndicatorView.IndicatorParameterRangesView[i].SelectedAlgorithmParameterView;
                        }
                        indicatorParameterRangesView.Add(indicatorParameterRangeView);
                    }
                    algorithmIndicatorView.IndicatorParameterRangesView = indicatorParameterRangesView;
                    //вставляем в порядке следования индикаторов
                    int nextIndex = -1; //индкс элемента с id индикатора больше созданного
                    foreach (AlgorithmIndicatorView algorithmIndicatorView1 in AlgorithmIndicatorsView)
                    {
                        if (algorithmIndicatorView1.Indicator.Id > algorithmIndicatorView.Indicator.Id)
                        {
                            if (nextIndex == -1)
                            {
                                nextIndex = AlgorithmIndicatorsView.IndexOf(algorithmIndicatorView1);
                            }
                        }
                    }
                    if (nextIndex == -1)
                    {
                        AlgorithmIndicatorsView.Add(algorithmIndicatorView);
                    }
                    else
                    {
                        AlgorithmIndicatorsView.Insert(nextIndex, algorithmIndicatorView);
                    }

                    //удаляем параметры редактируемого индикатора алгоритма
                    for (int i = IndicatorParameterRangesView.Count - 1; i >= 0; i--)
                    {
                        if (IndicatorParameterRangesView[i].AlgorithmIndicatorView == _editedAlgorithmIndicatorView)
                        {
                            IndicatorParameterRangesView.RemoveAt(i);
                        }
                    }
                    nextIndex = -1; //индкс элемента с id индикатора больше созданного
                    foreach (IndicatorParameterRangeView indicatorParameterRangeView in IndicatorParameterRangesView)
                    {
                        if (indicatorParameterRangeView.IndicatorParameterTemplate.IdIndicator > algorithmIndicatorView.Indicator.Id)
                        {
                            if (nextIndex == -1)
                            {
                                nextIndex = IndicatorParameterRangesView.IndexOf(indicatorParameterRangeView);
                            }
                        }
                    }
                    //вставляем параметры индикатора алгоритма в IndicatorParameterRangesView, в порядке следования индикаторов
                    if (nextIndex == -1) //вставляем в конец если не было индикатора с id больше текущего
                    {
                        foreach (IndicatorParameterRangeView indicatorParameterRangeView in algorithmIndicatorView.IndicatorParameterRangesView)
                        {
                            IndicatorParameterRangesView.Add(indicatorParameterRangeView);
                        }
                    }
                    else //вставляем на место индикатора с id больше текущего
                    {
                        for (int i = 0; i < algorithmIndicatorView.IndicatorParameterRangesView.Count; i++)
                        {
                            IndicatorParameterRangesView.Insert(nextIndex, algorithmIndicatorView.IndicatorParameterRangesView[i]);
                        }
                    }
                    UpdateIndicatorParameterRangeViewAlgorithmParameters(); //обновляем списки с параметрами алгоритма для выбора параметра алгоритма параметру индикатора алгоритма

                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => IsFieldsAddAlgorithmIndicatorCorrect(false));
            }
        }

        public ICommand DeleteAlgorithmIndicator_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //удаляем параметры индикаторов со значениями
                    for(int i = IndicatorParameterRangesView.Count - 1; i >= 0; i--)
                    {
                        if(IndicatorParameterRangesView[i].AlgorithmIndicatorView == SelectedAlgorithmIndicatorView)
                        {
                            IndicatorParameterRangesView.RemoveAt(i);
                        }
                    }
                    //удаляем индикатор алгоритма
                    AlgorithmIndicatorsView.Remove(SelectedAlgorithmIndicatorView);

                }, (obj) => SelectedAlgorithmIndicatorView != null && IsAddOrEditAlgorithm());
            }
        }

        #endregion










        #region view add edit delete AlgorithmParameters

        private ObservableCollection<AlgorithmParameter> _algorithmParameters = new ObservableCollection<AlgorithmParameter>();
        public ObservableCollection<AlgorithmParameter> AlgorithmParameters //параметры алгоритма
        {
            get { return _algorithmParameters; }
            private set
            {
                _algorithmParameters = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<AlgorithmParameterView> _algorithmParametersView = new ObservableCollection<AlgorithmParameterView>();
        public ObservableCollection<AlgorithmParameterView> AlgorithmParametersView //параметры алгоритма
        {
            get { return _algorithmParametersView; }
            private set
            {
                _algorithmParametersView = value;
                OnPropertyChanged();
            }
        }

        private AlgorithmParameterView _selectedAlgorithmParameterView;
        public AlgorithmParameterView SelectedAlgorithmParameterView //выбранный параметр алгоритма
        {
            get { return _selectedAlgorithmParameterView; }
            set
            {
                _selectedAlgorithmParameterView = value;
                OnPropertyChanged();
            }
        }

        private void CreateAlgorithmParametersView()
        {
            AlgorithmParametersView.Clear();
            foreach (AlgorithmParameter algorithmParameter in SelectedAlgorithm.AlgorithmParameters)
            {
                string rangeValuesView = algorithmParameter.MinValue.ToString() + " – " + algorithmParameter.MaxValue.ToString();
                string stepView = algorithmParameter.Step.ToString();
                if (algorithmParameter.IsStepPercent == true)
                {
                    stepView += "%";
                }
                AlgorithmParametersView.Add(new AlgorithmParameterView { Id = algorithmParameter.Id, Name = algorithmParameter.Name, Description = algorithmParameter.Description, ParameterValueType = algorithmParameter.ParameterValueType, MinValue = algorithmParameter.MinValue.ToString(), MaxValue = algorithmParameter.MaxValue.ToString(), Step = algorithmParameter.Step.ToString(), IsStepPercent = algorithmParameter.IsStepPercent, IdAlgorithm = algorithmParameter.IdAlgorithm, RangeValuesView = rangeValuesView, StepView = stepView });
            }
        }

        private ObservableCollection<AlgorithmParameterView> _algorithmParametersViewInt = new ObservableCollection<AlgorithmParameterView>();
        public ObservableCollection<AlgorithmParameterView> AlgorithmParametersViewInt //параметры алгоритма с типом значения int (используется как источник данных для combobox IndicatorParameterRangesView)
        {
            get { return _algorithmParametersViewInt; }
            private set
            {
                _algorithmParametersViewInt = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<AlgorithmParameterView> _algorithmParametersViewDouble = new ObservableCollection<AlgorithmParameterView>();
        public ObservableCollection<AlgorithmParameterView> AlgorithmParametersViewDouble //параметры алгоритма с типом значения double (используется как источник данных для combobox IndicatorParameterRangesView)
        {
            get { return _algorithmParametersViewDouble; }
            private set
            {
                _algorithmParametersViewDouble = value;
                OnPropertyChanged();
            }
        }

        public void CreateAlgorithmParametersViewIntDouble() //формирует списки с параметрами int и double для combobox выбора переменной в параметрах индикаторов
        {
            AlgorithmParametersViewInt.Clear();
            AlgorithmParametersViewDouble.Clear();
            foreach (AlgorithmParameterView algorithmParameterView in AlgorithmParametersView)
            {
                if (algorithmParameterView.ParameterValueType.Id == 1) //целое
                {
                    AlgorithmParametersViewInt.Add(algorithmParameterView);
                }
                else //дробное
                {
                    AlgorithmParametersViewDouble.Add(algorithmParameterView);
                }
            }
        }

        public void UpdateAlgorithmParametersViewIntDouble() //обновляет списки с параметрами int и double для combobox выбора переменной в параметрах индикаторов, добавляя недостающие параметры и удаляя лишние
        {
            int intNumber = -1; //номер int параметра
            int doubleNumber = -1; //номер double параметра
            for(int i = 0; i < AlgorithmParametersView.Count; i++)
            {
                if (AlgorithmParametersView[i].ParameterValueType.Id == 1) //int параметр
                {
                    intNumber++; //увеличиваем номер параметра, чтобы знать по какому индексу вставлять параметр
                    if(AlgorithmParametersViewInt.Where(j=>j.Name == AlgorithmParametersView[i].Name).Any() == false) //если данного параметра недостает, добавляем его
                    {
                        AlgorithmParametersViewInt.Insert(intNumber, AlgorithmParametersView[i]);
                    }
                    if (AlgorithmParametersViewDouble.Where(j => j.Name == AlgorithmParametersView[i].Name).Any() == true) //если данный параметр лишний, удаляем его
                    {
                        AlgorithmParametersViewDouble.Remove(AlgorithmParametersViewDouble.Where(j => j.Name == AlgorithmParametersView[i].Name).First());
                    }
                }
                else //double параметр
                {
                    doubleNumber++; //увеличиваем номер параметра, чтобы знать по какому индексу вставлять параметр
                    if (AlgorithmParametersViewDouble.Where(j => j.Name == AlgorithmParametersView[i].Name).Any() == false) //если данного параметра недостает, добавляем его
                    {
                        AlgorithmParametersViewDouble.Insert(doubleNumber, AlgorithmParametersView[i]);
                    }
                    if (AlgorithmParametersViewInt.Where(j => j.Name == AlgorithmParametersView[i].Name).Any() == true) //если данный параметр лишний, удаляем его
                    {
                        AlgorithmParametersViewInt.Remove(AlgorithmParametersViewInt.Where(j => j.Name == AlgorithmParametersView[i].Name).First());
                    }
                }
            }
            //удаляем параметры int и double, которых нет в AlgorithmParametersView. Чтобы при удалении параметра, он удалялся из списка
            for(int i = AlgorithmParametersViewInt.Count - 1; i >= 0; i--)
            {
                if (AlgorithmParametersView.Where(j => j.Name == AlgorithmParametersViewInt[i].Name).Any() == false)
                {
                    AlgorithmParametersViewInt.RemoveAt(i);
                }
            }
            for(int i = AlgorithmParametersViewDouble.Count - 1; i >= 0; i--)
            {
                if (AlgorithmParametersView.Where(j => j.Name == AlgorithmParametersViewDouble[i].Name).Any() == false)
                {
                    AlgorithmParametersViewDouble.RemoveAt(i);
                }
            }
        }

        private string _algorithmParameterName;
        public string AlgorithmParameterName //название параметра алгоритма
        {
            get { return _algorithmParameterName; }
            set
            {
                _algorithmParameterName = value;
                OnPropertyChanged();
            }
        }

        private string _algorithmParameterDescription;
        public string AlgorithmParameterDescription //описание параметра алгоритма
        {
            get { return _algorithmParameterDescription; }
            set
            {
                _algorithmParameterDescription = value;
                OnPropertyChanged();
            }
        }

        private ParameterValueType _algorithmParameterSelectedParameterValueType;
        public ParameterValueType AlgorithmParameterSelectedParameterValueType //выбранный тип числа
        {
            get { return _algorithmParameterSelectedParameterValueType; }
            set
            {
                _algorithmParameterSelectedParameterValueType = value;
                OnPropertyChanged();
            }
        }

        private string _algorithmParameterMinValue;
        public string AlgorithmParameterMinValue //минимальное значение параметра алгоритма
        {
            get { return _algorithmParameterMinValue; }
            set
            {
                _algorithmParameterMinValue = value;
                OnPropertyChanged();
            }
        }

        private string _algorithmParameterMaxValue;
        public string AlgorithmParameterMaxValue //максимальное значение параметра алгоритма
        {
            get { return _algorithmParameterMaxValue; }
            set
            {
                _algorithmParameterMaxValue = value;
                OnPropertyChanged();
            }
        }

        private string _AlgorithmParameterstep;
        public string AlgorithmParameterstep //шаг параметра алгоритма
        {
            get { return _AlgorithmParameterstep; }
            set
            {
                _AlgorithmParameterstep = value;
                OnPropertyChanged();
            }
        }

        private List<string> _algorithmParameterTypesStep = new List<string> { "процентный", "числовой" };
        public List<string> AlgorithmParameterTypesStep //список с возможными типами шага оптимизируемого параметра
        {
            get { return _algorithmParameterTypesStep; }
            set
            {
                _algorithmParameterTypesStep = value;
                OnPropertyChanged();
            }
        }

        private string _AlgorithmParameterselectedTypeStep;
        public string AlgorithmParameterselectedTypeStep //выбранный тип шага оптимизируемого параметра
        {
            get { return _AlgorithmParameterselectedTypeStep; }
            set
            {
                _AlgorithmParameterselectedTypeStep = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddAlgorithmParameter_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmParameterName = "";
                    AlgorithmParameterDescription = "";
                    AlgorithmParameterSelectedParameterValueType = ParameterValueTypes[0];
                    AlgorithmParameterMinValue = "";
                    AlgorithmParameterMaxValue = "";
                    AlgorithmParameterstep = "";
                    AlgorithmParameterselectedTypeStep = AlgorithmParameterTypesStep[0];

                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    AddAlgorithmParameter addAlgorithmParameter = new AddAlgorithmParameter();
                    addAlgorithmParameter.Show();

                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        private ObservableCollection<string> _tooltipAddAddAlgorithmParameter = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipAddAddAlgorithmParameter //подсказка, показываемая при наведении на кнопку добавить
        {
            get { return _tooltipAddAddAlgorithmParameter; }
            set
            {
                _tooltipAddAddAlgorithmParameter = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsAddAlgorithmParameterCorrect()
        {
            bool result = true;
            TooltipAddAddAlgorithmParameter.Clear(); //очищаем подсказку кнопки добавить

            string name = AlgorithmParameterName != null? AlgorithmParameterName.Replace(" ", "") : "";

            //проверка на пустое значение
            if (name == "" || AlgorithmParameterMinValue == "" || AlgorithmParameterMaxValue == "" || AlgorithmParameterstep == "")
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Не заполнены все поля.");
            }

            //проверка на допустимые символы
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool isNotFind = false;
            for (int i = 0; i < name.Length; i++)
            {
                if (letters.IndexOf(name[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Допустимо использование только английского алфавита.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            foreach (AlgorithmParameterView item in AlgorithmParametersView)
            {
                if (name == item.Name) //проверяем имя на уникальность среди всех записей
                {
                    isUnique = false;
                }
            }
            if (isUnique == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Данное название уже используется.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(AlgorithmParameterMinValue, out double res) == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Минимальное значение должно быть числом.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(AlgorithmParameterMaxValue, out res) == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Максимальное значение должно быть числом.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(AlgorithmParameterstep, out res) == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Шаг должен быть числом.");
            }

            //проверка на возможность достигнуть максимума с минимума с шагом
            if (double.TryParse(AlgorithmParameterMinValue, out double min) && double.TryParse(AlgorithmParameterMaxValue, out double max) && double.TryParse(AlgorithmParameterstep, out double step))
            {
                if ((max > min && step > 0) == false)
                {
                    result = false;
                    TooltipAddAddAlgorithmParameter.Add("Максимум должен быть больше минимума, а шаг должен быть положительным.");
                }
            }

            return result;
        }

        public ICommand AddAddAlgorithmParameter_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    string name = AlgorithmParameterName != null ? AlgorithmParameterName.Replace(" ", "") : "";

                    bool isStepPercent = false;
                    if (AlgorithmParameterselectedTypeStep == AlgorithmParameterTypesStep[0])
                    {
                        isStepPercent = true;
                    }
                    string rangeValuesView = AlgorithmParameterMinValue + " – " + AlgorithmParameterMaxValue;

                    string steView = AlgorithmParameterstep;
                    if (isStepPercent)
                    {
                        steView += "%";
                    }

                    AlgorithmParameterView algorithmParameterView = new AlgorithmParameterView { Name = name, Description = AlgorithmParameterDescription, ParameterValueType = AlgorithmParameterSelectedParameterValueType, MinValue = AlgorithmParameterMinValue, MaxValue = AlgorithmParameterMaxValue, IsStepPercent = isStepPercent, Step = AlgorithmParameterstep, RangeValuesView = rangeValuesView, StepView = steView };
                    AlgorithmParametersView.Add(algorithmParameterView);

                    UpdateAlgorithmParametersViewIntDouble(); //обновляем списки с параметрами агоритма типов int и double

                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => IsFieldsAddAlgorithmParameterCorrect());
            }
        }

        public ICommand EditAlgorithmParameter_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmParameterName = SelectedAlgorithmParameterView.Name;
                    AlgorithmParameterDescription = SelectedAlgorithmParameterView.Description;
                    AlgorithmParameterSelectedParameterValueType = SelectedAlgorithmParameterView.ParameterValueType;
                    AlgorithmParameterMinValue = SelectedAlgorithmParameterView.MinValue;
                    AlgorithmParameterMaxValue = SelectedAlgorithmParameterView.MaxValue;
                    AlgorithmParameterstep = SelectedAlgorithmParameterView.Step;
                    if (SelectedAlgorithmParameterView.IsStepPercent)
                    {
                        AlgorithmParameterselectedTypeStep = AlgorithmParameterTypesStep[0];
                    }
                    else
                    {
                        AlgorithmParameterselectedTypeStep = AlgorithmParameterTypesStep[1];
                    }

                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    EditAlgorithmParameter editAlgorithmParameter = new EditAlgorithmParameter();
                    editAlgorithmParameter.Show();

                }, (obj) => IsAddOrEditAlgorithm() && SelectedAlgorithmParameterView != null);
            }
        }

        private bool IsFieldsEditAlgorithmParameterCorrect()
        {
            bool result = true;
            TooltipAddAddAlgorithmParameter.Clear(); //очищаем подсказку кнопки добавить

            string name = AlgorithmParameterName != null ? AlgorithmParameterName.Replace(" ", "") : "";

            //проверка на пустое значение
            if (name == "" || AlgorithmParameterMinValue == "" || AlgorithmParameterMaxValue == "" || AlgorithmParameterstep == "")
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Не заполнены все поля.");
            }

            //проверка на допустимые символы
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool isNotFind = false;
            for (int i = 0; i < name.Length; i++)
            {
                if (letters.IndexOf(name[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Допустимо использование только английского алфавита.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            foreach (AlgorithmParameterView item in AlgorithmParametersView)
            {
                if (name == item.Name && item != SelectedAlgorithmParameterView) //проверяем имя на уникальность среди всех записей
                {
                    isUnique = false;
                }
            }
            if (isUnique == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Данное название уже используется.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(AlgorithmParameterMinValue, out double res) == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Минимальное значение должно быть числом.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(AlgorithmParameterMaxValue, out res) == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Максимальное значение должно быть числом.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(AlgorithmParameterstep, out res) == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Шаг должен быть числом.");
            }

            //проверка на возможность достигнуть максимума с минимума с шагом
            if (double.TryParse(AlgorithmParameterMinValue, out double min) && double.TryParse(AlgorithmParameterMaxValue, out double max) && double.TryParse(AlgorithmParameterstep, out double step))
            {
                if ((max > min && step > 0) == false)
                {
                    result = false;
                    TooltipAddAddAlgorithmParameter.Add("Максимум должен быть больше минимума, а шаг должен быть положительным.");
                }
            }

            return result;
        }

        public ICommand EditSaveAlgorithmParameter_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    string name = AlgorithmParameterName != null ? AlgorithmParameterName.Replace(" ", "") : "";



                    bool isStepPercent = false;
                    if (AlgorithmParameterselectedTypeStep == AlgorithmParameterTypesStep[0])
                    {
                        isStepPercent = true;
                    }
                    string rangeValuesView = AlgorithmParameterMinValue + " – " + AlgorithmParameterMaxValue;

                    string steView = AlgorithmParameterstep;
                    if (isStepPercent)
                    {
                        steView += "%";
                    }

                    AlgorithmParameterView algorithmParameterView = new AlgorithmParameterView { Name = name, Description = AlgorithmParameterDescription, ParameterValueType = AlgorithmParameterSelectedParameterValueType, MinValue = AlgorithmParameterMinValue, MaxValue = AlgorithmParameterMaxValue, IsStepPercent = isStepPercent, Step = AlgorithmParameterstep, RangeValuesView = rangeValuesView, StepView = steView };

                    int index = AlgorithmParametersView.IndexOf(SelectedAlgorithmParameterView);
                    AlgorithmParametersView.RemoveAt(index);
                    AlgorithmParametersView.Insert(index, algorithmParameterView);
                    SelectedAlgorithmParameterView = algorithmParameterView;

                    UpdateAlgorithmParametersViewIntDouble(); //обновляем списки с параметрами алгоритма типов int и double

                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => IsFieldsEditAlgorithmParameterCorrect());
            }
        }

        public ICommand DeleteAlgorithmParameter_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    int index = AlgorithmParametersView.IndexOf(SelectedAlgorithmParameterView); //находим индекс выбранного элемента
                    string msg = "Название: " + SelectedAlgorithmParameterView.Name;
                    string caption = "Удалить?";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == MessageBoxResult.Yes)
                    {
                        AlgorithmParametersView.RemoveAt(index);
                    }
                    UpdateAlgorithmParametersViewIntDouble(); //обновляем списки с параметрами агоритма типов int и double
                }, (obj) => SelectedAlgorithmParameterView != null && IsAddOrEditAlgorithm());
            }
        }

        #endregion










        #region view add delete DataSourceGroupsView

        private ObservableCollection<DataSourceGroupView> _dataSourceGroupsView = new ObservableCollection<DataSourceGroupView>();
        public ObservableCollection<DataSourceGroupView> DataSourceGroupsView //группы источников данных (соответствие макетов и источников данных, то есть выбранные источники данных для тестирования)
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
            DataSources = _viewModelPageDataSource.DataSourcesForSubscribers;
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
                    DataSourcesForAddingDsGroupsView.Clear();
                    foreach(DataSourceTemplate dataSourceTemplate in SelectedAlgorithm.DataSourceTemplates)
                    {
                        DataSourcesForAddingDsGroupsView.Add(new DataSourcesForAddingDsGroupView { DataSources = _viewModelPageDataSource.DataSourcesForSubscribers, DataSourceTemplate = dataSourceTemplate });
                    }

                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    AddDataSourceGroupView addDataSourceGroupView = new AddDataSourceGroupView();
                    addDataSourceGroupView.Show();

                }, (obj) => SelectedAlgorithm != null);
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
                    for(int i = 0; i < DataSourceGroupsView.Count; i++)
                    {
                        DataSourceGroupsView[i].Number = i + 1;
                    }
                }, (obj) => SelectedDataSourceGroupView != null);
            }
        }

        private ObservableCollection<string> _tooltipAddDataSourceGroupView = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipAddDataSourceGroupView //подсказка, показываемая при наведении на кнопку добавить
        {
            get { return _tooltipAddDataSourceGroupView; }
            set
            {
                _tooltipAddDataSourceGroupView = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsAddDataSourceGroupViewCorrect()
        {
            bool result = true;

            TooltipAddDataSourceGroupView.Clear(); //очищаем подсказку кнопки добавить
            //проверяем на заполненность полей
            bool isAllDataSourceSelected = true;
            foreach(DataSourcesForAddingDsGroupView dataSourcesForAddingDsGroupView in DataSourcesForAddingDsGroupsView)
            {
                if(dataSourcesForAddingDsGroupView.SelectedDataSource == null)
                {
                    isAllDataSourceSelected = false;
                }
            }
            if (isAllDataSourceSelected)
            {
                //удостоверяемся в том что заполненные поля не содержат одинаковых источников данных
                bool isFindEqual = false;
                for(int i = 0; i < DataSourcesForAddingDsGroupsView.Count; i++)
                {
                    for(int k = 0; k < DataSourcesForAddingDsGroupsView.Count; k++)
                    {
                        if(i != k)
                        {
                            if(DataSourcesForAddingDsGroupsView[i].SelectedDataSource == DataSourcesForAddingDsGroupsView[k].SelectedDataSource)
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
                    for(int i = 1; i < DataSourcesForAddingDsGroupsView.Count; i++)
                    {
                        if(DateTime.Compare(startDate, DataSourcesForAddingDsGroupsView[i].SelectedDataSource.StartDate) < 0)
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
                                if ((dataSourceGroupView.DataSourcesAccordances[i].DataSourceTemplate == DataSourcesForAddingDsGroupsView[i].DataSourceTemplate && dataSourceGroupView.DataSourcesAccordances[i].DataSource == DataSourcesForAddingDsGroupsView[i].SelectedDataSource) == false) //данный DataSourcesAccordance не имеет полного совпадения
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
                            TooltipAddDataSourceGroupView.Add("Данная комбинация источников данных уже добавлена, выберите другую.");
                            result = false;
                        }
                    }
                    else
                    {
                        TooltipAddDataSourceGroupView.Add("Выбранные источники данных не имеют пересекающихся дат.");
                        result = false;
                    }
                }
                else
                {
                    TooltipAddDataSourceGroupView.Add("Выбран один источник данных для нескольких макетов.");
                    result = false;
                }
            }
            else
            {
                TooltipAddDataSourceGroupView.Add("Не выбраны источники данных.");
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
                    foreach(DataSourcesForAddingDsGroupView dataSourcesForAddingDsGroupView in DataSourcesForAddingDsGroupsView)
                    {
                        dataSourcesAccordances.Add(new DataSourceAccordanceView { DataSourceTemplate = dataSourcesForAddingDsGroupView.DataSourceTemplate, DataSource = dataSourcesForAddingDsGroupView.SelectedDataSource });
                        if (isFirstIteration)
                        {
                            startDateTime = dataSourcesForAddingDsGroupView.SelectedDataSource.StartDate;
                            endDateTime = dataSourcesForAddingDsGroupView.SelectedDataSource.EndDate;
                        }
                        else
                        {
                            if(DateTime.Compare(startDateTime, dataSourcesForAddingDsGroupView.SelectedDataSource.StartDate) < 0)
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
                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => IsFieldsAddDataSourceGroupViewCorrect());
            }
        }

        public ICommand DataSourceGroupViewCancel_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => true);
            }
        }

        #endregion










        #region view edit add CriteriaTopModel

        private ObservableCollection<EvaluationCriteria> _evaluationCriterias = new ObservableCollection<EvaluationCriteria>(); //критерии оценки тестирования
        public ObservableCollection<EvaluationCriteria> EvaluationCriterias
        {
            get { return _evaluationCriterias; }
            private set
            {
                _evaluationCriterias = value;
                OnPropertyChanged();
                CreateEvaluationCriteriasView();
            }
        }

        private void modelData_EvaluationCriteriasChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EvaluationCriterias = (ObservableCollection<EvaluationCriteria>)sender;
        }

        private ObservableCollection<EvaluationCriteriaView> _evaluationCriteriasView = new ObservableCollection<EvaluationCriteriaView>(); //критерии оценки тестирования для combobox
        public ObservableCollection<EvaluationCriteriaView> EvaluationCriteriasView
        {
            get { return _evaluationCriteriasView; }
            private set
            {
                _evaluationCriteriasView = value;
                OnPropertyChanged();
            }
        }

        private EvaluationCriteriaView _selectedEvaluationCriteriaView = new EvaluationCriteriaView(); //выбранный критерий оценки тестирования для combobox
        public EvaluationCriteriaView SelectedEvaluationCriteriaView
        {
            get { return _selectedEvaluationCriteriaView; }
            set
            {
                _selectedEvaluationCriteriaView = value;
                OnPropertyChanged();
            }
        }

        private void CreateEvaluationCriteriasView() //создает критерии оценки для представления. Добавляет только те, которые имеют числовое значение
        {
            EvaluationCriteriasView.Clear();
            foreach (EvaluationCriteria evaluationCriteria in EvaluationCriterias)
            {
                if (evaluationCriteria.IsDoubleValue)
                {
                    EvaluationCriteriasView.Add(new EvaluationCriteriaView { EvaluationCriteria = evaluationCriteria, Name = evaluationCriteria.Name });
                }
            }
            SelectedCompareSignsEvaluationCriteria = CompareSignsEvaluationCriteria[0];
        }

        private ObservableCollection<string> _compareSignsEvaluationCriteria = new ObservableCollection<string> { "Максимальное", "Минимальное" };
        public ObservableCollection<string> CompareSignsEvaluationCriteria
        {
            get { return _compareSignsEvaluationCriteria; }
            private set
            {
                _compareSignsEvaluationCriteria = value;
                OnPropertyChanged();
            }
        }

        private string _selectedCompareSignsEvaluationCriteria;
        public string SelectedCompareSignsEvaluationCriteria
        {
            get { return _selectedCompareSignsEvaluationCriteria; }
            set
            {
                _selectedCompareSignsEvaluationCriteria = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<FilterTopModelView> _filtersTopModelView = new ObservableCollection<FilterTopModelView>();
        public ObservableCollection<FilterTopModelView> FiltersTopModelView
        {
            get { return _filtersTopModelView; }
            set
            {
                _filtersTopModelView = value;
                OnPropertyChanged();
            }
        }

        private FilterTopModelView _selectedFilterTopModelView;
        public FilterTopModelView SelectedFilterTopModelView
        {
            get { return _selectedFilterTopModelView; }
            set
            {
                _selectedFilterTopModelView = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddFilterTopModelView_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    FiltersTopModelView.Add(new FilterTopModelView { CompareSings = new ObservableCollection<string> { ">", "<" }, EvaluationCriteriasView = EvaluationCriteriasView });
                }, (obj) => SelectedAlgorithm != null);
            }
        }

        public ICommand DeleteFilterTopModelView_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    FiltersTopModelView.Remove(SelectedFilterTopModelView);
                }, (obj) => SelectedFilterTopModelView != null);
            }
        }

        #endregion










        #region IsConsiderNeighbours, view edit AxesParameters, IsForwardTesting, Deposit, Period, Duration, LaunchTesting

        private bool _isConsiderNeighbours = true;
        public bool IsConsiderNeighbours //выбран ли параметр, учитывать соседнии результаты при поике топ-модели
        {
            get { return _isConsiderNeighbours; }
            set
            {
                _isConsiderNeighbours = value;
                OnPropertyChanged();
            }
        }

        private string _sizeNeighboursGroupPercent = "16";
        public string SizeNeighboursGroupPercent //размер группы соседних тестов от общей площади поисковой плоскости
        {
            get { return _sizeNeighboursGroupPercent; }
            set
            {
                _sizeNeighboursGroupPercent = value;
                OnPropertyChanged();
            }
        }

        private bool _isForwardTesting = false;
        public bool IsForwardTesting //проводить ли форвардное тестирование
        {
            get { return _isForwardTesting; }
            set
            {
                _isForwardTesting = value;
                OnPropertyChanged();
            }
        }

        private bool _isForwardDepositTesting = false;
        public bool IsForwardDepositTesting //добавить ли торговлю депозитом для форвардного тестирования
        {
            get { return _isForwardDepositTesting; }
            set
            {
                _isForwardDepositTesting = value;
                OnPropertyChanged();
            }
        }

        private string _forwardDeposit;
        public string ForwardDeposit //размер депозита
        {
            get { return _forwardDeposit; }
            set
            {
                _forwardDeposit = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Currency> _currencies;
        public ObservableCollection<Currency> Currencies //валюты для депозита
        {
            get { return _currencies; }
            set
            {
                _currencies = value;
                OnPropertyChanged();
            }
        }

        private Currency _selectedCurrency;
        public Currency SelectedCurrency //выбранная валюта для депозита
        {
            get { return _selectedCurrency; }
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged();
            }
        }

        private string _durationOptimizationYears;
        public string DurationOptimizationYears
        {
            get { return _durationOptimizationYears; }
            set
            {
                _durationOptimizationYears = value;
                OnPropertyChanged();
            }
        }

        private string _durationOptimizationMonths;
        public string DurationOptimizationMonths
        {
            get { return _durationOptimizationMonths; }
            set
            {
                _durationOptimizationMonths = value;
                OnPropertyChanged();
            }
        }

        private string _durationOptimizationDays;
        public string DurationOptimizationDays
        {
            get { return _durationOptimizationDays; }
            set
            {
                _durationOptimizationDays = value;
                OnPropertyChanged();
            }
        }

        private string _optimizationSpacingYears;
        public string OptimizationSpacingYears
        {
            get { return _optimizationSpacingYears; }
            set
            {
                _optimizationSpacingYears = value;
                OnPropertyChanged();
            }
        }

        private string _optimizationSpacingMonths;
        public string OptimizationSpacingMonths
        {
            get { return _optimizationSpacingMonths; }
            set
            {
                _optimizationSpacingMonths = value;
                OnPropertyChanged();
            }
        }

        private string _optimizationSpacingDays;
        public string OptimizationSpacingDays
        {
            get { return _optimizationSpacingDays; }
            set
            {
                _optimizationSpacingDays = value;
                OnPropertyChanged();
            }
        }

        private string _durationForwardYears;
        public string DurationForwardYears
        {
            get { return _durationForwardYears; }
            set
            {
                _durationForwardYears = value;
                OnPropertyChanged();
            }
        }

        private string _durationForwardMonths;
        public string DurationForwardMonths
        {
            get { return _durationForwardMonths; }
            set
            {
                _durationForwardMonths = value;
                OnPropertyChanged();
            }
        }

        private string _durationForwardDays;
        public string DurationForwardDays
        {
            get { return _durationForwardDays; }
            set
            {
                _durationForwardDays = value;
                OnPropertyChanged();
            }
        }

        public ICommand DurationOptimizationTestsYearsIncrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationOptimizationYears, out int res))
                    {
                        DurationOptimizationYears = (res + 1).ToString();
                    }
                    else
                    {
                        DurationOptimizationYears = "1";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand DurationOptimizationTestsYearsDecrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationOptimizationYears, out int res))
                    {
                        DurationOptimizationYears = (res - 1).ToString();
                    }
                    else
                    {
                        DurationOptimizationYears = "0";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand DurationOptimizationTestsMonthsIncrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationOptimizationMonths, out int res))
                    {
                        DurationOptimizationMonths = (res + 1).ToString();
                    }
                    else
                    {
                        DurationOptimizationMonths = "1";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand DurationOptimizationTestsMonthsDecrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationOptimizationMonths, out int res))
                    {
                        DurationOptimizationMonths = (res - 1).ToString();
                    }
                    else
                    {
                        DurationOptimizationMonths = "0";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand DurationOptimizationTestsDaysIncrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationOptimizationDays, out int res))
                    {
                        DurationOptimizationDays = (res + 1).ToString();
                    }
                    else
                    {
                        DurationOptimizationDays = "1";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand DurationOptimizationTestsDaysDecrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationOptimizationDays, out int res))
                    {
                        DurationOptimizationDays = (res - 1).ToString();
                    }
                    else
                    {
                        DurationOptimizationDays = "0";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }

        public ICommand SpacingOptimizationTestsYearsIncrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(OptimizationSpacingYears, out int res))
                    {
                        OptimizationSpacingYears = (res + 1).ToString();
                    }
                    else
                    {
                        OptimizationSpacingYears = "1";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand SpacingOptimizationTestsYearsDecrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(OptimizationSpacingYears, out int res))
                    {
                        OptimizationSpacingYears = (res - 1).ToString();
                    }
                    else
                    {
                        OptimizationSpacingYears = "0";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand SpacingOptimizationTestsMonthsIncrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(OptimizationSpacingMonths, out int res))
                    {
                        OptimizationSpacingMonths = (res + 1).ToString();
                    }
                    else
                    {
                        OptimizationSpacingMonths = "1";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand SpacingOptimizationTestsMonthsDecrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(OptimizationSpacingMonths, out int res))
                    {
                        OptimizationSpacingMonths = (res - 1).ToString();
                    }
                    else
                    {
                        OptimizationSpacingMonths = "0";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand SpacingOptimizationTestsDaysIncrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(OptimizationSpacingDays, out int res))
                    {
                        OptimizationSpacingDays = (res + 1).ToString();
                    }
                    else
                    {
                        OptimizationSpacingDays = "1";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }
        public ICommand SpacingOptimizationTestsDaysDecrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(OptimizationSpacingDays, out int res))
                    {
                        OptimizationSpacingDays = (res - 1).ToString();
                    }
                    else
                    {
                        OptimizationSpacingDays = "0";
                    }
                }, (obj) => SelectedAlgorithm != null);
            }
        }

        public ICommand DurationForwardTestYearsIncrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationForwardYears, out int res))
                    {
                        DurationForwardYears = (res + 1).ToString();
                    }
                    else
                    {
                        DurationForwardYears = "1";
                    }
                }, (obj) => SelectedAlgorithm != null && IsForwardTesting);
            }
        }
        public ICommand DurationForwardTestYearsDecrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationForwardYears, out int res))
                    {
                        DurationForwardYears = (res - 1).ToString();
                    }
                    else
                    {
                        DurationForwardYears = "0";
                    }
                }, (obj) => SelectedAlgorithm != null && IsForwardTesting);
            }
        }
        public ICommand DurationForwardTestMonthsIncrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationForwardMonths, out int res))
                    {
                        DurationForwardMonths = (res + 1).ToString();
                    }
                    else
                    {
                        DurationForwardMonths = "1";
                    }
                }, (obj) => SelectedAlgorithm != null && IsForwardTesting);
            }
        }
        public ICommand DurationForwardTestMonthsDecrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationForwardMonths, out int res))
                    {
                        DurationForwardMonths = (res - 1).ToString();
                    }
                    else
                    {
                        DurationForwardMonths = "0";
                    }
                }, (obj) => SelectedAlgorithm != null && IsForwardTesting);
            }
        }
        public ICommand DurationForwardTestDaysIncrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationForwardDays, out int res))
                    {
                        DurationForwardDays = (res + 1).ToString();
                    }
                    else
                    {
                        DurationForwardDays = "1";
                    }
                }, (obj) => SelectedAlgorithm != null && IsForwardTesting);
            }
        }
        public ICommand DurationForwardTestDaysDecrement_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (int.TryParse(DurationForwardDays, out int res))
                    {
                        DurationForwardDays = (res - 1).ToString();
                    }
                    else
                    {
                        DurationForwardDays = "0";
                    }
                }, (obj) => SelectedAlgorithm != null && IsForwardTesting);
            }
        }

        private ObservableCollection<string> _tooltipLaunchTesting = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipLaunchTesting
        {
            get { return _tooltipLaunchTesting; }
            set
            {
                _tooltipLaunchTesting = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsTestingCorrect()
        {
            bool result = true;
            TooltipLaunchTesting.Clear();

            //проверяем, добавлены ли источники данных
            if(DataSourceGroupsView.Count == 0)
            {
                result = false;
                TooltipLaunchTesting.Add("Добавьте источники данных.");
            }

            //проверяем, выбран ли критерий оценки топ-модели
            if(EvaluationCriteriasView.Contains(SelectedEvaluationCriteriaView) == false)
            {
                result = false;
                TooltipLaunchTesting.Add("Выберите критерий оценки топ-модели.");
            }

            //проверяем что все фильтры заполнены корректными значениями
            if(FiltersTopModelView.Count > 0)
            {
                bool isCorrect = true;
                foreach(FilterTopModelView filterTopModelView in FiltersTopModelView)
                {
                    if(filterTopModelView.SelectedEvaluationCriteriaView == null || filterTopModelView.SelectedCompareSing == null || double.TryParse(filterTopModelView.FilterValue, out double res) == false)
                    {
                        isCorrect = false;
                    }
                }
                if(isCorrect == false)
                {
                    result = false;
                    TooltipLaunchTesting.Add("Не заполнены все поля фильтров топ-модели, или значения некорректны.");
                }
            }

            //проверяем что размер группы соседних тестов имеет корректное значение
            if (IsConsiderNeighbours)
            {
                bool isCorrect = true;
                if(double.TryParse(SizeNeighboursGroupPercent, out double res))
                {
                    if(res <= 0)
                    {
                        isCorrect = false;
                    }
                }
                else
                {
                    isCorrect = false;
                }
                if (isCorrect == false)
                {
                    result = false;
                    TooltipLaunchTesting.Add("Размер группы соседних тестов должен быть положительным числом.");
                }
            }

            //проверяем что размер депозита имеет корректное значение
            if(IsForwardTesting && IsForwardDepositTesting)
            {
                if(double.TryParse(ForwardDeposit, out double res) == false)
                {
                    result = false;
                    TooltipLaunchTesting.Add("Размер депозита для форвардного тестирования имеет некорректное значение.");
                }
            }

            //проверяем корректность длительности оптимизационных тестов
            bool isYearsNotEmpty = true;
            bool isMonthsNotEmpty = true;
            bool isDaysNotEmpty = true;
            if(DurationOptimizationYears == "" || DurationOptimizationYears == null)
            {
                isYearsNotEmpty = false;
            }
            if(DurationOptimizationMonths == "" || DurationOptimizationMonths == null)
            {
                isMonthsNotEmpty = false;
            }
            if(DurationOptimizationDays == "" || DurationOptimizationDays == null)
            {
                isDaysNotEmpty = false;
            }
            if(isYearsNotEmpty || isMonthsNotEmpty || isDaysNotEmpty)
            {
                int resSum = 0; //сумма длительностей (чтобы допускать когда в некоторых 0 а в некоторых больше нуля)
                bool isCorrect = true;
                if (isYearsNotEmpty)
                {
                    if(int.TryParse(DurationOptimizationYears, out int res))
                    {
                        if(res < 0)
                        {
                            isCorrect = false;
                        }
                        else
                        {
                            resSum += res;
                        }
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }
                if (isMonthsNotEmpty)
                {
                    if(int.TryParse(DurationOptimizationMonths, out int res))
                    {
                        if (res < 0)
                        {
                            isCorrect = false;
                        }
                        else
                        {
                            resSum += res;
                        }
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }
                if (isDaysNotEmpty)
                {
                    if(int.TryParse(DurationOptimizationDays, out int res))
                    {
                        if (res < 0)
                        {
                            isCorrect = false;
                        }
                        else
                        {
                            resSum += res;
                        }
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }
                if(isCorrect == false || resSum == 0)
                {
                    result = false;
                    TooltipLaunchTesting.Add("Длительность оптимизационных тестов должна быть целым положительным числом.");
                }
            }
            else
            {
                result = false;
                TooltipLaunchTesting.Add("Не заполнена длительность оптимизационных тестов.");
            }

            //проверяем корректность промежутка между оптимизационными тестами
            isYearsNotEmpty = true;
            isMonthsNotEmpty = true;
            isDaysNotEmpty = true;
            if (OptimizationSpacingYears == "" || OptimizationSpacingYears == null)
            {
                isYearsNotEmpty = false;
            }
            if (OptimizationSpacingMonths == "" || OptimizationSpacingMonths == null)
            {
                isMonthsNotEmpty = false;
            }
            if (OptimizationSpacingDays == "" || OptimizationSpacingDays == null)
            {
                isDaysNotEmpty = false;
            }
            if (isYearsNotEmpty || isMonthsNotEmpty || isDaysNotEmpty)
            {
                int resSum = 0; //сумма длительностей (чтобы допускать когда в некоторых 0 а в некоторых больше нуля)
                bool isCorrect = true;
                if (isYearsNotEmpty)
                {
                    if (int.TryParse(OptimizationSpacingYears, out int res))
                    {
                        if (res < 0)
                        {
                            isCorrect = false;
                        }
                        else
                        {
                            resSum += res;
                        }
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }
                if (isMonthsNotEmpty)
                {
                    if (int.TryParse(OptimizationSpacingMonths, out int res))
                    {
                        if (res < 0)
                        {
                            isCorrect = false;
                        }
                        else
                        {
                            resSum += res;
                        }
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }
                if (isDaysNotEmpty)
                {
                    if (int.TryParse(OptimizationSpacingDays, out int res))
                    {
                        if (res < 0)
                        {
                            isCorrect = false;
                        }
                        else
                        {
                            resSum += res;
                        }
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }
                if (isCorrect == false || resSum == 0)
                {
                    result = false;
                    TooltipLaunchTesting.Add("Промежуток между оптимизационными тестами должен быть целым положитльным числом.");
                }
            }
            else
            {
                result = false;
                TooltipLaunchTesting.Add("Не заполнен промежуток между оптимизационными тестами.");
            }

            //проверяем корректность длительности форвардного теста
            if (IsForwardTesting)
            {
                isYearsNotEmpty = true;
                isMonthsNotEmpty = true;
                isDaysNotEmpty = true;
                if (DurationForwardYears == "" || DurationForwardYears == null)
                {
                    isYearsNotEmpty = false;
                }
                if (DurationForwardMonths == "" || DurationForwardMonths == null)
                {
                    isMonthsNotEmpty = false;
                }
                if (DurationForwardDays == "" || DurationForwardDays == null)
                {
                    isDaysNotEmpty = false;
                }
                if (isYearsNotEmpty || isMonthsNotEmpty || isDaysNotEmpty)
                {
                    int resSum = 0; //сумма длительностей (чтобы допускать когда в некоторых 0 а в некоторых больше нуля)
                    bool isCorrect = true;
                    if (isYearsNotEmpty)
                    {
                        if (int.TryParse(DurationForwardYears, out int res))
                        {
                            if (res < 0)
                            {
                                isCorrect = false;
                            }
                            else
                            {
                                resSum += res;
                            }
                        }
                        else
                        {
                            isCorrect = false;
                        }
                    }
                    if (isMonthsNotEmpty)
                    {
                        if (int.TryParse(DurationForwardMonths, out int res))
                        {
                            if (res < 0)
                            {
                                isCorrect = false;
                            }
                            else
                            {
                                resSum += res;
                            }
                        }
                        else
                        {
                            isCorrect = false;
                        }
                    }
                    if (isDaysNotEmpty)
                    {
                        if (int.TryParse(DurationForwardDays, out int res))
                        {
                            if (res < 0)
                            {
                                isCorrect = false;
                            }
                            else
                            {
                                resSum += res;
                            }
                        }
                        else
                        {
                            isCorrect = false;
                        }
                    }
                    if (isCorrect == false || resSum == 0)
                    {
                        result = false;
                        TooltipLaunchTesting.Add("Длительность форвардного теста должна быть целым положительным числом.");
                    }
                }
                else
                {
                    result = false;
                    TooltipLaunchTesting.Add("Не заполнена длительность форвардного теста.");
                }
            }

            return result;
        }

        public ICommand LaunchTesting_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //DataSourceGroups
                    List<DataSourceGroup> dataSourceGroups = new List<DataSourceGroup>();
                    foreach(DataSourceGroupView dataSourceGroupView in DataSourceGroupsView)
                    {
                        List<DataSourceAccordance> dataSourceAccordances = new List<DataSourceAccordance>();
                        foreach (DataSourceAccordanceView dataSourceAccordanceView in dataSourceGroupView.DataSourcesAccordances)
                        {
                            dataSourceAccordances.Add(new DataSourceAccordance { DataSourceTemplate = dataSourceAccordanceView.DataSourceTemplate, DataSource = dataSourceAccordanceView.DataSource });
                        }
                        dataSourceGroups.Add(new DataSourceGroup { DataSourceAccordances = dataSourceAccordances, StartPeriodTesting = dataSourceGroupView.StartPeriodTesting, EndPeriodTesting = dataSourceGroupView.EndPeriodTesting });
                    }

                    //TopModelCriteria
                    TopModelCriteria topModelCriteria = new TopModelCriteria();
                    topModelCriteria.EvaluationCriteria = SelectedEvaluationCriteriaView.EvaluationCriteria;
                    topModelCriteria.CompareSign = SelectedCompareSignsEvaluationCriteria == CompareSignsEvaluationCriteria[0] ? CompareSign.GetMax() : CompareSign.GetMin();
                    List<TopModelFilter> topModelFilters = new List<TopModelFilter>();
                    foreach (FilterTopModelView filterTopModelView in FiltersTopModelView)
                    {
                        topModelFilters.Add(new TopModelFilter { EvaluationCriteria = filterTopModelView.SelectedEvaluationCriteriaView.EvaluationCriteria, CompareSign = filterTopModelView.SelectedCompareSing == ">" ? CompareSign.GetMore() : CompareSign.GetLess(), Value = double.Parse(filterTopModelView.FilterValue) });
                    }
                    topModelCriteria.TopModelFilters = topModelFilters;

                    //DurationOptimizationTests
                    DateTimeDuration durationOptimizationTests = new DateTimeDuration();
                    durationOptimizationTests.Years = (DurationOptimizationYears == "" || DurationOptimizationYears == null) ? 0 : int.Parse(DurationOptimizationYears);
                    durationOptimizationTests.Months = (DurationOptimizationMonths == "" || DurationOptimizationMonths == null) ? 0 : int.Parse(DurationOptimizationMonths);
                    durationOptimizationTests.Days = (DurationOptimizationDays == "" || DurationOptimizationDays == null) ? 0 : int.Parse(DurationOptimizationDays);

                    //OptimizationTestSpacing
                    DateTimeDuration optimizationTestSpacing = new DateTimeDuration();
                    optimizationTestSpacing.Years = (OptimizationSpacingYears == "" || OptimizationSpacingYears == null) ? 0 : int.Parse(OptimizationSpacingYears);
                    optimizationTestSpacing.Months = (OptimizationSpacingMonths == "" || OptimizationSpacingMonths == null) ? 0 : int.Parse(OptimizationSpacingMonths);
                    optimizationTestSpacing.Days = (OptimizationSpacingDays == "" || OptimizationSpacingDays == null) ? 0 : int.Parse(OptimizationSpacingDays);

                    //DurationForwardTest
                    DateTimeDuration durationForwardTest = new DateTimeDuration();
                    durationForwardTest.Years = (DurationForwardYears == "" || DurationForwardYears == null || IsForwardTesting == false) ? 0 : int.Parse(DurationForwardYears);
                    durationForwardTest.Months = (DurationForwardMonths == "" || DurationForwardMonths == null || IsForwardTesting == false) ? 0 : int.Parse(DurationForwardMonths);
                    durationForwardTest.Days = (DurationForwardDays == "" || DurationForwardDays == null || IsForwardTesting == false) ? 0 : int.Parse(DurationForwardDays);

                    //создаем объект Testing
                    Testing testing = new Testing();
                    testing.Algorithm = SelectedAlgorithm;
                    testing.DataSourceGroups = dataSourceGroups;
                    testing.TopModelCriteria = topModelCriteria;
                    testing.IsConsiderNeighbours = IsConsiderNeighbours;
                    testing.SizeNeighboursGroupPercent = double.Parse(SizeNeighboursGroupPercent);
                    testing.IsForwardTesting = IsForwardTesting;
                    testing.IsForwardDepositTrading = IsForwardDepositTesting;
                    testing.ForwardDeposit = double.Parse(ForwardDeposit);
                    testing.DefaultCurrency = SelectedCurrency;
                    testing.DurationOptimizationTests = durationOptimizationTests;
                    testing.OptimizationTestSpacing = optimizationTestSpacing;
                    testing.DurationForwardTest = durationForwardTest;

                    _viewmodelData.IsPagesAndMainMenuButtonsEnabled = false; //делаем форму недоступной для действий пользователя
                    _viewmodelData.StatusBarTestingShow(); //показываем строку состояния

                    //передаем объект в модель
                    Task.Run(() => _modelTesting.TestingLaunch(testing)); //запускаем в отдельном потоке чтобы форма обновлялась

                }, (obj) => IsFieldsTestingCorrect());
            }
        }

        #endregion
    }
}