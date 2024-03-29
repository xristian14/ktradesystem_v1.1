﻿using System;
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
using System.Windows;
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

            _modelData.MarginTypes.CollectionChanged += modelData_MarginTypesCollectionChanged;
            MarginTypes = _modelData.MarginTypes;

            _modelData.Currencies.CollectionChanged += modelData_CurrenciesCollectionChanged;
            Currencies = _modelData.Currencies;

            _modelData.Intervals.CollectionChanged += modelData_IntervalsCollectionChanged;
            Intervals = _modelData.Intervals;

            _modelData.Comissiontypes.CollectionChanged += modelData_ComissiontypesCollectionChanged;
            Comissiontypes = _modelData.Comissiontypes;

            _modelData.DataSourcesForSubscribers.CollectionChanged += modelData_DataSourcesForSubscribersCollectionChanged; //вешаем на обновление DataSourcesForSubscribers в модели метод: присвоить текущему DataSourcesForSubscribers DataSourcesForSubscribers из модели
            DataSourcesForSubscribers = _modelData.DataSourcesForSubscribers;

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
        public ViewmodelData ViewmodelData
        {
            get
            {
                if (_viewmodelData == null)
                {
                    _viewmodelData = ViewmodelData.getInstance(); //реализовано таким образом, т.к. объекты ссылаюстя друг на друга и идет бесконечный цикл инициализации
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

        private ObservableCollection<MarginType> _marginTypes = new ObservableCollection<MarginType>();
        public ObservableCollection<MarginType> MarginTypes
        {
            get { return _marginTypes; }
            set
            {
                _marginTypes = value;
                OnPropertyChanged();
            }
        }

        private void modelData_MarginTypesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MarginTypes = (ObservableCollection<MarginType>)sender;
        }

        private ObservableCollection<Currency> _currencies = new ObservableCollection<Currency>();

        public ObservableCollection<Currency> Currencies
        {
            get { return _currencies; }
            set
            {
                _currencies = value;
                OnPropertyChanged();
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
            }
        }
        private void modelData_ComissiontypesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Comissiontypes = (ObservableCollection<Comissiontype>)sender;
        }

        private ObservableCollection<DataSource> _dataSources = new ObservableCollection<DataSource>(); //источники данных
        public ObservableCollection<DataSource> DataSourcesForSubscribers
        {
            get { return _dataSources; }
            private set
            {
                _dataSources = value;
                OnPropertyChanged();
                CreateDataSourceView(); //вызвает метод формирования списка источников данных для отображения
            }
        }

        private void modelData_DataSourcesForSubscribersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DataSourcesForSubscribers = (ObservableCollection<DataSource>)sender;
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

        private void CreateDataSourceView() //создает DataSourcesView на основе DataSourcesForSubscribers
        {
            DataSourcesView.Clear();
            foreach(DataSource dsItem in DataSourcesForSubscribers)
            {
                //определяет название интервала
                string intervalName = "";
                foreach (Interval interval in Intervals)
                {
                    if (interval.Id == dsItem.Interval.Id)
                    {
                        intervalName = interval.Name;
                    }
                }
                //формирует комиссию с типом комиссии
                string comission = "";
                if(dsItem.Comissiontype.Id == 1)
                {
                    comission = dsItem.Comission.ToString() + dsItem.Currency.Name;
                }
                else
                {
                    comission = dsItem.Comission.ToString() + "%";
                }
                //формирует данные за период
                string datePeriod = dsItem.StartDate.ToShortDateString() + " – " + dsItem.EndDate.ToShortDateString();
                //формирует список файлов
                List<string> files = new List<string>();
                foreach (DataSourceFile dataSourceFile in dsItem.DataSourceFiles)
                {
                    files.Add(dataSourceFile.Path);
                }

                DataSourceView dsView = new DataSourceView { Id = dsItem.Id, Name = dsItem.Name, Currency = dsItem.Currency, MarginCost = dsItem.MarginCost, MinLotCount = dsItem.MinLotCount, MinLotMarginPartCost = (decimal)dsItem.MinLotMarginPartCost, Interval = intervalName, MarginType = MarginTypes.Where(a => a.Id == dsItem.MarginType.Id).First(), Comissiontype = dsItem.Comissiontype, Comission = (decimal)dsItem.Comission, ComissionView = comission, PriceStep = (decimal)dsItem.PriceStep, CostPriceStep = (decimal)dsItem.CostPriceStep, PointsSlippage = dsItem.PointsSlippage, DatePeriod = datePeriod, Files = files };
                DataSourcesView.Add(dsView);
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

        public DataSourceView SelectedDataSource { get; set; }

        public ICommand AddDataSource_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    IsAddDataSource = true;
                    AddDsName = "";
                    AddDsMarginType = null;
                    AddDsCurrency = null;
                    AddDsComissiontype = null;
                    AddDsMarginCost = "";
                    AddDsMinLotCount = "";
                    AddDsMinLotMarginPartCost = "";

                    AddDsComission = "";
                    AddDsPriceStep = "";
                    AddDsCostPriceStep = "";
                    AddDsPointsSlippage = "";
                    AddDataSourceFolder = "";

                    ViewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
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
                    IsAddDataSource = false;
                    EditDsId = SelectedDataSource.Id;
                    AddDsName = SelectedDataSource.Name;
                    AddDsMarginType = SelectedDataSource.MarginType;
                    AddDsCurrency = SelectedDataSource.Currency;
                    AddDsComissiontype = SelectedDataSource.Comissiontype;
                    AddDsMarginCost = SelectedDataSource.MarginCost.ToString();
                    AddDsMinLotCount = SelectedDataSource.MinLotCount.ToString();
                    AddDsMinLotMarginPartCost = SelectedDataSource.MinLotMarginPartCost.ToString();

                    AddDsComission = SelectedDataSource.Comission.ToString();
                    AddDsPriceStep = SelectedDataSource.PriceStep.ToString();
                    AddDsCostPriceStep = SelectedDataSource.CostPriceStep.ToString();
                    AddDsPointsSlippage = SelectedDataSource.PointsSlippage.ToString();
                    AddDataSourceFolder = SelectedDataSource.Files[0].Substring(0, SelectedDataSource.Files[0].LastIndexOf('\\'));
                    if (Directory.Exists(AddDataSourceFolder)) //если директория существует, добавляем файлы из нее
                    {
                        string[] files = Directory.GetFiles(AddDataSourceFolder); //получаем массив с названиями файлов
                        foreach (string item in files) //проходимся по списку файлу и те котоыре имеют расширение .csv добавляем в список невыбранных файлов
                        {
                            if (item.Substring(item.Length - 4) == ".csv")
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

                    ViewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewAddDataSource viewAddDataSource = new ViewAddDataSource();
                    viewAddDataSource.Show();

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
                    var result = System.Windows.Forms.MessageBox.Show(msg, caption, messageBoxButton);
                    if (result == DialogResult.Yes)
                    {
                        _modelDataSource.DeleteDataSource(SelectedDataSource.Id);
                    }
                }, (obj) => SelectedDataSource != null );
            }
        }

        #region add datasource

        private bool _isAddDataSource;
        public bool IsAddDataSource //добавляется источник данных или редактируется
        {
            get { return _isAddDataSource; }
            set
            {
                _isAddDataSource = value;
                AddDataSourceButtonContent = value ? "Добавить'" : "Сохранить'";
                AddDataSourceWindowName = value ? "Добавление источника данных'" : "Редактирование источника данных'";
                OnPropertyChanged();
            }
        }

        private string _addDataSourceButtonContent;
        public string AddDataSourceButtonContent //текст кнопки добавить
        {
            get { return _addDataSourceButtonContent; }
            set
            {
                _addDataSourceButtonContent = value;
                OnPropertyChanged();
            }
        }

        private string _addDataSourceWindowName;
        public string AddDataSourceWindowName //название окна
        {
            get { return _addDataSourceWindowName; }
            set
            {
                _addDataSourceWindowName = value;
                OnPropertyChanged();
            }
        }

        private string _addDataSourceFolder;
        public string AddDataSourceFolder
        {
            get { return _addDataSourceFolder; }
            set
            {
                _addDataSourceFolder = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _filesUnselected = new ObservableCollection<string>(); //список невыбранных файлов
        public ObservableCollection<string> FilesUnselected
        {
            get { return _filesUnselected; }
            set
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
            set
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
        private MarginType _addDsMarginType;
        public MarginType AddDsMarginType
        {
            get { return _addDsMarginType; }
            set
            {
                _addDsMarginType = value;
                OnPropertyChanged();
            }
        }
        private Currency _addDsCurrency;
        public Currency AddDsCurrency
        {
            get { return _addDsCurrency; }
            set
            {
                _addDsCurrency = value;
                OnPropertyChanged();
            }
        }
        private Comissiontype _addDsComissiontype;
        public Comissiontype AddDsComissiontype
        {
            get { return _addDsComissiontype; }
            set
            {
                _addDsComissiontype = value;
                OnPropertyChanged();
            }
        }
        private string _addDsCost;
        public string AddDsMarginCost
        {
            get { return _addDsCost; }
            set
            {
                _addDsCost = value;
                OnPropertyChanged();
            }
        }
        private string _addDsMinLotCount;
        public string AddDsMinLotCount
        {
            get { return _addDsMinLotCount; }
            set
            {
                _addDsMinLotCount = value;
                OnPropertyChanged();
            }
        }
        private string _addDsMinLotMarginPartCost;
        public string AddDsMinLotMarginPartCost
        {
            get { return _addDsMinLotMarginPartCost; }
            set
            {
                _addDsMinLotMarginPartCost = value;
                OnPropertyChanged();
            }
        }

        private string _addDsComission;
        public string AddDsComission
        {
            get { return _addDsComission; }
            set
            {
                _addDsComission = value;
            }
        }
        private string _addDsPriceStep;
        public string AddDsPriceStep
        {
            get { return _addDsPriceStep; }
            set
            {
                _addDsPriceStep = value;
            }
        }
        private string _addDsCostPriceStep;
        public string AddDsCostPriceStep
        {
            get { return _addDsCostPriceStep; }
            set
            {
                _addDsCostPriceStep = value;
            }
        }
        private string _addDsPointsSlippage;
        public string AddDsPointsSlippage
        {
            get { return _addDsPointsSlippage; }
            set
            {
                _addDsPointsSlippage = value;
            }
        }

        public void AddDataSource_Closing(object sender, CancelEventArgs e)
        {
            AddDataSourceFolder = null; //сбрасываем название папки
            FilesUnselected.Clear(); //очищаем список файлы
            FilesSelected.Clear();
            if (ViewmodelData.StatusBarDataSourceVisibility == Visibility.Visible) //окно было закрыто по нажатию кнопки добавить или по отмене
            {
                ViewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
            }
            else
            {
                ViewmodelData.IsPagesAndMainMenuButtonsEnabled = true;
            }
            
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
                        foreach (string item in files) //проходимся по списку файлу и те которые имеют расширение .csv добавляем в список невыбранных файлов
                        {
                            if(item.Substring(item.Length - 4) == ".csv")
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

            int marginTypeId = -1;
            if(AddDsMarginType != null)
            {
                marginTypeId = AddDsMarginType.Id;
            }
            //проверка на пустые поля
            if (String.IsNullOrEmpty(AddDsName) || AddDsMarginType == null || AddDsCurrency == null || (String.IsNullOrEmpty(AddDsMarginCost) && marginTypeId == 2) || String.IsNullOrEmpty(AddDsMinLotCount) || String.IsNullOrEmpty(AddDsMinLotMarginPartCost) || AddDsComissiontype == null || String.IsNullOrEmpty(AddDsComission) || String.IsNullOrEmpty(AddDsPriceStep) || String.IsNullOrEmpty(AddDsCostPriceStep) || String.IsNullOrEmpty(AddDsPointsSlippage) || FilesSelected.Count == 0)
            {
                result = false;
                TooltipAddAddDataSource.Add("Не заполнены все поля или не выбраны файлы с котировками.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            
            foreach(DataSource itemDs in DataSourcesForSubscribers)
            {
                if (IsAddDataSource)
                {
                    if (AddDsName == itemDs.Name)
                    {
                        isUnique = false;
                    }
                }
                else
                {
                    if (AddDsName == itemDs.Name && itemDs.Id != EditDsId) //проверяем имя на уникальность среди всех записей кроме редактируемой
                    {
                        isUnique = false;
                    }
                }
            }
            if(isUnique == false)
            {
                result = false;
                TooltipAddAddDataSource.Add("Данное название уже используется.");
            }

            //проверка на возможность конвертации AddDsMarginCost в число с плавающей точкой
            if(double.TryParse(AddDsMarginCost, out double res) == false && marginTypeId == 2)
            {
                result = false;
                TooltipAddAddDataSource.Add("Значение фиксированной маржи должно быть числом.");
            }

            //проверка на возможность конвертации AddDsMinLotCount в число с плавающей точкой
            if (decimal.TryParse(AddDsMinLotCount, out decimal res2) == false)
            {
                result = false;
                TooltipAddAddDataSource.Add("Минимальное количество лотов должно быть числом.");
            }

            //проверка на возможность конвертации AddDsMinLotMarginPartCost в число с плавающей точкой
            if (double.TryParse(AddDsMinLotMarginPartCost, out res) == false)
            {
                result = false;
                TooltipAddAddDataSource.Add("Стоимость минимального количества лотов относительно маржи должна быть числом.");
            }

            //проверка на возможность конвертации AddDsComission в число с плавающей точкой
            if (double.TryParse(AddDsComission, out res) == false)
            {
                result = false;
                TooltipAddAddDataSource.Add("Комиссия на сделку для минимального количества лотов должна быть числом.");
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

            //проверка на возможность конвертации AddDsPointsSlippage в целое число
            if (int.TryParse(AddDsPointsSlippage, out int res3) == false)
            {
                result = false;
                TooltipAddAddDataSource.Add("Базовое поскальзывание в пунктах должно быть целым числом.");
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
                    //формирует стоимость
                    double marginCost = AddDsMarginType.Id == 2 ? double.Parse(AddDsMarginCost) : 0;

                    //показываем statusBarDataSource
                    ViewmodelData.StatusBarDataSourceShow();

                    if (IsAddDataSource)
                    {
                        //запускаем добавление в отдельном потоке чтобы форма обновлялась
                        Task.Run(() => _modelDataSource.CreateDataSourceInsertUpdate(AddDsName, AddDsMarginType, AddDsCurrency, marginCost, decimal.Parse(AddDsMinLotCount), double.Parse(AddDsMinLotMarginPartCost), AddDsComissiontype, double.Parse(AddDsComission), double.Parse(AddDsPriceStep), double.Parse(AddDsCostPriceStep), int.Parse(AddDsPointsSlippage), dataSourceFiles));
                    }
                    else
                    {
                        //запускаем редактирование в отдельном потоке чтобы форма обновлялась
                        Task.Run(() => _modelDataSource.CreateDataSourceInsertUpdate(AddDsName, AddDsMarginType, AddDsCurrency, marginCost, decimal.Parse(AddDsMinLotCount), double.Parse(AddDsMinLotMarginPartCost), AddDsComissiontype, double.Parse(AddDsComission), double.Parse(AddDsPriceStep), double.Parse(AddDsCostPriceStep), int.Parse(AddDsPointsSlippage), dataSourceFiles, EditDsId));
                    }

                    CloseAddDataSourceAction?.Invoke();
                }, (obj) => IsFieldsAddDataSourceCurrect());
            }
        }
        #endregion

    }
}
