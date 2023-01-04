using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ktradesystem.Models;
using System.Windows.Media;
using System.Windows;

namespace ktradesystem.ViewModels
{
    class ViewModelPageProfitChart : ViewModelBase
    {
        public ViewModelPageProfitChart()
        {
            ViewModelPageTestingResult.TestRunsUpdatePages += UpdatePage;
            _viewModelPageTradeChart = ViewModelPageTradeChart.getInstance();
        }

        private ViewModelPageTradeChart _viewModelPageTradeChart;
        private TestRun _testRun;
        //private SolidColorBrush _indicatorStrokeColor = new SolidColorBrush(Color.FromRgb(128, 128, 128)); //цвет линии индикаторов
        private SolidColorBrush _indicatorStrokeColor = new SolidColorBrush(Color.FromRgb(0, 162, 232)); //цвет линии индикаторов
        private SolidColorBrush _scaleValueStrokeLineColor = new SolidColorBrush(Color.FromRgb(230, 230, 230)); //цвет линии шкалы значения
        private SolidColorBrush _scaleValueTextColor = new SolidColorBrush(Color.FromRgb(80, 80, 80)); //цвет текста шкалы значения
        private SolidColorBrush _timeLineStrokeLineColor = new SolidColorBrush(Color.FromRgb(230, 230, 230)); //цвет линии шкалы времени
        private SolidColorBrush _timeLineTextColor = new SolidColorBrush(Color.FromRgb(40, 40, 40)); //цвет текста даты и времени
        private int _topMargin = 10; //отступ сверху и снизу
        private int _timeLineFontSize = 9; //размер шрифта даты и времени на шкале даты и времени
        private double _timeLineFullDateTimeLeft = 33; //отступ слева для полной даты и времени на шкале даты и времени
        private double _timeLineTimePixelsPerCut = 80; //количество пикселей на один отрезок на шкале даты и времени
        private int _scaleValueFontSize = 9; //размер шрифта цены на шкале значения
        private double _scaleValueTextTop = 6; //отступ сверху цены на шкале значения
        private int scaleValuesCount = 5; //количество отрезков на шкале значений
        private int _scaleValuesWidth = 46; //ширина левой области со шкалой значений
        private int _timeLineHeight = 20; //высота временной шкалы
        List<double> _dateRatesDepositStateChanges = new List<double>(); //список с значениями от 0 до 1, для элементов DepositStateChanges, где 0 - начало тестового прогона, а 1 - окончание, значение отражает положение даты изменения депозита в диапазоне от начала периода теста до окончания

