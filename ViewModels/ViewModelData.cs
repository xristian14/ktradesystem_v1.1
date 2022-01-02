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

namespace ktradesystem.ViewModels
{
    class ViewmodelData : ViewModelBase
    {
        private static ViewmodelData _instance;

        private ViewmodelData()
        {
            _modelDataSource = ModelDataSource.getInstance();
            _modelDataSource.PropertyChanged += Model_PropertyChanged;

            _communicationChannel = CommunicationChannel.getInstance();
            _communicationChannel.PropertyChanged += Model_PropertyChanged;
            _communicationChannel.MainMessages.CollectionChanged += CommunicationChannel_MainMessagesCollectionChanged;

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

        private CommunicationChannel _communicationChannel;
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

        private string _statusBarDataSourceDonePercent;
        public string StatusBarDataSourceDonePercent //на сколько процентов завершено
        {
            get { return _statusBarDataSourceDonePercent; }
            private set
            {
                _statusBarDataSourceDonePercent = value;
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

        private int _statusBarDataSourceProgressMinValue;
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

        private void CommunicationChannel_MainMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
