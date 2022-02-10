using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ktradesystem.Models;

namespace ktradesystem.CommunicationChannel
{
    class MainCommunicationChannel : ModelBase //класс реализует канал связи через который ViewModel получает сообщения от модели
    {
        private static MainCommunicationChannel _instance;

        public static MainCommunicationChannel getInstance()
        {
            if (_instance == null)
            {
                _instance = new MainCommunicationChannel();
            }
            return _instance;
        }

        private MainCommunicationChannel()
        {

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

        public void AddMainMessage(string msg)
        {
            DateTime time;
            time = DateTime.Now;
            string hour = time.Hour.ToString().Length == 2 ? time.Hour.ToString() : "0" + time.Hour;
            string minute = time.Minute.ToString().Length == 2 ? time.Minute.ToString() : "0" + time.Minute;
            string second = time.Second.ToString().Length == 2 ? time.Second.ToString() : "0" + time.Second;
            string timeStr = hour + ":" + minute + ":" + second;

            int number = MainMessages.Count + 1;
            Message message = new Message() { Number = number, Time = timeStr, Text = msg };
            MainMessages.Add(message);
        }

        private ObservableCollection<DataSourceAddingProgress> _dataSourceAddingProgress = new ObservableCollection<DataSourceAddingProgress>(); //прогресс выполнения операции добавления источника дынных
        public ObservableCollection<DataSourceAddingProgress> DataSourceAddingProgress
        {
            get { return _dataSourceAddingProgress; }
            private set
            {
                _dataSourceAddingProgress = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TestingProgress> _testingProgress = new ObservableCollection<TestingProgress>(); //прогресс выполнения тестирования
        public ObservableCollection<TestingProgress> TestingProgress
        {
            get { return _testingProgress; }
            private set
            {
                _testingProgress = value;
                OnPropertyChanged();
            }
        }
    }
}