        private double _canvasProfitChartWidth;
        public double СanvasProfitChartWidth //ширина canvas с графиком
        {
            get { return _canvasProfitChartWidth; }
            set
            {
                _canvasProfitChartWidth = value;
                OnPropertyChanged();
            }
        }
        private double _canvasProfitChartHeight;
        public double СanvasProfitChartHeight //высота canvas с графиком
        {
            get { return _canvasProfitChartHeight; }
            set
            {
                _canvasProfitChartHeight = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TimeLinePageTradeChart> _timeLinesPageProfitChart = new ObservableCollection<TimeLinePageTradeChart>();
        public ObservableCollection<TimeLinePageTradeChart> TimeLinesPageProfitChart //линии и значения даты и времени, которые будут отображатся на графике
        {
            get { return _timeLinesPageProfitChart; }
            set
            {
                _timeLinesPageProfitChart = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ScaleValuePageTradeChart> _scaleValuesPageProfitChart = new ObservableCollection<ScaleValuePageTradeChart>();
        public ObservableCollection<ScaleValuePageTradeChart> ScaleValuesPageProfitChart //значения шкал значений, которые будут отображатся на графике
        {
            get { return _scaleValuesPageProfitChart; }
            set
            {
                _scaleValuesPageProfitChart = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IndicatorPolylinePageTradeChart> _indicatorsPolylines = new ObservableCollection<IndicatorPolylinePageTradeChart>();
        public ObservableCollection<IndicatorPolylinePageTradeChart> IndicatorsPolylines //линии индикаторов, которые будут отображатся на графике
        {
            get { return _indicatorsPolylines; }
            set
            {
                _indicatorsPolylines = value;
                OnPropertyChanged();
            }
        }
        private void BuildProfitChart()
        {
            ScaleValuesPageProfitChart.Clear();
            IndicatorsPolylines.Clear();
            TimeLinesPageProfitChart.Clear();
            int availableHeight = (int)Math.Truncate(СanvasProfitChartHeight - _topMargin - _timeLineHeight);
            int availableChartWidth = (int)Math.Truncate(СanvasProfitChartWidth - _scaleValuesWidth);
            double minDeposit = 0;
            double maxDeposit = 0;
            //определяем минимальный и максимальный размер депозита
            for (int i = 0; i < _testRun.Account.DepositStateChanges.Count; i++)
            {
                if (i == 0)
                {
                    minDeposit = _testRun.Account.DepositStateChanges[i].Deposit;
                    maxDeposit = _testRun.Account.DepositStateChanges[i].Deposit;
                }
                else
                {
                    minDeposit = _testRun.Account.DepositStateChanges[i].Deposit < minDeposit ? _testRun.Account.DepositStateChanges[i].Deposit : minDeposit;
                    maxDeposit = _testRun.Account.DepositStateChanges[i].Deposit > maxDeposit ? _testRun.Account.DepositStateChanges[i].Deposit : maxDeposit;
                }
            }
            double depositRange = maxDeposit - minDeposit;

            //определяем количество знаков после запятой, до которых нужно округлять значение
            double permissibleError = 0.01; //допустимая погрешность, значение будет округляться не больше чем на данную часть от диапазона значений
            double permissibleErrorRange = depositRange * permissibleError;
            int digits = 0; //количество знаков после запятой, до которого нужно округлять значения шкалы значений
            while (permissibleErrorRange * Math.Pow(10, digits) < 1 && depositRange > 0)
            {
                digits++;
            }

            //добавляем шкалы значений
            double scaleValueCutPrice = depositRange / (scaleValuesCount - 1);
            for (int i = 0; i < scaleValuesCount; i++)
            {
                string priceText = Math.Round(minDeposit + scaleValueCutPrice * i, digits).ToString();
                double lineTop = _topMargin + availableHeight * (1 - (i / (double)(scaleValuesCount - 1)));
                ScaleValuesPageProfitChart.Add(new ScaleValuePageTradeChart { StrokeLineColor = _scaleValueStrokeLineColor, TextColor = _scaleValueTextColor, FontSize = _scaleValueFontSize, Price = priceText, PriceLeft = 3, PriceTop = lineTop - _scaleValueTextTop, LineLeft = _scaleValuesWidth, LineTop = 0, X1 = 0, Y1 = lineTop, X2 = availableChartWidth, Y2 = lineTop });
            }

            //добавляем линию графика и временную шкалу
            IndicatorPolylinePageTradeChart indicatorPolyline = new IndicatorPolylinePageTradeChart { StrokeColor = _indicatorStrokeColor, Left = 0, Points = new PointCollection() };
            double lastTimeLineLeft = -_timeLineFullDateTimeLeft; //отступ последнего элемента временной шкалы
            for (int i = 0; i < _testRun.Account.DepositStateChanges.Count; i++)
            {
                //добавляем линию графика
                double left = _scaleValuesWidth + _dateRatesDepositStateChanges[i] * availableChartWidth;
                indicatorPolyline.Points.Add(new Point(left, _topMargin + availableHeight * (1 - (_testRun.Account.DepositStateChanges[i].Deposit - minDeposit) / depositRange)));
                //добавляем линию таймлайна
                if(left - lastTimeLineLeft >= _timeLineTimePixelsPerCut) //если отступ от прошлой линии таймлайна, равен или больше требуемого, добавляем линию таймлайна
                {
                    lastTimeLineLeft = left;
                    TimeLinesPageProfitChart.Add(new TimeLinePageTradeChart { DateTime = _testRun.Account.DepositStateChanges[i].DateTime, StrokeLineColor = _timeLineStrokeLineColor, TextColor = _timeLineTextColor, FontSize = _timeLineFontSize, TextLeft = left - _timeLineFullDateTimeLeft, TextTop = _topMargin + availableHeight + 3, LineLeft = left, X1 = Math.Truncate(left), Y1 = 0, X2 = Math.Truncate(left), Y2 = _topMargin + availableHeight });
                }
            }
            IndicatorsPolylines.Add(indicatorPolyline);
        }
        public void UpdatePage()
        {
            if (ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null && ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox != null)
            {
                _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
                _dateRatesDepositStateChanges = _viewModelPageTradeChart.GetDateRatesDepositStateChanges();
                BuildProfitChart();
            }
        }
    }
}
