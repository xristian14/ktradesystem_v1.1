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
        public static TradeChartAreaPageTradeChart CreateDataSourceArea(DataSourceAccordance dataSourceAccordance)
        {
            TradeChartAreaPageTradeChart tradeChartAreaPageTradeChart = new TradeChartAreaPageTradeChart();
            tradeChartAreaPageTradeChart.Name = dataSourceAccordance.DataSourceTemplate.Name + "  –  " + dataSourceAccordance.DataSource.Name;
            tradeChartAreaPageTradeChart.IsDataSource = true;
            tradeChartAreaPageTradeChart.DataSourceAccordance = dataSourceAccordance;
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
