﻿using System;
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
        private int _candlesMinWidth = 1; //минимальная ширина свечки, в пикселях
        private int _candlesMaxWidth = 11; //максимальная ширина свечки, в пикселях
        private int _tradeChartScale; //масштаб графика, сколько свечек должно уместиться в видимую область графика
        private int _scaleValuesWidth = 40; //ширина правой области со шкалой значений
        private double _tradeChartHiddenCandlesSize = 1; //размер генерируемых свечек справа и слева от видимых свечек, относительно видимых свечек. Размер отдельно для левых и правых свечек
        private double[] _indicatorAreasHeight = new double[3] { 0.15, 0.25, 0.35 }; //суммарная высота областей для индикаторов, как часть от главной области, номер элемента соответствует количеству индикаторов и показывает суммарную высоту для них, если количество индикаторов больше, берется последний элемент
        private int _timeLineHeight = 24; //высота временной шкалы
        private int _currentFileIndex; //индекс текущего файла со свечками
        private int _currentCandleIndex; //индекс текущей свечки
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
        public ObservableCollection<TradeChartAreaPageTradeChart> TradeChartAreas //области графика с названием и указанной высотой области, первый элемент - главная область со свечками, следующие - области для индикаторов
        {
            get { return _tradeChartAreas; }
            set
            {
                _tradeChartAreas = value;
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
                }
            }
            if (propertyName == "IsButtonAddAreaChecked") //была нажата кнопка поместить в новую область
            {
                if(indicatorMenuItemPageTradeChart.IsButtonAddAreaChecked)//если кнопка в состоянии true, - добавляем область и выбираем её для текущего индикатора
                {
                    indicatorMenuItemPageTradeChart.IsButtonAddAreaChecked = false;
                    int mainAreaHeight = (int)Math.Truncate(СanvasTradeChartHeight) - _timeLineHeight;
                    int indiactorAreaHeight = (int)Math.Truncate((mainAreaHeight * (IndicatorsMenuItemPageTradeChart.Count > _indicatorAreasHeight.Length ? _indicatorAreasHeight.Last() : _indicatorAreasHeight[IndicatorsMenuItemPageTradeChart.Count])) / TradeChartAreas.Count); //высота области индикаторов
                    mainAreaHeight = mainAreaHeight - indiactorAreaHeight * TradeChartAreas.Count; //вычитаем из высоты главной области, высоту текущих областей + высоту добавляемой области
                                                                                                   //обновляем высоту областей
                    TradeChartAreas[0].AreaHeight = mainAreaHeight;
                    for (int i = 1; i < TradeChartAreas.Count; i++)
                    {
                        TradeChartAreas[i].AreaHeight = indiactorAreaHeight;
                    }
                    //добавляем новую область
                    TradeChartAreas.Add(new TradeChartAreaPageTradeChart { Name = "#" + TradeChartAreas.Count.ToString(), AreaHeight = indiactorAreaHeight });
                    //выбираем добавленную область для текущего индикатора
                    indicatorMenuItemPageTradeChart.SelectedTradeChartArea = TradeChartAreas.Last();
                }
            }
        }

        private void CreateIndicatorsMenuItemPageTradeChart() //создает элементы для меню выбора областей для индикаторов, на основе выбранного результата тестирования
        {
            for(int i = 0; i < _testing.Algorithm.AlgorithmIndicators.Count; i++)
            {
                IndicatorsMenuItemPageTradeChart.Add(IndicatorMenuItemPageTradeChart.CreateIndicator(IndicatorsMenuItemPageTradeChart_PropertyChanged, _testing.Algorithm.AlgorithmIndicators[i], TradeChartAreas, 0));
            }
        }
        private void DefineTradeChartInitialScale() //определяем начальный масштаб графика: количество свечек, которое должно поместиться на графике
        {
            int candleWidth = 3; //ширина свечки, исходя из которой будет определяться количество свечек
            _tradeChartScale = (int)Math.Truncate((СanvasTradeChartWidth - _scaleValuesWidth) / candleWidth);
        }

        private void CreateTradeChartAreas() //создает области для графика котировок
        {
            TradeChartAreas.Clear();
            TradeChartAreas.Add(new TradeChartAreaPageTradeChart { Name = "Главная область", AreaHeight = (int)Math.Truncate(СanvasTradeChartHeight) - _timeLineHeight });
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
            _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
            CreateTradeChartAreas(); //создаем области для графика котировок
            CreateIndicatorsMenuItemPageTradeChart(); //создаем элементы для меню выбора областей для индикаторов
            DefineTradeChartInitialScale(); //определяем начальный масштаб графика
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
