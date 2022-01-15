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

            _modelData.Algorithms.CollectionChanged += modelData_AlgorithmsCollectionChanged;
            Algorithms = _modelData.Algorithms;

            _modelData.EvaluationCriterias.CollectionChanged += modelData_EvaluationCriteriasChanged;
            EvaluationCriterias = _modelData.EvaluationCriterias;

            _modelTesting = ModelTesting.getInstance();
            _viewModelPageDataSource = ViewModelPageDataSource.getInstance();

            Currencies = _modelData.Currencies;
            SelectedCurrency = Currencies[0];

            DataSourceGroupsView.CollectionChanged += DataSourceGroupsViewChanged; //добавили создание периода тестирования при изменении групп источников данных для тестирования
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

        private Visibility _indicatorsVisibility = Visibility.Collapsed;
        public Visibility IndicatorsVisibility //видимость панели индикаторов
        {
            get { return _indicatorsVisibility; }
            private set
            {
                _indicatorsVisibility = value;
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

        public ICommand IndicatorsVisibility_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (IndicatorsVisibility == Visibility.Visible)
                    {
                        IndicatorsVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        IndicatorsVisibility = Visibility.Visible;
                    }
                }, (obj) => true);
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
                    IndicatorScript = Environment.NewLine + Environment.NewLine + "return 0;";
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteCandles_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "candles[0]");
                }, (obj) => IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteSelectedParameter_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "Parameter." + SelectedIndicatorParameterTemplateView.Name);
                }, (obj) => SelectedIndicatorParameterTemplateView != null && IsAddOrEditIndicator());
            }
        }

        public ICommand IndicatorPasteReturn_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "return 0;");
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
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "int i = 1;" + Environment.NewLine + "while (  ) {" + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + "i++;" + Environment.NewLine + "}");
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

                    ///*
                    Microsoft.CSharp.CSharpCodeProvider Provider = new Microsoft.CSharp.CSharpCodeProvider();
                    System.CodeDom.Compiler.CompilerParameters Param = new System.CodeDom.Compiler.CompilerParameters();
                    Param.GenerateExecutable = false;
                    Param.GenerateInMemory = true;


                    var Result = Provider.CompileAssemblyFromSource(Param, new string[]
                    {
                        @"
                        using System;
                        public class Test
                        {
                            public double Main()
                            {
                                double a = 5;
                                for(int i = 0; i < 10000; i++)
                                {
                                    a /= 2;
                                    a *= 2;
                                    a += 1;
                                }
                                return a;
                            }
                        }"
                    });
                    /*dynamic test = Result.CompiledAssembly.CreateInstance("Test");

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    double b = test.Main();
                    stopwatch.Stop();*/

                    dynamic[] tests = new dynamic[4];
                    tests[0] = Result.CompiledAssembly.CreateInstance("Test");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    double b = tests[0].Main();
                    stopwatch.Stop();
                    MessageBox.Show("b= " + b.ToString() + " Milliseconds= " + stopwatch.Elapsed.TotalMilliseconds.ToString());
                    //*/

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
                SelectLatestAlgorithm(); //выбирает алгоритм который был последним выбранным
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

        private void SelectLatestAlgorithm() //выбирает алгоритм который был последним выбранным
        {
            bool isFindLastAlgorithm = false; //был ли найден последний алгоритм. Если нет - значит был добавлен новый
            foreach(Algorithm algorithm in Algorithms)
            {
                if(algorithm.Id == _algorithmId)
                {
                    SelectedAlgorithm = algorithm;
                    isFindLastAlgorithm = true;
                }
            }
            if (isFindLastAlgorithm == false) //выбираем добавленный алгоритм
            {
                if(Algorithms.Count > 0)
                {
                    SelectedAlgorithm = Algorithms.Last();
                }
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
                CreateIndicatorParameterRangesView();
                UpdateAlgorithmIndicators();
                CreateAlgorithmParametersView();
                AlgorithmScript = SelectedAlgorithm.Script;
            }
            else
            {
                AlgorithmName = "";
                AlgorithmDescription = "";
                DataSourceTemplatesView.Clear();
                IndicatorParameterRangesView.Clear();
                AlgorithmIndicators.Clear();
                AlgorithmParametersView.Clear();
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

        public ICommand AlgorithmResetToTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = Environment.NewLine + Environment.NewLine + "return 0;";
                }, (obj) => IsAddOrEditAlgorithm());
            }
        }

        public ICommand AlgorithmPasteCandles_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AlgorithmScript = AlgorithmScript.Insert(AlgorithmScriptTextBox.CaretIndex, "candles[0]");
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
                name = AlgorithmName.Replace(" ", "");
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
                TooltipSaveAlgorithm.Add("Допустимо использование для названия только русского и английского алфавитов, и цифр.");
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
                TooltipSaveAlgorithm.Add("Необходимо добавить минимум один макет источника данных.");
            }

            //проверка на заполненные значения параметров индикаторов
            bool isFilledRanges = true;
            foreach(IndicatorParameterRangeView indicatorParameterRangeView in IndicatorParameterRangesView)
            {
                if(indicatorParameterRangeView.MinValue == "")
                {
                    isFilledRanges = false;
                }
            }
            if(isFilledRanges == false)
            {
                result = false;
                TooltipSaveAlgorithm.Add("Не заполнены значения параметров индикаторов.");
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

                    List<IndicatorParameterRange> indicatorParameterRanges = new List<IndicatorParameterRange>();
                    foreach(IndicatorParameterRangeView indicatorParameterRangeView in IndicatorParameterRangesView)
                    {
                        IndicatorParameterRange indicatorParameterRange = new IndicatorParameterRange {Id = indicatorParameterRangeView.Id,  MinValue = double.Parse(indicatorParameterRangeView.MinValue), MaxValue = double.Parse(indicatorParameterRangeView.MaxValue), Step = double.Parse(indicatorParameterRangeView.Step), IsStepPercent = indicatorParameterRangeView.IsStepPercent, IdAlgorithm = indicatorParameterRangeView.IdAlgorithm, IndicatorParameterTemplate = indicatorParameterRangeView.IndicatorParameterTemplate, Indicator = indicatorParameterRangeView.Indicator };
                        indicatorParameterRanges.Add(indicatorParameterRange);
                    }

                    List<AlgorithmParameter> algorithmParameters = new List<AlgorithmParameter>();
                    foreach(AlgorithmParameterView algorithmParameterView in AlgorithmParametersView)
                    {
                        AlgorithmParameter algorithmParameter = new AlgorithmParameter { Id = algorithmParameterView.Id, Name = algorithmParameterView.Name, Description = algorithmParameterView.Description, ParameterValueType = algorithmParameterView.ParameterValueType, MinValue = double.Parse(algorithmParameterView.MinValue), MaxValue = double.Parse(algorithmParameterView.MaxValue), Step = double.Parse(algorithmParameterView.Step), IsStepPercent = algorithmParameterView.IsStepPercent, IdAlgorithm = algorithmParameterView.IdAlgorithm };
                        algorithmParameters.Add(algorithmParameter);
                    }
                    
                    if (IsAlgorithmAdded)
                    {
                        _modelTesting.AlgorithmInsertUpdate(AlgorithmName, AlgorithmDescription, dataSourceTemplates, indicatorParameterRanges, algorithmParameters, AlgorithmScript);
                    }
                    else if (IsAlgorithmEdited)
                    {
                        _modelTesting.AlgorithmInsertUpdate(AlgorithmName, AlgorithmDescription, dataSourceTemplates, indicatorParameterRanges, algorithmParameters, AlgorithmScript, _algorithmId);
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
            foreach (DataSourceTemplate item in DataSourceTemplates)
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
            foreach (DataSourceTemplate item in DataSourceTemplates)
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

        private void CreateIndicatorParameterRangesView()
        {
            IndicatorParameterRangesView.Clear();

            foreach(IndicatorParameterRange indicatorParameterRange in SelectedAlgorithm.IndicatorParameterRanges)
            {
                int index = -1;
                foreach(IndicatorParameterTemplate indicatorParameterTemplate in IndicatorParameterTemplates)
                {
                    if(indicatorParameterTemplate == indicatorParameterRange.IndicatorParameterTemplate)
                    {
                        index = IndicatorParameterTemplates.IndexOf(indicatorParameterTemplate);
                    }
                }
                string rangeValuesView = indicatorParameterRange.MinValue.ToString() + " – " + indicatorParameterRange.MaxValue.ToString();
                string stepView = "";
                stepView += indicatorParameterRange.Step.ToString();
                if (indicatorParameterRange.IsStepPercent == true)
                {
                    stepView += "%";
                }

                IndicatorParameterRangeView indicatorParameterRangeView = new IndicatorParameterRangeView { Id = indicatorParameterRange.Id, MinValue = indicatorParameterRange.MinValue.ToString(), MaxValue = indicatorParameterRange.MaxValue.ToString(), Step = indicatorParameterRange.Step.ToString(), IsStepPercent = indicatorParameterRange.IsStepPercent, IdAlgorithm = indicatorParameterRange.IdAlgorithm, IndicatorParameterTemplate = indicatorParameterRange.IndicatorParameterTemplate, Indicator = indicatorParameterRange.Indicator, NameIndicator = indicatorParameterRange.Indicator.Name, NameIndicatorParameterTemplate = IndicatorParameterTemplates[index].Name, DescriptionIndicatorParameterTemplate = IndicatorParameterTemplates[index].Description, RangeValuesView = rangeValuesView, StepView = stepView };
                IndicatorParameterRangesView.Add(indicatorParameterRangeView);
            }
        }

        private ObservableCollection<Indicator> _algorithmIndicators = new ObservableCollection<Indicator>();
        public ObservableCollection<Indicator> AlgorithmIndicators //индикаторы алгоритма
        {
            get { return _algorithmIndicators; }
            private set
            {
                _algorithmIndicators = value;
                OnPropertyChanged();
            }
        }
        
        private Indicator _selectedAlgorithmIndicator;
        public Indicator SelectedAlgorithmIndicator //выбранный индикатор алгоритма
        {
            get { return _selectedAlgorithmIndicator; }
            set
            {
                _selectedAlgorithmIndicator = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddAlgorithmIndicator_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //вставляет parameterTemplates выбранного индикатора в IndicatorParameterRangesView, так чтобы индикаторы распологались по порядку в IndicatorParameterRangesView
                    int nextIdIndex = -1; //индекс IndicatorParameterRangeView, у которого id индикатора больше id добавляемого индикатора (добавляемые параметры индикатора будут добавлены до него)
                    foreach (IndicatorParameterRangeView item in IndicatorParameterRangesView)
                    {
                        if(nextIdIndex == -1)
                        {
                            if(item.Indicator.Id > SelectedIndicator.Id)
                            {
                                nextIdIndex = IndicatorParameterRangesView.IndexOf(item);
                            }
                        }
                    }
                    //добавляет parameterTemplates в IndicatorParameterRangesView
                    int i = 0; //показывает количество вставленных элементов (при добавлении параметра перед каким-либо, индекс следующего элемента сдвинется, а добавление по старому индексу поставит элемент перед только что добавленным. Чтобы это исправить элемент вставляется на nextIdIndex + i)
                    foreach (IndicatorParameterTemplate item in SelectedIndicator.IndicatorParameterTemplates)
                    {
                        IndicatorParameterRangeView indicatorParameterRangeView = new IndicatorParameterRangeView { MinValue = "", MaxValue = "", Step = "", IndicatorParameterTemplate = item, Indicator = item.Indicator, NameIndicator = item.Indicator.Name, NameIndicatorParameterTemplate = item.Name, DescriptionIndicatorParameterTemplate = item.Description };
                        if (nextIdIndex == -1)
                        {
                            IndicatorParameterRangesView.Add(indicatorParameterRangeView);
                        }
                        else
                        {
                            IndicatorParameterRangesView.Insert(nextIdIndex + i, indicatorParameterRangeView);
                            i++;
                        }
                    }
                    UpdateAlgorithmIndicators();
                }, (obj) => IsAddOrEditAlgorithm() && SelectedIndicator != null && AlgorithmIndicators.IndexOf(SelectedIndicator) == -1 );
            }
        }

        private void UpdateAlgorithmIndicators() //обновляет AlgorithmIndicators в соответствии с IndicatorParameterRangesView
        {
            AlgorithmIndicators.Clear();
            int lastId = -1; //id последнего добавленного индикатора
            foreach(IndicatorParameterRangeView item in IndicatorParameterRangesView)
            {
                if(item.Indicator.Id != lastId)
                {
                    AlgorithmIndicators.Add(item.Indicator);
                    lastId = item.Indicator.Id;
                }
            }
        }

        public ICommand DeleteAlgorithmIndicator_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    List<int> removeIndexs = new List<int>(); //список с индексами элементов которые нужно удалить в порядке убывания (т.к. при удалении первых индексы следующих будут смещаться)
                    for (int i = IndicatorParameterRangesView.Count - 1; i >= 0; i--)
                    {
                        if (IndicatorParameterRangesView[i].Indicator.Id == SelectedAlgorithmIndicator.Id)
                        {
                            removeIndexs.Add(i);
                        }
                    }
                    foreach(int item in removeIndexs)
                    {
                        IndicatorParameterRangesView.RemoveAt(item);
                    }
                    UpdateAlgorithmIndicators();

                }, (obj) => SelectedAlgorithmIndicator != null && IsAddOrEditAlgorithm());
            }
        }
        
        private string _editIndicatorParameterRangesViewMinValue;
        public string EditIndicatorParameterRangesViewMinValue //минимальное значение оптимизируемого параметра
        {
            get { return _editIndicatorParameterRangesViewMinValue; }
            set
            {
                _editIndicatorParameterRangesViewMinValue = value;
                OnPropertyChanged();
            }
        }
        
        private string _editIndicatorParameterRangesViewMaxValue;
        public string EditIndicatorParameterRangesViewMaxValue //максимальное значение оптимизируемого параметра
        {
            get { return _editIndicatorParameterRangesViewMaxValue; }
            set
            {
                _editIndicatorParameterRangesViewMaxValue = value;
                OnPropertyChanged();
            }
        }
        
        private string _editIndicatorParameterRangesViewStep;
        public string EditIndicatorParameterRangesViewStep //шаг оптимизируемого параметра
        {
            get { return _editIndicatorParameterRangesViewStep; }
            set
            {
                _editIndicatorParameterRangesViewStep = value;
                OnPropertyChanged();
            }
        }

        private List<string> _editIndicatorParameterRangesViewTypesStep = new List<string> { "процентный", "числовой" };
        public List<string> EditIndicatorParameterRangesViewTypesStep //список с возможными типами шага оптимизируемого параметра
        {
            get { return _editIndicatorParameterRangesViewTypesStep; }
            set
            {
                _editIndicatorParameterRangesViewTypesStep = value;
                OnPropertyChanged();
            }
        }

        private string _editIndicatorParameterRangesViewSelectedTypeStep;
        public string EditIndicatorParameterRangesViewSelectedTypeStep //выбранный тип шага оптимизируемого параметра
        {
            get { return _editIndicatorParameterRangesViewSelectedTypeStep; }
            set
            {
                _editIndicatorParameterRangesViewSelectedTypeStep = value;
                OnPropertyChanged();
            }
        }

        public ICommand EditIndicatorParameterRangesView_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    EditIndicatorParameterRangesViewMinValue = SelectedIndicatorParameterRangeView.MinValue != null ? SelectedIndicatorParameterRangeView.MinValue : null;
                    EditIndicatorParameterRangesViewMaxValue = SelectedIndicatorParameterRangeView.MaxValue != null ? SelectedIndicatorParameterRangeView.MaxValue : null;
                    EditIndicatorParameterRangesViewStep = SelectedIndicatorParameterRangeView.Step != null ? SelectedIndicatorParameterRangeView.Step : null;

                    if (SelectedIndicatorParameterRangeView.IsStepPercent == true)
                    {
                        EditIndicatorParameterRangesViewSelectedTypeStep = EditIndicatorParameterRangesViewTypesStep[0];
                    }
                    else if(SelectedIndicatorParameterRangeView.IsStepPercent == false)
                    {
                        EditIndicatorParameterRangesViewSelectedTypeStep = EditIndicatorParameterRangesViewTypesStep[1];
                    }
                    else //если null, выбираем первый
                    {
                        EditIndicatorParameterRangesViewSelectedTypeStep = EditIndicatorParameterRangesViewTypesStep[0];
                    }

                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewEditIndicatorParameterRangesView viewEditIndicatorParameterRangesView = new ViewEditIndicatorParameterRangesView();
                    viewEditIndicatorParameterRangesView.Show();
                }, (obj) => SelectedIndicatorParameterRangeView != null && IsAddOrEditAlgorithm());
            }
        }

        private ObservableCollection<string> _tooltipEditIndicatorParameterRangesView = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipEditIndicatorParameterRangesView //подсказка, показываемая при наведении на кнопку добавить
        {
            get { return _tooltipEditIndicatorParameterRangesView; }
            set
            {
                _tooltipEditIndicatorParameterRangesView = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsEditIndicatorParameterRangesViewCorrect()
        {
            bool result = true;
            TooltipEditIndicatorParameterRangesView.Clear(); //очищаем подсказку кнопки добавить

            //проверка на пустое значение
            if (EditIndicatorParameterRangesViewMinValue == "" || EditIndicatorParameterRangesViewMaxValue == "" || EditIndicatorParameterRangesViewStep == "")
            {
                result = false;
                TooltipEditIndicatorParameterRangesView.Add("Не заполнены все поля.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(EditIndicatorParameterRangesViewMinValue, out double res) == false)
            {
                result = false;
                TooltipEditIndicatorParameterRangesView.Add("Минимальное значение должно быть числом.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(EditIndicatorParameterRangesViewMaxValue, out res) == false)
            {
                result = false;
                TooltipEditIndicatorParameterRangesView.Add("Максимальное значение должно быть числом.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(EditIndicatorParameterRangesViewStep, out res) == false)
            {
                result = false;
                TooltipEditIndicatorParameterRangesView.Add("Шаг должен быть числом.");
            }

            //проверка на возможность достигнуть максимума с минимума с шагом
            if (double.TryParse(EditIndicatorParameterRangesViewMinValue, out double min) && double.TryParse(EditIndicatorParameterRangesViewMaxValue, out double max) && double.TryParse(EditIndicatorParameterRangesViewStep, out double step))
            {
                if( (max > min && step > 0) == false)
                {
                    result = false;
                    TooltipEditIndicatorParameterRangesView.Add("Максимум должен быть больше минимума, а шаг должен быть положительным.");
                }
            }

                return result;
        }

        public ICommand EditSaveIndicatorParameterRangesView_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    bool isStepPercent = false;
                    if(EditIndicatorParameterRangesViewSelectedTypeStep == EditIndicatorParameterRangesViewTypesStep[0])
                    {
                        isStepPercent = true;
                    }
                    string rangeValuesView = EditIndicatorParameterRangesViewMinValue + " – " + EditIndicatorParameterRangesViewMaxValue;

                    string steView = EditIndicatorParameterRangesViewStep;
                    if (isStepPercent)
                    {
                        steView += "%";
                    }
                    IndicatorParameterRangeView indicatorParameterRangeView = new IndicatorParameterRangeView { Id = SelectedIndicatorParameterRangeView.Id, MinValue = EditIndicatorParameterRangesViewMinValue, MaxValue = EditIndicatorParameterRangesViewMaxValue, Step = EditIndicatorParameterRangesViewStep, IsStepPercent = isStepPercent, IdAlgorithm = SelectedIndicatorParameterRangeView.IdAlgorithm, IndicatorParameterTemplate = SelectedIndicatorParameterRangeView.IndicatorParameterTemplate, Indicator = SelectedIndicatorParameterRangeView.Indicator, NameIndicator = SelectedIndicatorParameterRangeView.NameIndicator, NameIndicatorParameterTemplate = SelectedIndicatorParameterRangeView.NameIndicatorParameterTemplate, DescriptionIndicatorParameterTemplate = SelectedIndicatorParameterRangeView.DescriptionIndicatorParameterTemplate, RangeValuesView = rangeValuesView, StepView = steView };

                    int index = IndicatorParameterRangesView.IndexOf(SelectedIndicatorParameterRangeView);
                    IndicatorParameterRangesView.RemoveAt(index);
                    IndicatorParameterRangesView.Insert(index, indicatorParameterRangeView);
                    SelectedIndicatorParameterRangeView = indicatorParameterRangeView;

                    CloseAddDataSourceTemplateAction?.Invoke();
                }, (obj) => IsFieldsEditIndicatorParameterRangesViewCorrect());
            }
        }

        public ICommand CloseEditIndicatorParameterRangesView_Click
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

        private void DataSourceGroupsViewChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetPeriodTesting();
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
                    List<DataSourceAccordanceView> dataSourcesAccordances = new List<DataSourceAccordanceView>();
                    foreach(DataSourcesForAddingDsGroupView dataSourcesForAddingDsGroupView in DataSourcesForAddingDsGroupsView)
                    {
                        dataSourcesAccordances.Add(new DataSourceAccordanceView { DataSourceTemplate = dataSourcesForAddingDsGroupView.DataSourceTemplate, DataSource = dataSourcesForAddingDsGroupView.SelectedDataSource });
                    }

                    DataSourceGroupsView.Add( new DataSourceGroupView { Number = DataSourceGroupsView.Count + 1, DataSourcesAccordances = dataSourcesAccordances });
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

        private void CreateEvaluationCriteriasView()
        {
            EvaluationCriteriasView.Clear();
            foreach (EvaluationCriteria evaluationCriteria in EvaluationCriterias)
            {
                EvaluationCriteriasView.Add(new EvaluationCriteriaView { EvaluationCriteria = evaluationCriteria, Name = evaluationCriteria.ShortName + " (" + evaluationCriteria.Name + ")" });
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
                IsAxesParametersSpecified = false;
            }
        }

        private string _sizeNeighboursGroupPercent = "9";
        public string SizeNeighboursGroupPercent //размер группы соседних тестов от общей площади поисковой плоскости
        {
            get { return _sizeNeighboursGroupPercent; }
            set
            {
                _sizeNeighboursGroupPercent = value;
                OnPropertyChanged();
            }
        }

        private bool _isAxesParametersSpecified;
        public bool IsAxesParametersSpecified //показатель выбранности чек-бокса, указаны ли оси
        {
            get { return _isAxesParametersSpecified; }
            set
            {
                _isAxesParametersSpecified = value;
                OnPropertyChanged();
                CreateAxesParametersSelectView();
            }
        }

        private ObservableCollection<AxesParameterSelectView> _axesParametersSelectView = new ObservableCollection<AxesParameterSelectView>();
        public ObservableCollection<AxesParameterSelectView> AxesParametersSelectView //выбор осей двумерной плоскости на которой будет искаться топ-модель с соседями
        {
            get { return _axesParametersSelectView; }
            set
            {
                _axesParametersSelectView = value;
                OnPropertyChanged();
            }
        }

        private void CreateAxesParametersSelectView()
        {
            AxesParametersSelectView.Clear();
            if (IsAxesParametersSpecified)
            {
                List<string> namesParameters = new List<string>();
                foreach(IndicatorParameterRangeView indicatorParameterRangeView in IndicatorParameterRangesView)
                {
                    namesParameters.Add("Индикатор " + indicatorParameterRangeView.NameIndicator +  " – " + indicatorParameterRangeView.NameIndicatorParameterTemplate);
                }
                foreach(AlgorithmParameterView algorithmParameterView in AlgorithmParametersView)
                {
                    namesParameters.Add("Алгоритм – " + algorithmParameterView.Name);
                }

                AxesParametersSelectView.Add(new AxesParameterSelectView { Axis = "X", NamesParameters = namesParameters });
                AxesParametersSelectView.Add(new AxesParameterSelectView { Axis = "Y", NamesParameters = namesParameters });
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
                CreateDepositInAnotherCurrencies();
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
                CreateDepositInAnotherCurrencies();
            }
        }

        private ObservableCollection<string> _depositInAnotherCurrencies = new ObservableCollection<string>();
        public ObservableCollection<string> DepositInAnotherCurrencies //размер депозита в других валютах
        {
            get { return _depositInAnotherCurrencies; }
            set
            {
                _depositInAnotherCurrencies = value;
                OnPropertyChanged();
            }
        }

        private void CreateDepositInAnotherCurrencies()
        {
            DepositInAnotherCurrencies.Clear();
            if(double.TryParse(ForwardDeposit, out double res))
            {
                //определяем доллоровую стоимость депозита
                double dollarCostDeposit = res / SelectedCurrency.DollarCost;
                foreach (Currency currency in Currencies)
                {
                    if (currency != SelectedCurrency)
                    {
                        //переводим доллоровую стоимость в валютную, умножая на стоимость 1 доллара
                        double cost = Math.Round(dollarCostDeposit * currency.DollarCost, 2);
                        DepositInAnotherCurrencies.Add(cost.ToString() + currency.Name);
                    }
                }
            }
            
        }

        private bool _isPeriodTestingEnabled = false;
        public bool IsPeriodTestingEnabled
        {
            get { return _isPeriodTestingEnabled; }
            set
            {
                _isPeriodTestingEnabled = value;
                OnPropertyChanged();
            }
        }

        private DateTime _startPeriodTesting;
        public DateTime StartPeriodTesting //начало перида тестирования
        {
            get { return _startPeriodTesting; }
            set
            {
                _startPeriodTesting = value;
                OnPropertyChanged();
            }
        }

        private DateTime _endPeriodTesting;
        public DateTime EndPeriodTesting //окончание перида тестирования
        {
            get { return _endPeriodTesting; }
            set
            {
                _endPeriodTesting = value;
                OnPropertyChanged();
            }
        }

        private DateTime _displayDateStartStartPeriodTesting;
        public DateTime DisplayDateStartStartPeriodTesting //начало доступных дат для начала периода тестирования
        {
            get { return _displayDateStartStartPeriodTesting; }
            set
            {
                _displayDateStartStartPeriodTesting = value;
                OnPropertyChanged();
            }
        }

        private DateTime _displayDateStartEndPeriodTesting;
        public DateTime DisplayDateStartEndPeriodTesting //начало доступных дат для окончания периода тестирования
        {
            get { return _displayDateStartEndPeriodTesting; }
            set
            {
                _displayDateStartEndPeriodTesting = value;
                OnPropertyChanged();
            }
        }

        private DateTime _displayDateEndEndPeriodTesting;
        public DateTime DisplayDateEndEndPeriodTesting //окончание доступных дат периода окончания тестирования
        {
            get { return _displayDateEndEndPeriodTesting; }
            set
            {
                _displayDateEndEndPeriodTesting = value;
                OnPropertyChanged();
            }
        }

        private void SetPeriodTesting() //устанавливаем начальную и конечную даты исходя из доступных дат в выбранных источниках данных
        {
            if(DataSourceGroupsView.Count > 0)
            {
                IsPeriodTestingEnabled = true;
                //определяем самую раннюю дату из: самой поздней первой даты среди DataSourcesAccordances (чтобы начало периода была первой датой, на которой все источники данных группы имеют данные)
                
                //определяем дату для первого источника данных, чтобы потом с ней сравнивать другие
                List<DateTime> StartDates = new List<DateTime>(); //список с датами начала
                foreach(DataSourceAccordanceView dataSourceAccordanceView in DataSourceGroupsView[0].DataSourcesAccordances)
                {
                    StartDates.Add(dataSourceAccordanceView.DataSource.StartDate);
                }
                DateTime lastStartDate = StartDates[0]; //самая поздняя дата начала
                for(int y = 1; y < StartDates.Count; y++)
                {
                    if(DateTime.Compare(lastStartDate, StartDates[y]) < 0)
                    {
                        lastStartDate = StartDates[y];
                    }
                }
                DateTime startDate = lastStartDate; //самая ранняя дата начала, среди доступных для всех комбинаций источников данных, дат

                //проверяем остальные элементы DataSourceGroupsView
                for (int i = 1; i < DataSourceGroupsView.Count; i++)
                {
                    StartDates.Clear();
                    foreach (DataSourceAccordanceView dataSourceAccordanceView in DataSourceGroupsView[i].DataSourcesAccordances)
                    {
                        StartDates.Add(dataSourceAccordanceView.DataSource.StartDate);
                    }
                    lastStartDate = StartDates[0]; //самая поздняя дата начала
                    for (int y = 1; y < StartDates.Count; y++)
                    {
                        if (DateTime.Compare(lastStartDate, StartDates[y]) < 0)
                        {
                            lastStartDate = StartDates[y];
                        }
                    }
                    //проверяем, дата текущего DataSourceGroupsView раньше startDate
                    if(DateTime.Compare(lastStartDate, startDate) < 0)
                    {
                        startDate = lastStartDate;
                    }
                }

                //определяем последнюю дату среди группы источников данных, на которых все источники данных имеют данные
                //определяем дату для первого источника данных, чтобы потом с ней сравнивать другие
                List<DateTime> EndDates = new List<DateTime>(); //список с датами окончания
                foreach (DataSourceAccordanceView dataSourceAccordanceView in DataSourceGroupsView[0].DataSourcesAccordances)
                {
                    EndDates.Add(dataSourceAccordanceView.DataSource.EndDate);
                }
                DateTime firstEndDate = EndDates[0]; //самая ранняя дата окончания
                for (int y = 1; y < EndDates.Count; y++)
                {
                    if (DateTime.Compare(firstEndDate, EndDates[y]) > 0)
                    {
                        firstEndDate = EndDates[y];
                    }
                }
                DateTime endDate = firstEndDate; //самая поздняя дата окончания, среди доступных для всех комбинаций источников данных, дат

                //проверяем остальные элементы DataSourceGroupsView
                for (int i = 1; i < DataSourceGroupsView.Count; i++)
                {
                    EndDates.Clear();
                    foreach (DataSourceAccordanceView dataSourceAccordanceView in DataSourceGroupsView[i].DataSourcesAccordances)
                    {
                        EndDates.Add(dataSourceAccordanceView.DataSource.EndDate);
                    }
                    firstEndDate = EndDates[0]; //самая ранняя дата окончания
                    for (int y = 1; y < EndDates.Count; y++)
                    {
                        if (DateTime.Compare(firstEndDate, EndDates[y]) > 0)
                        {
                            firstEndDate = EndDates[y];
                        }
                    }
                    //проверяем, дата текущего DataSourceGroupsView позже endDate
                    if (DateTime.Compare(firstEndDate, endDate) > 0)
                    {
                        endDate = firstEndDate;
                    }
                }

                StartPeriodTesting = startDate.Date;
                EndPeriodTesting = endDate.AddDays(1).Date; //прибавил 1 день, т.к. в вычислениях endDate последний день рассматривался как торговый, а в тестировании он будет рассматриваться как день на котором заканчивается торговля
                DisplayDateStartStartPeriodTesting = new DateTime(startDate.Year, startDate.Month, 1);
                DisplayDateStartEndPeriodTesting = startDate.Date; //дату окончания можно установить только в доступные даты
                DisplayDateEndEndPeriodTesting = endDate.Date;
            }
            else
            {
                IsPeriodTestingEnabled = false;
                StartPeriodTesting = new DateTime(1, 1, 1);
                EndPeriodTesting = new DateTime(1, 1, 1);
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

            //проверяем что для осей плоскости выбраны разные параметры
            if (IsAxesParametersSpecified)
            {
                if(AxesParametersSelectView.Count > 0)
                {
                    if(AxesParametersSelectView[0].SelectedNameParameter == null || AxesParametersSelectView[1].SelectedNameParameter == null)
                    {
                        result = false;
                        TooltipLaunchTesting.Add("Не выбраны оси двумерной плоскости поиска топ-модели с соседями.");
                    }
                    else if(AxesParametersSelectView[0].SelectedNameParameter == AxesParametersSelectView[1].SelectedNameParameter)
                    {
                        result = false;
                        TooltipLaunchTesting.Add("Выбраны одинаковые параметры для осей двумерной плоскости поиска топ-модели с соседями.");
                    }
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

            //проверяем что дата начала периода тестирования раньше даты окончания периода тестирования
            if(DateTime.Compare(EndPeriodTesting.Date, StartPeriodTesting.Date) <= 0)
            {
                result = false;
                TooltipLaunchTesting.Add("Дата начала периода тестирования должна быть раньше даты окончания.");
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
                bool isCorrect = true;
                if (isYearsNotEmpty)
                {
                    if(int.TryParse(DurationOptimizationYears, out int res))
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
                }
                if (isMonthsNotEmpty)
                {
                    if(int.TryParse(DurationOptimizationMonths, out int res))
                    {
                        if (res <= 0)
                        {
                            isCorrect = false;
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
                        if (res <= 0)
                        {
                            isCorrect = false;
                        }
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }
                if(isCorrect == false)
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
                bool isCorrect = true;
                if (isYearsNotEmpty)
                {
                    if (int.TryParse(OptimizationSpacingYears, out int res))
                    {
                        if (res <= 0)
                        {
                            isCorrect = false;
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
                        if (res <= 0)
                        {
                            isCorrect = false;
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
                        if (res <= 0)
                        {
                            isCorrect = false;
                        }
                    }
                    else
                    {
                        isCorrect = false;
                    }
                }
                if (isCorrect == false)
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
                    bool isCorrect = true;
                    if (isYearsNotEmpty)
                    {
                        if (int.TryParse(DurationForwardYears, out int res))
                        {
                            if (res <= 0)
                            {
                                isCorrect = false;
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
                            if (res <= 0)
                            {
                                isCorrect = false;
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
                            if (res <= 0)
                            {
                                isCorrect = false;
                            }
                        }
                        else
                        {
                            isCorrect = false;
                        }
                    }
                    if (isCorrect == false)
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
                        dataSourceGroups.Add(new DataSourceGroup { DataSourceAccordances = dataSourceAccordances });
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

                    //AxesTopModelSearchPlane
                    List<AxesParameter> axesTopModelSearchPlane = new List<AxesParameter>();
                    foreach(AxesParameterSelectView axesParameterSelectView in AxesParametersSelectView)
                    {
                        string[] arr = axesParameterSelectView.SelectedNameParameter.Split(' ');
                        if(arr[0] == "Индикатор")
                        {
                            Indicator indicator = new Indicator();
                            foreach (Indicator indicatorItem in Indicators)
                            {
                                if(indicatorItem.Name == arr[1])
                                {
                                    indicator = indicatorItem;
                                }
                            }
                            IndicatorParameterTemplate indicatorParameterTemplate = new IndicatorParameterTemplate();
                            foreach (IndicatorParameterTemplate indicatorParameterTemplateItem in indicator.IndicatorParameterTemplates)
                            {
                                if(indicatorParameterTemplateItem.Name == arr[3])
                                {
                                    indicatorParameterTemplate = indicatorParameterTemplateItem;
                                }
                            }
                            axesTopModelSearchPlane.Add(new AxesParameter { IndicatorParameterTemplate = indicatorParameterTemplate });
                        }
                        else
                        {
                            AlgorithmParameter algorithmParameter = new AlgorithmParameter();
                            foreach (AlgorithmParameter algorithmParameterItem in SelectedAlgorithm.AlgorithmParameters)
                            {
                                if(algorithmParameterItem.Name == arr[2])
                                {
                                    algorithmParameter = algorithmParameterItem;
                                }
                            }
                            axesTopModelSearchPlane.Add(new AxesParameter { AlgorithmParameter = algorithmParameter });
                        }
                    }

                    //ForwardDepositCurrencies
                    List<DepositCurrency> forwardDepositCurrencies = new List<DepositCurrency>();
                    if(IsForwardTesting && IsForwardDepositTesting)
                    {
                        //определяем доллоровую стоимость депозита
                        double dollarCostDeposit = double.Parse(ForwardDeposit) / SelectedCurrency.DollarCost;
                        foreach (Currency currency in Currencies)
                        {
                            //переводим доллоровую стоимость в валютную, умножая на стоимость 1 доллара
                            double cost = Math.Round(dollarCostDeposit * currency.DollarCost, 2);
                            forwardDepositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = cost });
                        }
                    }

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
                    testing.IsAxesSpecified = IsAxesParametersSpecified;
                    testing.AxesTopModelSearchPlane = axesTopModelSearchPlane;
                    testing.IsForwardTesting = IsForwardTesting;
                    testing.IsForwardDepositTrading = IsForwardDepositTesting;
                    testing.ForwardDepositCurrencies = forwardDepositCurrencies;
                    testing.StartPeriod = StartPeriodTesting.Date;
                    testing.EndPeriod = EndPeriodTesting.Date;
                    testing.DurationOptimizationTests = durationOptimizationTests;
                    testing.OptimizationTestSpacing = optimizationTestSpacing;
                    testing.DurationForwardTest = durationForwardTest;

                    //передаем объект в модель
                    Task.Run(() => _modelTesting.TestingLaunch(testing)); //запускаем в отдельном потоке чтобы форма обновлялась

                }, (obj) => IsFieldsTestingCorrect());
            }
        }

        #endregion
    }
}