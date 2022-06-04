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

namespace ktradesystem.ViewModels
{
    class ViewModelPageTradeChart : ViewModelBase
    {
        public ViewModelPageTradeChart()
        {
            ViewModelPageTestingResult.TestRunsUpdatePages += UpdatePage;
        }
        private Testing _testing; //результат тестирования
        private TestBatch _testBatch; //тестовая связка
        private TestRun _testRun; //тестовый прогон, для которого строится график
        private SolidColorBrush _candleStrokeColor = new SolidColorBrush(Color.FromRgb(40, 40, 40)); //цвет линии свечки
        private SolidColorBrush _risingCandleFillColor = new SolidColorBrush(Color.FromRgb(200, 200, 200)); //цвет заливки растущей свечки
        private SolidColorBrush _fallingCandleFillColor = new SolidColorBrush(Color.FromRgb(172, 172, 172)); //цвет заливки падающей свечки
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
        private int _dataSourceAreasHighlightHeight = 15; //высота линии, на которой написано название источника данных для которого следуют ниже области
        private int _candleMinWidth = 1; //минимальная ширина свечки, в пикселях
        private int _candleMaxWidth = 11; //максимальная ширина свечки, в пикселях
        private int _initialCandleWidth = 11; //начальная ширина свечки
        private int _candleWidth; //текущая ширина свечки
        private double _partOfOnePriceStepHeightForOrderVerticalLine = 0.667; //часть от высоты одного пункта центы, исходя из которой будет вычитсляться высота вертикальной линии для заявки
        private int _tradeChartScale; //масштаб графика, сколько свечек должно уместиться в видимую область графика
        private int _divideWidth = 10; //ширина разрыва
        private double _scaleValuesAddingRange = 0.05; //дополнительный диапазон для шкалы значений в каждую из сторон
        private int _scaleValuesWidth = 27; //ширина правой области со шкалой значений
        private double _tradeChartHiddenSegmentsSize = 1; //размер генерируемой области с сегментами и слева от видимых свечек, относительно размера видимых свечек. Размер отдельно для левых и правых свечек
        private double[] _indicatorAreasHeight = new double[3] { 0.15, 0.225, 0.3 }; //суммарная высота областей для индикаторов, как часть от доступной высоты под области источника данных, номер элемента соответствует количеству индикаторов и показывает суммарную высоту для них, если количество индикаторов больше, берется последний элемент
        private int _timeLineHeight = 24; //высота временной шкалы
        private List<SegmentPageTradeChart> _segments { get; set; } //сегменты из которых состоит график
        private int _segmentIndex; //текущий индекс сегмента
        private List<SectionPageTradeChart> _sections { get; set; } //секции для сегментов
        private List<SegmentOrderIndexPageTradeChart> _segmentOrders; //индексы сегмента и заявки, чтобы можно было быстро найти сегмент с заявкой
        private List<SegmentDealIndexPageTradeChart> _segmentDeals; //индексы сегмента и сделки, чтобы можно было быстро найти сегмент со сделкой
        public double СanvasTradeChartWidth { get; set; } //ширина canvas с графиком
        public double СanvasTradeChartHeight { get; set; } //высота canvas с графиком

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
            int indiactorAreaHeight = indicatorAreasCount > 0 ? (int)Math.Truncate((dataSourceAreasAvailableHeight * (indicatorAreasCount > _indicatorAreasHeight.Length ? _indicatorAreasHeight.Last() : _indicatorAreasHeight[indicatorAreasCount - 1])) / TradeChartAreas.Count - 1) : 0; //высота для областей индикаторов
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

        private void CreateSegments() //создает сегменты, на основе которых будет строиться график
        {
            //формируем сегменты для всех групп источников данных
            int[] fileIndexes = Enumerable.Repeat(0, _testing.DataSourcesCandles.Count).ToArray(); //индексы файлов для всех источников данных группы
            int[] candleIndexes = Enumerable.Repeat(0, _testing.DataSourcesCandles.Count).ToArray(); //индексы свечек для всех источников данных группы
            _sections = new List<SectionPageTradeChart>();
            _segments = new List<SegmentPageTradeChart>();
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
        }

