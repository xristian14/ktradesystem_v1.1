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
                CreateIndicatorsView(); //вызвает метод формирования списка источников данных для отображения
            }
        }

        private ObservableCollection<IndicatorView> _indicatorsView = new ObservableCollection<IndicatorView>(); //индикаторы
        public ObservableCollection<IndicatorView> IndicatorsView
        {
            get { return _indicatorsView; }
            private set
            {
                _indicatorsView = value;
                OnPropertyChanged();
            }
        }

        private void modelData_IndicatorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Indicators = (ObservableCollection<Indicator>)sender;
        }

        private void CreateIndicatorsView() //создает IndicatorsView на основе Indicators
        {
            IndicatorsView.Clear();
            foreach (Indicator indicator in Indicators)
            {
                IndicatorView indicatorView = new IndicatorView();


                IndicatorsView.Add(indicatorView);
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

        private Visibility _indicatorsVisibility = Visibility.Visible;
        public Visibility IndicatorsVisibility //видимость панели индикаторов
        {
            get { return _indicatorsVisibility; }
            private set
            {
                _indicatorsVisibility = value;
                OnPropertyChanged();
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
            }
            else if (IsIndicatorEdited == true)
            {
                IndicatorStatusText = "Редактирование";
            }
            else
            {
                IndicatorStatusText = "Просмотр";
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
                }, (obj) => !IsAddOrEditIndicator());
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
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "Parameter." + SelectedIndicatorParameterTemplate);
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
                    IndicatorScript = IndicatorScript.Insert(IndicatorScriptTextBox.CaretIndex, "int i = 1;" + Environment.NewLine + "while (  ) {" + Environment.NewLine + Environment.NewLine + "i++;" + Environment.NewLine + "}");
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
            foreach (IndicatorView item in IndicatorsView)
            {
                if (name == item.Name) //проверяем имя на уникальность среди всех записей
                {
                    isUnique = false;
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
                    TestSerialize testSerialize = new TestSerialize { Id = 1, Name = "name1", Description = "desc1", ParameterTemplates = new List<ParameterTemplate> { new ParameterTemplate { Name = "param1" }, new ParameterTemplate { Name = "param2" } } };
                    //testSerialize.Ca




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

        #endregion






        #region add edit delete select IndicatorParameterTemplate

        private ObservableCollection<string> _indicatorParameterTemplates = new ObservableCollection<string>();
        public ObservableCollection<string> IndicatorParameterTemplates //шаблоны параметров индикатора
        {
            get { return _indicatorParameterTemplates; }
            set
            {
                _indicatorParameterTemplates = value;
                OnPropertyChanged();
            }
        }

        private string _selectedIndicatorParameterTemplate;
        public string SelectedIndicatorParameterTemplate //выбранный шаблон параметра индикатора
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
            foreach (string item in IndicatorParameterTemplates)
            {
                if (name == item) //проверяем имя на уникальность среди всех записей
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
                    IndicatorParameterTemplates.Add(name);

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
                    AddIndicatorParameterTemplateName = SelectedIndicatorParameterTemplate;

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
            foreach (string item in IndicatorParameterTemplates)
            {
                if (name == item && name != SelectedIndicatorParameterTemplate) //проверяем имя на уникальность среди всех записей кроме редактируемой
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

        public ICommand EditSaveIndicatorParameterTemplate_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    string name = AddIndicatorParameterTemplateName.Replace(" ", "");
                    int index = IndicatorParameterTemplates.IndexOf(SelectedIndicatorParameterTemplate);
                    IndicatorParameterTemplates[index] = name;

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
                    string msg = "Название: " + SelectedIndicatorParameterTemplate;
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
    }
}