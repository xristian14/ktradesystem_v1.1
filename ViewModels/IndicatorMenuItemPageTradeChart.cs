using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.Collections.ObjectModel;

namespace ktradesystem.ViewModels
{
    //класс описывает элементы для управления выбором области для отображения индикаторов
    class IndicatorMenuItemPageTradeChart : ViewModelBase
    {
        private IndicatorMenuItemPageTradeChart()
        {

        }
        public static IndicatorMenuItemPageTradeChart CreateIndicator(PropertyChangedAction propertyChangedAction, AlgorithmIndicator algorithmIndicator, ObservableCollection<TradeChartAreaPageTradeChart> tradeChartAreas, int indexSelectedTradeChartArea)
        {
            IndicatorMenuItemPageTradeChart indicatorMenuItemPageTradeChart = new IndicatorMenuItemPageTradeChart();
            indicatorMenuItemPageTradeChart.AlgorithmIndicator = algorithmIndicator;
            indicatorMenuItemPageTradeChart.TradeChartAreas = tradeChartAreas;
            indicatorMenuItemPageTradeChart.SelectedTradeChartArea = tradeChartAreas[indexSelectedTradeChartArea];
            indicatorMenuItemPageTradeChart.IsButtonAddAreaChecked = false;
            indicatorMenuItemPageTradeChart.UpdatePropertyAction += propertyChangedAction;
            return indicatorMenuItemPageTradeChart;
        }
        public delegate void PropertyChangedAction(IndicatorMenuItemPageTradeChart indicatorMenuItemPageTradeChart, string propertyName);
        public PropertyChangedAction UpdatePropertyAction; //метод, обрабатывающий обновления в свойствах объекта
        private AlgorithmIndicator _algorithmIndicator;
        public AlgorithmIndicator AlgorithmIndicator //индикатор алгоритма
        {
            get { return _algorithmIndicator; }
            set
            {
                _algorithmIndicator = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<TradeChartAreaPageTradeChart> _tradeChartAreas;
        public ObservableCollection<TradeChartAreaPageTradeChart> TradeChartAreas //список с параметрами для combobox
        {
            get { return _tradeChartAreas; }
            set
            {
                _tradeChartAreas = value;
                OnPropertyChanged();
            }
        }
        private TradeChartAreaPageTradeChart _selectedTradeChartArea; //выбранный параметр в combobox
        public TradeChartAreaPageTradeChart SelectedTradeChartArea
        {
            get { return _selectedTradeChartArea; }
            set
            {
                _selectedTradeChartArea = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "SelectedTradeChartArea"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
        private bool _isButtonAddAreaChecked;
        public bool IsButtonAddAreaChecked //нажата ли кнопка поместить в новую область
        {
            get { return _isButtonAddAreaChecked; }
            set
            {
                _isButtonAddAreaChecked = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "IsButtonAddAreaChecked"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
    }
}
