using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class DataSourceGroupView : ViewModelBase //выбранные источники данных для макетов источников данных
    {
        public DataSourceGroupView(int number, List<DataSourceAccordanceView> dataSourcesAccordances, DateTime availableStartPeriodTesting, DateTime availableEndPeriodTesting)
        {
            _number = number;
            DataSourcesAccordances = dataSourcesAccordances;
            _availableStartPeriodTesting = availableStartPeriodTesting;
            _availableEndPeriodTesting = availableEndPeriodTesting;
            _startPeriodTesting = availableStartPeriodTesting;
            _endPeriodTesting = availableEndPeriodTesting;
        }
        private int _number;
        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                OnPropertyChanged();
            }
        }
        public List<DataSourceAccordanceView> DataSourcesAccordances { get; set; } //соответствие шаблонов источников данных и источников данных
        private DateTime _startPeriodTesting;
        public DateTime StartPeriodTesting //начало перида тестирования
        {
            get { return _startPeriodTesting; }
            set
            {
                if(DateTime.Compare(value, _endPeriodTesting) < 0)
                {
                    _startPeriodTesting = value;
                }
                OnPropertyChanged();
            }
        }
        private DateTime _endPeriodTesting;
        public DateTime EndPeriodTesting //окончание перида тестирования
        {
            get { return _endPeriodTesting; }
            set
            {
                if (DateTime.Compare(value, _startPeriodTesting) > 0)
                {
                    _endPeriodTesting = value;
                }
                OnPropertyChanged();
            }
        }
        private DateTime _availableStartPeriodTesting;
        public DateTime AvailableStartPeriodTesting //доступная дата для начала периода тестирования
        {
            get { return _availableStartPeriodTesting; }
            set
            {
                _availableStartPeriodTesting = value;
                OnPropertyChanged();
            }
        }
        private DateTime _availableEndPeriodTesting;
        public DateTime AvailableEndPeriodTesting //доступная дата для окончания периода тестирования
        {
            get { return _availableEndPeriodTesting; }
            set
            {
                _availableEndPeriodTesting = value;
                OnPropertyChanged();
            }
        }
    }
}
