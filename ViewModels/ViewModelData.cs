using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
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

        private bool _isMainWindowEnabled = true;
        public bool IsMainWindowEnabled
        {
            get { return _isMainWindowEnabled; }
            set
            {
                _isMainWindowEnabled = value;
                OnPropertyChanged();
            }
        }
        
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var fieldViewModel = this.GetType().GetProperty(e.PropertyName);
            var fieldModel = sender.GetType().GetProperty(e.PropertyName);
            fieldViewModel?.SetValue(this, fieldModel.GetValue(sender));
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
                }, (obj) => true);
            }
        }

        public ICommand MenuTesting_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CurrentPage = _testing;
                }, (obj) => true);
            }
        }
    }
}
