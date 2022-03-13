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
            _modelTestingResult.TestingSavesForSubscribers.CollectionChanged += ModelTestingResult_TestingSavesCollectionChanged;

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
        private ObservableCollection<TestingResultHeader> _testingHistory = new ObservableCollection<TestingResultHeader>();
        public ObservableCollection<TestingResultHeader> TestingHistory //история результатов тестирования
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
            TestingHistory = (ObservableCollection<TestingResultHeader>)sender;
        }
        private TestingResultHeader _selectedTestingHistory;
        public TestingResultHeader SelectedTestingHistory //выбранный результат тестирования из истории
        {
            get { return _selectedTestingHistory; }
            set
            {
                _selectedTestingHistory = value;
                OnPropertyChanged();
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
        private ObservableCollection<TestingResultHeader> _testingSaves = new ObservableCollection<TestingResultHeader>();
        public ObservableCollection<TestingResultHeader> TestingSaves //сохраненные результаты тестирования
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
            TestingSaves = (ObservableCollection<TestingResultHeader>)sender;
        }
        private TestingResultHeader _selectedTestingSaves;
        public TestingResultHeader SelectedTestingSaves //выбранный сохраненный результат тестирования
        {
            get { return _selectedTestingSaves; }
            set
            {
                _selectedTestingSaves = value;
                OnPropertyChanged();
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
