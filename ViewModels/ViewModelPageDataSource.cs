using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using ktradesystem.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using ktradesystem.Views;
using System.Windows.Forms;
using System.IO;
using Ookii.Dialogs.Wpf;

namespace ktradesystem.ViewModels
{
    class ViewModelPageDataSource : ViewModelBase
    {
        private static ViewModelPageDataSource _instance;

        private ViewModelPageDataSource()
        {
            _modelData = ModelData.getInstance();
            _modelData.PropertyChanged += Model_PropertyChanged;

            _modelData.Instruments.CollectionChanged += modelData_InstrumenstCollectionChanged;
            Instruments = _modelData.Instruments;

            _modelData.Currencies.CollectionChanged += modelData_CurrenciesCollectionChanged;
            Currencies = _modelData.Currencies;

            _modelData.Intervals.CollectionChanged += modelData_IntervalsCollectionChanged;
            Intervals = _modelData.Intervals;

            _modelData.Comissiontypes.CollectionChanged += modelData_ComissiontypesCollectionChanged;
            Comissiontypes = _modelData.Comissiontypes;

            _modelData.DataSources.CollectionChanged += modelData_DataSourcesCollectionChanged; //вешаем на обновление DataSources в модели метод: присвоить текущему DataSources DataSources из модели
            DataSources = _modelData.DataSources;

            _modelDataSource = ModelDataSource.getInstance();
            _modelDataSource.PropertyChanged += Model_PropertyChanged;
        }

        public static ViewModelPageDataSource getInstance()
        {
            if (_instance == null)
            {
                _instance = new ViewModelPageDataSource();
            }
            return _instance;
        }

        private ModelData _modelData;
        private ModelDataSource _modelDataSource;

        private ViewmodelData _viewmodelData;
        public ViewmodelData viewmodelData
        {
            get
            {
                if(_viewmodelData == null)
                {
                    _viewmodelData = ViewmodelData.getInstance();
                }
                return _viewmodelData;
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var fieldViewModel = this.GetType().GetProperty(e.PropertyName);
            var fieldModel = sender.GetType().GetProperty(e.PropertyName);
            fieldViewModel?.SetValue(this, fieldModel.GetValue(sender));
        }

        private ObservableCollection<Instrument> _instruments;
        public ObservableCollection<Instrument> Instruments
        {
            get { return _instruments; }
            set
            {
                _instruments = value;
                OnPropertyChanged();
                CreateInstrumentsView(); //вызвает метод формирования списка инструментов для отображения
            }
        }

        private void modelData_InstrumenstCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Instruments = (ObservableCollection<Instrument>)sender;
        }

        private ObservableCollection<Currency> _currencies = new ObservableCollection<Currency>();

        public ObservableCollection<Currency> Currencies
        {
            get { return _currencies; }
            set
            {
                _currencies = value;
                OnPropertyChanged();
                CreateCurrenciesView(); //вызвает метод формирования списка валют для отображения
            }
        }

        private void modelData_CurrenciesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Currencies = (ObservableCollection<Currency>)sender;
        }

        private ObservableCollection<Interval> _intervals = new ObservableCollection<Interval>();

