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

            SelectedResultTestingMenu = ResultTestingMenu[0]; //выбираем пункт меню История
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

        public void ResetTestingResult() //обнуляет объект тестирования и очищает поля
        {
            TestingResult = null;
        }

        private void LoadSelectedTestingResult() //загружает выбранный результат тестирования
        {
            ResetTestingResult(); //обнуляем объект тестирования и очищаем поля
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
                if(testing != null) //если тестирование успешно считано, записываем его и заносим в поля занчения
                {
                    TestingResult = testing;
                }
            }
        }

        private string _testText;
        public string TestText
        {
            get { return _testText; }
            private set
            {
                _testText = value;
                OnPropertyChanged();
            }
        }
        public ICommand ButtonTest_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {

                }, (obj) => true);
            }
        }
    }
}
