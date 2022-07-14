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
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTradeChart : ViewModelBase
    {
        private static ViewModelPageTradeChart _instance;

        private ViewModelPageTradeChart()
        {
            ViewModelPageTestingResult.TestRunsUpdatePages += UpdatePage;
            SetInitialCandleWidth(); //устанавливаем начальную ширину свечки
        }

        public static ViewModelPageTradeChart getInstance()
        {
            if (_instance == null)
            {
                _instance = new ViewModelPageTradeChart();
            }
            return _instance;
        }

        private Testing _testing; //результат тестирования
        private TestBatch _testBatch; //тестовая связка
        private TestRun _testRun; //тестовый прогон, для которого строится график
        private SolidColorBrush _candleStrokeColor = new SolidColorBrush(Color.FromRgb(40, 40, 40)); //цвет линии свечки
        private SolidColorBrush _risingCandleFillColor = new SolidColorBrush(Color.FromRgb(230, 230, 230)); //цвет заливки растущей свечки
        private SolidColorBrush _fallingCandleFillColor = new SolidColorBrush(Color.FromRgb(200, 200, 200)); //цвет заливки падающей свечки
        private SolidColorBrush _indicatorStrokeColor = new SolidColorBrush(Color.FromRgb(128, 128, 128)); //цвет линии индикаторов
        private SolidColorBrush _limitBuyOrderFillColor = new SolidColorBrush(Color.FromRgb(181, 230, 29)); //цвет заливки лимитной заявки на покупку
        private SolidColorBrush _limitSellOrderFillColor = new SolidColorBrush(Color.FromRgb(255, 174, 201)); //цвет заливки лимитной заявки на продажу
        private SolidColorBrush _marketBuyOrderFillColor = new SolidColorBrush(Color.FromRgb(0, 232, 163)); //цвет заливки рыночной заявки на покупку
        private SolidColorBrush _marketSellOrderFillColor = new SolidColorBrush(Color.FromRgb(255, 55, 190)); //цвет заливки рыночной заявки на продажу
        private SolidColorBrush _stopBuyOrderFillColor = new SolidColorBrush(Color.FromRgb(255, 222, 0)); //цвет заливки стоп заявки на покупку
        private SolidColorBrush _stopSellOrderFillColor = new SolidColorBrush(Color.FromRgb(255, 127, 39)); //цвет заливки стоп заявки на продажу
        private SolidColorBrush _buyDealStrokeColor = new SolidColorBrush(Color.FromRgb(0, 196, 0)); //цвет линии сделки на покупку
        private SolidColorBrush _buyDealFillColor = new SolidColorBrush(Color.FromRgb(0, 255, 0)); //цвет заливки сделки на покупку
        private SolidColorBrush _sellDealStrokeColor = new SolidColorBrush(Color.FromRgb(196, 0, 0)); //цвет линии сделки на продажу
        private SolidColorBrush _sellDealFillColor = new SolidColorBrush(Color.FromRgb(255, 0, 0)); //цвет заливки сделки на продажу
        private SolidColorBrush _scaleValueStrokeLineColor = new SolidColorBrush(Color.FromRgb(230, 230, 230)); //цвет линии шкалы значения
        private SolidColorBrush _scaleValueTextColor = new SolidColorBrush(Color.FromRgb(80, 80, 80)); //цвет текста шкалы значения
        private SolidColorBrush _timeLineStrokeLineColor = new SolidColorBrush(Color.FromRgb(230, 230, 230)); //цвет линии шкалы значения
        private SolidColorBrush _timeLineTextColor = new SolidColorBrush(Color.FromRgb(40, 40, 40)); //цвет текста даты и времени
        private bool _isLoadingTestResultComplete = false; //флаг того что результаты загружены
        private int _timeLineFontSize = 9; //размер шрифта даты и времени на шкале даты и времени
        private double _timeLineFullDateTimeLeft = 33; //отступ слева для полной даты и времени на шкале даты и времени
        private double _timeLineTimePixelsPerCut = 80; //количество пикселей на один отрезок на шкале даты и времени
        private int _dataSourceAreasHighlightHeight = 15; //высота линии, на которой написано название источника данных для которого следуют ниже области
        private int _candleMinWidth = 1; //минимальная ширина свечки, в пикселях
        private int _candleMaxWidth = 37; //максимальная ширина свечки, в пикселях
        private int _initialCandleWidth = 3; //начальная ширина свечки
        private double _partOfOnePriceStepHeightForOrderVerticalLine = 0.667; //часть от высоты одного пункта центы, исходя из которой будет вычитсляться высота вертикальной линии для заявки
        private int _divideWidth = 10; //ширина разрыва
        private double _scaleValuesAddingRange = 0.03; //дополнительный диапазон для шкалы значений в каждую из сторон
        private double _tradeChartHiddenSegmentsSize = 0.85; //размер генерируемой области с сегментами и слева от видимых свечек, относительно размера видимых свечек. Размер отдельно для левых и правых свечек
        private int _scaleValueFontSize = 9; //размер шрифта цены на шкале значения
        private double _scaleValueTextTop = 6; //отступ сверху цены на шкале значения
        private int[] _scaleValuesCuts = new int[4] { 1, 2, 3, 5 }; //отрезки для шкалы значений (отрезки по сколько пунктов будут. Будет браться значение, при котором количество пикселей на один отрезок будет ближе всего к _scaleValuePixelPerCut) Поиск подходящего размера будет продолжаться, исходя из помноженных на 10 значений массива, на 100, и т.д.
        private double _scaleValuePixelPerCut = 35; //количество пикселей для одного отрезка на шкале значений. К этому значению будет стремиться размер отрезка
        private double[] _indicatorAreasHeight = new double[3] { 0.15, 0.225, 0.3 }; //суммарная высота областей для индикаторов, как часть от доступной высоты под области источника данных, номер элемента соответствует количеству индикаторов и показывает суммарную высоту для них, если количество индикаторов больше, берется последний элемент
        private int _timeLineHeight = 20; //высота временной шкалы
        private List<SegmentPageTradeChart> _segments { get; set; } //сегменты из которых состоит график
        private int _startPeriodSegmentIndex; //индекс сегмента, на котором начинается тестовый период
        private int _endPeriodSegmentIndex; //индекс сегмента, на котором заканчивается тестовый период
        private int _segmentIndex; //текущий индекс сегмента
        private List<SectionPageTradeChart> _sections { get; set; } //секции для сегментов
        private List<SegmentOrderIndexPageTradeChart> _segmentOrders; //индексы сегмента и заявки, чтобы можно было быстро найти сегмент с заявкой
        private List<SegmentDealIndexPageTradeChart> _segmentDeals; //индексы сегмента и сделки, чтобы можно было быстро найти сегмент со сделкой

        private double _tradeChartMovePosition = 0; //значение, на которое нужно сдвинуть элементы графика
        private double _tradeChartMovedPosition = 0; //значение, на которое уже сдвинуты элементы графика
        private double _tradeChartScale = 0.0833; //масштаб графика, 0 - минимальный масштаб (_candleMinWidth), 1 - максимальный (_candleMaxWidth)
        private bool _isMouseDown = false; //зажата ли левая клавиша мыши
        private bool _isMoveTradeChart = true; //двигаем график или масштабируем
        private Point _mouseDownPosition; //позиция мыши при нажатии мыши
        private double _moveToMoveFactor = 1; //скольким значениям движения элементов графика соответствует 1 значение движения мыши
        private double _moveToScaleFactor = 0.005; //скольким значениям _tradeChartScale соответствует 1 значение движения мыши
        private double _mouseDownTradeChartMovePosition; //значение масштаба в момент нажатия левой клавиши мыши
        private double _mouseDownTradeChartScale; //значение масштаба в момент нажатия левой клавиши мыши

        private int _candleWidth;
        public int CandleWidth //текущая ширина свечки
        {
            get { return _candleWidth; }
            set
            {
                _candleWidth = value;
                OnPropertyChanged();
                if (_isLoadingTestResultComplete)
                {
                    BuildTradeChart(); //строим график котировок
                    UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
                }
            }
        }
        private List<int> _candleWidths = new List<int>() { 1, 2, 3, 4, 5, 7, 9, 11, 15, 21, 29, 37 };
        public List<int> CandleWidths //варианты ширины свечки
        {
            get { return _candleWidths; }
            set
            {
                _candleWidths = value;
                OnPropertyChanged();
            }
        }
        private Canvas _canvasTradeChart;
        public Canvas СanvasTradeChart //canvas с графиком
        {
            get { return _canvasTradeChart; }
            set
            {
                _canvasTradeChart = value;
                OnPropertyChanged();
            }
        }
        private double _canvasTradeChartWidth;
        public double СanvasTradeChartWidth //ширина canvas с графиком
        {
            get { return _canvasTradeChartWidth; }
            set
            {
                _canvasTradeChartWidth = value;
                OnPropertyChanged();
            }
        }
        private double _canvasTradeChartHeight;
        public double СanvasTradeChartHeight //высота canvas с графиком
        {
            get { return _canvasTradeChartHeight; }
            set
            {
                _canvasTradeChartHeight = value;
                OnPropertyChanged();
            }
        }
        private int _scaleValuesWidth = 45;
        public int ScaleValuesWidth //ширина правой области со шкалой значений
        {
            get { return _scaleValuesWidth; }
            set
            {
                _scaleValuesWidth = value;
                OnPropertyChanged();
            }
        }
        private double _areasWidth;
        public double AreasWidth //ширина областей для графиков и индикаторов
        {
            get { return _areasWidth; }
            set
            {
                _areasWidth = value;
                OnPropertyChanged();
            }
        }
        private bool _isHideOrders = false;
        public bool IsHideOrders //не показывать заявки
        {
            get { return _isHideOrders; }
            set
            {
                _isHideOrders = value;
                OrdersVisibility = value ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged();
                BuildTradeChart(); //строим график котировок
                UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
            }
        }
        private Visibility _ordersVisibility = Visibility.Visible;
        public Visibility OrdersVisibility //не показывать заявки
        {
            get { return _ordersVisibility; }
            set
            {
                _ordersVisibility = value;
                OnPropertyChanged();
            }
        }
        private bool _isHideDuplicateDate = false;
        public bool IsHideDuplicateDate //скрывать дублирующиеся даты
        {
            get { return _isHideDuplicateDate; }
            set
            {
                _isHideDuplicateDate = value;
                OnPropertyChanged();
                BuildTradeChart(); //строим график котировок
                UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
            }
        }

        private ObservableCollection<TimeLinePageTradeChart> _timeLinesPageTradeChart = new ObservableCollection<TimeLinePageTradeChart>();
        public ObservableCollection<TimeLinePageTradeChart> TimeLinesPageTradeChart //линии и значения даты и времени, которые будут отображатся на графике
        {
            get { return _timeLinesPageTradeChart; }
            set
            {
                _timeLinesPageTradeChart = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ScaleValuePageTradeChart> _scaleValuesPageTradeChart = new ObservableCollection<ScaleValuePageTradeChart>();
        public ObservableCollection<ScaleValuePageTradeChart> ScaleValuesPageTradeChart //значения шкал значений, которые будут отображатся на графике
        {
            get { return _scaleValuesPageTradeChart; }
            set
            {
                _scaleValuesPageTradeChart = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CandlePageTradeChart> _candles = new ObservableCollection<CandlePageTradeChart>();
        public ObservableCollection<CandlePageTradeChart> Candles //свечки, которые будут отображатся на графике
        {
            get { return _candles; }
            set
            {
                _candles = value;
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

        private ObservableCollection<OrderPageTradeChart> _orders = new ObservableCollection<OrderPageTradeChart>();
        public ObservableCollection<OrderPageTradeChart> Orders //заявки, которые будут отображатся на графике
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged();
            }

        }

        private ObservableCollection<DealPageTradeChart> _deals = new ObservableCollection<DealPageTradeChart>();
        public ObservableCollection<DealPageTradeChart> Deals //сделки, которые будут отображатся на графике
        {
            get { return _deals; }
            set
            {
                _deals = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TradeChartAreaPageTradeChart> _tradeChartAreas = new ObservableCollection<TradeChartAreaPageTradeChart>();
        public ObservableCollection<TradeChartAreaPageTradeChart> TradeChartAreas //области графика с названием, указанной высотой области а так же с указанием имеет ли та область источник данных, свечки которого будут на ней отображены
        {
            get { return _tradeChartAreas; }
            set
            {
                _tradeChartAreas = value;
                OnPropertyChanged();
            }
        }
        private void CreateTradeChartAreas() //создает области для графика котировок
        {
            TradeChartAreas.Clear();
            TradeChartAreas.Add(TradeChartAreaPageTradeChart.CreateDataSourceArea());
            UpdateTradeChartAreasHeight(); //обновляем высоту у областей графика котировок
        }

        private ObservableCollection<IndicatorMenuItemPageTradeChart> _indicatorsMenuItemPageTradeChart = new ObservableCollection<IndicatorMenuItemPageTradeChart>();
        public ObservableCollection<IndicatorMenuItemPageTradeChart> IndicatorsMenuItemPageTradeChart //элементы для управления выбором области отображения индикаторов
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
                //проверяем, если есть области которые не выбраны ни у одного индиктаора, кроме главной, удаляем их
                bool isDelete = false; //была ли удалена область
                for(int i = TradeChartAreas.Count - 1; i > 0; i--)
                {
                    if (IndicatorsMenuItemPageTradeChart.Where(j => j.SelectedTradeChartArea == TradeChartAreas[i]).Any() == false) //если данная область не выбрана ни у одного их индикаторов
                    {
                        TradeChartAreas.RemoveAt(i); //удаляем данную область
                        isDelete = true;
                    }
                }
                if (isDelete) //если были удалены области, обновляем названия у областей чтобы они назывались по порядку
                {
                    for(int i = 1; i < TradeChartAreas.Count; i++)
                    {
                        TradeChartAreas[i].Name = "#" + i.ToString();
                    }
                    UpdateTradeChartAreasHeight(); //обновляем высоту у областей графика котировок
                }
            }
            if (propertyName == "IsButtonAddAreaChecked") //была нажата кнопка поместить в новую область
            {
                if(indicatorMenuItemPageTradeChart.IsButtonAddAreaChecked)//если кнопка в состоянии true, - добавляем область и выбираем её для текущего индикатора
                {
                    indicatorMenuItemPageTradeChart.IsButtonAddAreaChecked = false;
                    TradeChartAreas.Add(TradeChartAreaPageTradeChart.CreateIndicatorArea("#" + (TradeChartAreas.Where(j => j.IsDataSource == false).Count() + 1).ToString())); //добавляем новую область
                    indicatorMenuItemPageTradeChart.SelectedTradeChartArea = TradeChartAreas.Last(); //выбираем добавленную область для текущего индикатора
                    UpdateTradeChartAreasHeight(); //обновляем высоту у областей графика котировок
                }
            }
        }
        private void UpdateTradeChartAreasHeight() //обновляет высоту у областей графика котировок
        {
            int dataSourceCount = _testing.DataSourcesCandles.Count; //количество источников данных
            int indicatorAreasCount = TradeChartAreas.Where(j => j.IsDataSource == false).Count(); //количество областей с индикаторами
            int dataSourceAreasAvailableHeight = (int)Math.Truncate((СanvasTradeChartHeight - _timeLineHeight) / dataSourceCount - _dataSourceAreasHighlightHeight); //доступная высота под области одного источника данных
            int indiactorAreaHeight = indicatorAreasCount > 0 ? (int)Math.Truncate((dataSourceAreasAvailableHeight * (indicatorAreasCount > _indicatorAreasHeight.Length ? _indicatorAreasHeight.Last() : _indicatorAreasHeight[indicatorAreasCount - 1])) / indicatorAreasCount) : 0; //высота для областей индикаторов
            int dataSourceAreaHeight = (int)Math.Truncate(dataSourceAreasAvailableHeight - indiactorAreaHeight * (double)indicatorAreasCount); //высота для областей с источниками данных
            foreach(TradeChartAreaPageTradeChart tradeChartAreaPageTradeChart in TradeChartAreas)
            {
                tradeChartAreaPageTradeChart.AreaHeight = tradeChartAreaPageTradeChart.IsDataSource ? dataSourceAreaHeight : indiactorAreaHeight;
            }
        }
        private void CreateIndicatorsMenuItemPageTradeChart() //создает элементы для меню выбора областей для индикаторов, на основе выбранного результата тестирования
        {
            IndicatorsMenuItemPageTradeChart.Clear();
            for (int i = 0; i < _testing.Algorithm.AlgorithmIndicators.Count; i++)
            {
                IndicatorsMenuItemPageTradeChart.Add(IndicatorMenuItemPageTradeChart.CreateIndicator(IndicatorsMenuItemPageTradeChart_PropertyChanged, _testing.Algorithm.AlgorithmIndicators[i], TradeChartAreas, 0));
            }
        }

        private ObservableCollection<DataSourceOrderDisplayPageTradeChart> _dataSourcesOrderDisplayPageTradeChart = new ObservableCollection<DataSourceOrderDisplayPageTradeChart>();
        public ObservableCollection<DataSourceOrderDisplayPageTradeChart> DataSourcesOrderDisplayPageTradeChart
        {
            get { return _dataSourcesOrderDisplayPageTradeChart; }
            set
            {
                _dataSourcesOrderDisplayPageTradeChart = value;
                OnPropertyChanged();
            }
        }
        public void DataSourcesOrderDisplayPageTradeChart_PropertyChanged(DataSourceOrderDisplayPageTradeChart dataSourceOrderDisplayPageTradeChart, string propertyName) //обработчик изменения свойств у объектов в DataSourcesOrderDisplayPageTradeChart
        {
            if (propertyName == "IsButtonUpChecked") //была нажата кнопка вверх
            {
                if (dataSourceOrderDisplayPageTradeChart.IsButtonUpChecked)
                {
                    dataSourceOrderDisplayPageTradeChart.IsButtonUpChecked = false;
                    int index = DataSourcesOrderDisplayPageTradeChart.IndexOf(dataSourceOrderDisplayPageTradeChart);
                    if (index > 0)
                    {
                        DataSourcesOrderDisplayPageTradeChart.Move(index, index - 1);
                        //UpdateTradeChartAreasOrder(); //обновляем порядок следования областей с источниками данных
                    }
                }
            }
            if (propertyName == "IsButtonDownChecked") //была нажата кнопка вниз
            {
                if (dataSourceOrderDisplayPageTradeChart.IsButtonDownChecked)
                {
                    dataSourceOrderDisplayPageTradeChart.IsButtonDownChecked = false;
                    int index = DataSourcesOrderDisplayPageTradeChart.IndexOf(dataSourceOrderDisplayPageTradeChart);
                    if (index < DataSourcesOrderDisplayPageTradeChart.Count - 1)
                    {
                        DataSourcesOrderDisplayPageTradeChart.Move(index, index + 1);
                        //UpdateTradeChartAreasOrder(); //обновляем порядок следования областей с источниками данных
                    }
                }
            }
        }
        private void CreateDataSourcesOrderDisplayPageTradeChart() //создает элементы для меню управления порядком следования областей с источниками данных
        {
            DataSourcesOrderDisplayPageTradeChart.Clear();
            foreach (DataSourceAccordance dataSourceAccordance in _testBatch.DataSourceGroup.DataSourceAccordances)
            {
                DataSourcesOrderDisplayPageTradeChart.Add(DataSourceOrderDisplayPageTradeChart.CreateDataSource(DataSourcesOrderDisplayPageTradeChart_PropertyChanged, dataSourceAccordance));
            }
        }

        public void MouseDown(Point position)
        {
            _isMouseDown = true;
            _mouseDownPosition = position; //запоминаем позицию мыши в момент нажатия клавиши мыши
            _tradeChartMovedPosition = 0; //сбрасываем величину, на которую сдвинут график
        }

        public void MouseMove(Point position)
        {
            if (_isMouseDown)
            {
                _tradeChartMovePosition = (position.X - _mouseDownPosition.X) * _moveToMoveFactor - _tradeChartMovedPosition;
                _tradeChartMovedPosition += _tradeChartMovePosition;
                MoveTradeChart();
            }
        }

        public void MouseUp()
        {
            _isMouseDown = false;
            _segmentIndex += (int)Math.Round(-_tradeChartMovedPosition / _candleWidth);
            _segmentIndex = _segmentIndex >= _segments.Count ? _segments.Count - 1 : _segmentIndex;
            if (IsHideDuplicateDate)
            {
                if(_segments[_segmentIndex].Section.IsPresent == false)
                {
                    int index = FindPresentSegmentIndex(_segmentIndex, true);
                    if(index > -1)
                    {
                        _segmentIndex = index;
                    }
                }
            }
            BuildTradeChart(); //строим график котировок
            UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
        }

        private void MoveTradeChart()
        {
            //двигаем таймлайн
            foreach(TimeLinePageTradeChart timeLinePageTradeChart in TimeLinesPageTradeChart)
            {
                timeLinePageTradeChart.TextLeft += _tradeChartMovePosition;
                timeLinePageTradeChart.LineLeft += _tradeChartMovePosition;
                timeLinePageTradeChart.X1 += _tradeChartMovePosition;
                timeLinePageTradeChart.X2 += _tradeChartMovePosition;
            }
            //двигаем свечки
            foreach(CandlePageTradeChart candlePageTradeChart in Candles)
            {
                candlePageTradeChart.BodyLeft += _tradeChartMovePosition;
                candlePageTradeChart.StickLeft += _tradeChartMovePosition;
            }
            //двигаем индикаторы
            foreach(IndicatorPolylinePageTradeChart indicatorPolylinePageTradeChart in IndicatorsPolylines)
            {
                PointCollection newPoints = new PointCollection();
                for (int i = 0; i < indicatorPolylinePageTradeChart.Points.Count; i++)
                {
                    newPoints.Add(new Point(indicatorPolylinePageTradeChart.Points[i].X + _tradeChartMovePosition, indicatorPolylinePageTradeChart.Points[i].Y));
                }
                indicatorPolylinePageTradeChart.Points = newPoints;
            }
            //двигаем заявки
            foreach(OrderPageTradeChart orderPageTradeChart in Orders)
            {
                orderPageTradeChart.HorizontalLineLeft += _tradeChartMovePosition;
                orderPageTradeChart.VerticalLineLeft += _tradeChartMovePosition;
            }
            //двигаем сделки
            foreach (DealPageTradeChart dealPageTradeChart in Deals)
            {
                dealPageTradeChart.Left += _tradeChartMovePosition;
                for (int i = 0; i < dealPageTradeChart.Points.Count; i++)
                {
                    dealPageTradeChart.Points[i] = new Point(dealPageTradeChart.Points[i].X + _tradeChartMovePosition, dealPageTradeChart.Points[i].Y);
                }
            }
            UpdateScaleValues();
        }

        private void CreateSegments() //создает сегменты, на основе которых будет строиться график
        {
            //формируем сегменты для всех групп источников данных
            int[] fileIndexes = Enumerable.Repeat(0, _testing.DataSourcesCandles.Count).ToArray(); //индексы файлов для всех источников данных группы
            int[] candleIndexes = Enumerable.Repeat(0, _testing.DataSourcesCandles.Count).ToArray(); //индексы свечек для всех источников данных группы
            _sections = new List<SectionPageTradeChart>();
            _segments = new List<SegmentPageTradeChart>();
            _segmentOrders = new List<SegmentOrderIndexPageTradeChart>();
            _segmentDeals = new List<SegmentDealIndexPageTradeChart>();
            _startPeriodSegmentIndex = -1;
            _endPeriodSegmentIndex = -1;
            List<DataSource> endedDataSources = new List<DataSource>(); //источники данных, которые закончились (индекс файла вышел за границы массива)
            DateTime currentDateTime = new DateTime(); //текущая дата
            //определяем самую раннюю дату среди всех источников данных группы
            for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
            {
                if (i == 0)
                {
                    currentDateTime = _testing.DataSourcesCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                }
                else
                {
                    DateTime dateTime = _testing.DataSourcesCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                    if (DateTime.Compare(dateTime, currentDateTime) < 0) //если дата свечки у текущего источника данных раньше текущей даты, обновляем текущую дату
                    {
                        currentDateTime = dateTime;
                    }
                }
            }
            DateTime laterDateTime = currentDateTime; //самая поздняя дата и время, используется для определения дат которые уже были

            bool isAllDataSourcesEnd = false; //закончились ли все источники данных
            SectionPageTradeChart section = new SectionPageTradeChart(); //первая секция
            section.IsPresent = true;
            section.DataSources = new List<DataSource>();
            for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
            {
                section.DataSources.Add(_testing.DataSourcesCandles[i].DataSource);
            }
            _sections.Add(section);
            while (isAllDataSourcesEnd == false)
            {
                int[] sectionDataSourceCountSegments = Enumerable.Repeat(0, _sections.Last().DataSources.Count).ToArray(); //количество сегментов для источников данных в секции. Значение в sectionDataSourceCountSegments[i] соответствует количеству сегментов с источником данных: sections.Last().DataSources[i]
                bool isNewSection = false; //перешли ли на новую секцию
                while (isNewSection == false)
                {
                    //определяем текущую дату (самую раннюю дату среди всех источников данных секции)
                    bool isFirstIteration = true;
                    for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
                    {
                        if (_sections.Last().DataSources.Where(a => a.Id == _testing.DataSourcesCandles[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                        {
                            if (isFirstIteration)
                            {
                                currentDateTime = _testing.DataSourcesCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                                isFirstIteration = false;
                            }
                            else
                            {
                                DateTime dateTime = _testing.DataSourcesCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                                if (DateTime.Compare(dateTime, currentDateTime) < 0) //если дата свечки у текущего источника данных раньше текущей даты, обновляем текущую дату
                                {
                                    currentDateTime = dateTime;
                                }
                            }
                        }
                        
                    }
                    if (DateTime.Compare(currentDateTime, laterDateTime) > 0) //если текущая дата позже самой поздней, обновляем самую позднюю дату
                    {
                        laterDateTime = currentDateTime;
                    }
                    //формируем сегмент
                    SegmentPageTradeChart segment = new SegmentPageTradeChart();
                    segment.Section = _sections.Last();
                    segment.IsDivide = false;
                    segment.CandleIndexes = new List<CandleIndexPageTradeChart>();
                    bool isDivide = false; //нужно ли добавить сегмент с разрывом, если будет создана новая секция
                    //проходим по источникам данных секции, и добавляем в сегмент свечки тех которые имеют текущую дату
                    for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
                    {
                        if (_sections.Last().DataSources.Where(a => a.Id == _testing.DataSourcesCandles[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                        {
                            if (DateTime.Compare(currentDateTime, _testing.DataSourcesCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime) == 0) //если текущая дата и дата текущей свечки у текущего источника данных равны
                            {
                                segment.CandleIndexes.Add(new CandleIndexPageTradeChart { DataSourceCandlesIndex = i, FileIndex = fileIndexes[i], CandleIndex = candleIndexes[i], OrderIndexes = new List<OrderIndexPageTradeChart>(), DealIndexes = new List<int>() });
                                int sectionDataSourceIndex = _sections.Last().DataSources.FindIndex(a => a.Id == _testing.DataSourcesCandles[i].DataSource.Id);
                                sectionDataSourceCountSegments[sectionDataSourceIndex]++; //увеличиваем количество свечек с данным источником данных
                                //переходим на следующую свечку у данного источника данных
                                candleIndexes[i]++;
                                if (candleIndexes[i] >= _testing.DataSourcesCandles[i].Candles[fileIndexes[i]].Length)
                                {
                                    candleIndexes[i] = 0;
                                    fileIndexes[i]++;
                                    if (fileIndexes[i] >= _testing.DataSourcesCandles[i].Candles.Length) //если вышли за предел файла, запоминаем источник данных, для которого закончились файлы
                                    {
                                        endedDataSources.Add(_testing.DataSourcesCandles[i].DataSource); //запоминаем источник данных, для которого закончились файлы, в последствии при создании новой секции, этот источник данных не будет включен в секцию
                                        isNewSection = true; //отмечаем, что нужно создать новую секцию, т.к. при текущей секции будут обращения к несуществующему файлу
                                        isDivide = true;
                                    }
                                }
                            }
                        }
                    }
                    _segments.Add(segment);
                    if(_startPeriodSegmentIndex == -1) //если не был присвоен индекс сегмента на котором начинается период тестового прогона
                    {
                        if(DateTime.Compare(_testing.DataSourcesCandles[segment.CandleIndexes[0].DataSourceCandlesIndex].Candles[segment.CandleIndexes[0].FileIndex][segment.CandleIndexes[0].CandleIndex].DateTime, _testRun.StartPeriod) >= 0) //если дата текущего сегмента равна или позже даты начала периода тестового прогона, сохраняем индекс сегмента
                        {
                            _startPeriodSegmentIndex = _segments.Count - 1;
                        }
                    }
                    if (_endPeriodSegmentIndex == -1) //если не был присвоен индекс сегмента на котором заканчивается период тестового прогона
                    {
                        if (DateTime.Compare(_testing.DataSourcesCandles[segment.CandleIndexes[0].DataSourceCandlesIndex].Candles[segment.CandleIndexes[0].FileIndex][segment.CandleIndexes[0].CandleIndex].DateTime, _testRun.EndPeriod) >= 0) //если дата текущего сегмента равна или позже даты начала периода тестового прогона, сохраняем индекс сегмента
                        {
                            _endPeriodSegmentIndex = _segments.Count - 1;
                        }
                    }
                    if (isNewSection == false) //если не было добавлено новой секции по причине окончания одного из источников данных, проверяем, не закончилась ли секция по причине выхода на дату которая не позже самой поздней или которая позже самой поздней
                    {
                        if (_sections.Last().IsPresent) //если секция в настоящем, условием для создания новой секции является переход одной из свечек на дату которая равна или раньше самой поздней
                        {
                            bool isAllCandlesLater = true; //все ли свечки позднее самой поздней даты
                            for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
                            {
                                if (_sections.Last().DataSources.Where(a => a.Id == _testing.DataSourcesCandles[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                                {
                                    if (DateTime.Compare(_testing.DataSourcesCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, laterDateTime) <= 0) //если дата свечки раньше или равняется самой последней дате
                                    {
                                        isAllCandlesLater = false;
                                    }
                                }
                            }
                            if (isAllCandlesLater == false) //если хоть одна из свечек не была позднее
                            {
                                isNewSection = true;
                            }
                        }
                        else //если даты секции уже были, значит условием перехода на следующую секцию является переход на дату, которая позже самой поздней
                        {
                            bool isAllCandlesLater = true; //все ли свечки позднее самой поздней даты
                            for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
                            {
                                if (_sections.Last().DataSources.Where(a => a.Id == _testing.DataSourcesCandles[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                                {
                                    if (DateTime.Compare(_testing.DataSourcesCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, laterDateTime) <= 0) //если дата свечки раньше или равняется самой последней дате
                                    {
                                        isAllCandlesLater = false;
                                    }
                                }
                            }
                            if (isAllCandlesLater) //если все свечки были позднее
                            {
                                isNewSection = true;
                            }
                        }
                    }

                    if (isNewSection) //если нужно добавить новую секцию, добавляем её
                    {
                        //удаляем из текущей секции источники данных, свечки которых не были добавлены в сегменты секции
                        for (int i = sectionDataSourceCountSegments.Length - 1; i >= 0; i--)
                        {
                            if (sectionDataSourceCountSegments[i] == 0) //если не было добавлено свечек с данным источником данных, удаляем его из секции
                            {
                                _sections.Last().DataSources.RemoveAt(i);
                            }
                        }

                        //добавляем новую секцию
                        SectionPageTradeChart newSection = new SectionPageTradeChart(); //новая секция
                        newSection.DataSources = new List<DataSource>();
                        for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
                        {
                            if (endedDataSources.Where(a => a.Id == _testing.DataSourcesCandles[i].DataSource.Id).Any() == false) //если данного источника данных нет в списке закончившихся источников данных
                            {
                                newSection.DataSources.Add(_testing.DataSourcesCandles[i].DataSource);
                            }
                        }
                        //определяем, секция в настоящем или прошлом
                        bool isAllCandlesLater = true; //все ли свечки позднее самой поздней даты
                        for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
                        {
                            if (newSection.DataSources.Where(a => a.Id == _testing.DataSourcesCandles[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                            {
                                if (DateTime.Compare(_testing.DataSourcesCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, laterDateTime) <= 0) //если дата свечки раньше или равняется самой последней дате
                                {
                                    isAllCandlesLater = false;
                                }
                            }
                        }
                        newSection.IsPresent = isAllCandlesLater ? true : false;
                        _sections.Add(newSection);
                    }
                    //нужно ли добавить сегмент с разрывом
                    if (isDivide)
                    {
                        SegmentPageTradeChart divideSegment = new SegmentPageTradeChart();
                        divideSegment.Section = _sections.Last();
                        divideSegment.IsDivide = true;
                        divideSegment.CandleIndexes = new List<CandleIndexPageTradeChart>();
                        _segments.Add(divideSegment);
                    }
                }
                //проверяем, закончились ли все источники данных
                if (endedDataSources.Count == _testing.DataSourcesCandles.Count)
                {
                    isAllDataSourcesEnd = true;
                    //при переходе на новый файл создается новая секция, поэтому когда все файлы заканчиваются создается пустая секция, удаляем её сейчас
                    if (_sections.Any())
                    {
                        _sections.RemoveAt(_sections.Count - 1);
                    }
                }
            }

            //добавляем в сегменты заявки и сделки
            List<int>[] ordersIndexes = new List<int>[_testing.DataSourcesCandles.Count]; //массив со списками индексов заявок, для всех источников данных
            List<int>[] dealsIndexes = new List<int>[_testing.DataSourcesCandles.Count]; //массив со списками индексов сделок, для всех источников данных
            int[] currentOrderIndexes = Enumerable.Repeat(0, _testing.DataSourcesCandles.Count).ToArray(); //индексы текущего индекса заявки для всех источников данных
            int[] currentDealIndexes = Enumerable.Repeat(0, _testing.DataSourcesCandles.Count).ToArray(); //индексы текущего индекса сделки для всех источников данных
            List<int>[] submitedOrdersIndexes = new List<int>[_testing.DataSourcesCandles.Count]; //массив со списками индексов заявок, которые выставлены но еще не сняты/исполнены, для всех источников данных
            //заполняем массивы списками
            for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
            {
                ordersIndexes[i] = new List<int>();
                for(int k = 0; k < _testRun.Account.AllOrders.Count; k++)
                {
                    if(_testRun.Account.AllOrders[k].IdDataSource == _testing.DataSourcesCandles[i].DataSource.Id)
                    {
                        ordersIndexes[i].Add(k); //добавялем индекс заявки, которая имеет текущий источник данных
                    }
                }
                dealsIndexes[i] = new List<int>();
                for (int k = 0; k < _testRun.Account.AllDeals.Count; k++)
                {
                    if (_testRun.Account.AllDeals[k].IdDataSource == _testing.DataSourcesCandles[i].DataSource.Id)
                    {
                        dealsIndexes[i].Add(k); //добавялем индекс сделки, которая имеет текущий источник данных
                    }
                }
                submitedOrdersIndexes[i] = new List<int>();
            }
            //проходим по всем сегментам
            for (int i = 0; i < _segments.Count; i++)
            {
                //проходим по всем источникам данных в сегменте
                for (int k = 0; k < _segments[i].CandleIndexes.Count; k++)
                {
                    int dataSourceCandleIndex = _segments[i].CandleIndexes[k].DataSourceCandlesIndex;

                    //если есть выствленные, но еще не снятые заявки для текущего источника данных, добавляем их в сегмент
                    for(int u = submitedOrdersIndexes[dataSourceCandleIndex].Count - 1; u >= 0; u--)
                    {
                        bool isOrderEnd = DateTime.Compare(_testRun.Account.AllOrders[submitedOrdersIndexes[dataSourceCandleIndex][u]].DateTimeRemove, _testing.DataSourcesCandles[dataSourceCandleIndex].Candles[_segments[i].CandleIndexes[k].FileIndex][_segments[i].CandleIndexes[k].CandleIndex].DateTime) == 0; //равняется ли текущая дата, дате снятия заявки
                        _segments[i].CandleIndexes[k].OrderIndexes.Add(new OrderIndexPageTradeChart { IsStart = false, isEnd = isOrderEnd, OrderIndex = submitedOrdersIndexes[dataSourceCandleIndex][u] });
                        if (isOrderEnd) //если заявка снимается на текущем сегменте, удаляем её из выставленных, но еще не снятых заявок
                        {
                            submitedOrdersIndexes[dataSourceCandleIndex].RemoveAt(u);
                        }
                    }

                    //проверяем, равняется ли дата текущей заявки для текущего источника данных, дате сегмента
                    if(currentOrderIndexes[dataSourceCandleIndex] < ordersIndexes[dataSourceCandleIndex].Count) //если не вышли за границы списка
                    {
                        if(DateTime.Compare(_testing.DataSourcesCandles[dataSourceCandleIndex].Candles[_segments[i].CandleIndexes[k].FileIndex][_segments[i].CandleIndexes[k].CandleIndex].DateTime, _testRun.Account.AllOrders[ordersIndexes[dataSourceCandleIndex][currentOrderIndexes[dataSourceCandleIndex]]].DateTimeSubmit) == 0)
                        {
                            bool isDateEqual = true; //даты свечки и текущей заявки равны или нет
                            //записываем все заявки, дата выставления которых совпадает с датой текущей свечки
                            do
                            {
                                bool isOrderEnd = DateTime.Compare(_testRun.Account.AllOrders[ordersIndexes[dataSourceCandleIndex][currentOrderIndexes[dataSourceCandleIndex]]].DateTimeRemove, _testRun.Account.AllOrders[ordersIndexes[dataSourceCandleIndex][currentOrderIndexes[dataSourceCandleIndex]]].DateTimeSubmit) == 0;
                                _segments[i].CandleIndexes[k].OrderIndexes.Add(new OrderIndexPageTradeChart { IsStart = true, isEnd = isOrderEnd, OrderIndex = ordersIndexes[dataSourceCandleIndex][currentOrderIndexes[dataSourceCandleIndex]] });
                                
                                _segmentOrders.Add(new SegmentOrderIndexPageTradeChart { SegmentIndex = i, OrderIndex = ordersIndexes[dataSourceCandleIndex][currentOrderIndexes[dataSourceCandleIndex]] }); //добавляем в список, позволяющий использовать навигацию по заявкам на графике

                                if (isOrderEnd == false) //если заявка не снимается на текущей свечке, запоминаем её для того чтобы указывать для следующих сегментов, пока она не будет снята
                                {
                                    submitedOrdersIndexes[dataSourceCandleIndex].Add(ordersIndexes[dataSourceCandleIndex][currentOrderIndexes[dataSourceCandleIndex]]);
                                }
                                currentOrderIndexes[dataSourceCandleIndex]++; //переходим на следующую заявку
                                if (currentOrderIndexes[dataSourceCandleIndex] < ordersIndexes[dataSourceCandleIndex].Count) //проверяем, не вышли ли за границы списка
                                {
                                    if (DateTime.Compare(_testing.DataSourcesCandles[dataSourceCandleIndex].Candles[_segments[i].CandleIndexes[k].FileIndex][_segments[i].CandleIndexes[k].CandleIndex].DateTime, _testRun.Account.AllOrders[ordersIndexes[dataSourceCandleIndex][currentOrderIndexes[dataSourceCandleIndex]]].DateTimeSubmit) != 0)
                                    {
                                        isDateEqual = false;
                                    }
                                }
                            }
                            while (currentOrderIndexes[dataSourceCandleIndex] < ordersIndexes[dataSourceCandleIndex].Count && isDateEqual);
                        }
                    }

                    //проверяем, равняется ли дата текущей сделки для текущего источника данных, дате сегмента
                    if(currentDealIndexes[dataSourceCandleIndex] < dealsIndexes[dataSourceCandleIndex].Count) //если не вышли за границы списка
                    {
                        if(DateTime.Compare(_testing.DataSourcesCandles[dataSourceCandleIndex].Candles[_segments[i].CandleIndexes[k].FileIndex][_segments[i].CandleIndexes[k].CandleIndex].DateTime, _testRun.Account.AllDeals[dealsIndexes[dataSourceCandleIndex][currentDealIndexes[dataSourceCandleIndex]]].DateTime) == 0)
                        {
                            bool isDateEqual = true; //даты свечки и текущей сделки равны или нет
                            //записываем все сделки, дата выставления которых совпадает с датой текущей свечки
                            do
                            {
                                _segments[i].CandleIndexes[k].DealIndexes.Add(dealsIndexes[dataSourceCandleIndex][currentDealIndexes[dataSourceCandleIndex]]);
                                _segmentDeals.Add(new SegmentDealIndexPageTradeChart { SegmentIndex = i, DealIndex = dealsIndexes[dataSourceCandleIndex][currentDealIndexes[dataSourceCandleIndex]] }); //добавляем в список, позволяющий использовать навигацию по сделкам на графике
                                currentDealIndexes[dataSourceCandleIndex]++; //переходим на следующую сделку
                                if (currentDealIndexes[dataSourceCandleIndex] < dealsIndexes[dataSourceCandleIndex].Count) //проверяем, не вышли ли за границы списка
                                {
                                    if (DateTime.Compare(_testing.DataSourcesCandles[dataSourceCandleIndex].Candles[_segments[i].CandleIndexes[k].FileIndex][_segments[i].CandleIndexes[k].CandleIndex].DateTime, _testRun.Account.AllDeals[dealsIndexes[dataSourceCandleIndex][currentDealIndexes[dataSourceCandleIndex]]].DateTime) != 0)
                                    {
                                        isDateEqual = false;
                                    }
                                }
                            }
                            while (currentDealIndexes[dataSourceCandleIndex] < dealsIndexes[dataSourceCandleIndex].Count && isDateEqual);
                        }
                    }
                }
            }
            if(_endPeriodSegmentIndex == -1)
            {
                _endPeriodSegmentIndex = _segments.Count - 1;
            }
        }

        private void SetInitialCandleWidth() //устанавливает начальную ширину свечки
        {
            CandleWidth = CandleWidths[2]; //текущая ширина свечки
        }

        private int FindPresentSegmentIndex(int startIndex, bool direction) //находит индекс первого сегмента в настоящем, от стартового индекса в выбранном направлении, если сегмент в настоящем не найден, вернет -1
        {
            int index = startIndex;
            bool isIndexOver = direction ? index >= _segments.Count : index < 0; //флаг того что вышли за пределы списка с сегментами
            bool isPresent = isIndexOver == false ? _segments[index].Section.IsPresent : false; //флаг того что сегмент находится в настоящем
            while (isIndexOver == false && isPresent == false)
            {
                index += direction ? 1 : -1;
                isIndexOver = direction ? index >= _segments.Count : index < 0;
                isPresent = isIndexOver == false ? _segments[index].Section.IsPresent : false;
            }
            return isPresent ? index : -1;
        }
        private void BuildTradeChart() //строит график котировок
        {
            TimeLinesPageTradeChart.Clear();
            Candles.Clear();
            Orders.Clear();
            Deals.Clear();
            IndicatorsPolylines.Clear();
            List<DateTime> timeLineDateTimes = new List<DateTime>(); //список с датами и временем сегментов, по возрастанию
            List<double> timeLineLefts = new List<double>(); //список с отступами слева, соответствующими дате и времени в timeLineDateTimes
            //формируем индикаторы для каждого источника данных
            for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
            {
                foreach (AlgorithmIndicatorValues algorithmIndicatorValues in _testing.DataSourcesCandles[i].AlgorithmIndicatorsValues)
                {
                    IndicatorsPolylines.Add(new IndicatorPolylinePageTradeChart { IdDataSource = _testing.DataSourcesCandles[i].DataSource.Id, IdIndicator = algorithmIndicatorValues.AlgorithmIndicator.IdIndicator, StrokeColor = _indicatorStrokeColor, Left = 0, Points = new PointCollection(), PointsPrices = new List<double>() });
                }
            }

            AreasWidth = СanvasTradeChartWidth - _scaleValuesWidth;
            int areasWidth = (int)Math.Truncate(СanvasTradeChartWidth - _scaleValuesWidth); //ширина областей
            //находим индекс самого правого сегмента на графике, а так же суммарную ширину сегментов, которые правее текущего сегмента
            List<int> rightSegmentIndexes = new List<int>(); //индексы правого сегмента, эти два списка содержат крайние индексы группы сегментов, если нужно пропустить прошлые даты, будет создано несколько групп сегментов, ограничивающиеся крайними индексами
            List<int> leftSegmentIndexes = new List<int>(); //индексы левого сегмента
            rightSegmentIndexes.Add(_segmentIndex + 1);
            leftSegmentIndexes.Add(_segmentIndex);
            bool isSegmentsEnd = !(rightSegmentIndexes[rightSegmentIndexes.Count - 1] < _segments.Count);
            int totalRightSegmentsWidth = 0;
            while(totalRightSegmentsWidth < Math.Truncate(areasWidth * _tradeChartHiddenSegmentsSize) && isSegmentsEnd == false)
            {
                totalRightSegmentsWidth += _segments[rightSegmentIndexes[rightSegmentIndexes.Count - 1]].IsDivide ? _divideWidth : _candleWidth;
                rightSegmentIndexes[rightSegmentIndexes.Count - 1]++;
                if (rightSegmentIndexes[rightSegmentIndexes.Count - 1] < _segments.Count)
                {
                    if (IsHideDuplicateDate) //если нужно скрыть дублирующися даты, проверяем, находится ли сегмент, на который мы перешли, в настоящем
                    {
                        if (_segments[rightSegmentIndexes[rightSegmentIndexes.Count - 1]].Section.IsPresent == false) //если сегмент, на который мы перешли, в прошлом, находим следующий индекс сегмента в настоящем, и добавляем новую пару правого и левого индексов
                        {
                            int nextPresentSegmentIndex = FindPresentSegmentIndex(rightSegmentIndexes[rightSegmentIndexes.Count - 1], true);
                            if(nextPresentSegmentIndex > -1) //если сегмент найден, добавляем новую пару правого и левого индексов
                            {
                                rightSegmentIndexes.Add(nextPresentSegmentIndex + 1);
                                leftSegmentIndexes.Add(nextPresentSegmentIndex);
                            }
                            else //если сегмент в настоящем не был найден, отмечаем что сегменты закончились
                            {
                                isSegmentsEnd = true;
                            }
                        }
                    }
                }
                if((rightSegmentIndexes[rightSegmentIndexes.Count - 1] < _segments.Count) == false)
                {
                    isSegmentsEnd = true; //если индекс превысил диапазон списка, отмечаем что сегменты закончились
                }
            }
            //уменьшаем правые индексы, т.к. они перешли на сегмент, котоырй не удовлетворяет условиям
            for(int u = 0; u < rightSegmentIndexes.Count; u++)
            {
                rightSegmentIndexes[u]--;
            }
            //находим индекс самого левого сегмента на графике
            isSegmentsEnd = !(leftSegmentIndexes[0] >= 0);
            int totalLeftSegmentsWidth = 0;
            while (totalLeftSegmentsWidth < Math.Truncate(areasWidth + areasWidth * _tradeChartHiddenSegmentsSize) && isSegmentsEnd == false)
            {
                totalLeftSegmentsWidth += _segments[leftSegmentIndexes[0]].IsDivide ? _divideWidth : _candleWidth;
                leftSegmentIndexes[0]--;
                if (leftSegmentIndexes[0] >= 0)
                {
                    if (IsHideDuplicateDate) //если нужно скрыть дублирующися даты, проверяем, находится ли сегмент, на который мы перешли, в настоящем
                    {
                        if (_segments[leftSegmentIndexes[0]].Section.IsPresent == false) //если сегмент, на который мы перешли, в прошлом, находим следующий индекс сегмента в настоящем, и добавляем новую пару правого и левого индексов
                        {
                            int nextPresentSegmentIndex = FindPresentSegmentIndex(leftSegmentIndexes[0], false);
                            if (nextPresentSegmentIndex > -1) //если сегмент найден, добавляем новую пару правого и левого индексов
                            {
                                rightSegmentIndexes.Insert(0, nextPresentSegmentIndex);
                                leftSegmentIndexes.Insert(0, nextPresentSegmentIndex - 1);
                            }
                            else //если сегмент в настоящем не был найден, отмечаем что сегменты закончились
                            {
                                isSegmentsEnd = true;
                            }
                        }
                    }
                }
                if ((leftSegmentIndexes[0] >= 0) == false)
                {
                    isSegmentsEnd = true; //если индекс превысил диапазон списка, отмечаем что сегменты закончились
                }
            }
            //увеличиваем левые индексы, т.к. они перешли на сегмент, который не удовлетворяет условиям
            for (int u = 0; u < leftSegmentIndexes.Count; u++)
            {
                leftSegmentIndexes[u]++;
            }
            
            //проходим по всем сегментам и формируем свечки, заявки, сделки и индикаторы для сегментов
            double lastTimeLineLeft = areasWidth + totalRightSegmentsWidth - Math.Truncate(_timeLineTimePixelsPerCut / 2) - 6; //последний отступ слева для элементов таймлайна
            int segmentIndexesIndex = rightSegmentIndexes.Count - 1; //текущий индекс группы сегментов, ограниченных индексами сегментов слева и справа
            int segmentIndex = rightSegmentIndexes[segmentIndexesIndex]; //текущий индекс сегмента
            int totalSegmentsWidth = 0; //суммарная ширина сегментов
            bool isEndSegments = segmentIndexesIndex >= 0 ? false : true;
            while(isEndSegments == false)
            {
                if (_segments[segmentIndex].IsDivide)
                {
                    totalSegmentsWidth += _divideWidth;
                }
                else
                {
                    //проходим по всем источникам данных текущего сегмента
                    for (int k = 0; k < _segments[segmentIndex].CandleIndexes.Count; k++)
                    {
                        //добавляем свечку
                        bool isCandleFalling = _testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[k].DataSourceCandlesIndex].Candles[_segments[segmentIndex].CandleIndexes[k].FileIndex][_segments[segmentIndex].CandleIndexes[k].CandleIndex].C < _testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[k].DataSourceCandlesIndex].Candles[_segments[segmentIndex].CandleIndexes[k].FileIndex][_segments[segmentIndex].CandleIndexes[k].CandleIndex].O; //свечка падающая или растущая
                        double bodyLeft = areasWidth + totalRightSegmentsWidth - totalSegmentsWidth;
                        Candles.Insert(0, new CandlePageTradeChart { IdDataSource = _testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[k].DataSourceCandlesIndex].DataSource.Id, StrokeColor = _candleStrokeColor, FillColor = isCandleFalling ? _fallingCandleFillColor : _risingCandleFillColor, BodyLeft = bodyLeft - _candleWidth, Candle = _testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[k].DataSourceCandlesIndex].Candles[_segments[segmentIndex].CandleIndexes[k].FileIndex][_segments[segmentIndex].CandleIndexes[k].CandleIndex], BodyWidth = _candleWidth, StickLeft = bodyLeft - _candleWidth / 2.0 - 1 / 2.0, StickWidth = 1 });

                        //добавляем заявки
                        //проходим по всем заявкам
                        foreach (OrderIndexPageTradeChart orderIndexPageTradeChart in _segments[segmentIndex].CandleIndexes[k].OrderIndexes)
                        {
                            SolidColorBrush orderFillColor = _limitBuyOrderFillColor;
                            if (_testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].TypeOrder.Id == 1) //лимитная заявка
                            {
                                orderFillColor = _testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].Direction ? _limitBuyOrderFillColor : _limitSellOrderFillColor;
                            }
                            else if (_testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].TypeOrder.Id == 2) //рыночная заявка
                            {
                                orderFillColor = _testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].Direction ? _marketBuyOrderFillColor : _marketSellOrderFillColor;
                            }
                            else if (_testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].TypeOrder.Id == 3) //стоп заявка
                            {
                                orderFillColor = _testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].Direction ? _stopBuyOrderFillColor : _stopSellOrderFillColor;
                            }
                            double verticalLineWidth = _candleWidth >= 2 ? _candleWidth / 2 : 1;
                            Orders.Insert(0, new OrderPageTradeChart { IdDataSource = _testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[k].DataSourceCandlesIndex].DataSource.Id, FillColor = orderFillColor, Order = _testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex], IsStart = orderIndexPageTradeChart.IsStart, HorizontalLineLeft = bodyLeft - _candleWidth, HorizontalLineWidth = _candleWidth, VerticalLineLeft = bodyLeft - verticalLineWidth, VerticalLineWidth = verticalLineWidth });
                        }

                        //добавляем сделки
                        //проходим по всем сделкам
                        foreach (int dealIndex in _segments[segmentIndex].CandleIndexes[k].DealIndexes)
                        {
                            int triangleWidth = 0;
                            int triangleHeight = 0;
                            if (_candleWidth < 3)
                            {
                                triangleWidth = 7;
                                triangleHeight = 4;
                            }
                            else if (_candleWidth < 11)
                            {
                                triangleWidth = 9;
                                triangleHeight = 5;
                            }
                            else
                            {
                                triangleWidth = 11;
                                triangleHeight = 6;
                            }
                            Deals.Insert(0, new DealPageTradeChart { IdDataSource = _testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[k].DataSourceCandlesIndex].DataSource.Id, StrokeColor = _testRun.Account.AllDeals[dealIndex].Direction ? _buyDealStrokeColor : _sellDealStrokeColor, FillColor = _testRun.Account.AllDeals[dealIndex].Direction ? _buyDealFillColor : _sellDealFillColor, Deal = _testRun.Account.AllDeals[dealIndex], Left = bodyLeft - CandleWidth + Math.Truncate((CandleWidth - triangleWidth) / 2.0), TriangleWidth = triangleWidth, TriangleHeight = triangleHeight, Points = new PointCollection() });
                        }

                        //добавляем точки со значениями для всех индикаторов текущего источника данных
                        //проходим по всем индикаторам
                        foreach (AlgorithmIndicatorValues algorithmIndicatorValues in _testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[k].DataSourceCandlesIndex].AlgorithmIndicatorsValues)
                        {
                            if (algorithmIndicatorValues.Values[_segments[segmentIndex].CandleIndexes[k].FileIndex][_segments[segmentIndex].CandleIndexes[k].CandleIndex].IsNotOverIndex) //если не было превышения индекса при рассчете данного значения индикатора
                            {
                                IndicatorPolylinePageTradeChart indicatorPolyline = IndicatorsPolylines.Where(a => a.IdDataSource == _testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[k].DataSourceCandlesIndex].DataSource.Id && a.IdIndicator == algorithmIndicatorValues.AlgorithmIndicator.IdIndicator).First(); //индикатор с текущим источником данных
                                indicatorPolyline.Points.Insert(0, new Point(bodyLeft - _candleWidth / 2, 0)); //добавляем точку для графика
                                indicatorPolyline.PointsPrices.Insert(0, algorithmIndicatorValues.Values[_segments[segmentIndex].CandleIndexes[k].FileIndex][_segments[segmentIndex].CandleIndexes[k].CandleIndex].Value); //добавляем значение цены для данной точки, на основании которой будет вычисляться координата Y точки
                            }
                        }
                    }

                    //добавляем элемент таймлайна для текущего сегмента
                    double timeLineLeft = areasWidth + totalRightSegmentsWidth - totalSegmentsWidth;
                    if (lastTimeLineLeft - timeLineLeft >= _timeLineTimePixelsPerCut) //если отступ между предыдущей линией таймлайна и текущим сегментом, больше или равен расстоянию между линиями таймлайна, добавляем линию
                    {
                        lastTimeLineLeft = timeLineLeft;//_testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[_segments[segmentIndex].CandleIndexes[0].FileIndex][_segments[segmentIndex].CandleIndexes[0].CandleIndex].DateTime
                        DateTime segmentDateTime = _testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[_segments[segmentIndex].CandleIndexes[0].FileIndex][_segments[segmentIndex].CandleIndexes[0].CandleIndex].DateTime;
                        TimeLinesPageTradeChart.Add(new TimeLinePageTradeChart { DateTime = segmentDateTime, StrokeLineColor = _timeLineStrokeLineColor, TextColor = _timeLineTextColor, FontSize = _timeLineFontSize, TextLeft = timeLineLeft - _timeLineFullDateTimeLeft, TextTop = СanvasTradeChartHeight - _timeLineHeight + 3, LineLeft = timeLineLeft, X1 = Math.Truncate(timeLineLeft), Y1 = 0, X2 = Math.Truncate(timeLineLeft), Y2 = СanvasTradeChartHeight - _timeLineHeight });
                    }

                    totalSegmentsWidth += _candleWidth;
                }

                //переходим на следующий сегмент
                segmentIndex--;
                if(segmentIndex < leftSegmentIndexes[segmentIndexesIndex]) //если текущий индекс сегмента, вышел за правый индекс группы сегментов, переходим на следующую группу
                {
                    segmentIndexesIndex--;
                    if(segmentIndexesIndex >= 0) //если индекс группы сегментов, не ревысил количество групп
                    {
                        segmentIndex = rightSegmentIndexes[segmentIndexesIndex];
                    }
                    else //если индекс группы сегментов, превысил количество групп сегментов, отмечаем что сегменты закончились
                    {
                        isEndSegments = true;
                    }
                }
            }
        }

        private List<double> GetScalueValuesCuts(double maxPrice, double minPrice, double priceStep, double areaHeight) //возвращает список с ценами отрезков шкалы значений
        {
            //находим размер отрезка, при котором высота одного пункта цены будет ближе всего к _scaleValuePixelPerCut _scaleValuesCuts
            int maxPriceStep = (int)Math.Round(maxPrice / priceStep); //значение максимальной цены в пунктах
            int minPriceStep = (int)Math.Round(minPrice / priceStep); //значение минимальной цены в пунктах
            int iteration = 0;
            int cut = 0; //количество пунктов, которому должны быть кратны отрезки
            int lastCut = 0; //прошлое количество пунктов, которому должны быть кратны отрезки
            double currentPixelPerCut = 0; //текущее количество пикселей на 1 пункт цены
            double lastPixelPerCut = 0; //прошлое количество пикселей на 1 пункт цены
            int scaleValuesCutsIndex = 0; //индекс значения в _scaleValuesCuts (если индекс выходит за пределы массива, из индекса вычитается длина массива, а для значений устанавливается множитель 10)
            int multiply = 1; //множитель для значений в _scaleValuesCuts
            do
            {
                iteration++; //номер итерации цикла
                lastCut = cut;
                lastPixelPerCut = currentPixelPerCut;
                while (scaleValuesCutsIndex >= _scaleValuesCuts.Length)
                {
                    scaleValuesCutsIndex -= _scaleValuesCuts.Length;
                    multiply *= 10;
                }
                cut = _scaleValuesCuts[scaleValuesCutsIndex] * multiply; //количество пунктов, которому должны быть кратны отрезки
                int currentCuts = 0; //текущее количество отрезков
                int currentPriceStep = minPriceStep;
                //проходим по пунктам цены от минимума до максимума, и если значение кратно cut (количеству пунктов для одного отрезка), увеличиваем количество отрезков для текущего значения в _scaleValuesCuts
                //доходим до первого кратного числа cut
                while (currentPriceStep <= maxPriceStep && currentPriceStep % cut != 0)
                {
                    currentPriceStep++;
                }
                //увеличиваем значение на cut, чтобы экономить на итерациях
                 while (currentPriceStep <= maxPriceStep)
                {
                    if (currentPriceStep % cut == 0)
                    {
                        currentCuts++;
                    }
                    currentPriceStep += cut;
                }
                //вычисляем текущее количество пикселей на один пункт цены
                currentPixelPerCut = areaHeight / currentCuts;
                scaleValuesCutsIndex++;
            }
            while (Math.Abs(_scaleValuePixelPerCut - lastPixelPerCut) >= Math.Abs(_scaleValuePixelPerCut - currentPixelPerCut) || iteration == 1); //выходим, когда разница между прошлым и номинальным количеством пикселей на 1 пункт цены, меньше разницы между текущим и номинальным. То есть когда следующий элемент (currentPixelPerCut) будет дальше от номинального нежели предыдущий (lastPixelPerCut). iteration == 1 чтобы пройти 2 раза по циклу, чтобы сформировать как текущее так и прошлое значение
            //создаем список с ценами отрезков шкалы значений
            List<double> cuts = new List<double>();
            int currentPrice = minPriceStep;
            //доходим до первого кратного числа cut
            while (currentPrice < maxPriceStep && currentPrice % lastCut != 0)
            {
                currentPrice++;
            }
            //увеличиваем значение на lastCut, чтобы экономить на итерациях
            while (currentPrice < maxPriceStep)
            {
                if (currentPrice % lastCut == 0)
                {
                    cuts.Add(currentPrice * priceStep);
                }
                currentPrice += lastCut;
            }
            return cuts;
        }

        public void UpdateScaleValues() //создает шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
        {
            ScaleValuesPageTradeChart.Clear();
            int areasWidth = (int)Math.Truncate(СanvasTradeChartWidth - _scaleValuesWidth); //ширина областей
            int dataSourceAreasTotalHeight = _dataSourceAreasHighlightHeight; //высота областей для одного источника данных с учетом высоты строки с подписью источника данных
            foreach(TradeChartAreaPageTradeChart tradeChartArea in TradeChartAreas)
            {
                dataSourceAreasTotalHeight += tradeChartArea.AreaHeight;
            }
            //проходим по всем источникам данных
            for (int i = 0; i < DataSourcesOrderDisplayPageTradeChart.Count; i++)
            {
                int countDsPriceStepDigitsAfterComma = 0; //количество цифр после запятой у шага цены источника данных
                string stringPriceStep = DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance.DataSource.PriceStep.ToString();
                if (stringPriceStep.Contains(","))
                {
                    string[] arrStr = stringPriceStep.Split(',');
                    countDsPriceStepDigitsAfterComma = arrStr[1].Length;
                }
                int currentTop = dataSourceAreasTotalHeight * i + _dataSourceAreasHighlightHeight; //текущий отступ сверху (с учетом уже отрисованных областей с источниками данных)
                List<CandlePageTradeChart> candlesCurrentDs = Candles.Where(a => a.IdDataSource == DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance.DataSource.Id).ToList(); //список со свечками, которые имеют id текущего источника данных
                List<OrderPageTradeChart> ordersCurrentDs = Orders.Where(a => a.IdDataSource == DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance.DataSource.Id).ToList(); //список с заявками, которые имеют id текущего источника данных
                List<DealPageTradeChart> dealsCurrentDs = Deals.Where(a => a.IdDataSource == DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance.DataSource.Id).ToList(); //список с сделками, которые имеют id текущего источника данных
                //определяем максимальную и минимальную цены для видимых свечек и заявок на графике
                double maxPrice = 0;
                double minPrice = 0;
                bool isFirstCandle = true;
                int candleIndex = 0;
                bool isBodyLeftLowThanAreasWidth = candlesCurrentDs.Count > 0 ? candlesCurrentDs[candleIndex].BodyLeft <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                //находим максимальную и минимальную цены для свечек
                while (isBodyLeftLowThanAreasWidth && candleIndex < candlesCurrentDs.Count)
                {
                    if(candlesCurrentDs[candleIndex].BodyLeft + candlesCurrentDs[candleIndex].BodyWidth > 0) //правая координата свечки положительная, значит эта свечка в видимой области
                    {
                        if (isFirstCandle)
                        {
                            maxPrice = candlesCurrentDs[candleIndex].Candle.H;
                            minPrice = candlesCurrentDs[candleIndex].Candle.L;
                            isFirstCandle = false;
                        }
                        else
                        {
                            maxPrice = candlesCurrentDs[candleIndex].Candle.H > maxPrice ? candlesCurrentDs[candleIndex].Candle.H : maxPrice;
                            minPrice = candlesCurrentDs[candleIndex].Candle.L < minPrice ? candlesCurrentDs[candleIndex].Candle.L : minPrice;
                        }
                    }
                    candleIndex++;
                    if(candleIndex < candlesCurrentDs.Count)
                    {
                        if (candlesCurrentDs[candleIndex].BodyLeft > areasWidth)
                        {
                            isBodyLeftLowThanAreasWidth = false;
                        }
                    }
                }
                //так же ищем максимальную и минимальную цены в заявках
                int orderIndex = 0;
                bool isHorizontalLineLeftLowThanAreasWidth = ordersCurrentDs.Count > 0 ? ordersCurrentDs[orderIndex].HorizontalLineLeft <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                if(IsHideOrders == false)
                {
                    while (isHorizontalLineLeftLowThanAreasWidth && orderIndex < ordersCurrentDs.Count)
                    {
                        if (ordersCurrentDs[orderIndex].HorizontalLineLeft + ordersCurrentDs[orderIndex].HorizontalLineWidth > 0) //правая координата заявки положительная, значит эта заявка в видимой области
                        {
                            maxPrice = ordersCurrentDs[orderIndex].Order.Price > maxPrice ? ordersCurrentDs[orderIndex].Order.Price : maxPrice;
                            minPrice = ordersCurrentDs[orderIndex].Order.Price < minPrice ? ordersCurrentDs[orderIndex].Order.Price : minPrice;
                        }
                        orderIndex++;
                        if (orderIndex < ordersCurrentDs.Count)
                        {
                            if (ordersCurrentDs[orderIndex].HorizontalLineLeft > areasWidth)
                            {
                                isHorizontalLineLeftLowThanAreasWidth = false;
                            }
                        }
                    }
                }
                //так же ищем максимальную и минимальную цены в сделках
                int dealIndex = 0;
                bool isDealLeftLowThanAreasWidth = dealsCurrentDs.Count > 0 ? dealsCurrentDs[dealIndex].Left <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                while (isDealLeftLowThanAreasWidth && dealIndex < dealsCurrentDs.Count)
                {
                    if(dealsCurrentDs[dealIndex].Left + dealsCurrentDs[dealIndex].TriangleWidth > 0) //правая координата заявки положительная, значит эта заявка в видимой области
                    {
                        maxPrice = dealsCurrentDs[dealIndex].Deal.Price > maxPrice ? dealsCurrentDs[dealIndex].Deal.Price : maxPrice;
                        minPrice = dealsCurrentDs[dealIndex].Deal.Price < minPrice ? dealsCurrentDs[dealIndex].Deal.Price : minPrice;
                    }
                    dealIndex++;
                    if(dealIndex < dealsCurrentDs.Count)
                    {
                        if (dealsCurrentDs[dealIndex].Left > areasWidth)
                        {
                            isDealLeftLowThanAreasWidth = false;
                        }
                    }
                }
                //так же ищем максимальную и минимальную цены в индикаторах главной области
                //проходим по всем индикаторам главной области
                foreach(IndicatorMenuItemPageTradeChart indicatorMenuItemPageTradeChart in IndicatorsMenuItemPageTradeChart.Where(a => a.SelectedTradeChartArea.IsDataSource))
                {
                    IndicatorPolylinePageTradeChart indicatorPolyline = IndicatorsPolylines.Where(a => a.IdDataSource == DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance.DataSource.Id && a.IdIndicator == indicatorMenuItemPageTradeChart.AlgorithmIndicator.IdIndicator).First(); //текущий индикатор в IndicatorsMenuItemPageTradeChart с текущим источником данных
                    int pointIndex = 0;
                    bool isXLowThanAreasWidth = indicatorPolyline.Points.Count > 0 ? indicatorPolyline.Points[pointIndex].X <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                    while (isXLowThanAreasWidth && pointIndex < indicatorPolyline.Points.Count)
                    {
                        if (indicatorPolyline.Points[pointIndex].X > 0) //координата точки положительная, значит она в видимой области
                        {
                            maxPrice = indicatorPolyline.PointsPrices[pointIndex] > maxPrice ? indicatorPolyline.PointsPrices[pointIndex] : maxPrice;
                            minPrice = indicatorPolyline.PointsPrices[pointIndex] < minPrice ? indicatorPolyline.PointsPrices[pointIndex] : minPrice;
                        }
                        pointIndex++;
                        if (pointIndex < indicatorPolyline.Points.Count)
                        {
                            if (indicatorPolyline.Points[pointIndex].X > areasWidth)
                            {
                                isXLowThanAreasWidth = false;
                            }
                        }
                    }
                }
                double addingRange = (maxPrice - minPrice) * _scaleValuesAddingRange; //дополнительный диапазон. Чтобы свечки графика не касались верхнего и нижнего краев области
                maxPrice += addingRange;
                minPrice -= addingRange;
                double priceRange = maxPrice - minPrice;
                //устанавливаем отступ сверху и высоту для видимых свечек
                candleIndex = 0;
                isBodyLeftLowThanAreasWidth = candlesCurrentDs.Count > 0 ? candlesCurrentDs[candleIndex].BodyLeft <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                while (isBodyLeftLowThanAreasWidth && candleIndex < candlesCurrentDs.Count)
                {
                    if (candlesCurrentDs[candleIndex].BodyLeft + candlesCurrentDs[candleIndex].BodyWidth > 0) //правая координата свечки положительная, значит эта свечка в видимой области
                    {
                        double highBodyPrice = candlesCurrentDs[candleIndex].Candle.O > candlesCurrentDs[candleIndex].Candle.C ? candlesCurrentDs[candleIndex].Candle.O : candlesCurrentDs[candleIndex].Candle.C; //цена верхней границы тела свечки
                        candlesCurrentDs[candleIndex].BodyTop = /*Math.Round*/(TradeChartAreas[0].AreaHeight * (1 - (highBodyPrice - minPrice) / priceRange)) + currentTop;
                        double lowBodyPrice = candlesCurrentDs[candleIndex].Candle.O < candlesCurrentDs[candleIndex].Candle.C ? candlesCurrentDs[candleIndex].Candle.O : candlesCurrentDs[candleIndex].Candle.C; //цена нижней границы тела свечки
                        candlesCurrentDs[candleIndex].BodyHeight = /*Math.Round*/(TradeChartAreas[0].AreaHeight * ((highBodyPrice - lowBodyPrice) / priceRange));
                        if(candlesCurrentDs[candleIndex].BodyHeight < 0.1) //если высота тела близка к 0, устанавливаем её в 1,а отступ сверху уменьшаем на 0,5
                        {
                            candlesCurrentDs[candleIndex].BodyTop -= 0.5;
                            candlesCurrentDs[candleIndex].BodyHeight = 1;
                        }
                        candlesCurrentDs[candleIndex].StickTop = /*Math.Round*/(TradeChartAreas[0].AreaHeight * (1 - (candlesCurrentDs[candleIndex].Candle.H - minPrice) / priceRange)) + currentTop;
                        candlesCurrentDs[candleIndex].StickHeight = /*Math.Round*/(TradeChartAreas[0].AreaHeight * ((candlesCurrentDs[candleIndex].Candle.H - candlesCurrentDs[candleIndex].Candle.L) / priceRange));
                        if (candlesCurrentDs[candleIndex].StickHeight < 0.1) //если высота линии близка к 0, устанавливаем её в 1,а отступ сверху уменьшаем на 0,5
                        {
                            candlesCurrentDs[candleIndex].StickTop -= 0.5;
                            candlesCurrentDs[candleIndex].StickHeight = 1;
                        }
                    }
                    candleIndex++;
                    if (candleIndex < candlesCurrentDs.Count)
                    {
                        if (candlesCurrentDs[candleIndex].BodyLeft > areasWidth)
                        {
                            isBodyLeftLowThanAreasWidth = false;
                        }
                    }
                }

                //устанавливаем отступ сверху и высоту для видимых заявок
                if (IsHideOrders == false)
                {
                    orderIndex = 0;
                    isHorizontalLineLeftLowThanAreasWidth = ordersCurrentDs.Count > 0 ? ordersCurrentDs[orderIndex].HorizontalLineLeft <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                    double onePriceStepHeight = TradeChartAreas[0].AreaHeight / (priceRange / DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance.DataSource.PriceStep); //высота одного пункта цены
                    while (isHorizontalLineLeftLowThanAreasWidth && orderIndex < ordersCurrentDs.Count)
                    {
                        if (ordersCurrentDs[orderIndex].HorizontalLineLeft + ordersCurrentDs[orderIndex].HorizontalLineWidth > 0) //правая координата заявки положительная, значит эта заявка в видимой области
                        {
                            double horizontalLineThickness = _candleWidth > 4 ? 2 : 1;
                            ordersCurrentDs[orderIndex].HorizontalLineTop = /*Math.Round*/(TradeChartAreas[0].AreaHeight * (1 - (ordersCurrentDs[orderIndex].Order.Price - minPrice) / priceRange) - horizontalLineThickness / 2) + currentTop;
                            ordersCurrentDs[orderIndex].HorizontalLineHeight = horizontalLineThickness;
                            if (ordersCurrentDs[orderIndex].IsStart) //если заявка начинается на этом сегменте, рисуем вертикальную линию
                            {
                                ordersCurrentDs[orderIndex].VerticalLineHeight = _partOfOnePriceStepHeightForOrderVerticalLine * onePriceStepHeight;
                                ordersCurrentDs[orderIndex].VerticalLineHeight = ordersCurrentDs[orderIndex].VerticalLineHeight < horizontalLineThickness + 2 ? horizontalLineThickness + 2 : ordersCurrentDs[orderIndex].VerticalLineHeight; //если высота вертикальной линии меньше чем толщина горизонтальной линии + 2, устанавливаем на такую толщину
                                ordersCurrentDs[orderIndex].VerticalLineTop = ordersCurrentDs[orderIndex].HorizontalLineTop - ordersCurrentDs[orderIndex].VerticalLineHeight / 2;
                                ordersCurrentDs[orderIndex].VerticalLineTop = ordersCurrentDs[orderIndex].HorizontalLineTop - (ordersCurrentDs[orderIndex].VerticalLineHeight - horizontalLineThickness) / 2;
                            }
                        }
                        orderIndex++;
                        if (orderIndex < ordersCurrentDs.Count)
                        {
                            if (ordersCurrentDs[orderIndex].HorizontalLineLeft > areasWidth)
                            {
                                isHorizontalLineLeftLowThanAreasWidth = false;
                            }
                        }
                    }
                }

                //устанавливаем отступ сверху и высоту для видимых сделок
                dealIndex = 0;
                isDealLeftLowThanAreasWidth = dealsCurrentDs.Count > 0 ? dealsCurrentDs[dealIndex].Left <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                while (isDealLeftLowThanAreasWidth && dealIndex < dealsCurrentDs.Count)
                {
                    if (dealsCurrentDs[dealIndex].Left + dealsCurrentDs[dealIndex].TriangleWidth > 0) //правая координата заявки положительная, значит эта заявка в видимой области
                    {
                        int verticalOffset = dealsCurrentDs[dealIndex].Deal.Direction ? 0 : -dealsCurrentDs[dealIndex].TriangleHeight; //вертикальное смещение, для сделки на покупку отсутствует, т.к. верхний край треугольника сделки на уровне цены, для продажи равняется высоте треугольника, т.к. уровень цены находится на нижней части треугольника
                        dealsCurrentDs[dealIndex].Top = (TradeChartAreas[0].AreaHeight * (1 - (dealsCurrentDs[dealIndex].Deal.Price - minPrice) / priceRange) + verticalOffset) + currentTop;
                        PointCollection newPoints = new PointCollection();
                        if (dealsCurrentDs[dealIndex].Deal.Direction) //сделка на покупку
                        {
                            newPoints.Add(new Point(0, dealsCurrentDs[dealIndex].TriangleHeight - 1)); //левая координата
                            newPoints.Add(new Point(Math.Truncate(dealsCurrentDs[dealIndex].TriangleWidth / 2.0), 0)); //вершина треугольника
                            newPoints.Add(new Point(dealsCurrentDs[dealIndex].TriangleWidth - 1, dealsCurrentDs[dealIndex].TriangleHeight - 1)); //правая координата
                        }
                        else //сделка на продажу
                        {
                            newPoints.Add(new Point(0, 0)); //левая координата
                            newPoints.Add(new Point(Math.Truncate(dealsCurrentDs[dealIndex].TriangleWidth / 2.0), dealsCurrentDs[dealIndex].TriangleHeight - 1)); //вершина треугольника
                            newPoints.Add(new Point(dealsCurrentDs[dealIndex].TriangleWidth - 1, 0)); //правая координата
                        }
                        dealsCurrentDs[dealIndex].Points = newPoints;
                    }
                    dealIndex++;
                    if (dealIndex < dealsCurrentDs.Count)
                    {
                        if (dealsCurrentDs[dealIndex].Left > areasWidth)
                        {
                            isDealLeftLowThanAreasWidth = false;
                        }
                    }
                }

                //создаем шкалы значений для всех областей текущего источника данных, а так же устанавливаем отступ сверху для видимых точек линий индикаторов
                //проходим по всем областям
                int pastAreasHeight = 0; //высота предыдущих, уже обработанных областей. Чтобы указывать отступ сверху для текущей области
                for(int k = 0; k < TradeChartAreas.Count; k++)
                {
                    //находим максимальное и минимальное значение области
                    double areaMaxPrice = 0;
                    double areaMinPrice = 0;
                    if(k == 0) //текущая область главная, значит берем максимум и минимум цены главной области
                    {
                        areaMaxPrice = maxPrice;
                        areaMinPrice = minPrice;
                    }
                    else //текущая область для индикаторов, значит вычисляем максимум и минимм текущей области
                    {
                        bool isFirstPrice = true;
                        //проходим по всем индикаторам текущей области
                        foreach (IndicatorMenuItemPageTradeChart indicatorMenuItemPageTradeChart in IndicatorsMenuItemPageTradeChart.Where(a => a.SelectedTradeChartArea == TradeChartAreas[k]))
                        {
                            IndicatorPolylinePageTradeChart indicatorPolyline = IndicatorsPolylines.Where(a => a.IdDataSource == DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance.DataSource.Id && a.IdIndicator == indicatorMenuItemPageTradeChart.AlgorithmIndicator.IdIndicator).First(); //текущий индикатор в IndicatorsMenuItemPageTradeChart с текущим источником данных
                            int pointIndex = 0;
                            bool isXLowThanAreasWidth = indicatorPolyline.Points.Count > 0 ? indicatorPolyline.Points[pointIndex].X <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                            while (isXLowThanAreasWidth && pointIndex < indicatorPolyline.Points.Count)
                            {
                                if (indicatorPolyline.Points[pointIndex].X > 0) //координата точки положительная, значит она в видимой области
                                {
                                    if (isFirstPrice)
                                    {
                                        areaMaxPrice = indicatorPolyline.PointsPrices[pointIndex];
                                        areaMinPrice = indicatorPolyline.PointsPrices[pointIndex];
                                        isFirstPrice = false;
                                    }
                                    else
                                    {
                                        areaMaxPrice = indicatorPolyline.PointsPrices[pointIndex] > areaMaxPrice ? indicatorPolyline.PointsPrices[pointIndex] : areaMaxPrice;
                                        areaMinPrice = indicatorPolyline.PointsPrices[pointIndex] < areaMinPrice ? indicatorPolyline.PointsPrices[pointIndex] : areaMinPrice;
                                    }
                                }
                                pointIndex++;
                                if (pointIndex < indicatorPolyline.Points.Count)
                                {
                                    if (indicatorPolyline.Points[pointIndex].X > areasWidth)
                                    {
                                        isXLowThanAreasWidth = false;
                                    }
                                }
                            }
                        }
                        double addingAreaRange = (areaMaxPrice - areaMinPrice) * _scaleValuesAddingRange; //дополнительный диапазон. Чтобы значения индикатора не касались верхнего и нижнего краев области
                        areaMaxPrice += addingAreaRange;
                        areaMinPrice -= addingAreaRange;
                    }
                    
                    double areaPriceRange = areaMaxPrice - areaMinPrice;
                    //устанавливаем отступ сверху для всех индикаторов и точек индикаторов текущей области
                    //проходим по всем индикаторам текущей области
                    foreach (IndicatorMenuItemPageTradeChart indicatorMenuItemPageTradeChart in IndicatorsMenuItemPageTradeChart.Where(a => a.SelectedTradeChartArea == TradeChartAreas[k]))
                    {
                        IndicatorPolylinePageTradeChart indicatorPolyline = IndicatorsPolylines.Where(a => a.IdDataSource == DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance.DataSource.Id && a.IdIndicator == indicatorMenuItemPageTradeChart.AlgorithmIndicator.IdIndicator).First(); //текущий индикатор в IndicatorsMenuItemPageTradeChart с текущим источником данных
                        indicatorPolyline.Top = currentTop + pastAreasHeight;
                        int pointIndex = 0;
                        bool isXLowThanAreasWidth = indicatorPolyline.Points.Count > 0 ? indicatorPolyline.Points[pointIndex].X <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                        while (isXLowThanAreasWidth && pointIndex < indicatorPolyline.Points.Count)
                        {
                            if (indicatorPolyline.Points[pointIndex].X + _candleWidth > 0) //координата точки положительная, значит она в видимой области (+ _candleWidth для того чтобы установить высоту и для точки которая слева от самой крайней, т.к. иначе у неё будет позиция 0, то есть на самом верху, и слиния будет туда тянуться)
                            {
                                indicatorPolyline.Points[pointIndex] = new Point(indicatorPolyline.Points[pointIndex].X, (TradeChartAreas[k].AreaHeight * (1 - (indicatorPolyline.PointsPrices[pointIndex] - areaMinPrice) / areaPriceRange)));
                            }
                            pointIndex++;
                            if (pointIndex < indicatorPolyline.Points.Count)
                            {
                                if (indicatorPolyline.Points[pointIndex].X - _candleWidth > areasWidth)
                                {
                                    isXLowThanAreasWidth = false;
                                }
                            }
                        }
                    }

                    //создаем шкалу значений для текущей области
                    List<double> cuts = GetScalueValuesCuts(areaMaxPrice, areaMinPrice, DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance.DataSource.PriceStep, TradeChartAreas[k].AreaHeight); //список с ценами отрезков шкалы значений
                    for(int u = 0; u < cuts.Count; u++)
                    {
                        double lineTop = (TradeChartAreas[k].AreaHeight * (1 - (cuts[u] - areaMinPrice) / areaPriceRange));
                        string priceText = cuts[u].ToString();
                        if(countDsPriceStepDigitsAfterComma > 0) //если у источника данных, имеются цифры после запятой, проверяем чтобы были нули на их позициях в тексте
                        {
                            if (priceText.Contains(","))
                            {
                                string[] priceTextArr = priceText.Split(',');
                                if(priceTextArr[1].Length < countDsPriceStepDigitsAfterComma)
                                {
                                    for(int n = 0; n < countDsPriceStepDigitsAfterComma - priceTextArr[1].Length; n++)
                                    {
                                        priceText += "0";
                                    }
                                }
                            }
                            else
                            {
                                priceText += ",";
                                for (int n = 0; n < countDsPriceStepDigitsAfterComma; n++)
                                {
                                    priceText += "0";
                                }
                            }
                        }
                        ScaleValuesPageTradeChart.Add(new ScaleValuePageTradeChart { StrokeLineColor = _scaleValueStrokeLineColor, TextColor = _scaleValueTextColor, FontSize = _scaleValueFontSize, Price = priceText, PriceLeft = areasWidth + 3, PriceTop = currentTop + pastAreasHeight + (TradeChartAreas[k].AreaHeight * (1 - (cuts[u] - areaMinPrice) / areaPriceRange)) - _scaleValueTextTop, LineLeft = 0, LineTop = currentTop + pastAreasHeight, X1 = 0, Y1 = lineTop, X2 = areasWidth, Y2 = lineTop });
                    }

                    pastAreasHeight += TradeChartAreas[k].AreaHeight;
                }

            }
        }

        public void UpdatePage() //обновляет страницу на новый источник данных
        {
            if(ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null && ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox != null)
            {
                _isLoadingTestResultComplete = false; //флаг того что результаты загружены
                _testing = ViewModelPageTestingResult.getInstance().TestingResult;
                _testBatch = ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox.TestBatch;
                _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
                СanvasTradeChartWidth = СanvasTradeChart.ActualWidth;
                СanvasTradeChartHeight = СanvasTradeChart.ActualHeight;
                CreateDataSourcesOrderDisplayPageTradeChart(); //создаем элементы для меню управления порядком следования областей с источниками данных
                CreateTradeChartAreas(); //создаем области для графика котировок
                CreateIndicatorsMenuItemPageTradeChart(); //создаем элементы для меню выбора областей для индикаторов
                CreateSegments(); //создаем сегменты, на основе которых будет строиться график
                MoveToStartPeriod(); //переходим на сегмент с началом теста
                BuildTradeChart(); //строим график котировок
                UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
                _isLoadingTestResultComplete = true;
            }
        }
        public void GoToOrder(int orderIndex) //переходит на сегмент с заявкой с указанным индексом
        {
            double areasWidth = СanvasTradeChartWidth - _scaleValuesWidth; //ширина областей
            _segmentIndex = _segmentOrders.Find(a => a.OrderIndex == orderIndex).SegmentIndex + (int)Math.Truncate(areasWidth * 0.7 / CandleWidth);
            BuildTradeChart(); //строим график котировок
            UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
        }
        public void GoToDeal(int dealIndex) //переходит на сегмент со сделкой с указанным индексом
        {
            double areasWidth = СanvasTradeChartWidth - _scaleValuesWidth; //ширина областей
            _segmentIndex = _segmentDeals.Find(a => a.DealIndex == dealIndex).SegmentIndex + (int)Math.Truncate(areasWidth * 0.7 / CandleWidth);
            BuildTradeChart(); //строим график котировок
            UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
        }
        private void MoveToStartPeriod() //переходит на сегмент с началом теста
        {
            double areasWidth = СanvasTradeChartWidth - _scaleValuesWidth; //ширина областей
            _segmentIndex = _startPeriodSegmentIndex + (int)Math.Truncate(areasWidth * 0.8 / CandleWidth);
            _segmentIndex = _segmentIndex >= _segments.Count ? _segments.Count - 1 : _segmentIndex;
        }
        public List<double> GetDateRatesDepositCurrenciesChanges() //возвращает список с значениями от 0 до 1, для элементов DepositCurrenciesChanges, где 0 - начало тестового прогона, а 1 - окончание, значение отражает положение даты изменения депозита в диапазоне от начала периода теста до окончания
        {
            List<double> dateRatesDepositCurrenciesChanges = new List<double>();
            int defaultCurrencyIndex = _testRun.Account.DepositCurrenciesChanges[0].FindIndex(a => a.Currency.Id == _testRun.Account.DefaultCurrency.Id); //индекс элемента, с валютой по умолчанию
            int depositCurrenciesChangesIndex = 1; //индекс изменения депозита, устанавливаем в 1, чтобы пропустить начальное состояние депозита
            int segmentsNumber = 0; //количество пройденных сегментов
            dateRatesDepositCurrenciesChanges.Add(segmentsNumber); //добавляем первое изменение депозита, которое добавляется по умолчанию, как начальный депозит
            int segmentIndex = _startPeriodSegmentIndex;
            while (segmentIndex <= _endPeriodSegmentIndex)
            {
                if (_segments[segmentIndex].Section.IsPresent)
                {
                    segmentsNumber++;
                    if (depositCurrenciesChangesIndex < _testRun.Account.DepositCurrenciesChanges.Count) //проверяем, не вышли ли за границы списка
                    {
                        if (DateTime.Compare(_testing.DataSourcesCandles[_segments[segmentIndex].CandleIndexes[0].DataSourceCandlesIndex].Candles[_segments[segmentIndex].CandleIndexes[0].FileIndex][_segments[segmentIndex].CandleIndexes[0].CandleIndex].DateTime, _testRun.Account.DepositCurrenciesChanges[depositCurrenciesChangesIndex][defaultCurrencyIndex].DateTime) >= 0) //если дата текущего сегмента, раньше или равна дате текущего изменения депозита, запоминаем номер текущего сегмента
                        {
                            depositCurrenciesChangesIndex++;
                            dateRatesDepositCurrenciesChanges.Add(segmentsNumber);
                        }
                    }
                }
                segmentIndex++;
            }
            //делим номер сегмента изменения депозита, на количество сегментов
            for (int i = 0; i < dateRatesDepositCurrenciesChanges.Count; i++)
            {
                dateRatesDepositCurrenciesChanges[i] /= segmentsNumber;
            }
            return dateRatesDepositCurrenciesChanges;
        }
        public ICommand ToStartPeriod_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    MoveToStartPeriod();
                    BuildTradeChart(); //строим график котировок
                    UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
                }, (obj) => true);
            }
        }
        public ICommand ToEndPeriod_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _segmentIndex = _endPeriodSegmentIndex;
                    _segmentIndex = _segmentIndex >= _segments.Count ? _segments.Count - 1 : _segmentIndex;
                    BuildTradeChart(); //строим график котировок
                    UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
                }, (obj) => true);
            }
        }
    }
}
