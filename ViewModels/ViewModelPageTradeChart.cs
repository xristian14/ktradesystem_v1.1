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
        private int _scaleValuesWidth = 40; //ширина правой области со шкалой значений
        private double _tradeChartHiddenCandlesSize = 1; //размер генерируемых свечек справа и слева от видимых свечек, относительно видимых свечек. Размер отдельно для левых и правых свечек
        private double[] _indicatorAreasHeight = new double[3] { 0.15, 0.225, 0.3 }; //суммарная высота областей для индикаторов, как часть от доступной под области высоты, номер элемента соответствует количеству индикаторов и показывает суммарную высоту для них, если количество индикаторов больше, берется последний элемент
        private int _timeLineHeight = 24; //высота временной шкалы
        private FileCandleIndexesPageTradeChart[] _currentFileCandleIndexes; //текущие индексы файлов и свечек для источников данных
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

        private void DefineTradeChartInitialScaleAndSetCurrentFileCandleIndexes() //определяет начальный масштаб графика: количество свечек, которое должно поместиться на графике, и индексы текущего файла и свечки для всех источников данных
        {
            int candleWidth = 5; //ширина свечки, исходя из которой будет определяться количество свечек
            _tradeChartScale = (int)Math.Truncate((СanvasTradeChartWidth - _scaleValuesWidth) / candleWidth);
            int candlesCount = (int)Math.Round(_tradeChartScale * _minCandlesFullness); //количество свечек, которое нужно отобразить на графике
            bool isEndFiles = false; //закончились ли файлы
            int candleNumber = 1; //количество свечек которые уже прошли
            _currentFileIndexes = new int[_testBatch.DataSourceGroup.DataSourceAccordances.Count];
            _currentCandleIndexes = new int[_testBatch.DataSourceGroup.DataSourceAccordances.Count];
            while (candleNumber <= candlesCount && isEndFiles == false)
            {
                //переходим на следующую свечку
                _currentCandleIndex++;
                
            }
        }

        private Tuple<List<CandleTemplatePageTradeChart>, List<DivideTemplatePageTradeChart>> GetChartTemplate(bool isLeft) //возвращает шаблон графика, при isLeft = true вернет шаблон для левой части, иначе - для правой
        {
            List<CandleTemplatePageTradeChart> candlesTemplate = new List<CandleTemplatePageTradeChart>(); //шаблоны свечек
            List<DivideTemplatePageTradeChart> dividesTemplate = new List<DivideTemplatePageTradeChart>(); //шаблоны разрывов

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