        public ObservableCollection<Interval> Intervals
        {
            get { return _intervals; }
            set
            {
                _intervals = value;
                OnPropertyChanged();
                CreateIntervalsView(); //вызвает метод формирования списка интервалов для отображения
            }
        }
        private void modelData_IntervalsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Intervals = (ObservableCollection<Interval>)sender;
        }

        private ObservableCollection<Comissiontype> _comissiontypes = new ObservableCollection<Comissiontype>();

        public ObservableCollection<Comissiontype> Comissiontypes
        {
            get { return _comissiontypes; }
            set
            {
                _comissiontypes = value;
                OnPropertyChanged();
                CreateComissiontypesView(); //вызвает метод формирования списка типов комиссии для отображения
            }
        }
        private void modelData_ComissiontypesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Comissiontypes = (ObservableCollection<Comissiontype>)sender;
        }

        private ObservableCollection<DataSource> _dataSources = new ObservableCollection<DataSource>(); //источники данных
        public ObservableCollection<DataSource> DataSources
        {
            get { return _dataSources; }
            private set
            {
                _dataSources = value;
                OnPropertyChanged();
                CreateDataSourceView(); //вызвает метод формирования списка источников данных для отображения
            }
        }

        private void modelData_DataSourcesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DataSources = (ObservableCollection<DataSource>)sender;
        }



        private ObservableCollection<DataSourceView> _dataSourcesView = new ObservableCollection<DataSourceView>(); //источники данных в удобном для представления виде
        public ObservableCollection<DataSourceView> DataSourcesView
        {
            get { return _dataSourcesView; }
            private set
            {
                _dataSourcesView = value;
                OnPropertyChanged();
            }
        }

        private void CreateDataSourceView() //создает DataSourcesView на основе DataSources
        {
            DataSourcesView.Clear();
            foreach(DataSource dsItem in DataSources)
            {
                //определяет название валюты
                string currencyName = "";
                foreach(Currency currency in Currencies)
                {
                    if(currency.Id == dsItem.Currency.Id)
                    {
                        currencyName = currency.Name;
                    }
                }
                //определяет название интервала
                string intervalName = "";
                foreach (Interval interval in Intervals)
                {
                    if (interval.Id == dsItem.Interval.Id)
                    {
                        intervalName = interval.Name;
                    }
                }
                //определяет название инструмента
                string instrumentName = "";
                foreach (Instrument instrument in Instruments)
                {
                    if (instrument.Id == dsItem.Instrument.Id)
                    {
                        instrumentName = instrument.Name;
                    }
                }
                //формирует комиссию с типом комиссии
                string comission = "";
                if(dsItem.Comissiontype.Id == 1)
                {
                    comission = dsItem.Comission.ToString() + currencyName;
                }
                else
                {
                    comission = dsItem.Comission.ToString() + "%";
                }
                //формирует список файлов
                List<string> files = new List<string>();
                foreach (DataSourceFile dataSourceFile in dsItem.DataSourceFiles)
                {
                    files.Add(dataSourceFile.Path);
                }

                DataSourceView dsView = new DataSourceView { Id = dsItem.Id, Name = dsItem.Name, Currency = currencyName, Interval = intervalName, Instrument = instrumentName, Cost = dsItem.Cost, Comissiontype = dsItem.Comissiontype, Comission = dsItem.Comission, ComissionView = comission, PriceStep = dsItem.PriceStep, CostPriceStep = dsItem.CostPriceStep, Files = files };
                DataSourcesView.Add(dsView);
            }
        }

        private ObservableCollection<string> _instrumentsView = new ObservableCollection<string>(); //инструменты в удобном для представления виде
        public ObservableCollection<string> InstrumentsView
        {
            get { return _instrumentsView; }
            private set
            {
                _instrumentsView = value;
                OnPropertyChanged();
            }
        }

        private void CreateInstrumentsView() //создает InstrumentsView на основе Instruments
        {
            InstrumentsView.Clear();
            foreach(Instrument instrument in Instruments)
            {
                InstrumentsView.Add(instrument.Name);
            }
        }

        private ObservableCollection<string> _intervalsView = new ObservableCollection<string>(); //интервалы в удобном для представления виде
        public ObservableCollection<string> IntervalsView
        {
            get { return _intervalsView; }
            private set
            {
                _intervalsView = value;
                OnPropertyChanged();
            }
        }

        private void CreateIntervalsView() //создает IntervalsView на основе Intervals
        {
            IntervalsView.Clear();
            foreach (Interval interval in Intervals)
            {
                IntervalsView.Add(interval.Name);
            }
        }

        private ObservableCollection<string> _currenciesView = new ObservableCollection<string>(); //валюты в удобном для представления виде
        public ObservableCollection<string> CurrenciesView
        {
            get { return _currenciesView; }
            private set
            {
                _currenciesView = value;
                OnPropertyChanged();
            }
        }

        private void CreateCurrenciesView() //создает CurrenciesView на основе Currencies
        {
            CurrenciesView.Clear();
            foreach (Currency currency in Currencies)
            {
                CurrenciesView.Add(currency.Name);
            }
        }

        private ObservableCollection<string> _comissiontypesView = new ObservableCollection<string>(); //типы комиссии в удобном для представления виде
        public ObservableCollection<string> ComissiontypesView
        {
            get { return _comissiontypesView; }
            private set
            {
                _comissiontypesView = value;
                OnPropertyChanged();
            }
        }

        private void CreateComissiontypesView() //создает CurrenciesView на основе Currencies
        {
            ComissiontypesView.Clear();
            foreach (Comissiontype comissiontype in Comissiontypes)
            {
                ComissiontypesView.Add(comissiontype.Name);
            }
        }

        public DataSourceView SelectedDataSource { get; set; }

        public ICommand AddDataSource_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Comissiontype1 = true;
                    Comissiontype2 = false;

                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewAddDataSource viewAddDataSource = new ViewAddDataSource();
                    viewAddDataSource.Show();
                }, (obj) => true);
            }
        }

        public ICommand EditDataSource_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    EditDsId = SelectedDataSource.Id;
                    AddDsName = SelectedDataSource.Name;
                    AddDsInstrument = SelectedDataSource.Instrument;
                    AddDsCurrency = SelectedDataSource.Currency;
                    AddDsComissiontype = SelectedDataSource.Comissiontype.Name;
                    AddDsCost = SelectedDataSource.Cost.ToString();
                    //выставляем radiobuttons
                    if(SelectedDataSource.Comissiontype.Id == 1)
                    {
                        Comissiontype1 = true;
                        Comissiontype2 = false;
                    }
                    else
                    {
                        Comissiontype1 = false;
                        Comissiontype2 = true;
                    }

                    AddDsComission = SelectedDataSource.Comission.ToString();
                    AddDsPriceStep = SelectedDataSource.PriceStep.ToString();
                    AddDsCostPriceStep = SelectedDataSource.CostPriceStep.ToString();
                    AddDataSourceFolder = SelectedDataSource.Files[0].Substring(0, SelectedDataSource.Files[0].LastIndexOf('\\'));
                    if (Directory.Exists(AddDataSourceFolder)) //если директория существует, добавляем файлы из нее
                    {
                        string[] files = Directory.GetFiles(AddDataSourceFolder); //получаем массив с названиями файлов
                        foreach (string item in files) //проходимся по списку файлу и те котоыре имеют расширение .txt добавляем в список невыбранных файлов
                        {
                            if (item.Substring(item.Length - 4) == ".txt")
                            {
                                FilesUnselected.Add(item.Substring(item.LastIndexOf("\\") + 1)); //обрезаем полный путь к файлу, оставляя только имя и расширение
                            }
                        }
                        //проходимся по файлам, и перемещаем найденые в список выбранных файлов
                        foreach (string item in SelectedDataSource.Files)
                        {
                            int index = FilesUnselected.IndexOf(item.Substring(item.LastIndexOf("\\") + 1));
                            if (index != -1)
                            {
                                FilesSelected.Add(FilesUnselected[index]);
                                FilesUnselected.RemoveAt(index);
                            }
                        }
                    }
                    else
                    {
                        AddDataSourceFolder = "";
                    }

                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewEditDataSource viewEditDataSource = new ViewEditDataSource();
                    viewEditDataSource.Show();
                    
                }, (obj) => SelectedDataSource != null );
            }
        }

        public ICommand DeleteDataSource_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    int index = DataSourcesView.IndexOf(SelectedDataSource); //находим индекс выбранного элемента
                    string msg = "Id: " + SelectedDataSource.Id + "   Название: " + SelectedDataSource.Name;
                    string caption = "Удалить источник данных?";
                    MessageBoxButtons messageBoxButton = MessageBoxButtons.YesNo;
                    var result = MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == DialogResult.Yes)
                    {
                        _modelDataSource.DeleteDataSource(SelectedDataSource.Id);
                    }
                }, (obj) => SelectedDataSource != null );
            }
        }

        #region add datasource

        private string _addDataSourceFolder;
        public string AddDataSourceFolder
        {
            get { return _addDataSourceFolder; }
            private set
            {
                _addDataSourceFolder = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _filesUnselected = new ObservableCollection<string>(); //список невыбранных файлов
        public ObservableCollection<string> FilesUnselected
        {
            get { return _filesUnselected; }
            private set
            {
                _filesUnselected = value;
                OnPropertyChanged();
            }
        }
        private string _filesUnselectedSelectedItem;
        public string FilesUnselectedSelectedItem
        {
            get { return _filesUnselectedSelectedItem; }
            set
            {
                _filesUnselectedSelectedItem = value;
                OnPropertyChanged();
            }
        }
        private string _filesSelectedSelectedItem;
        private ObservableCollection<string> _filesSelected = new ObservableCollection<string>(); //список выбранных файлов
        public ObservableCollection<string> FilesSelected
        {
            get { return _filesSelected; }
            private set
            {
                _filesSelected = value;
                OnPropertyChanged();
            }
        }
        public string FilesSelectedSelectedItem
        {
            get { return _filesSelectedSelectedItem; }
            set
            {
                _filesSelectedSelectedItem = value;
                OnPropertyChanged();
            }
        }

        public int EditDsId { get; set; }
        public string AddDsName{ get; set; }
        public string AddDsInstrument { get; set; }
        public string AddDsCurrency { get; set; }
        public string AddDsComissiontype { get; set; }
        private string _addDsCost;
        public string AddDsCost
        {
            get { return _addDsCost; }
            set
            {
                _addDsCost = value.Replace('.', ',');
            }
        }

        private bool _comissiontype1;
        public bool Comissiontype1 //выбран тип комиссии 1
        {
            get { return _comissiontype1; }
            set
            {
                _comissiontype1 = value;
                OnPropertyChanged();
            }
        }

        private bool _comissiontype2;
        public bool Comissiontype2 //выбран тип комиссии 2
        {
            get { return _comissiontype2; }
            set
            {
                _comissiontype2 = value;
                OnPropertyChanged();
            }
        }

        private string _addDsComission;
        public string AddDsComission
        {
            get { return _addDsComission; }
            set
            {
                _addDsComission = value.Replace('.', ',');
            }
        }
        private string _addDsPriceStep;
        public string AddDsPriceStep
        {
            get { return _addDsPriceStep; }
            set
            {
                _addDsPriceStep = value.Replace('.', ',');
            }
        }
        private string _addDsCostPriceStep;
        public string AddDsCostPriceStep
        {
            get { return _addDsCostPriceStep; }
            set
            {
                _addDsCostPriceStep = value.Replace('.', ',');
            }
        }

        public void AddDataSource_Closing(object sender, CancelEventArgs e)
        {
            AddDataSourceFolder = null; //сбрасываем название папки
            FilesUnselected.Clear(); //очищаем список файлы
            FilesSelected.Clear();
            viewmodelData.IsPagesAndMainMenuButtonsEnabled = true;
            CloseAddDataSourceAction = null; //сбрасываем Action, чтобы при инициализации нового окна в него поместился метод его закрытия
        }

        public ICommand OpenFolder_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    VistaFolderBrowserDialog vistaFolderBrowserDialog = new VistaFolderBrowserDialog();
                    bool? isSuccess = vistaFolderBrowserDialog.ShowDialog();
                    if(isSuccess == true)
                    {
                        AddDataSourceFolder = vistaFolderBrowserDialog.SelectedPath; //получаем путь выбранной папки
                        string[] files = Directory.GetFiles(vistaFolderBrowserDialog.SelectedPath); //получаем массив с названиями файлов
                        FilesUnselected.Clear(); //очищаем список невыбранных файлов
                        FilesSelected.Clear(); //очищаем список выбранных файлов
                        foreach (string item in files) //проходимся по списку файлу и те котоыре имеют расширение .txt добавляем в список невыбранных файлов
                        {
                            if(item.Substring(item.Length - 4) == ".txt")
                            {
                                FilesUnselected.Add(item.Substring(item.LastIndexOf("\\") + 1)); //обрезаем полный путь к файлу, оставляя только имя и расширение
                            }
                        }
                    }
                }, (obj) => true);
            }
        }

        public ICommand MoveSingleItemToSelected_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    int index = FilesUnselected.IndexOf(FilesUnselectedSelectedItem); //находим индекс выбранного элемента
                    if(index != -1) //если элемент найден
                    {
                        FilesSelected.Add(FilesUnselected[index]); //добавляем выбранный элемент в список выбранных
                        FilesUnselected.RemoveAt(index); //удаляем элемент из списка невыбранных
                    }
                }, (obj) => true);
            }
        }

        public ICommand MoveAllItemsToSelected_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    foreach(string item in FilesUnselected)
                    {
                        FilesSelected.Add(item);
                    }
                    FilesUnselected.Clear();
                }, (obj) => true);
            }
        }

        public ICommand MoveSingleItemToUnselected_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    int index = FilesSelected.IndexOf(FilesSelectedSelectedItem);
                    if(index != -1)
                    {
                        FilesUnselected.Add(FilesSelected[index]);
                        FilesSelected.RemoveAt(index);
                    }
                }, (obj) => true);
            }
        }

        public ICommand MoveAllItemsToUnselected_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    foreach(string item in FilesSelected)
                    {
                        FilesUnselected.Add(item);
                    }
                    FilesSelected.Clear();
                }, (obj) => true);
            }
        }

        public Action CloseAddDataSourceAction { get; set; }

        public ICommand CloseAddDataSource_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CloseAddDataSourceAction?.Invoke();
                }, (obj) => true);
            }
        }

        private ObservableCollection<string> _tooltipAddAddDataSource = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipAddAddDataSource
        {
            get { return _tooltipAddAddDataSource; }
            set
            {
                _tooltipAddAddDataSource = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsAddDataSourceCurrect()
        {
            bool result = true;
            TooltipAddAddDataSource.Clear(); //очищаем подсказку кнопки добавить

            //проверка на пустые поля
            if (String.IsNullOrEmpty(AddDsName) || (String.IsNullOrEmpty(AddDsCost) && AddDsInstrument == InstrumentsView[1]) || String.IsNullOrEmpty(AddDsComission) || String.IsNullOrEmpty(AddDsPriceStep) || String.IsNullOrEmpty(AddDsCostPriceStep) || FilesSelected.Count == 0)
            {
                result = false;
                TooltipAddAddDataSource.Add("Не заполнены все поля или не выбраны файлы с котировками.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            foreach(DataSource itemDs in DataSources)
            {
                if(AddDsName == itemDs.Name)
                {
                    isUnique = false;
                }
            }
            if(isUnique == false)
            {
                result = false;
                TooltipAddAddDataSource.Add("Данное название уже используется.");
            }

            //проверка на возможность конвертации AddDsCost в число с плавающей точкой
            if(double.TryParse(AddDsCost, out double res) == false && AddDsInstrument == InstrumentsView[1])
            {
                result = false;
                TooltipAddAddDataSource.Add("Стоимость фьючерсного контракта должна быть числом.");
            }

            //проверка на возможность конвертации AddDsComission в число с плавающей точкой
            if (double.TryParse(AddDsComission, out res) == false)
            {
                result = false;
                TooltipAddAddDataSource.Add("Комиссия за одну операцию должна быть числом.");
            }

            //проверка на возможность конвертации AddDsPriceStep в число с плавающей точкой
            if (double.TryParse(AddDsPriceStep, out res) == false)
            {
                result = false;
                TooltipAddAddDataSource.Add("Шаг одного пункта цены должен быть числом.");
            }

            //проверка на возможность конвертации AddDsCostPriceStep в число с плавающей точкой
            if (double.TryParse(AddDsCostPriceStep, out res) == false)
            {
                result = false;
                TooltipAddAddDataSource.Add("Стоимость одного пункта цены должна быть числом.");
            }

            return result;
        }

        public ICommand AddAddDataSource_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //формирует список с файлами
                    List<DataSourceFile> dataSourceFiles = new List<DataSourceFile>();
                    foreach(string item in FilesSelected)
                    {
                        DataSourceFile dataSourceFile = new DataSourceFile { Path = AddDataSourceFolder + "\\" + item };
                        dataSourceFiles.Add(dataSourceFile);
                    }
                    //формирует addInstrument
                    Instrument addInstrument = null;
                    foreach (Instrument instrument in Instruments)
                    {
                        if (instrument.Name == AddDsInstrument)
                        {
                            addInstrument = instrument;
                        }
                    }
                    //формирует addCurrency
                    Currency addCurrency = null;
                    foreach (Currency currency in Currencies)
                    {
                        if (currency.Name == AddDsCurrency)
                        {
                            addCurrency = currency;
                        }
                    }
                    //форимрует addComissiontype
                    Comissiontype addComissiontype = null;
                    foreach (Comissiontype comissiontype in Comissiontypes)
                    {
                        if(comissiontype.Name == AddDsComissiontype)
                        {
                            addComissiontype = comissiontype;
                        }
                    }
                    //формирует стоимость в формате nullable<double>
                    double? cost = null;
                    if (double.TryParse(AddDsCost, out double costdouble))
                    {
                        cost = costdouble;
                    }
                    _modelDataSource.CreateDataSourceInsertUpdate(AddDsName, addInstrument, addCurrency, cost, addComissiontype, double.Parse(AddDsComission), double.Parse(AddDsPriceStep), double.Parse(AddDsCostPriceStep), dataSourceFiles);
                    CloseAddDataSourceAction?.Invoke();
                }, (obj) => IsFieldsAddDataSourceCurrect());
            }
        }
        #endregion

        #region edit datasource

        private ObservableCollection<string> _tooltipSaveEditDataSource = new ObservableCollection<string>();
        public ObservableCollection<string> TooltipSaveEditDataSource
        {
            get { return _tooltipSaveEditDataSource; }
            set
            {
                _tooltipSaveEditDataSource = value;
                OnPropertyChanged();
            }
        }

        private bool IsFieldsEditDataSourceCurrect()
        {
            bool result = true;
            TooltipSaveEditDataSource.Clear(); //очищаем подсказку кнопки сохранить

            //проверка на пустые поля
            if (String.IsNullOrEmpty(AddDsName) || (String.IsNullOrEmpty(AddDsCost) && AddDsInstrument == InstrumentsView[1]) || String.IsNullOrEmpty(AddDsComission) || String.IsNullOrEmpty(AddDsPriceStep) || String.IsNullOrEmpty(AddDsCostPriceStep) || FilesSelected.Count == 0)
            {
                result = false;
                TooltipSaveEditDataSource.Add("Не заполнены все поля или не выбраны файлы с котировками.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            foreach (DataSource itemDs in DataSources)
            {
                if (AddDsName == itemDs.Name && itemDs.Id != EditDsId) //проверяем имя на уникальность среди всех записей кроме редактируемой
                {
                    isUnique = false;
                }
            }
            if (isUnique == false)
            {
                result = false;
                TooltipSaveEditDataSource.Add("Данное название уже используется.");
            }

            //проверка на возможность конвертации AddDsCost в число с плавающей точкой
            if (double.TryParse(AddDsCost, out double res) == false && AddDsInstrument == InstrumentsView[1])
            {
                result = false;
                TooltipSaveEditDataSource.Add("Стоимость фьючерсного контракта должна быть числом.");
            }

            //проверка на возможность конвертации AddDsComission в число с плавающей точкой
            if (double.TryParse(AddDsComission, out res) == false)
            {
                result = false;
                TooltipSaveEditDataSource.Add("Комиссия за одну операцию должна быть числом.");
            }

            //проверка на возможность конвертации AddDsPriceStep в число с плавающей точкой
            if (double.TryParse(AddDsPriceStep, out res) == false)
            {
                result = false;
                TooltipSaveEditDataSource.Add("Шаг одного пункта цены должен быть числом.");
            }

            //проверка на возможность конвертации AddDsCostPriceStep в число с плавающей точкой
            if (double.TryParse(AddDsCostPriceStep, out res) == false)
            {
                result = false;
                TooltipSaveEditDataSource.Add("Стоимость одного пункта цены должна быть числом.");
            }

            return result;
        }

        public ICommand SaveEditDataSource_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //формирует список с файлами
                    List<DataSourceFile> dataSourceFiles = new List<DataSourceFile>();
                    foreach (string item in FilesSelected)
                    {
                        DataSourceFile dataSourceFile = new DataSourceFile { Path = AddDataSourceFolder + "\\" + item };
                        dataSourceFiles.Add(dataSourceFile);
                    }
                    //формирует addInstrument
                    Instrument addInstrument = null;
                    foreach (Instrument instrument in Instruments)
                    {
                        if (instrument.Name == AddDsInstrument)
                        {
                            addInstrument = instrument;
                        }
                    }
                    //формирует addCurrency
                    Currency addCurrency = null;
                    foreach (Currency currency in Currencies)
                    {
                        if (currency.Name == AddDsCurrency)
                        {
                            addCurrency = currency;
                        }
                    }
                    //форимрует addComissiontype
                    Comissiontype addComissiontype = null;
                    foreach (Comissiontype comissiontype in Comissiontypes)
                    {
                        if (comissiontype.Name == AddDsComissiontype)
                        {
                            addComissiontype = comissiontype;
                        }
                    }
                    //формирует стоимость в формате nullable<double>
                    double? cost = null;
                    if(double.TryParse(AddDsCost, out double costdouble)){
                        cost = costdouble;
                    }
                    _modelDataSource.CreateDataSourceInsertUpdate(AddDsName, addInstrument, addCurrency, cost, addComissiontype, double.Parse(AddDsComission), double.Parse(AddDsPriceStep), double.Parse(AddDsCostPriceStep), dataSourceFiles, EditDsId);

                    CloseAddDataSourceAction?.Invoke();
                }, (obj) => IsFieldsEditDataSourceCurrect());
            }
        }
        #endregion

    }
}
