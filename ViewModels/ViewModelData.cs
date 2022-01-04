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

            _mainCommunicationChannel = MainCommunicationChannel.getInstance();
            _mainCommunicationChannel.PropertyChanged += Model_PropertyChanged;
            _mainCommunicationChannel.MainMessages.CollectionChanged += MainCommunicationChannel_MainMessagesCollectionChanged;
            _mainCommunicationChannel.DataSourceAddingProgress.CollectionChanged += MainCommunicationChannel_DataSourceAddingProgressCollectionChanged;

            _dataSource = new Views.Pages.PageDataSource();
            _testing = new Views.Pages.PageTesting();
            CurrentPage = _dataSource;
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
                CreateStatusBarDataSourceContent();
            }
        }

        private void MainCommunicationChannel_DataSourceAddingProgressCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DataSourceAddingProgress = (ObservableCollection<DataSourceAddingProgress>)sender;
        }

        private Visibility _statusBarDataSourceVisibility = Visibility.Visible;
        public Visibility StatusBarDataSourceVisibility //видимость элементов строки состояния для источников данных
        {
            get { return _statusBarDataSourceVisibility; }
            private set
            {
                _statusBarDataSourceVisibility = value;
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
                    _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                    //очищаем поля
                    StatusBarDataSourceDoneText = "";
                    StatusBarDataSourceRemainingTime = "";
                    StatusBarDataSourceProgressMaxValue = 0;
                    StatusBarDataSourceProgressValue = 0;
                }
                else
                {
                    //обновляем значения полей statusBarDataSource
                    StatusBarDataSourceDoneText = DataSourceAddingProgress[0].CompletedTasksCount.ToString() + "/" + DataSourceAddingProgress[0].TasksCount.ToString();

                    int totalRemainingSeconds = (int)((DataSourceAddingProgress[0].ElapsedTime.TotalSeconds / (DataSourceAddingProgress[0].CompletedTasksCount / DataSourceAddingProgress[0].TasksCount)) - DataSourceAddingProgress[0].ElapsedTime.TotalSeconds); //делим пройденное время на завершенную часть от целого и получаем общее время, необходимое для выполнения всей работы, и вычитаем из него пройденное время
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
                }
            }
        }

        public ICommand StatusBarDataSourceCancel_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    MessageBox.Show(Environment.ProcessorCount.ToString());
                }, (obj) => true);
            }
        }

        #endregion

        private Visibility _statusBarTesting = Visibility.Collapsed;
        public Visibility StatusBarTesting //видимость элементов строки состояния для тестирования
        {
            get { return _statusBarTesting; }
            private set
            {
                _statusBarTesting = value;
                OnPropertyChanged();
            }
        }

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
