using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTradeChart : ViewModelBase
    {
        public ViewModelPageTradeChart()
        {
            ViewModelPageTestingResult.TestRunsUpdatePages += UpdatePage;
            Candles.Clear();
            double bodyWidth = 4;
            for(int i = 0; i < 20; i++)
            {
                Candles.Add(new CandlePageTradeChart { BodyLeft = i * bodyWidth + i, BodyTop = 10, BodyHeight = 20, BodyWidth = bodyWidth, StickLeft = i * bodyWidth + i + Math.Truncate(bodyWidth / 2), StickTop = 4, StickHeight = 30, StickWidth = 1 });
            }
        }
        private Testing _testing; //результат тестирования
        private TestRun _testRun; //тестовый прогон, для которого строится график
        private int _candlesMaxWidth = 11; //максимальная ширина свечки, в пикселях
        private int _tradeChartScale; //масштаб графика, сколько свечек должно уместиться в видимую область графика
        private ObservableCollection<TradeChartAreaPageTradeChart> _tradeChartAreas = new ObservableCollection<TradeChartAreaPageTradeChart>(); //области графика с названием и указанной высотой области, первый элемент - главная область со свечками, следующие - области для индикаторов
        private int _timeLineHeight = 24; //высота временной шкалы
        public double СanvasChartWidth { get; set; } //ширина canvas с графиком
        public double СanvasChartHeight { get; set; } //высота canvas с графиком

        private ObservableCollection<CandlePageTradeChart> _candles = new ObservableCollection<CandlePageTradeChart>();
        public ObservableCollection<CandlePageTradeChart> Candles
        {
            get { return _candles; }
            set
            {
                _candles = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IndicatorMenuItemPageTradeChart> _indicatorsMenuItemPageTradeChart = new ObservableCollection<IndicatorMenuItemPageTradeChart>();
        public ObservableCollection<IndicatorMenuItemPageTradeChart> IndicatorsMenuItemPageTradeChart
        {
            get { return _indicatorsMenuItemPageTradeChart; }
            set
            {
                _indicatorsMenuItemPageTradeChart = value;
                OnPropertyChanged();
            }
        }
        public void IndicatorsMenuItemPageTradeChart_PropertyChanged(IndicatorMenuItemPageTradeChart indicatorMenuItemPageTradeChart, string propertyName) //обработчик изменения свойств у объектов в IndicatorsMenuItemPageTradeChart
        {
            if (propertyName == "SelectedTradeChartArea") //была выбрана другая область
            {

            }
            if (propertyName == "IsButtonAddAreaChecked") //была нажата кнопка поместить в новую область
            {

            }
        }

        private void CreateIndicatorsMenuItemPageTradeChart() //создает элементы для меню выбора областей для индикаторов, на основе выбранного результата тестирования
        {
            for(int i = 0; i < _testing.Algorithm.AlgorithmIndicators.Count; i++)
            {
                IndicatorsMenuItemPageTradeChart.Add(IndicatorMenuItemPageTradeChart.CreateIndicator(IndicatorsMenuItemPageTradeChart_PropertyChanged, _testing.Algorithm.AlgorithmIndicators[i], _tradeChartAreas, 0));
            }
        }

        private void CreateTradeChartAreas() //создает области для графика котировок
        {
            _tradeChartAreas.Clear();
            _tradeChartAreas.Add(new TradeChartAreaPageTradeChart { Name = "Главная область", AreaHeight = (int)Math.Truncate(СanvasChartHeight) - _timeLineHeight });
        }

        public void UpdatePage() //обновляет страницу на новый источник данных
        {
            _testing = ViewModelPageTestingResult.getInstance().TestingResult;
            _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
            CreateTradeChartAreas(); //создаем области для графика котировок
            CreateIndicatorsMenuItemPageTradeChart(); //создаем элементы для меню выбора областей для индикаторов
        }
        public ICommand Button1_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    foreach(CandlePageTradeChart candle in Candles)
                    {
                        candle.BodyLeft -= 1;
                        candle.StickLeft -= 1;
                    }
                }, (obj) => true);
            }
        }
    }
}
