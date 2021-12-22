using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ktradesystem.Models
{
    class CommunicationChannel : ModelBase //класс реализует канал связи через который ViewModel получает сообщения от модели
    {
        private static CommunicationChannel _instance;

        public static CommunicationChannel getInstance()
        {
            if (_instance == null)
            {
                _instance = new CommunicationChannel();
            }
            return _instance;
        }

        private CommunicationChannel()
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
    }
}
