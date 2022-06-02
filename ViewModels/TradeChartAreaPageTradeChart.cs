using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class TradeChartAreaPageTradeChart : ViewModelBase
    {
        private TradeChartAreaPageTradeChart()
        {

        }
        public static TradeChartAreaPageTradeChart CreateDataSourceArea()
        {
            TradeChartAreaPageTradeChart tradeChartAreaPageTradeChart = new TradeChartAreaPageTradeChart();
            tradeChartAreaPageTradeChart.Name = "Главная область";
            tradeChartAreaPageTradeChart.IsDataSource = true;
            return tradeChartAreaPageTradeChart;
        }
        public static TradeChartAreaPageTradeChart CreateIndicatorArea(string name)
        {
            TradeChartAreaPageTradeChart tradeChartAreaPageTradeChart = new TradeChartAreaPageTradeChart();
            tradeChartAreaPageTradeChart.Name = name;
            tradeChartAreaPageTradeChart.IsDataSource = false;
            return tradeChartAreaPageTradeChart;
        }
        private string _name;
        public string Name //название области
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        private bool _isDataSource;
        public bool IsDataSource //отображает ли данная область свечки источника данных
        {
            get { return _isDataSource; }
            set
            {
                _isDataSource = value;
                OnPropertyChanged();
            }
        }
        private int _areaHeight;
        public int AreaHeight //высота области
        {
            get { return _areaHeight; }
            set
            {
                _areaHeight = value;
                OnPropertyChanged();
            }
        }
    }
}