        private void SetInitialCandleWidth() //устанавливает начальную ширину свечки
        {
            _candleWidth = _initialCandleWidth; //текущая ширина свечки
        }
        private void SetInitialSegmentIndex() //устанавливает начальный индекс сегмента
        {
            _segmentIndex = 0;
            while (_segmentIndex < _segments.Count && IsMostLeftSegmentIndex() == true) //доходим до индекса который не считается самым левым
            {
                _segmentIndex++;
            }
            _segmentIndex -= _segmentIndex > 0 ? 1 : 0; //преходим на индекс меньше, то есть на тот который последним считался самым левым
        }

        private bool IsMostLeftSegmentIndex() //определяет, является ли текущий индекс сегмента самым левым, или же можно сдвинуть индекс еще левее
        {
            int totalSegmentsWidth = 0; //суммарная ширина сегментов
            int tradeChartWidth = (int)Math.Truncate(СanvasTradeChartWidth - _scaleValuesWidth);
            int segmentIndex = _segmentIndex;
            while(totalSegmentsWidth <= tradeChartWidth && segmentIndex >= 0)
            {
                totalSegmentsWidth += _segments[segmentIndex].IsDivide ? _divideWidth : _candleWidth;
                segmentIndex--;
            }
            return totalSegmentsWidth <= tradeChartWidth ? true : false;
        }

