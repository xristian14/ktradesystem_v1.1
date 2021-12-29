﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Data;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    //хранит таблицы БД, реализует методы работы с БД
    class ModelData : ModelBase
    {
        private static ModelData _instance;

        public static ModelData getInstance()
        {
            if (_instance == null)
            {
                _instance = new ModelData();
            }
            return _instance;
        }

        private ModelData()
        {
            _database = Database.getInstance(); //вызываем подключение к бд

            //считываем интервалы из базы данных
            DataTable dataIntervals = _database.QuerySelect("SELECT * FROM Intervals");
            foreach(DataRow row in dataIntervals.Rows)
            {
                Interval interval = new Interval { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name") };
                Intervals.Add(interval);
            }

            //считываем инструменты из базы данных
            DataTable dataInstruments = _database.QuerySelect("SELECT * FROM Instruments");
            foreach (DataRow row in dataInstruments.Rows)
            {
                Instrument instrument = new Instrument { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name") };
                Instruments.Add(instrument);
            }

            //считываем валюты из базы данных
            DataTable dataCurrencies = _database.QuerySelect("SELECT * FROM Currencies");
            foreach (DataRow row in dataCurrencies.Rows)
            {
                Currency currency = new Currency { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name") };
                Currencies.Add(currency);
            }

            //считываем типы комиссии из базы данных
            DataTable dataComissiontypes = _database.QuerySelect("SELECT * FROM Comissiontypes");
            foreach (DataRow row in dataComissiontypes.Rows)
            {
                Comissiontype сomissiontype = new Comissiontype { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name") };
                Comissiontypes.Add(сomissiontype);
            }

            ReadDataSources();

            ReadIndicators();
        }

        private Database _database;

        private ObservableCollection<Interval> _intervals = new ObservableCollection<Interval>(); //интервалы (минутный, пятиминутный, ...)

        public ObservableCollection<Interval> Intervals
        {
            get { return _intervals; }
            private set
            {
                _intervals = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Instrument> _instruments = new ObservableCollection<Instrument>();

        public ObservableCollection<Instrument> Instruments
        {
            get { return _instruments; }
            set
            {
                _instruments = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Currency> _currencies = new ObservableCollection<Currency>();

        public ObservableCollection<Currency> Currencies
        {
            get { return _currencies; }
            set
            {
                _currencies = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Comissiontype> _comissiontypes = new ObservableCollection<Comissiontype>();

        public ObservableCollection<Comissiontype> Comissiontypes
        {
            get { return _comissiontypes; }
            set
            {
                _comissiontypes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DataSource> _dataSources = new ObservableCollection<DataSource>(); //источники данных
        public ObservableCollection<DataSource> DataSources
        {
            get { return _dataSources; }
            private set
            {
                _dataSources = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IndicatorParameterTemplate> _indicatorParameterTemplates = new ObservableCollection<IndicatorParameterTemplate>(); //шаблоны параметров (для индикаторов)
        public ObservableCollection<IndicatorParameterTemplate> IndicatorParameterTemplates
        {
            get { return _indicatorParameterTemplates; }
            private set
            {
                _indicatorParameterTemplates = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Indicator> _indicators = new ObservableCollection<Indicator>(); //индикаторы
        public ObservableCollection<Indicator> Indicators
        {
            get { return _indicators; }
            private set
            {
                _indicators = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DataSourceTemplate> _dataSourceTemplates = new ObservableCollection<DataSourceTemplate>(); //макеты источников данных
        public ObservableCollection<DataSourceTemplate> DataSourceTemplates
        {
            get { return _dataSourceTemplates; }
            private set
            {
                _dataSourceTemplates = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<IndicatorParameterRange> _indicatorParameterRanges = new ObservableCollection<IndicatorParameterRange>(); //диапазоны значений для параметров индикаторов
        public ObservableCollection<IndicatorParameterRange> IndicatorParameterRanges
        {
            get { return _indicatorParameterRanges; }
            private set
            {
                _indicatorParameterRanges = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<AlgorithmParameter> _algorithmParameters = new ObservableCollection<AlgorithmParameter>(); //оптимизируемые параметры алгоритмов
        public ObservableCollection<AlgorithmParameter> AlgorithmParameters
        {
            get { return _algorithmParameters; }
            private set
            {
                _algorithmParameters = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Algorithm> _algorithms = new ObservableCollection<Algorithm>(); //алгоритмы
        public ObservableCollection<Algorithm> Algorithms
        {
            get { return _algorithms; }
            private set
            {
                _algorithms = value;
                OnPropertyChanged();
            }
        }

        public void ReadDataSources()
        {
            DataTable data = _database.QuerySelect("SELECT * FROM Datasources");
            DataSources.Clear();
            foreach (DataRow row in data.Rows)
            {
                Datatables.DataSource ds = new Datatables.DataSource();
                ds.Id = (int)row.Field<long>("id");
                ds.Name = row.Field<string>("name");
                int idCurrency = (int)row.Field<long>("idCurrency");
                foreach (Currency currency in Currencies)
                {
                    if (currency.Id == idCurrency)
                    {
                        ds.Currency = currency;
                    }
                }
                int idInterval = (int)row.Field<long>("idInterval");
                foreach (Interval interval in Intervals)
                {
                    if (interval.Id == idInterval)
                    {
                        ds.Interval = interval;
                    }
                }
                int idInstrument = (int)row.Field<long>("idInstrument");
                foreach (Instrument instrument in Instruments)
                {
                    if (instrument.Id == idInstrument)
                    {
                        ds.Instrument = instrument;
                    }
                }
                ds.Cost = row.Field<double?>("cost");
                int idComissiontype = (int)row.Field<long>("idComissiontype");
                foreach (Comissiontype comissiontype in Comissiontypes)
                {
                    if (comissiontype.Id == idComissiontype)
                    {
                        ds.Comissiontype = comissiontype;
                    }
                }
                ds.Comission = row.Field<double>("comission");
                ds.PriceStep = row.Field<double>("priceStep");
                ds.CostPriceStep = row.Field<double>("costPriceStep");
                ds.Files = row.Field<string>("files");

                DataSources.Add(ds);
            }
        }

        public void ReadIndicators()
        {
            //считываем шаблоны параметров из базы данных
            IndicatorParameterTemplates.Clear();

            DataTable dataParameterTemplates = _database.QuerySelect("SELECT * FROM IndicatorParameterTemplates");
            foreach (DataRow row in dataParameterTemplates.Rows)
            {
                IndicatorParameterTemplate parameterTemplate = new IndicatorParameterTemplate { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), Description = row.Field<string>("description"), IdIndicator = (int)row.Field<long>("idIndicator") };
                IndicatorParameterTemplates.Add(parameterTemplate);
            }

            //считываем индикаторы из базы данных
            Indicators.Clear();

            DataTable dataIndicators = _database.QuerySelect("SELECT * FROM Indicators");
            foreach (DataRow row in dataIndicators.Rows)
            {
                int idIndicator = (int)row.Field<long>("id");
                bool isStandart = false;
                if((int)row.Field<long>("isStandart") == 1)
                {
                    isStandart = true;
                }
                Indicator indicator = new Indicator { Id = idIndicator, Name = row.Field<string>("name"), Description = row.Field<string>("description"), Script = row.Field<string>("script"), IsStandart = isStandart };
                List<IndicatorParameterTemplate> indicatorParameterTemplates = new List<IndicatorParameterTemplate>();
                foreach (IndicatorParameterTemplate item in IndicatorParameterTemplates)
                {
                    if (item.IdIndicator == idIndicator)
                    {
                        item.Indicator = indicator;
                        indicatorParameterTemplates.Add(item);
                    }
                }
                indicator.IndicatorParameterTemplates = indicatorParameterTemplates;
                Indicators.Add(indicator);
            }

            //надо здесь добавить проверку на недостающее indicatorParameterRange, при добавлении нового IndicatorParameterTemplate, перед считывание алгоритмов, т.к. сложный запрос в считывании алгоритмов
            ReadAlgorithms(); //считываем алгоритмы, т.к. после изменения индикаторов, при каскадном удалении параметров или индикаторов, в алгоритмах будут новые данные
        }

        public void ReadAlgorithms()
        {
            //считываем макеты источников данных из базы данных
            DataSourceTemplates.Clear();

            DataTable dataDataSourceTemplates = _database.QuerySelect("SELECT * FROM DataSourceTemplates");
            foreach (DataRow row in dataDataSourceTemplates.Rows)
            {
                DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), Description = row.Field<string>("description"), IdAlgorithm = (int)row.Field<long>("idAlgorithm") };
                DataSourceTemplates.Add(dataSourceTemplate);
            }

            //считываем диапазоны значений параметров индикаторов данных из базы данных
            IndicatorParameterRanges.Clear();

            //данный запрос выполняет операцию FULL OUTER JOIN с помощью двух LEFT JOIN и UNION т.к. FULL OUTER JOIN не поддерживается, это нужно чтобы отсортировать записи по id индикатора к которому принадлежит параметр, чтобы в таблице значений параметров выбранных индикаторов, записи шли по индикаторам
            DataTable dataIndicatorParameterRanges = _database.QuerySelect("SELECT ipr.*, ipt.idIndicator AS iptIdIndicator, ipt.id AS iptId, ipt.name AS iptName FROM IndicatorParameterRanges AS ipr LEFT JOIN IndicatorParameterTemplates AS ipt ON ipr.idIndicatorParameterTemplate = ipt.id    UNION ALL    SELECT ipr.*, ipt.idIndicator AS iptIdIndicator, ipt.id AS iptId, ipt.name AS iptName FROM IndicatorParameterTemplates AS ipt LEFT JOIN IndicatorParameterRanges AS ipr ON ipr.idIndicatorParameterTemplate = ipt.id    WHERE ipr.idIndicatorParameterTemplate IS NULL    ORDER BY idIndicator");
            foreach (DataRow row in dataIndicatorParameterRanges.Rows)
            {
                bool isStepPercent = false;
                if((int)row.Field<long>("isStepPercent") == 1)
                {
                    isStepPercent = true;
                }
                int idIndicatorParameterTemplate = (int)row.Field<long>("idIndicatorParameterTemplate");
                Indicator indicator = new Indicator();
                foreach(IndicatorParameterTemplate indicatorParameterTemplate in IndicatorParameterTemplates)
                {
                    if(indicatorParameterTemplate.Id == idIndicatorParameterTemplate)
                    {
                        indicator = indicatorParameterTemplate.Indicator;
                    }
                }
                IndicatorParameterRange indicatorParameterRange = new IndicatorParameterRange { Id = (int)row.Field<long>("id"), MinValue = row.Field<double>("minValue"), MaxValue = row.Field<double>("maxValue"), Step = row.Field<double>("step"), IsStepPercent = isStepPercent, IdAlgorithm = (int)row.Field<long>("idAlgorithm"), IdIndicatorParameterTemplate = idIndicatorParameterTemplate, Indicator = indicator };
                IndicatorParameterRanges.Add(indicatorParameterRange);
            }

            //считываем параметры алгоритмов из базы данных
            AlgorithmParameters.Clear();

            DataTable dataAlgorithmParameters = _database.QuerySelect("SELECT * FROM AlgorithmParameters");
            foreach (DataRow row in dataAlgorithmParameters.Rows)
            {
                bool isStepPercent = false;
                if((int)row.Field<long>("isStepPercent") == 1)
                {
                    isStepPercent = true;
                }
                AlgorithmParameter algorithmParameter = new AlgorithmParameter { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), Description = row.Field<string>("description"), MinValue = row.Field<double>("minValue"), MaxValue = row.Field<double>("maxValue"), Step = row.Field<double>("step"), IsStepPercent = isStepPercent, IdAlgorithm = (int)row.Field<long>("idAlgorithm") };
                AlgorithmParameters.Add(algorithmParameter);
            }

            //считываем алгоритмы из базы данных
            Algorithms.Clear();

            DataTable dataAlgorithms = _database.QuerySelect("SELECT * FROM Algorithms");
            foreach (DataRow row in dataAlgorithms.Rows)
            {
                int idAlgorithm = (int)row.Field<long>("id");
                Algorithm algorithm = new Algorithm { Id = idAlgorithm, Name = row.Field<string>("name"), Description = row.Field<string>("description"), Script = row.Field<string>("script") };

                List<DataSourceTemplate> dataSourceTemplates = new List<DataSourceTemplate>();
                foreach (DataSourceTemplate dataSourceTemplate in DataSourceTemplates)
                {
                    if (dataSourceTemplate.IdAlgorithm == idAlgorithm)
                    {
                        dataSourceTemplates.Add(dataSourceTemplate);
                    }
                }
                algorithm.DataSourceTemplates = dataSourceTemplates;

                List<IndicatorParameterRange> indicatorParameterRanges = new List<IndicatorParameterRange>();
                foreach (IndicatorParameterRange indicatorParameterRange in IndicatorParameterRanges)
                {
                    if (indicatorParameterRange.IdAlgorithm == idAlgorithm)
                    {
                        indicatorParameterRanges.Add(indicatorParameterRange);
                    }
                }
                algorithm.IndicatorParameterRanges = indicatorParameterRanges;

                List<AlgorithmParameter> algorithmParameters = new List<AlgorithmParameter>();
                foreach (AlgorithmParameter algorithmParameter in AlgorithmParameters)
                {
                    if (algorithmParameter.IdAlgorithm == idAlgorithm)
                    {
                        algorithmParameters.Add(algorithmParameter);
                    }
                }
                algorithm.AlgorithmParameters = algorithmParameters;

                Algorithms.Add(algorithm);
            }
        }
    }
}
