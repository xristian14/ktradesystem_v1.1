using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ktradesystem.Views;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;
using ktradesystem.Views.Pages.TestingResultPages;
using System.IO;

using System.Reflection;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTestingResult : ViewModelBase
    {
        private static ViewModelPageTestingResult _instance;

        private ViewModelPageTestingResult()
        {
            _modelTestingResult = ModelTestingResult.getInstance();

            _modelTestingResult.TestingHistoryForSubscribers.CollectionChanged += ModelTestingResult_TestingHistoryCollectionChanged;
            TestingHistory = _modelTestingResult.TestingHistoryForSubscribers;

            _modelTestingResult.TestingSavesForSubscribers.CollectionChanged += ModelTestingResult_TestingSavesCollectionChanged;
            TestingSaves = _modelTestingResult.TestingSavesForSubscribers;

            SetMenuHistory(); //выбираем пункт меню История

            CreateTabControlTestingResultItems(); //создаем вкладки со страницами графиков и таблиц
        }

        public static ViewModelPageTestingResult getInstance()
        {
            if (_instance == null)
            {
                _instance = new ViewModelPageTestingResult();
            }
            return _instance;
        }

        private ModelTestingResult _modelTestingResult;

        private ObservableCollection<string> _resultTestingMenu = new ObservableCollection<string> { "История", "Сохраненные" };
        public ObservableCollection<string> ResultTestingMenu
        {
            get { return _resultTestingMenu; }
            private set
            {
                _resultTestingMenu = value;
                OnPropertyChanged();
            }
        }

        private string _selectedResultTestingMenu;
        public string SelectedResultTestingMenu
        {
            get { return _selectedResultTestingMenu; }
            set
            {
                _selectedResultTestingMenu = value;
                OnPropertyChanged();
                UpdateTestingComboboxesVisibility();
                LoadSelectedTestingResult(); //вызываем загрузку выбранного тестирования, т.к. после смены источника (история, сохраненные) нужно обновить форму
            }
        }


        private Visibility _testingHistoryVisibility;
        public Visibility TestingHistoryVisibility //видимость combobox с историей результатов тестирования
        {
            get { return _testingHistoryVisibility; }
            private set
            {
                _testingHistoryVisibility = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<TestingHeader> _testingHistory = new ObservableCollection<TestingHeader>();
        public ObservableCollection<TestingHeader> TestingHistory //история результатов тестирования
        {
            get { return _testingHistory; }
            private set
            {
                _testingHistory = value;
                OnPropertyChanged();
            }
        }
        private void ModelTestingResult_TestingHistoryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TestingHistory = (ObservableCollection<TestingHeader>)sender;
        }
        private TestingHeader _selectedTestingHistory;
        public TestingHeader SelectedTestingHistory //выбранный результат тестирования из истории
        {
            get { return _selectedTestingHistory; }
            set
            {
                _selectedTestingHistory = value;
                OnPropertyChanged();
                LoadSelectedTestingResult(); //вызываем загрузку выбраного тестирования
            }
        }


        private Visibility _testingSavesVisibility;
        public Visibility TestingSavesVisibility //видимость combobox с сохраненными результами тестирования
        {
            get { return _testingSavesVisibility; }
            private set
            {
                _testingSavesVisibility = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<TestingHeader> _testingSaves = new ObservableCollection<TestingHeader>();
        public ObservableCollection<TestingHeader> TestingSaves //сохраненные результаты тестирования
        {
            get { return _testingSaves; }
            private set
            {
                _testingSaves = value;
                OnPropertyChanged();
            }
        }
        private void ModelTestingResult_TestingSavesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TestingSaves = (ObservableCollection<TestingHeader>)sender;
        }
        private TestingHeader _selectedTestingSaves;
        public TestingHeader SelectedTestingSaves //выбранный сохраненный результат тестирования
        {
            get { return _selectedTestingSaves; }
            set
            {
                _selectedTestingSaves = value;
                OnPropertyChanged();
                LoadSelectedTestingResult(); //вызываем загрузку выбраного тестирования
            }
        }

        public void UpdateTestingComboboxesVisibility() //обновляет видимость списков с результатами тестирования в зависимости от выбранного пункта меню: история или сохраненные
        {
            if(SelectedResultTestingMenu == ResultTestingMenu[0])
            {
                TestingHistoryVisibility = Visibility.Visible;
                TestingSavesVisibility = Visibility.Collapsed;
            }
            else
            {
                TestingHistoryVisibility = Visibility.Collapsed;
                TestingSavesVisibility = Visibility.Visible;
            }
        }

        public void SetMenuHistory() //выбирает пункт меню История
        {
            SelectedResultTestingMenu = ResultTestingMenu[0];
        }

        public void SetLastHistoryTestingResult() //выбирает последний элемент списка история результатов тестирования
        {
            if(TestingHistory.Count > 0)
            {
                SelectedTestingHistory = TestingHistory.Last();
            }
        }

        private Testing _testingResult;
        public Testing TestingResult //результат тестирования
        {
            get { return _testingResult; }
            private set
            {
                _testingResult = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<DataSourceGroupTestingResultCombobox> _dataSourceGroupsTestingResultCombobox = new ObservableCollection<DataSourceGroupTestingResultCombobox>();
        public ObservableCollection<DataSourceGroupTestingResultCombobox> DataSourceGroupsTestingResultCombobox //список с группами источников данных выбранного результата тестирования
        {
            get { return _dataSourceGroupsTestingResultCombobox; }
            private set
            {
                _dataSourceGroupsTestingResultCombobox = value;
                OnPropertyChanged();
            }
        }

        private DataSourceGroupTestingResultCombobox _selectedDataSourceGroupTestingResultCombobox;
        public DataSourceGroupTestingResultCombobox SelectedDataSourceGroupTestingResultCombobox //выбранная группа источников данных выбранного результата тестирования
        {
            get { return _selectedDataSourceGroupTestingResultCombobox; }
            set
            {
                _selectedDataSourceGroupTestingResultCombobox = value;
                OnPropertyChanged();
                DataSourceGroupsUpdatePages?.Invoke(); //вызываем методы, обновляющие страницы, отображающие информацию о тестовых связках в рамках определенного источника данных
                //создаем список тестовых связок выбранной группы источников данных на основе загруженного результата тестирования
                ReadDataSourceCandles(); //считываем свечки источников данных, выбранной группы источников данных
                CreateTestBatchesTestingResultCombobox();
                if(TestBatchesTestingResultCombobox.Count > 0)
                {
                    SelectedTestBatchTestingResultCombobox = TestBatchesTestingResultCombobox.First(); //выбираем первую тестовую связку
                }
            }
        }

        private ObservableCollection<TestBatchTestingResultCombobox> _testBatchesTestingResultCombobox = new ObservableCollection<TestBatchTestingResultCombobox>();
        public ObservableCollection<TestBatchTestingResultCombobox> TestBatchesTestingResultCombobox //список с тестовыми связками выбранного результата тестирования
        {
            get { return _testBatchesTestingResultCombobox; }
            private set
            {
                _testBatchesTestingResultCombobox = value;
                OnPropertyChanged();
            }
        }

        private TestBatchTestingResultCombobox _selectedTestBatchTestingResultCombobox;
        public TestBatchTestingResultCombobox SelectedTestBatchTestingResultCombobox //выбранная тестовая связка выбранного результата тестирования
        {
            get { return _selectedTestBatchTestingResultCombobox; }
            set
            {
                _selectedTestBatchTestingResultCombobox = value;
                OnPropertyChanged();
                TestBatchesUpdatePages?.Invoke(); //вызываем методы, обновляющие страницы, отображающие информацию о конкретной тестовой связке. Обновление 3d графика находится здесь
                //создаем список тестовых прогонов выбранной тестовой связки на основе загруженного результата тестирования
                CreateTestRunsTestingResultCombobox();
                if(TestRunsTestingResultCombobox.Count > 0)
                {
                    SelectedTestRunTestingResultCombobox = TestRunsTestingResultCombobox.First(); //выбираем первый тестовый прогон
                }
            }
        }

        private ObservableCollection<TestRunTestingResultCombobox> _testRunsTestingResultCombobox = new ObservableCollection<TestRunTestingResultCombobox>();
        public ObservableCollection<TestRunTestingResultCombobox> TestRunsTestingResultCombobox //список с тестовыми прогонами выбранного результата тестирования
        {
            get { return _testRunsTestingResultCombobox; }
            private set
            {
                _testRunsTestingResultCombobox = value;
                OnPropertyChanged();
            }
        }

        private TestRunTestingResultCombobox _selectedTestRunTestingResultCombobox;
        public TestRunTestingResultCombobox SelectedTestRunTestingResultCombobox //выбранный тестовый прогон выбранного результата тестирования
        {
            get { return _selectedTestRunTestingResultCombobox; }
            set
            {
                _selectedTestRunTestingResultCombobox = value;
                OnPropertyChanged();
                ReadIndicatorValues(); //считываем значения индикаторов для выбранного testRun
                TestRunsUpdatePages?.Invoke(); //вызываем методы, обновляющие страницы, отображающие информацию о конкретном тестовом прогоне
            }
        }

        public void ResetTestingResult() //обнуляет объект тестирования и очищает поля
        {
            TestingResult = null;
            CreateDataSourceGroupsTestingResultCombobox(); //создаем группы источников данных на основе загруженного результата тестирования
        }

        private void LoadSelectedTestingResult() //загружает выбранный результат тестирования
        {
            bool isSelectMenuHistory = SelectedResultTestingMenu == ResultTestingMenu[0] ? true : false; //выбрана история или нет, выбрано сохраненные
            bool isSelectedTesting = false; //выбран ли результат тестирования
            TestingHeader testingHeader = null; //выбранный результат тестирования
            if (isSelectMenuHistory) //для списка с историей
            {
                isSelectedTesting = SelectedTestingHistory != null ? true : false;
                testingHeader = SelectedTestingHistory;
            }
            else //для списка с сохраненными
            {
                isSelectedTesting = SelectedTestingSaves != null ? true : false;
                testingHeader = SelectedTestingSaves;
            }
            //если тестирование выбрано, считываем его
            if (isSelectedTesting)
            {
                Testing testing = _modelTestingResult.LoadTesting(testingHeader);
                if(testing != null) //если тестирование успешно считано, записываем его и заносим в поля значения
                {
                    TestingResult = testing;

                    TestingResultUpdatePages?.Invoke(); //вызываем методы, обновляющие страницы, отображающие информацию о тестировании в целом
                    //формируем список с группами источников данных
                    CreateDataSourceGroupsTestingResultCombobox(); //создаем группы источников данных на основе загруженного результата тестирования
                    SelectedDataSourceGroupTestingResultCombobox = DataSourceGroupsTestingResultCombobox.First(); //выбираем первую группу источников данных
                }
            }
            else //если результат тестирования не выбран
            {
                ResetTestingResult(); //обнуляем объект тестирования и очищаем поля
            }
        }

        private void CreateDataSourceGroupsTestingResultCombobox() //создает группы источников данных на основе загруженного результата тестирования
        {
            DataSourceGroupsTestingResultCombobox.Clear();
            if(TestingResult != null)
            {
                foreach (DataSourceGroup dataSourceGroup in TestingResult.DataSourceGroups)
                {
                    string nameDataSourceGroup = "";
                    foreach (DataSourceAccordance dataSourceAccordance in dataSourceGroup.DataSourceAccordances)
                    {
                        nameDataSourceGroup += nameDataSourceGroup.Length != 0 ? ", " : "";
                        nameDataSourceGroup += dataSourceAccordance.DataSource.Name;
                    }
                    DataSourceGroupsTestingResultCombobox.Add(new DataSourceGroupTestingResultCombobox { DataSourceGroup = dataSourceGroup, NameDataSourceGroup = nameDataSourceGroup });
                }
            }
        }

        private void CreateTestBatchesTestingResultCombobox() //создает список тестовых связок выбранной группы источников данных на основе загруженного результата тестирования
        {
            TestBatchesTestingResultCombobox.Clear();
            if(SelectedDataSourceGroupTestingResultCombobox != null)
            {
                foreach (TestBatch testBatch in TestingResult.TestBatches)
                {
                    //проверяем совпадение группы истоников данных тествой связки и выбранной группы источников данных
                    bool isEqual = true;
                    foreach (DataSourceAccordance dataSourceAccordance in testBatch.DataSourceGroup.DataSourceAccordances)
                    {
                        if (SelectedDataSourceGroupTestingResultCombobox.DataSourceGroup.DataSourceAccordances.Where(j => j.DataSource.Id == dataSourceAccordance.DataSource.Id).Any() == false)
                        {
                            isEqual = false;
                        }
                    }
                    if (isEqual) //тестовая связка имеет группу источников данных как выбранная группа источников данных
                    {
                        DateTime dateTimeStart = testBatch.OptimizationTestRuns[0].StartPeriod; //начало периода тестирования в тестовой связке
                        DateTime dateTimeEnd = testBatch.ForwardTestRun == null ? testBatch.OptimizationTestRuns[0].EndPeriod : testBatch.ForwardTestRun.EndPeriod; //окончание периода тестирования в тестовой связке (если есть форвардное тестирование, то окончание возьмется из него)
                        string dateTimeStartStr = dateTimeStart.Day.ToString().Length == 1 ? "0" + dateTimeStart.Day.ToString() : dateTimeStart.Day.ToString();
                        dateTimeStartStr += dateTimeStart.Month.ToString().Length == 1 ? ".0" + dateTimeStart.Month.ToString() : "." + dateTimeStart.Month.ToString();
                        dateTimeStartStr += "." + dateTimeStart.Year.ToString();

                        string dateTimeEndStr = dateTimeEnd.Day.ToString().Length == 1 ? "0" + dateTimeEnd.Day.ToString() : dateTimeEnd.Day.ToString();
                        dateTimeEndStr += dateTimeEnd.Month.ToString().Length == 1 ? ".0" + dateTimeEnd.Month.ToString() : "." + dateTimeEnd.Month.ToString();
                        dateTimeEndStr += "." + dateTimeEnd.Year.ToString();

                        TestBatchesTestingResultCombobox.Add(new TestBatchTestingResultCombobox { TestBatch = testBatch, NameTestBatch = dateTimeStartStr + " - " + dateTimeEndStr });
                    }
                }
            }
        }

        private void CreateTestRunsTestingResultCombobox() //создает список тестовых прогонов выбранной тестовой связки на основе загруженного результата тестирования
        {
            TestRunsTestingResultCombobox.Clear();
            if(SelectedTestBatchTestingResultCombobox != null)
            {
                //добавляем топ-модель если она есть
                if (SelectedTestBatchTestingResultCombobox.TestBatch.IsTopModelWasFind)
                {
                    TestRunsTestingResultCombobox.Add(new TestRunTestingResultCombobox { TestRun = SelectedTestBatchTestingResultCombobox.TestBatch.TopModelTestRun, NameTestRun = SelectedTestBatchTestingResultCombobox.TestBatch.TopModelTestRun.Number.ToString() + " - топ-модель" });
                }
                //добавляем форвардное тестирование
                if (SelectedTestBatchTestingResultCombobox.TestBatch.ForwardTestRun != null && SelectedTestBatchTestingResultCombobox.TestBatch.IsTopModelWasFind) //если форвардный тест есть, и топ-модель была найдена
                {
                    TestRunsTestingResultCombobox.Add(new TestRunTestingResultCombobox { TestRun = SelectedTestBatchTestingResultCombobox.TestBatch.ForwardTestRun, NameTestRun = SelectedTestBatchTestingResultCombobox.TestBatch.ForwardTestRun.Number.ToString() + " - форвардный" });
                }
                //добавляем форвардное тестирование с торговлей депозитом
                if (SelectedTestBatchTestingResultCombobox.TestBatch.ForwardTestRunDepositTrading != null && SelectedTestBatchTestingResultCombobox.TestBatch.IsTopModelWasFind) //если форвардный тест есть, и топ-модель была найдена
                {
                    TestRunsTestingResultCombobox.Add(new TestRunTestingResultCombobox { TestRun = SelectedTestBatchTestingResultCombobox.TestBatch.ForwardTestRunDepositTrading, NameTestRun = SelectedTestBatchTestingResultCombobox.TestBatch.ForwardTestRunDepositTrading.Number.ToString() + " - форвардный с торговлей депозитом" });
                }
                //добавляем оптимизационные тесты кроме топ-модели
                foreach (TestRun testRun in SelectedTestBatchTestingResultCombobox.TestBatch.OptimizationTestRuns)
                {
                    bool isNotTopModel = true;
                    if (SelectedTestBatchTestingResultCombobox.TestBatch.IsTopModelWasFind) //если топ-модель была найдена, проверяем не является ли данный testRun топ-моделью
                    {
                        if (testRun.Number == SelectedTestBatchTestingResultCombobox.TestBatch.TopModelTestRun.Number)
                        {
                            isNotTopModel = false;
                        }
                    }
                    if (isNotTopModel) //если данный testRun не является топ-моделью, добавляем его
                    {
                        TestRunsTestingResultCombobox.Add(new TestRunTestingResultCombobox { TestRun = testRun, NameTestRun = testRun.Number.ToString() });
                    }
                }
            }
        }
        private void ReadDataSourceCandles() //считывает свечки источников данных, выбранной группы источников данных
        {
            if (TestingResult != null && SelectedDataSourceGroupTestingResultCombobox != null)
            {
                TestingResult.DataSourcesCandles = _modelTestingResult.ReadDataSourceCandles(TestingResult, SelectedResultTestingMenu == ResultTestingMenu[0] ? true : false, SelectedDataSourceGroupTestingResultCombobox.DataSourceGroup); //вызываем метод модели, считывающий, и возвращающий свечки для указанной группы источников данных
            }
        }
        private void ReadIndicatorValues() //считывает значения индикаторов для выбранного testRun
        {
            if (TestingResult != null && SelectedTestRunTestingResultCombobox != null)
            {
                _modelTestingResult.ReadIndicatorValues(TestingResult, SelectedResultTestingMenu == ResultTestingMenu[0] ? true : false, SelectedTestRunTestingResultCombobox.TestRun); //считываем значения индикаторов алгоритма
            }
        }

        private ObservableCollection<TabControlTestingResultItem> _tabControlTestingResultItems = new ObservableCollection<TabControlTestingResultItem>();
        public ObservableCollection<TabControlTestingResultItem> TabControlTestingResultItems //вкладки
        {
            get { return _tabControlTestingResultItems; }
            private set
            {
                _tabControlTestingResultItems = value;
                OnPropertyChanged();
            }
        }

        private Page _pageTheeDimensionChart = new PageTheeDimensionChart();
        public Page PageTheeDimensionChart //страница с трехмерным графиком
        {
            get { return _pageTheeDimensionChart; }
            private set
            {
                _pageTheeDimensionChart = value;
                OnPropertyChanged();
            }
        }
        private Page _pageTestBatchInfo = new PageTestBatchInfo();
        public Page PageTestBatchInfo //страница информацией о тестовой связке
        {
            get { return _pageTestBatchInfo; }
            private set
            {
                _pageTestBatchInfo = value;
                OnPropertyChanged();
            }
        }
        private Page _pageDataSourceGroupInfo = new PageDataSourceGroupInfo();
        public Page PageDataSourceGroupInfo //страница информацией о группе источников данных
        {
            get { return _pageDataSourceGroupInfo; }
            private set
            {
                _pageDataSourceGroupInfo = value;
                OnPropertyChanged();
            }
        }
        private Page _pageTradeChart = new PageTradeChart();
        public Page PageTradeChart //страница с графиком котировок
        {
            get { return _pageTradeChart; }
            private set
            {
                _pageTradeChart = value;
                OnPropertyChanged();
            }
        }
        private Page _pageOrders = new PageOrders();
        public Page PageOrders //страница с заявками
        {
            get { return _pageOrders; }
            private set
            {
                _pageOrders = value;
                OnPropertyChanged();
            }
        }
        private Page _pageDeals = new PageDeals();
        public Page PageDeals //страница с сделками
        {
            get { return _pageDeals; }
            private set
            {
                _pageDeals = value;
                OnPropertyChanged();
            }
        }
        private Page _pageTestRunInfo = new PageTestRunInfo();
        public Page PageTestRunInfo //страница с информацией о тестовом прогоне
        {
            get { return _pageTestRunInfo; }
            private set
            {
                _pageTestRunInfo = value;
                OnPropertyChanged();
            }
        }
        private Page _pageProfitChart = new PageProfitChart();
        public Page PageProfitChart //страница с графиком доходности
        {
            get { return _pageProfitChart; }
            private set
            {
                _pageProfitChart = value;
                OnPropertyChanged();
            }
        }

        private void CreateTabControlTestingResultItems()
        {
            /*TabControlTestingResultItem tabControlTestingResultItem1 = new TabControlTestingResultItem { Header = "Тестовая связка", HorizontalStackPanels = new List<StackPanelTestingResult>(), VerticalStackPanels = new List<StackPanelTestingResult>() };
            tabControlTestingResultItem1.HorizontalStackPanels.Add(new StackPanelTestingResult { PageItems = new List<PageItem>() { new PageItem { Page = new Views.Pages.TestingResultPages.PageTheeDimensionChart() } } });
            TabControlTestingResultItems.Add(tabControlTestingResultItem1);
            TabControlTestingResultItem tabControlTestingResultItem2 = new TabControlTestingResultItem { Header = "Тестовый прогон", HorizontalStackPanels = new List<StackPanelTestingResult>(), VerticalStackPanels = new List<StackPanelTestingResult>() };
            tabControlTestingResultItem2.VerticalStackPanels.Add(new StackPanelTestingResult { PageItems = new List<PageItem>() { new PageItem { Page = new Views.Pages.TestingResultPages.PageTradeChart() } } });
            tabControlTestingResultItem2.VerticalStackPanels.Add(new StackPanelTestingResult { PageItems = new List<PageItem>() { new PageItem { Page = new Views.Pages.TestingResultPages.PageOrders() }, new PageItem { Page = new Views.Pages.TestingResultPages.PageDeals() }, new PageItem { Page = new Views.Pages.TestingResultPages.PageProfitChart() }, new PageItem { Page = new Views.Pages.TestingResultPages.PageTestRunInfo() } } });
            TabControlTestingResultItems.Add(tabControlTestingResultItem2);*/
        }

        //делегаты, которые содержат методы страниц или ViewModel страниц которые обновляют информацию или график на странице в соответствии с новым выбранным элементом (TestingResult, или DataSourceGroup, или TestBatch, или TestRun
        public delegate void UpdatePages();

        public static UpdatePages TestingResultUpdatePages; //методы, обновляющие страницы, отображающие информацию о тестировании в целом
        public static UpdatePages DataSourceGroupsUpdatePages; //методы, обновляющие страницы, отображающие информацию о тестовых связках в рамках определенного источника данных
        public static UpdatePages TestBatchesUpdatePages; //методы, обновляющие страницы, отображающие информацию о конкретной тестовой связке. Обновление 3d графика находится здесь
        public static UpdatePages TestRunsUpdatePages; //методы, обновляющие страницы, отображающие информацию о конкретном тестовом прогоне
    }
}
