using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    //класс описывает элемент меню для управления порядком следования областей с источниками данных
    class DataSourceOrderDisplayPageTradeChart : ViewModelBase
    {
        private DataSourceOrderDisplayPageTradeChart()
        {

        }
        public static DataSourceOrderDisplayPageTradeChart CreateDataSource(PropertyChangedAction propertyChangedAction, DataSourceAccordance dataSourceAccordance)
        {
            DataSourceOrderDisplayPageTradeChart dataSourceOrderDisplayPageTradeChart = new DataSourceOrderDisplayPageTradeChart();
            dataSourceOrderDisplayPageTradeChart.DataSourceAccordance = dataSourceAccordance;
            dataSourceOrderDisplayPageTradeChart.IsButtonUpChecked = false;
            dataSourceOrderDisplayPageTradeChart.IsButtonDownChecked = false;
            dataSourceOrderDisplayPageTradeChart.UpdatePropertyAction += propertyChangedAction;
            return dataSourceOrderDisplayPageTradeChart;
        }
        public delegate void PropertyChangedAction(DataSourceOrderDisplayPageTradeChart dataSourceOrderDisplayPageTradeChart, string propertyName);
        public PropertyChangedAction UpdatePropertyAction; //метод, обрабатывающий обновления в свойствах объекта
        private DataSourceAccordance _dataSourceAccordance;
        public DataSourceAccordance DataSourceAccordance //источник данных и макет источника данных, свечки которого отображает данная область
        {
            get { return _dataSourceAccordance; }
            set
            {
                _dataSourceAccordance = value;
                OnPropertyChanged();
            }
        }
        private bool _isButtonUpChecked;
        public bool IsButtonUpChecked //нажата ли кнопка вверх
        {
            get { return _isButtonUpChecked; }
            set
            {
                _isButtonUpChecked = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "IsButtonUpChecked"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
        private bool _isButtonDownChecked;
        public bool IsButtonDownChecked //нажата ли кнопка вниз
        {
            get { return _isButtonDownChecked; }
            set
            {
                _isButtonDownChecked = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "IsButtonDownChecked"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
    }
}
