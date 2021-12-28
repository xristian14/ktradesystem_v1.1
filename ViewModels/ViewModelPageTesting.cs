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

            _modelTesting = ModelTesting.getInstance();
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
                IndicatorName = SelectedIndicator.Name;
                IndicatorDescription = SelectedIndicator.Description;
                IndicatorParameterTemplates.Clear();
                foreach(IndicatorParameterTemplate parameterTemplate in SelectedIndicator.IndicatorParameterTemplates)
                {
                    IndicatorParameterTemplates.Add(parameterTemplate);
                }
                IndicatorScript = SelectedIndicator.Script;
            }
            else
            {
                IndicatorName = "";
                IndicatorDescription = "";
                IndicatorParameterTemplates.Clear();
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
                }, (obj) => !IsAddOrEditIndicator());
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
                }, (obj) => !IsAddOrEditIndicator() && SelectedIndicator != null );
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
                }, (obj) => !IsAddOrEditIndicator() && SelectedIndicator != null );
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
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "Parameter." + SelectedIndicatorParameterTemplate.Name);
                }, (obj) => SelectedIndicatorParameterTemplate != null && IsAddOrEditIndicator());
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
                    foreach(IndicatorParameterTemplate item in IndicatorParameterTemplates)
                    {
                        InsertIndicatorParameterTemplates.Add(item);
                    }

                    if (IsIndicatorAdded)
                    {
                        _modelTesting.IndicatorInsertUpdate(IndicatorName, IndicatorDescription, InsertIndicatorParameterTemplates, IndicatorScript);
                    }
                    else if(IsIndicatorEdited)
                    {
                        _modelTesting.IndicatorInsertUpdate(IndicatorName, IndicatorDescription, InsertIndicatorParameterTemplates, IndicatorScript, SelectedIndicator.Id);
                    }

                    IsIndicatorAdded = false;
                    IsIndicatorEdited = false;
                    UpdateIndicatorStatusText();

                    /*
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
public string Main()
{
double a = 5;
a = a/2;
return a.ToString();
}
}"
                    });
                    dynamic Test = Result.CompiledAssembly.CreateInstance("Test");
                    MessageBox.Show(Test.Main());
                    */




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

        #endregion










        #region view add edit delete IndicatorParameterTemplate

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

        private IndicatorParameterTemplate _selectedIndicatorParameterTemplate;
        public IndicatorParameterTemplate SelectedIndicatorParameterTemplate //выбранный шаблон параметра индикатора
        {
            get { return _selectedIndicatorParameterTemplate; }
            set
            {
                _selectedIndicatorParameterTemplate = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddIndicatorParameterTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    viewmodelData.IsMainWindowEnabled = false;
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

        public void AddIndicatorParameterTemplate_Closing(object sender, CancelEventArgs e)
        {
            viewmodelData.IsMainWindowEnabled = true;
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

            string name = AddIndicatorParameterTemplateName.Replace(" ", "");

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
            foreach (IndicatorParameterTemplate item in IndicatorParameterTemplates)
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
                    IndicatorParameterTemplate parameterTemplate = new IndicatorParameterTemplate { Name = name, Description = AddIndicatorParameterTemplateDescription };
                    IndicatorParameterTemplates.Add(parameterTemplate);

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
                    AddIndicatorParameterTemplateName = SelectedIndicatorParameterTemplate.Name;
                    AddIndicatorParameterTemplateDescription = SelectedIndicatorParameterTemplate.Description;

                    viewmodelData.IsMainWindowEnabled = false;
                    ViewEditIndicatorParameterTemplate viewEditIndicatorParameterTemplate = new ViewEditIndicatorParameterTemplate();
                    viewEditIndicatorParameterTemplate.Show();
                }, (obj) => SelectedIndicatorParameterTemplate != null && IsAddOrEditIndicator());
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
            foreach (IndicatorParameterTemplate item in IndicatorParameterTemplates)
            {
                if(SelectedIndicatorParameterTemplate != null) //без этой проверки ошибка на обращение null полю, после сохранения
                {
                    if (name == item.Name && name != SelectedIndicatorParameterTemplate.Name) //проверяем имя на уникальность среди всех записей кроме редактируемой
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
                    int index = IndicatorParameterTemplates.IndexOf(SelectedIndicatorParameterTemplate);
                    IndicatorParameterTemplate parameterTemplate = new IndicatorParameterTemplate { Name = name, Description = AddIndicatorParameterTemplateDescription };
                    IndicatorParameterTemplates.RemoveAt(index);
                    IndicatorParameterTemplates.Insert(index, parameterTemplate);

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
                    int index = IndicatorParameterTemplates.IndexOf(SelectedIndicatorParameterTemplate); //находим индекс выбранного элемента
                    string msg = "Название: " + SelectedIndicatorParameterTemplate.Name;
                    string caption = "Удалить?";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == MessageBoxResult.Yes)
                    {
                        IndicatorParameterTemplates.RemoveAt(index);
                    }
                }, (obj) => SelectedIndicatorParameterTemplate != null && IsAddOrEditIndicator() );
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
            }
        }

        private void SelectedAlgorithmChanged() //помещяет в переменные редактора значения выбранного алгоритма для просмотра данного алгоритма
        {
            if (SelectedAlgorithm != null)
            {
                AlgorithmName = SelectedAlgorithm.Name;
                AlgorithmDescription = SelectedAlgorithm.Description;
                AlgorithmParameters.Clear();
                foreach (AlgorithmParamter algorithmParamter in SelectedAlgorithm.AlgorithmParamters)
                {
                    AlgorithmParameters.Add(algorithmParamter);
                }
                AlgorithmScript = SelectedAlgorithm.Script;
            }
            else
            {
                AlgorithmName = "";
                AlgorithmDescription = "";
                AlgorithmParameters.Clear();
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
                }, (obj) => !IsAddOrEditAlgorithm() );
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
                }, (obj) => !IsAddOrEditAlgorithm() && SelectedAlgorithm != null);
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
                        //_modelTesting.AlgorithmDelete(SelectedAlgorithm.Id);
                    }
                }, (obj) => !IsAddOrEditAlgorithm() && SelectedAlgorithm != null);
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










        #region add edit delete DataSourceTemplate

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

        private DataSourceTemplate _selectedDataSourceTemplate;
        public DataSourceTemplate SelectedDataSourceTemplate //выбранный шаблон источника данных
        {
            get { return _selectedDataSourceTemplate; }
            set
            {
                _selectedDataSourceTemplate = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddDataSourceTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    viewmodelData.IsMainWindowEnabled = false;
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
            viewmodelData.IsMainWindowEnabled = true;
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
                }, (obj) => IsAddOrEditIndicator());
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
                    string name = AddDataSourceTemplateName.Replace(" ", "");
                    DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Name = name, Description = AddDataSourceTemplateDescription };
                    DataSourceTemplates.Add(dataSourceTemplate);

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
                    AddDataSourceTemplateName = SelectedDataSourceTemplate.Name;
                    AddDataSourceTemplateDescription = SelectedDataSourceTemplate.Description;

                    viewmodelData.IsMainWindowEnabled = false;
                    ViewEditDataSourceTemplate viewEditDataSourceTemplate = new ViewEditDataSourceTemplate();
                    viewEditDataSourceTemplate.Show();
                }, (obj) => SelectedDataSourceTemplate != null && IsAddOrEditAlgorithm());
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
                if (SelectedDataSourceTemplate != null) //без этой проверки ошибка на обращение null полю, после сохранения
                {
                    if (name == item.Name && name != SelectedDataSourceTemplate.Name) //проверяем имя на уникальность среди всех записей кроме редактируемой
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
                    int index = DataSourceTemplates.IndexOf(SelectedDataSourceTemplate);
                    DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Name = name, Description = AddDataSourceTemplateDescription };
                    DataSourceTemplates.RemoveAt(index);
                    DataSourceTemplates.Insert(index, dataSourceTemplate);

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
                    int index = DataSourceTemplates.IndexOf(SelectedDataSourceTemplate); //находим индекс выбранного элемента
                    string msg = "Название: " + SelectedDataSourceTemplate.Name;
                    string caption = "Удалить?";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == MessageBoxResult.Yes)
                    {
                        DataSourceTemplates.RemoveAt(index);
                    }
                }, (obj) => SelectedDataSourceTemplate != null && IsAddOrEditAlgorithm());
            }
        }

        #endregion










        #region add edit IndicatorParameterRange    add delete AlgorithmIndicators

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
                    int nextIdIndex = -1; //индекс элемента, id индикатора которого превышает id добавляемого индикатора
                    foreach(IndicatorParameterRangeView item in IndicatorParameterRangesView)
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
                    foreach(IndicatorParameterTemplate item in SelectedIndicator.IndicatorParameterTemplates)
                    {
                        IndicatorParameterRangeView indicatorParameterRangeView = new IndicatorParameterRangeView { IdIndicatorParameterTemplate = item.Id, Indicator = item.Indicator, NameIndicator = item.Indicator.Name, NameIndicatorParameterTemplate = item.Name, DescriptionIndicatorParameterTemplate = item.Description };
                        if (nextIdIndex == -1)
                        {
                            IndicatorParameterRangesView.Add(indicatorParameterRangeView);
                        }
                        else
                        {
                            IndicatorParameterRangesView.Insert(nextIdIndex, indicatorParameterRangeView);
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
                    string msg = "Название: " + SelectedAlgorithmIndicator.Name;
                    string caption = "Удалить?";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == MessageBoxResult.Yes)
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
                    }
                }, (obj) => SelectedAlgorithmIndicator != null && IsAddOrEditAlgorithm());
            }
        }
        
        private double _editIndicatorParameterRangesViewMinValue;
        public double EditIndicatorParameterRangesViewMinValue //минимальное значение оптимизируемого параметра
        {
            get { return _editIndicatorParameterRangesViewMinValue; }
            set
            {
                _editIndicatorParameterRangesViewMinValue = value;
                OnPropertyChanged();
            }
        }
        
        private double _editIndicatorParameterRangesViewMaxValue;
        public double EditIndicatorParameterRangesViewMaxValue //максимальное значение оптимизируемого параметра
        {
            get { return _editIndicatorParameterRangesViewMaxValue; }
            set
            {
                _editIndicatorParameterRangesViewMaxValue = value;
                OnPropertyChanged();
            }
        }
        
        private double _editIndicatorParameterRangesViewStep;
        public double EditIndicatorParameterRangesViewMaxStep //шаг оптимизируемого параметра
        {
            get { return _editIndicatorParameterRangesViewStep; }
            set
            {
                _editIndicatorParameterRangesViewStep = value;
                OnPropertyChanged();
            }
        }

        public ICommand EditIndicatorParameterRangesView_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    AddIndicatorParameterTemplateName = SelectedIndicatorParameterTemplate.Name;
                    AddIndicatorParameterTemplateDescription = SelectedIndicatorParameterTemplate.Description;

                    viewmodelData.IsMainWindowEnabled = false;
                    ViewEditIndicatorParameterTemplate viewEditIndicatorParameterTemplate = new ViewEditIndicatorParameterTemplate();
                    viewEditIndicatorParameterTemplate.Show();
                }, (obj) => SelectedIndicatorParameterRangeView != null && IsAddOrEditAlgorithm());
            }
        }
        /*
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
            viewmodelData.IsMainWindowEnabled = true;
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
                }, (obj) => IsAddOrEditIndicator());
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
                    string name = AddDataSourceTemplateName.Replace(" ", "");
                    DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Name = name, Description = AddDataSourceTemplateDescription };
                    DataSourceTemplates.Add(dataSourceTemplate);

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
                    AddDataSourceTemplateName = SelectedDataSourceTemplate.Name;
                    AddDataSourceTemplateDescription = SelectedDataSourceTemplate.Description;

                    viewmodelData.IsMainWindowEnabled = false;
                    ViewEditDataSourceTemplate viewEditDataSourceTemplate = new ViewEditDataSourceTemplate();
                    viewEditDataSourceTemplate.Show();
                }, (obj) => SelectedDataSourceTemplate != null && IsAddOrEditAlgorithm());
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
                if (SelectedDataSourceTemplate != null) //без этой проверки ошибка на обращение null полю, после сохранения
                {
                    if (name == item.Name && name != SelectedDataSourceTemplate.Name) //проверяем имя на уникальность среди всех записей кроме редактируемой
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
                    int index = DataSourceTemplates.IndexOf(SelectedDataSourceTemplate);
                    DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Name = name, Description = AddDataSourceTemplateDescription };
                    DataSourceTemplates.RemoveAt(index);
                    DataSourceTemplates.Insert(index, dataSourceTemplate);

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
                    int index = DataSourceTemplates.IndexOf(SelectedDataSourceTemplate); //находим индекс выбранного элемента
                    string msg = "Название: " + SelectedDataSourceTemplate.Name;
                    string caption = "Удалить?";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == MessageBoxResult.Yes)
                    {
                        DataSourceTemplates.RemoveAt(index);
                    }
                }, (obj) => SelectedDataSourceTemplate != null && IsAddOrEditAlgorithm());
            }
        }*/

        #endregion










        #region view add edit delete AlgorithmParameters

        private ObservableCollection<AlgorithmParamter> _algorithmParameters = new ObservableCollection<AlgorithmParamter>();
        public ObservableCollection<AlgorithmParamter> AlgorithmParameters //параметры алгоритма
        {
            get { return _algorithmParameters; }
            private set
            {
                _algorithmParameters = value;
                OnPropertyChanged();
            }
        }

        private AlgorithmParamter _selectedAlgorithmParamter;
        public AlgorithmParamter SelectedAlgorithmParamter //выбранный параметр алгоритма
        {
            get { return _selectedAlgorithmParamter; }
            set
            {
                _selectedAlgorithmParamter = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}