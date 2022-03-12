using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ktradesystem.Models;
using ktradesystem.CommunicationChannel;

namespace ktradesystem.ViewModels
{
    class ViewmodelData : ViewModelBase
    {
        private static ViewmodelData _instance;

        private ViewmodelData()
        {
            _modelData = ModelData.getInstance();
            _modelDataSource = ModelDataSource.getInstance();
            _modelDataSource.PropertyChanged += Model_PropertyChanged;
            _modelTesting = ModelTesting.getInstance();
            _modelTestingResult = ModelTestingResult.getInstance();

            _mainCommunicationChannel = MainCommunicationChannel.getInstance();
            _mainCommunicationChannel.PropertyChanged += Model_PropertyChanged;
            _mainCommunicationChannel.MainMessages.CollectionChanged += MainCommunicationChannel_MainMessagesCollectionChanged;
            _mainCommunicationChannel.DataSourceAddingProgress.CollectionChanged += MainCommunicationChannel_DataSourceAddingProgressCollectionChanged;
            _mainCommunicationChannel.TestingProgress.CollectionChanged += MainCommunicationChannel_TestingProgressCollectionChanged;

            _dataSource = new Views.Pages.PageDataSource();
            _testing = new Views.Pages.PageTestingNavigation();
            CurrentPage = _testing;
        }

        public static ViewmodelData getInstance()
        {
            if (_instance == null)
            {
                _instance = new ViewmodelData();
            }
            return _instance;
        }

        private ModelData _modelData;
        private MainCommunicationChannel _mainCommunicationChannel;
        private ModelDataSource _modelDataSource;
        private ModelTesting _modelTesting;
        private ModelTestingResult _modelTestingResult;

        private Page _dataSource;
        private Page _testing;

        private Page _currentPage;
        public Page CurrentPage
        {
            get { return _currentPage; }
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        private bool _isPagesAndMainMenuButtonsEnabled = true;
        public bool IsPagesAndMainMenuButtonsEnabled
        {
            get { return _isPagesAndMainMenuButtonsEnabled; }
            set
            {
                _isPagesAndMainMenuButtonsEnabled = value;
                OnPropertyChanged();
            }
        }
        
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var fieldViewModel = this.GetType().GetProperty(e.PropertyName);
            var fieldModel = sender.GetType().GetProperty(e.PropertyName);
            fieldViewModel?.SetValue(this, fieldModel.GetValue(sender));
        }

        private int _statusBarHeight = 0;
        public int StatusBarHeight //высота строки с строкой состояния
        {
            get { return _statusBarHeight; }
            set
            {
                _statusBarHeight = value;
                OnPropertyChanged();
            }
        }






        #region statusBarDataSource
        public void StatusBarDataSourceShow()
        {
            StatusBarHeight = 25;
            StatusBarDataSourceVisibility = Visibility.Visible;
        }

        public void StatusBarDataSourceHide()
        {
            StatusBarHeight = 0;
            StatusBarDataSourceVisibility = Visibility.Collapsed;
        }

        private ObservableCollection<DataSourceAddingProgress> _dataSourceAddingProgress = new ObservableCollection<DataSourceAddingProgress>(); //прогресс выполнения операции добавления источника дынных
        public ObservableCollection<DataSourceAddingProgress> DataSourceAddingProgress
        {
            get { return _dataSourceAddingProgress; }
            private set
            {
                _dataSourceAddingProgress = value;
                OnPropertyChanged();
                DispatcherInvoke((Action)(() =>
                {
                    CreateStatusBarDataSourceContent();
                }));
                //CreateStatusBarDataSourceContent();
            }
        }

        private void MainCommunicationChannel_DataSourceAddingProgressCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DataSourceAddingProgress = (ObservableCollection<DataSourceAddingProgress>)sender;
        }