        private void BuildTradeChart() //строит график котировок
        {
            Candles.Clear();
            Orders.Clear();
            Deals.Clear();
            int areasWidth = (int)Math.Truncate(СanvasTradeChartWidth - _scaleValuesWidth); //ширина областей
            //находим индекс самого правого сегмента на графике, а так же суммарную ширину сегментов, которые правее текущего сегмента
            int rightSegmentIndex = _segmentIndex + 1;
            int totalRightSegmentsWidth = 0;
            while(totalRightSegmentsWidth < Math.Truncate(areasWidth * _tradeChartHiddenSegmentsSize) && rightSegmentIndex < _segments.Count)
            {
                totalRightSegmentsWidth += _segments[rightSegmentIndex].IsDivide ? _divideWidth : _candleWidth;
                rightSegmentIndex++;
            }
            rightSegmentIndex--;
            //находим индекс самого левого сегмента на графике
            int leftSegmentIndex = _segmentIndex;
            int totalLeftSegmentsWidth = 0;
            while (totalLeftSegmentsWidth < Math.Truncate(areasWidth + areasWidth * _tradeChartHiddenSegmentsSize) && leftSegmentIndex >= 0)
            {
                totalLeftSegmentsWidth += _segments[leftSegmentIndex].IsDivide ? _divideWidth : _candleWidth;
                leftSegmentIndex--;
            }
            leftSegmentIndex++;
            //проходим по всем сегментам и формируем свечки, заявки, сделки и индикаторы для сегментов
            int totalSegmentsWidth = 0; //суммарная ширина сегментов
            for (int i = rightSegmentIndex; i >= leftSegmentIndex; i--)
            {
                if (_segments[i].IsDivide)
                {
                    totalSegmentsWidth += _divideWidth;
                }
                else
                {
                    //проходим по всем источникам данных текущего сегмента
                    for (int k = 0; k < _segments[i].CandleIndexes.Count; k++)
                    {
                        //добавляем свечку
                        bool isCandleFalling = _testing.DataSourcesCandles[_segments[i].CandleIndexes[k].DataSourceCandlesIndex].Candles[_segments[i].CandleIndexes[k].FileIndex][_segments[i].CandleIndexes[k].CandleIndex].C < _testing.DataSourcesCandles[_segments[i].CandleIndexes[k].DataSourceCandlesIndex].Candles[_segments[i].CandleIndexes[k].FileIndex][_segments[i].CandleIndexes[k].CandleIndex].O; //свечка падающая или растущая
                        double bodyLeft = areasWidth + totalRightSegmentsWidth - totalSegmentsWidth;
                        Candles.Insert(0, new CandlePageTradeChart { IdDataSource = _testing.DataSourcesCandles[_segments[i].CandleIndexes[k].DataSourceCandlesIndex].DataSource.Id, StrokeColor = _candleStrokeColor, FillColor = isCandleFalling ? _fallingCandleFillColor : _risingCandleFillColor, BodyLeft = bodyLeft - _candleWidth, Candle = _testing.DataSourcesCandles[_segments[i].CandleIndexes[k].DataSourceCandlesIndex].Candles[_segments[i].CandleIndexes[k].FileIndex][_segments[i].CandleIndexes[k].CandleIndex], BodyWidth = _candleWidth, StickLeft = bodyLeft - _candleWidth / 2.0 - 1 / 2.0, StickWidth = 1 });

                        //добавляем заявки
                        //проходим по всем заявкам
                        foreach(OrderIndexPageTradeChart orderIndexPageTradeChart in _segments[i].CandleIndexes[k].OrderIndexes)
                        {
                            SolidColorBrush orderFillColor = _limitBuyOrderFillColor;
                            if (_testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].TypeOrder.Id == 1) //лимитная заявка
                            {
                                orderFillColor = _testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].Direction ? _limitBuyOrderFillColor : _limitSellOrderFillColor;
                            }
                            else if(_testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].TypeOrder.Id == 2) //рыночная заявка
                            {
                                orderFillColor = _testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].Direction ? _marketBuyOrderFillColor : _marketSellOrderFillColor;
                            }
                            else if(_testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].TypeOrder.Id == 3) //стоп заявка
                            {
                                orderFillColor = _testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex].Direction ? _stopBuyOrderFillColor : _stopSellOrderFillColor;
                            }
                            double verticalLineWidth = _candleWidth >= 2 ? _candleWidth / 2 : 1;
                            Orders.Insert(0, new OrderPageTradeChart { IdDataSource = _testing.DataSourcesCandles[_segments[i].CandleIndexes[k].DataSourceCandlesIndex].DataSource.Id, FillColor = orderFillColor, Order = _testRun.Account.AllOrders[orderIndexPageTradeChart.OrderIndex], IsStart = orderIndexPageTradeChart.IsStart, HorizontalLineLeft = bodyLeft - _candleWidth, HorizontalLineWidth = _candleWidth, VerticalLineLeft = bodyLeft - _candleWidth + verticalLineWidth / 2, VerticalLineWidth = verticalLineWidth });
                        }

                        //добавляем сделки
                        //проходим по всем сделкам
                        foreach (int dealIndex in _segments[i].CandleIndexes[k].DealIndexes)
                        {
                            int triangleWidth = 0;
                            int triangleHeight = 0;
                            if(_candleWidth < 3)
                            {
                                triangleWidth = 7;
                                triangleHeight = 4;
                            }
                            else if(_candleWidth < 11)
                            {
                                triangleWidth = 9;
                                triangleHeight = 5;
                            }
                            else
                            {
                                triangleWidth = 11;
                                triangleHeight = 6;
                            }
                            Deals.Insert(0, new DealPageTradeChart { IdDataSource = _testing.DataSourcesCandles[_segments[i].CandleIndexes[k].DataSourceCandlesIndex].DataSource.Id, StrokeColor = _testRun.Account.AllDeals[dealIndex].Direction ? _buyDealStrokeColor : _sellDealStrokeColor, FillColor = _testRun.Account.AllDeals[dealIndex].Direction ? _buyDealFillColor : _sellDealFillColor, Deal = _testRun.Account.AllDeals[dealIndex], Left = bodyLeft - triangleWidth, TriangleWidth = triangleWidth, TriangleHeight = triangleHeight });
                        }
                        totalSegmentsWidth += _candleWidth;
                    }
                }
            }
        }

        public void UpdateScaleValues() //создает шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
        {
            int areasWidth = (int)Math.Truncate(СanvasTradeChartWidth - _scaleValuesWidth); //ширина областей
            int dataSourceAreasTotalHeight = _dataSourceAreasHighlightHeight; //высота областей для одного источника данных с учетом высоты строки с подписью источника данных
            foreach(TradeChartAreaPageTradeChart tradeChartArea in TradeChartAreas)
            {
                dataSourceAreasTotalHeight += tradeChartArea.AreaHeight;
            }
            //проходим по всем источникам данных
            for (int i = 0; i < DataSourcesOrderDisplayPageTradeChart.Count; i++)
            {
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
                while (isHorizontalLineLeftLowThanAreasWidth && orderIndex < ordersCurrentDs.Count)
                {
                    if(ordersCurrentDs[orderIndex].HorizontalLineLeft + ordersCurrentDs[orderIndex].HorizontalLineWidth > 0) //правая координата заявки положительная, значит эта заявка в видимой области
                    {
                        maxPrice = ordersCurrentDs[orderIndex].Order.Price > maxPrice ? ordersCurrentDs[orderIndex].Order.Price : maxPrice;
                        minPrice = ordersCurrentDs[orderIndex].Order.Price < minPrice ? ordersCurrentDs[orderIndex].Order.Price : minPrice;
                    }
                    orderIndex++;
                    if(orderIndex < ordersCurrentDs.Count)
                    {
                        if (ordersCurrentDs[orderIndex].HorizontalLineLeft > areasWidth)
                        {
                            isHorizontalLineLeftLowThanAreasWidth = false;
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

                //устанавливаем отступ сверху и высоту для видимых сделок
                dealIndex = 0;
                isDealLeftLowThanAreasWidth = dealsCurrentDs.Count > 0 ? dealsCurrentDs[dealIndex].Left <= areasWidth : false; //поставил сюда условие на непустой список, чтобы не обращаться к несуществующему элементу
                while (isDealLeftLowThanAreasWidth && dealIndex < dealsCurrentDs.Count)
                {
                    if (dealsCurrentDs[dealIndex].Left + dealsCurrentDs[dealIndex].TriangleWidth > 0) //правая координата заявки положительная, значит эта заявка в видимой области
                    {
                        int verticalOffset = dealsCurrentDs[dealIndex].Deal.Direction ? 0 : -dealsCurrentDs[dealIndex].TriangleHeight; //вертикальное смещение, для сделки на покупку отсутствует, т.к. верхний край треугольника сделки на уровне цены, для продажи равняется высоте треугольника, т.к. уровень цены находится на нижней части треугольника
                        dealsCurrentDs[dealIndex].Top = (TradeChartAreas[0].AreaHeight * (1 - (dealsCurrentDs[dealIndex].Deal.Price - minPrice) / priceRange) + verticalOffset) + currentTop;
                        dealsCurrentDs[dealIndex].Points.Clear();
                        if (dealsCurrentDs[dealIndex].Deal.Direction) //сделка на покупку
                        {
                            dealsCurrentDs[dealIndex].Points.Add(new Point(0, dealsCurrentDs[dealIndex].TriangleHeight - 1)); //левая координата
                            dealsCurrentDs[dealIndex].Points.Add(new Point(Math.Truncate(dealsCurrentDs[dealIndex].TriangleWidth / 2.0), 0)); //вершина треугольника
                            dealsCurrentDs[dealIndex].Points.Add(new Point(dealsCurrentDs[dealIndex].TriangleWidth - 1, dealsCurrentDs[dealIndex].TriangleHeight - 1)); //правая координата
                        }
                        else //сделка на продажу
                        {
                            dealsCurrentDs[dealIndex].Points.Add(new Point(0, 0)); //левая координата
                            dealsCurrentDs[dealIndex].Points.Add(new Point(Math.Truncate(dealsCurrentDs[dealIndex].TriangleWidth / 2.0), dealsCurrentDs[dealIndex].TriangleHeight - 1)); //вершина треугольника
                            dealsCurrentDs[dealIndex].Points.Add(new Point(dealsCurrentDs[dealIndex].TriangleWidth - 1, 0)); //правая координата
                        }
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
            }
        }

        public void UpdatePage() //обновляет страницу на новый источник данных
        {
            if(ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null && ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox != null)
            {
                _testing = ViewModelPageTestingResult.getInstance().TestingResult;
                _testBatch = ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox.TestBatch;
                _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
                CreateDataSourcesOrderDisplayPageTradeChart(); //создаем элементы для меню управления порядком следования областей с источниками данных
                CreateTradeChartAreas(); //создаем области для графика котировок
                CreateIndicatorsMenuItemPageTradeChart(); //создаем элементы для меню выбора областей для индикаторов
                CreateSegments(); //создаем сегменты, на основе которых будет строиться график
                SetInitialCandleWidth(); //устанавливаем начальную ширину свечки
                SetInitialSegmentIndex(); //устанавливаем начальный индекс сегмента
                BuildTradeChart(); //строим график котировок
                UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
            }
        }
        public ICommand Button1_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _segmentIndex = 10560;
                    _segmentIndex = _segmentIndex >= _segments.Count ? _segments.Count - 1 : _segmentIndex;
                    BuildTradeChart(); //строим график котировок
                    UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
                }, (obj) => true);
            }
        }
        public ICommand Button2_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _segmentIndex += 200;
                    _segmentIndex = _segmentIndex >= _segments.Count ? _segments.Count - 1 : _segmentIndex;
                    BuildTradeChart(); //строим график котировок
                    UpdateScaleValues(); //создаем шкалы значений для всех областей, а так же обновляет вертикальную позицию элементов
                }, (obj) => true);
            }
        }
    }
}
