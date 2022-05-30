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
        private TestBatch _testBatch; //тестовая связка
        private TestRun _testRun; //тестовый прогон, для которого строится график
        private int _candlesMinWidth = 1; //минимальная ширина свечки, в пикселях
        private int _candlesMaxWidth = 11; //максимальная ширина свечки, в пикселях
        private int _tradeChartScale; //масштаб графика, сколько свечек должно уместиться в видимую область графика
        private double _minCandlesFullness = 0.7; //минимальная наполненность свечками. Относительно количества свечек в _tradeChartScale. Минимальное количество свечек которое будет показано на графике в самом левом положении
        private int _divideWidth = 10; //ширина разрыва
        private int _scaleValuesWidth = 40; //ширина правой области со шкалой значений
        private double _tradeChartHiddenCandlesSize = 1; //размер генерируемых свечек справа и слева от видимых свечек, относительно видимых свечек. Размер отдельно для левых и правых свечек
        private double[] _indicatorAreasHeight = new double[3] { 0.15, 0.225, 0.3 }; //суммарная высота областей для индикаторов, как часть от доступной под области высоты, номер элемента соответствует количеству индикаторов и показывает суммарную высоту для них, если количество индикаторов больше, берется последний элемент
        private int _timeLineHeight = 24; //высота временной шкалы
        private List<SegmentPageTradeChart> _segments { get; set; } //сегменты из которых состоит график
        private int _segmentIndex; //текущий индекс сегмента
        private List<SegmentOrderIndexPageTradeChart> _segmentOrders; //индексы сегмента и заявки, чтобы можно было быстро найти сегмент с заявкой
        private List<SegmentDealIndexPageTradeChart> _segmentDeals; //индексы сегмента и сделки, чтобы можно было быстро найти сегмент со сделкой
        public double СanvasTradeChartWidth { get; set; } //ширина canvas с графиком
        public double СanvasTradeChartHeight { get; set; } //высота canvas с графиком

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
            if(DataSourcesOrderDisplayPageTradeChart.Count > 0)
            {
                foreach (DataSourceOrderDisplayPageTradeChart dataSourceOrderDisplayPageTradeChart in DataSourcesOrderDisplayPageTradeChart)
                {
                    TradeChartAreas.Add(TradeChartAreaPageTradeChart.CreateDataSourceArea(dataSourceOrderDisplayPageTradeChart.DataSourceAccordance));
                }
                UpdateTradeChartAreasHeight(); //обновляем высоту у областей графика котировок
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
            int dataSourceAreasCount = TradeChartAreas.Where(j => j.IsDataSource).Count(); //количество областей с источниками данных
            int indicatorAreasCount = TradeChartAreas.Where(j => j.IsDataSource == false).Count(); //количество областей с индикаторами
            int availableHeight = (int)Math.Truncate(СanvasTradeChartHeight) - _timeLineHeight; //доступная под области высота
            int indiactorAreaHeight = indicatorAreasCount > 0 ? (int)Math.Truncate((availableHeight * (IndicatorsMenuItemPageTradeChart.Count > _indicatorAreasHeight.Length ? _indicatorAreasHeight.Last() : _indicatorAreasHeight[IndicatorsMenuItemPageTradeChart.Count - 1])) / TradeChartAreas.Count) : 0; //высота для областей индикаторов
            int dataSourceAreaHeight = (int)Math.Truncate((availableHeight - indiactorAreaHeight * indicatorAreasCount) / (double)dataSourceAreasCount); //высота для облестей с источниками данных
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
                        UpdateTradeChartAreasOrder(); //обновляем порядок следования областей с источниками данных
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
                        UpdateTradeChartAreasOrder(); //обновляем порядок следования областей с источниками данных
                    }
                }
            }
        }
        private void UpdateTradeChartAreasOrder() //обновляет порядок следования областей с источниками данных на то как источники данных следуют в DataSourcesOrderDisplayPageTradeChart
        {
            for(int i = 0; i < DataSourcesOrderDisplayPageTradeChart.Count; i++)
            {
                int areaIndex = TradeChartAreas.IndexOf(TradeChartAreas.Where(j => j.DataSourceAccordance == DataSourcesOrderDisplayPageTradeChart[i].DataSourceAccordance).First()); //индекс элемента в TradeChartAreas с таким же DataSourceAccordance
                if(areaIndex != i) //если индексы не совпадают, перемещаем элемент в TradeChartAreas на новый индекс
                {
                    TradeChartAreas.Move(areaIndex, i);
                }
            }
        }
        private void CreateDataSourcesOrderDisplayPageTradeChart() //создает элементы для меню управления порядком следования областей с источниками данных
        {
            foreach(DataSourceAccordance dataSourceAccordance in _testBatch.DataSourceGroup.DataSourceAccordances)
            {
                DataSourcesOrderDisplayPageTradeChart.Add(DataSourceOrderDisplayPageTradeChart.CreateDataSource(DataSourcesOrderDisplayPageTradeChart_PropertyChanged, dataSourceAccordance));
            }
        }

        private void CreateSegments() //создает сегменты на основе загруженных данных свечек
        {
            _segments = new List<SegmentPageTradeChart>();
            _segmentIndex = 0;
            _segmentOrders = new List<SegmentOrderIndexPageTradeChart>();
            _segmentDeals = new List<SegmentDealIndexPageTradeChart>();//Enumerable.Repeat(0, _leftAxisParameters.Count).ToArray();
            int[] fileIndexes = Enumerable.Repeat(0, _testing.DataSourcesCandles.Length).ToArray(); //индексы текущего файла для всех источников данных (заполнили массив нулями)
            int[] candleIndexes = Enumerable.Repeat(0, _testing.DataSourcesCandles.Length).ToArray(); //индексы текущей свечки для всех источников данных (заполнили массив нулями)
            List<int> currentOrdersIndex = new List<int>(); //индексы заявок, которыевыставлены, но еще не сняты, и поэтому будут в сегментах с источниками данных этой заявки пока не будет дата снятия/исполнения данной заявки
            int orderIndex = 0; //индекс заявки
            int dealIndex = 0; //индекс сделки
            //проходя по свечкам и файлам, я буду разбивать график на секции. У каждой секции имеется свой источник данных и файл, свечки которого отображаются в секции. Секция заканчивается если закончился один из файлов, тогда начинается новая с теми источниками данных, у которых произошел переход на новый файл, эта секция продолжается до последней даты прошлой секции, после дохода до той даты, начинается новая секция в которой учавствуют все источники данных. 
            List<int> sectionDataSourceIndexes = new List<int>(); //список с индексами источников данных в текущей секции
            for(int i = 0; i < _testing.DataSourcesCandles.Length; i++) //заполняем индексами всех источников данных
            {
                sectionDataSourceIndexes.Add(i);
            }
            DateTime savedDateTime = new DateTime(); //последняя дата прошлой секции
            bool isAllFilesEnd = false; //закончились ли все файлы
            while(isAllFilesEnd == false)
            {
                //находим самую раннюю дату среди текущих свечек источников данных
                DateTime earliestDateTime = _testing.DataSourcesCandles[sectionDataSourceIndexes[0]].Candles[fileIndexes[sectionDataSourceIndexes[0]]][candleIndexes[sectionDataSourceIndexes[0]]].DateTime; //самая ранняя дата среди текущих свечек, она же дата текущей свечки
                for(int i = 1; i < sectionDataSourceIndexes.Count; i++)
                {
                    if(DateTime.Compare(_testing.DataSourcesCandles[sectionDataSourceIndexes[i]].Candles[fileIndexes[sectionDataSourceIndexes[i]]][candleIndexes[sectionDataSourceIndexes[i]]].DateTime, earliestDateTime) < 0)
                    {
                        earliestDateTime = _testing.DataSourcesCandles[sectionDataSourceIndexes[i]].Candles[fileIndexes[sectionDataSourceIndexes[i]]][candleIndexes[sectionDataSourceIndexes[i]]].DateTime;
                    }
                }
                //формируем список с заявками у которых текущая дата выставления. Проходим по заявкам, дата выставления которых равняется текущей
                List<int> currentOrderIndexes = new List<int>();
                bool orderDateTimeNotEqual = false;
                while(orderIndex < _testRun.Account.AllOrders.Count && orderDateTimeNotEqual == false)
                {
                    if(DateTime.Compare(_testRun.Account.AllOrders[orderIndex].DateTimeSubmit, earliestDateTime) == 0)
                    {
                        currentOrderIndexes.Add(orderIndex);
                        orderIndex++;
                    }
                    else
                    {
                        orderDateTimeNotEqual = true;
                    }
                }
                //формируем сегмент
                SegmentPageTradeChart segment = new SegmentPageTradeChart();
                segment.IsDivide = false;
                segment.CandleIndexes = new List<CandleIndexPageTradeChart>();
                for (int i = 0; i < sectionDataSourceIndexes.Count; i++) //добавляем в сегмент свечки с текущей датой для источников данных текущей секции
                {
                    if (DateTime.Compare(_testing.DataSourcesCandles[sectionDataSourceIndexes[i]].Candles[fileIndexes[sectionDataSourceIndexes[i]]][candleIndexes[sectionDataSourceIndexes[i]]].DateTime, earliestDateTime) == 0) //равняется ли дата свечки текущей дате
                    {
                        CandleIndexPageTradeChart candleIndex = new CandleIndexPageTradeChart { DataSourceIndex = sectionDataSourceIndexes[i], FileIndex = fileIndexes[sectionDataSourceIndexes[i]], CandleIndex = candleIndexes[sectionDataSourceIndexes[i]] };
                        List<OrderIndexPageTradeChart> orderIndexes = new List<OrderIndexPageTradeChart>(); //индексы с заявками на текущей свечке
                        int currentIdDataSource = _testBatch.DataSourceGroup.DataSourceAccordances[sectionDataSourceIndexes[i]].DataSource.Id; //id текущего источника данных
                        //проходим по заявкам
                        segment.CandleIndexes.Add(candleIndex);
                    }
                }
                
            }
        }

        private void DefineTradeChartInitialScaleAndSetCurrentFileCandleIndexes() //определяет начальный масштаб графика: количество свечек, которое должно поместиться на графике, и индексы текущего файла и свечки для всех источников данных
        {
            int candleWidth = 5; //ширина свечки, исходя из которой будет определяться количество свечек
            _tradeChartScale = (int)Math.Truncate((СanvasTradeChartWidth - _scaleValuesWidth) / candleWidth);
            int candlesCount = (int)Math.Round(_tradeChartScale * _minCandlesFullness); //количество свечек, которое нужно отобразить на графике
            bool isEndFiles = false; //закончились ли файлы
            int candleNumber = 1; //количество свечек которые уже прошли
            /*_currentFileCandleIndexes = new FileCandleIndexesPageTradeChart[_testBatch.DataSourceGroup.DataSourceAccordances.Count];
            while (candleNumber <= candlesCount && isEndFiles == false)
            {
                //переходим на следующую свечку
                _currentCandleIndex++;
                
            }*/
        }
        private bool MoveCurrentFileCandleIndexes(int offset) //сдвигает индекс текущей свечки на указанное число, как положительное (вправо), так и отрицательное (влево), и возвращает true если смещение имело место даже не на все значение offset, если смещение не получилось (уже начальные индексы или конечные) возвращает false
        {
            bool isMove = false;
            int currentOffset = 0; //смещение которое уже достигнуто
            return isMove;
        }

        private bool IsCandlesMinCandlesFullness(int[] currentFilesIndexes, int[] currentCandlesIndexes) //определяет, заполняют ли свечки минимальную заполненность на графике
        {
            return true;
        }

        private void BiuldTradeChart() //строит график котировок
        {
            //проходим по всем областям, и строим шкалы значений, для главной области свечки и объемы, а так же индикаторы для всех областей
            for(int i = 0; i < TradeChartAreas.Count; i++)
            {

            }
        }

        public void UpdatePage() //обновляет страницу на новый источник данных
        {
            _testing = ViewModelPageTestingResult.getInstance().TestingResult;
            _testBatch = ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox.TestBatch;
            _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
            CreateDataSourcesOrderDisplayPageTradeChart(); //создаем элементы для меню управления порядком следования областей с источниками данных
            CreateTradeChartAreas(); //создаем области для графика котировок
            CreateIndicatorsMenuItemPageTradeChart(); //создаем элементы для меню выбора областей для индикаторов
            DefineTradeChartInitialScaleAndSetCurrentFileCandleIndexes(); //определяем начальный масштаб графика: количество свечек, которое должно поместиться на графике, и индексы текущего файла и свечки
            BiuldTradeChart(); //строим график котировок
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