        private Visibility _statusBarDataSourceVisibility = Visibility.Collapsed;
        public Visibility StatusBarDataSourceVisibility //видимость элементов строки состояния для источников данных
        {
            get { return _statusBarDataSourceVisibility; }
            private set
            {
                _statusBarDataSourceVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _statusBarDataSourceHeader;
        public string StatusBarDataSourceHeader //заголовок выполняемого действия
        {
            get { return _statusBarDataSourceHeader; }
            private set
            {
                _statusBarDataSourceHeader = value;
                OnPropertyChanged();
            }
        }

        private string _statusBarDataSourceDoneText;
        public string StatusBarDataSourceDoneText //на сколько выполнено, в формате 1/20
        {
            get { return _statusBarDataSourceDoneText; }
            private set
            {
                _statusBarDataSourceDoneText = value;
                OnPropertyChanged();
            }
        }

        private string _statusBarDataSourceRemainingTime;
        public string StatusBarDataSourceRemainingTime //оставшееся время
        {
            get { return _statusBarDataSourceRemainingTime; }
            private set
            {
                _statusBarDataSourceRemainingTime = value;
                OnPropertyChanged();
            }
        }

        private int _statusBarDataSourceProgressMinValue = 0;
        public int StatusBarDataSourceProgressMinValue //минимальное значение для progress bar
        {
            get { return _statusBarDataSourceProgressMinValue; }
            private set
            {
                _statusBarDataSourceProgressMinValue = value;
                OnPropertyChanged();
            }
        }

        private int _statusBarDataSourceProgressMaxValue;
        public int StatusBarDataSourceProgressMaxValue //максимальное значение для progress bar
        {
            get { return _statusBarDataSourceProgressMaxValue; }
            private set
            {
                _statusBarDataSourceProgressMaxValue = value;
                OnPropertyChanged();
            }
        }

        private int _statusBarDataSourceProgressValue;
        public int StatusBarDataSourceProgressValue //значение для progress bar
        {
            get { return _statusBarDataSourceProgressValue; }
            private set
            {
                _statusBarDataSourceProgressValue = value;
                OnPropertyChanged();
            }
        }

        private bool _statusBarDataSourceCancelPossibility = true;
        public bool StatusBarDataSourceCancelPossibility //определяет возможность нажатия кнопки Отменить
        {
            get { return _statusBarDataSourceCancelPossibility; }
            private set
            {
                _statusBarDataSourceCancelPossibility = value;
                OnPropertyChanged();
            }
        }

        private void CreateStatusBarDataSourceContent()
        {
            if(DataSourceAddingProgress.Count != 0)
            {
                if (DataSourceAddingProgress[0].IsFinish)
                {
                    //обновляем данные DataSourcesForSubscribers
                    _modelData.NotifyDataSourcesSubscribers();
                    //закрываем statusBarDataSource, делаем форму активной
                    StatusBarDataSourceHide();
                    IsPagesAndMainMenuButtonsEnabled = true;
                    _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                    //очищаем поля
                    StatusBarDataSourceHeader = "";
                    StatusBarDataSourceDoneText = "";
                    StatusBarDataSourceRemainingTime = "";
                    StatusBarDataSourceProgressMaxValue = 0;
                    StatusBarDataSourceProgressValue = 0;
                    StatusBarDataSourceCancelPossibility = true;
                }
                else
                {
                    //обновляем значения полей statusBarDataSource
                    StatusBarDataSourceHeader = DataSourceAddingProgress[0].Header;
                    StatusBarDataSourceDoneText = DataSourceAddingProgress[0].CompletedTasksCount.ToString() + "/" + DataSourceAddingProgress[0].TasksCount.ToString();
                    int totalRemainingSeconds = (int)((DataSourceAddingProgress[0].ElapsedTime.TotalSeconds / ((double)(DataSourceAddingProgress[0].CompletedTasksCount) / (double)(DataSourceAddingProgress[0].TasksCount))) - DataSourceAddingProgress[0].ElapsedTime.TotalSeconds); //делим пройденное время на завершенную часть от целого и получаем общее время, необходимое для выполнения всей работы, и вычитаем из него пройденное время
                    TimeSpan timeSpan = TimeSpan.FromSeconds(totalRemainingSeconds);
                    string timeRemaining = timeSpan.Hours.ToString();
                    if(timeRemaining.Length == 1)
                    {
                        timeRemaining = timeRemaining.Insert(0, "0");
                    }
                    timeRemaining += ":" + timeSpan.Minutes.ToString();
                    if (timeRemaining.Length == 4)
                    {
                        timeRemaining = timeRemaining.Insert(3, "0");
                    }
                    timeRemaining += ":" + timeSpan.Seconds.ToString();
                    if (timeRemaining.Length == 7)
                    {
                        timeRemaining = timeRemaining.Insert(6, "0");
                    }
                    if (timeSpan.Days > 0)
                    {
                        timeRemaining = timeRemaining.Insert(0, timeSpan.Days.ToString() + " дней ");
                    }

                    StatusBarDataSourceRemainingTime = timeRemaining;
                    StatusBarDataSourceProgressMaxValue = DataSourceAddingProgress[0].TasksCount;
                    StatusBarDataSourceProgressValue = DataSourceAddingProgress[0].CompletedTasksCount;
                    StatusBarDataSourceCancelPossibility = DataSourceAddingProgress[0].CancelPossibility;
                }
            }
        }

        public ICommand StatusBarDataSourceCancel_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _modelDataSource.CancellationTokenSourceDataSourceCancel();
                }, (obj) => true);
            }
        }

        #endregion




        #region statusBarTesting
        public void StatusBarTestingShow()
        {
            StatusBarHeight = 25;
            StatusBarTestingVisibility = Visibility.Visible;
        }

        public void StatusBarTestingHide()
        {
            StatusBarHeight = 0;
            StatusBarTestingVisibility = Visibility.Collapsed;
        }

        private ObservableCollection<TestingProgress> _testingProgress = new ObservableCollection<TestingProgress>(); //прогресс выполнения тестирования
        public ObservableCollection<TestingProgress> TestingProgress
        {
            get { return _testingProgress; }
            private set
            {
                _testingProgress = value;
                OnPropertyChanged();
                DispatcherInvoke((Action)(() =>
                {
                    CreateStatusBarTestingContent();
                }));
            }
        }

        private void MainCommunicationChannel_TestingProgressCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TestingProgress = (ObservableCollection<TestingProgress>)sender;
        }

        private Visibility _statusBarTestingVisibility = Visibility.Collapsed;
        public Visibility StatusBarTestingVisibility //видимость элементов строки состояния для тестирования
        {
            get { return _statusBarTestingVisibility; }
            private set
            {
                _statusBarTestingVisibility = value;
                OnPropertyChanged();
            }
        }

        private string _statusBarTestingStepDescription;
        public string StatusBarTestingStepDescription //шаг выполнения тестирования
        {
            get { return _statusBarTestingStepDescription; }
            private set
            {
                _statusBarTestingStepDescription = value;
                OnPropertyChanged();
            }
        }

        private string _statusBarTestingDoneText;
        public string StatusBarTestingDoneText //на сколько выполнено, в формате 1/20
        {
            get { return _statusBarTestingDoneText; }
            private set
            {
                _statusBarTestingDoneText = value;
                OnPropertyChanged();
            }
        }

        private string _statusBarTestingTotalTime;
        public string StatusBarTestingTotalTime //прошло времени с начала тестирования
        {
            get { return _statusBarTestingTotalTime; }
            private set
            {
                _statusBarTestingTotalTime = value;
                OnPropertyChanged();
            }
        }

        private string _statusBarTestingRemainingTime;
        public string StatusBarTestingRemainingTime //оставшееся время
        {
            get { return _statusBarTestingRemainingTime; }
            private set
            {
                _statusBarTestingRemainingTime = value;
                OnPropertyChanged();
            }
        }

        private int _statusBarTestingProgressMinValue = 0;
        public int StatusBarTestingProgressMinValue //минимальное значение для progress bar
        {
            get { return _statusBarTestingProgressMinValue; }
            private set
            {
                _statusBarTestingProgressMinValue = value;
                OnPropertyChanged();
            }
        }

        private int _statusBarTestingProgressMaxValue;
        public int StatusBarTestingProgressMaxValue //максимальное значение для progress bar
        {
            get { return _statusBarTestingProgressMaxValue; }
            private set
            {
                _statusBarTestingProgressMaxValue = value;
                OnPropertyChanged();
            }
        }

        private int _statusBarTestingProgressValue;
        public int StatusBarTestingProgressValue //значение для progress bar
        {
            get { return _statusBarTestingProgressValue; }
            private set
            {
                _statusBarTestingProgressValue = value;
                OnPropertyChanged();
            }
        }

        private bool _statusBarTestingCancelPossibility = true;
        public bool StatusBarTestingCancelPossibility //определяет возможность нажатия кнопки Отменить
        {
            get { return _statusBarTestingCancelPossibility; }
            private set
            {
                _statusBarTestingCancelPossibility = value;
                OnPropertyChanged();
            }
        }

        private void CreateStatusBarTestingContent()
        {
            if(TestingProgress.Count != 0)
            {
                if(TestingProgress[0].IsFinishSimulation && TestingProgress[0].IsSuccessSimulation) //если симуляция тестирования закончена успешно, переходим на запись результатов
                {
                    Task.Run(() => _modelTestingResult.WriteTestingResult(TestingProgress[0].Testing)); //вызываем метод записи результатов тестирования. Запускаем в отдельном потоке чтобы форма обновлялась
                }
                if (TestingProgress[0].IsFinish) //если тестирование завершено
                {
                    //иначе закрываем statusBarTesting, делаем форму активной
                    StatusBarTestingHide();
                    IsPagesAndMainMenuButtonsEnabled = true;
                    _mainCommunicationChannel.TestingProgress.Clear();
                    //очищаем поля
                    StatusBarTestingStepDescription = "";
                    StatusBarTestingDoneText = "";
                    StatusBarTestingTotalTime = "";
                    StatusBarTestingRemainingTime = "";
                    StatusBarTestingProgressMaxValue = 0;
                    StatusBarTestingProgressValue = 0;
                    StatusBarTestingCancelPossibility = true;
                }
                else
                {
                    //обновляем значения полей statusBarTesting
                    StatusBarTestingStepDescription = TestingProgress[0].StepDescription;
                    StatusBarTestingDoneText = TestingProgress[0].CompletedStepTasksCount.ToString() + "/" + TestingProgress[0].StepTasksCount.ToString();
                    int totalRemainingSeconds = (int)((TestingProgress[0].StepElapsedTime.TotalSeconds / ((double)(TestingProgress[0].CompletedStepTasksCount) / (double)(TestingProgress[0].StepTasksCount))) - TestingProgress[0].StepElapsedTime.TotalSeconds); //делим пройденное время на завершенную часть от целого и получаем общее время, необходимое для выполнения всей работы, и вычитаем из него пройденное время
                    TimeSpan timeSpan = TimeSpan.FromSeconds(totalRemainingSeconds);
                    string timeRemaining = timeSpan.Hours.ToString();
                    if(timeRemaining.Length == 1)
                    {
                        timeRemaining = timeRemaining.Insert(0, "0");
                    }
                    timeRemaining += ":" + timeSpan.Minutes.ToString();
                    if (timeRemaining.Length == 4)
                    {
                        timeRemaining = timeRemaining.Insert(3, "0");
                    }
                    timeRemaining += ":" + timeSpan.Seconds.ToString();
                    if (timeRemaining.Length == 7)
                    {
                        timeRemaining = timeRemaining.Insert(6, "0");
                    }
                    if (timeSpan.Days > 0)
                    {
                        timeRemaining = timeRemaining.Insert(0, timeSpan.Days.ToString() + " дней ");
                    }

                    string timeTotal = TestingProgress[0].TotalElapsedTime.Hours.ToString();
                    if(timeTotal.Length == 1)
                    {
                        timeTotal = timeTotal.Insert(0, "0");
                    }
                    timeTotal += ":" + TestingProgress[0].TotalElapsedTime.Minutes.ToString();
                    if (timeTotal.Length == 4)
                    {
                        timeTotal = timeTotal.Insert(3, "0");
                    }
                    timeTotal += ":" + TestingProgress[0].TotalElapsedTime.Seconds.ToString();
                    if (timeTotal.Length == 7)
                    {
                        timeTotal = timeTotal.Insert(6, "0");
                    }
                    if (TestingProgress[0].TotalElapsedTime.Days > 0)
                    {
                        timeTotal = timeTotal.Insert(0, TestingProgress[0].TotalElapsedTime.Days.ToString() + " дней ");
                    }

                    StatusBarTestingTotalTime = timeTotal;
                    StatusBarTestingRemainingTime = timeRemaining;
                    StatusBarTestingProgressMaxValue = TestingProgress[0].StepTasksCount;
                    StatusBarTestingProgressValue = TestingProgress[0].CompletedStepTasksCount;
                    StatusBarTestingCancelPossibility = TestingProgress[0].CancelPossibility;
                }
            }
        }

        public ICommand StatusBarTestingCancel_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _modelTesting.CancellationTokenTestingCancel();
                }, (obj) => true);
            }
        }

        #endregion

        private ObservableCollection<Message> _mainMessages = new ObservableCollection<Message>(); //сообщения которые будут выводиться пользователю
        public ObservableCollection<Message> MainMessages
        {
            get { return _mainMessages; }
            private set
            {
                _mainMessages = value;
                OnPropertyChanged();
            }
        }

        private void MainCommunicationChannel_MainMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MainMessages = (ObservableCollection<Message>)sender;
        }

        public ICommand MenuDataSource_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CurrentPage = _dataSource;
                }, (obj) => IsPagesAndMainMenuButtonsEnabled);
            }
        }

        public ICommand MenuTesting_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CurrentPage = _testing;
                }, (obj) => IsPagesAndMainMenuButtonsEnabled);
            }
        }

        public ICommand MenuSettings_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    //CurrentPage = _testing;
                }, (obj) => IsPagesAndMainMenuButtonsEnabled);
            }
        }
    }
}
