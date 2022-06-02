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
        private List<SectionPageTradeChart> _sections { get; set; } //секции для сегментов
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
                    if (DateTime.Compare(currentDateTime, laterDateTime) > 0) //если текущая дата позже самой поздней, обновляем самую позднюю дату
                    {
                        laterDateTime = currentDateTime;
                    }
                    //формируем сегмент
                    SegmentPageTradeChart segment = new SegmentPageTradeChart();
                    segment.Section = _sections.Last();
                    segment.CandleIndexes = new List<CandleIndexPageTradeChart>();
                    //проходим по источникам данных секции, и добавляем в сегмент свечки тех которые имеют текущую дату
                    for (int i = 0; i < _testing.DataSourcesCandles.Count; i++)
                    {
                        if (_sections.Last().DataSources.Where(a => a.Id == _testing.DataSourcesCandles[i].DataSource.Id).Any()) //если в текущей секции имеется текущий источник данных
                        {
                            if (DateTime.Compare(currentDateTime, _testing.DataSourcesCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime) == 0) //если текущая дата и дата текущей свечки у текущего источника данных равны
                            {
                                segment.CandleIndexes.Add(new CandleIndexPageTradeChart { DataSourceCandlesIndex = i, FileIndex = fileIndexes[i], CandleIndex = candleIndexes[i] });
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
        }

        private void DefineTradeChartInitialScaleAndSetCurrentFileCandleIndexes() //определяет начальный масштаб графика: количество свечек, которое должно поместиться на графике
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
        private void SetInitialSegmentIndex() //определяет начальный индекс сегмента
        {

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
            if(ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null && ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox != null)
            {
                _testing = ViewModelPageTestingResult.getInstance().TestingResult;
                _testBatch = ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox.TestBatch;
                _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
                CreateDataSourcesOrderDisplayPageTradeChart(); //создаем элементы для меню управления порядком следования областей с источниками данных
                CreateTradeChartAreas(); //создаем области для графика котировок
                CreateIndicatorsMenuItemPageTradeChart(); //создаем элементы для меню выбора областей для индикаторов
                CreateSegments(); //создаем сегменты, на основе которых будет строиться график
                DefineTradeChartInitialScaleAndSetCurrentFileCandleIndexes(); //определяем начальный масштаб графика: количество свечек, которое должно поместиться на графике, и индексы текущего файла и свечки
                BiuldTradeChart(); //строим график котировок
            }
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
