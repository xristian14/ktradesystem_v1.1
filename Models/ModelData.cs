using System;
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

            //считываем настройки из базы данных
            ReadSettings();
            Setting setting = Settings.Where(i => i.Id == 1).First(); //пример поиска настройки по Id

            //считываем интервалы из базы данных
            DataTable dataIntervals = _database.QuerySelect("SELECT * FROM Intervals");
            foreach(DataRow row in dataIntervals.Rows)
            {
                Interval interval = new Interval { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), Duration = TimeSpan.FromMinutes(row.Field<double>("duration")) };
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
                Currency currency = new Currency { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), DollarCost = row.Field<double>("dollarCost") };
                Currencies.Add(currency);
            }

            //считываем типы комиссии из базы данных
            DataTable dataComissiontypes = _database.QuerySelect("SELECT * FROM Comissiontypes");
            foreach (DataRow row in dataComissiontypes.Rows)
            {
                Comissiontype сomissiontype = new Comissiontype { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name") };
                Comissiontypes.Add(сomissiontype);
            }

            //считываем типы значений параметров
            DataTable dataParameterValueTypes = _database.QuerySelect("SELECT * FROM ParameterValueTypes");
            foreach (DataRow row in dataParameterValueTypes.Rows)
            {
                ParameterValueType parameterValueType = new ParameterValueType { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name") };
                ParameterValueTypes.Add(parameterValueType);
            }

            //считываем критерии оценки тестирвоания
            DataTable dataEvaluationCriterias = _database.QuerySelect("SELECT * FROM EvaluationCriterias");
            foreach (DataRow row in dataEvaluationCriterias.Rows)
            {
                EvaluationCriteria evaluationCriteria = new EvaluationCriteria { Id = (int)row.Field<long>("id"), ShortName = row.Field<string>("shortName"), Name = row.Field<string>("name"), Description = row.Field<string>("description"), Script = row.Field<string>("script") };
                EvaluationCriterias.Add(evaluationCriteria);
            }

            //считываем типы заявок
            DataTable dataTypeOrders = _database.QuerySelect("SELECT * FROM TypeOrders");
            foreach (DataRow row in dataTypeOrders.Rows)
            {
                TypeOrder typeOrder = new TypeOrder { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name") };
                TypeOrders.Add(typeOrder);
            }

            ReadDataSources();
            NotifyDataSourcesSubscribers();

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

        private ObservableCollection<Setting> _settings = new ObservableCollection<Setting>(); //настройки
        public ObservableCollection<Setting> Settings
        {
            get { return _settings; }
            private set
            {
                _settings = value;
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

        private ObservableCollection<DataSource> _dataSourcesForSubscribers = new ObservableCollection<DataSource>(); //источники данных для подписчиков (т.к. при подписке на основной будет ошибка изменения UI компонентов вне основного потока UI)
        public ObservableCollection<DataSource> DataSourcesForSubscribers
        {
            get { return _dataSourcesForSubscribers; }
            private set
            {
                _dataSourcesForSubscribers = value;
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

        private ObservableCollection<ParameterValueType> _parameterValueTypes = new ObservableCollection<ParameterValueType>(); //типы значений параметров
        public ObservableCollection<ParameterValueType> ParameterValueTypes
        {
            get { return _parameterValueTypes; }
            private set
            {
                _parameterValueTypes = value;
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

        private ObservableCollection<EvaluationCriteria> _evaluationCriterias = new ObservableCollection<EvaluationCriteria>(); //критерии оценки тестирования
        public ObservableCollection<EvaluationCriteria> EvaluationCriterias
        {
            get { return _evaluationCriterias; }
            private set
            {
                _evaluationCriterias = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TypeOrder> _typeOrders = new ObservableCollection<TypeOrder>(); //типы заявок
        public ObservableCollection<TypeOrder> TypeOrders
        {
            get { return _typeOrders; }
            private set
            {
                _typeOrders = value;
                OnPropertyChanged();
            }
        }

        public void ReadSettings()
        {
            Settings.Clear();
            DataTable dataSettings = _database.QuerySelect("SELECT * FROM Settings");
            foreach (DataRow row in dataSettings.Rows)
            {
                Setting setting = new Setting { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), IdSettingType = (int)row.Field<long>("idSettingType") };
                switch ((int)row.Field<long>("idSettingType"))
                {
                    case 1:
                        setting.BoolValue = int.Parse(row.Field<string>("value")) == 1 ? true : false;
                        break;
                    case 2:
                        setting.IntValue = int.Parse(row.Field<string>("value"));
                        break;
                    case 3:
                        setting.DoubleValue = double.Parse(row.Field<string>("value"));
                        break;
                }
                Settings.Add(setting);
            }
        }

        public DataSource SelectDataSourceById(int id)
        {
            DataTable data = _database.SelectDataSourceFromId(id);
            DataSource dataSource = new DataSource();
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
                ds.StartDate = DateTime.Parse(row.Field<string>("startDate"));
                ds.EndDate = DateTime.Parse(row.Field<string>("endDate"));

                //считываем dataSourceFiles для источника данных
                ds.DataSourceFiles = new List<DataSourceFile>();
                DataTable dataDataSourceFiles = _database.SelectDataSourceFiles(ds.Id);
                foreach (DataRow row1 in dataDataSourceFiles.Rows)
                {
                    DataSourceFile dataSourceFile = new DataSourceFile { Id = (int)row1.Field<long>("id"), Path = row1.Field<string>("path"), IdDataSource = (int)row1.Field<long>("idDataSource") };
                    dataSourceFile.DataSourceFileWorkingPeriods = new List<DataSourceFileWorkingPeriod>();
                    //считываем dataSourceFileWorkingPeriods для файла источника данных
                    DataTable dataDataSourceFileWorkingPeriods = _database.SelectDataSourceFileWorkingPeriods(dataSourceFile.Id);
                    foreach (DataRow row2 in dataDataSourceFileWorkingPeriods.Rows)
                    {
                        DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod = new DataSourceFileWorkingPeriod { Id = (int)row2.Field<long>("id"), StartPeriod = DateTime.Parse(row2.Field<string>("startPeriod")), TradingStartTime = DateTime.Parse(row2.Field<string>("tradingStartTime")), TradingEndTime = DateTime.Parse(row2.Field<string>("tradingEndTime")), IdDataSourceFile = (int)row2.Field<long>("idDataSourceFile") };
                        dataSourceFile.DataSourceFileWorkingPeriods.Add(dataSourceFileWorkingPeriod);
                    }
                    ds.DataSourceFiles.Add(dataSourceFile);
                }
                dataSource = ds;
            }
            return dataSource;
        }

        public void ReadDataSources()
        {
            //считываем источники данных
            DataSources.Clear();
            DataTable data = _database.QuerySelect("SELECT * FROM Datasources");
            
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
                ds.StartDate = DateTime.Parse(row.Field<string>("startDate"));
                ds.EndDate = DateTime.Parse(row.Field<string>("endDate"));

                //считываем dataSourceFiles для источника данных
                ds.DataSourceFiles = new List<DataSourceFile>();
                DataTable dataDataSourceFiles = _database.SelectDataSourceFiles(ds.Id);
                foreach (DataRow row1 in dataDataSourceFiles.Rows)
                {
                    DataSourceFile dataSourceFile = new DataSourceFile { Id = (int)row1.Field<long>("id"), Path = row1.Field<string>("path"), IdDataSource = (int)row1.Field<long>("idDataSource") };
                    dataSourceFile.DataSourceFileWorkingPeriods = new List<DataSourceFileWorkingPeriod>();
                    //считываем dataSourceFileWorkingPeriods для файла источника данных
                    DataTable dataDataSourceFileWorkingPeriods = _database.SelectDataSourceFileWorkingPeriods(dataSourceFile.Id);
                    foreach (DataRow row2 in dataDataSourceFileWorkingPeriods.Rows)
                    {
                        DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod = new DataSourceFileWorkingPeriod { Id = (int)row2.Field<long>("id"), StartPeriod = DateTime.Parse(row2.Field<string>("startPeriod")), TradingStartTime = DateTime.Parse(row2.Field<string>("tradingStartTime")), TradingEndTime = DateTime.Parse(row2.Field<string>("tradingEndTime")), IdDataSourceFile = (int)row2.Field<long>("idDataSourceFile") };
                        dataSourceFile.DataSourceFileWorkingPeriods.Add(dataSourceFileWorkingPeriod);
                    }
                    ds.DataSourceFiles.Add(dataSourceFile);
                }
                DataSources.Add(ds);
            }
        }

        public void NotifyDataSourcesSubscribers() //выполняет обновление DataSourcesForSubscribers, вследствии чего UI обновится на новые данные
        {
            DataSourcesForSubscribers.Clear();
            foreach(DataSource dataSource in DataSources)
            {
                DataSourcesForSubscribers.Add(dataSource);
            }
        }

        public void ReadIndicators()
        {
            //считываем шаблоны параметров из базы данных
            IndicatorParameterTemplates.Clear();

            DataTable dataParameterTemplates = _database.QuerySelect("SELECT * FROM IndicatorParameterTemplates");
            foreach (DataRow row in dataParameterTemplates.Rows)
            {
                int idParameterValueType = (int)row.Field<long>("idParameterValueType");
                ParameterValueType parameterValueType = idParameterValueType == ParameterValueTypes[0].Id ? ParameterValueTypes[0] : ParameterValueTypes[1];
                IndicatorParameterTemplate parameterTemplate = new IndicatorParameterTemplate { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), Description = row.Field<string>("description"), IdIndicator = (int)row.Field<long>("idIndicator"), ParameterValueType = parameterValueType };
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

            CheckMissingIndicatorParameterRange();

            ReadAlgorithms(); //считываем алгоритмы, т.к. после изменения индикаторов, при каскадном удалении параметров или индикаторов, в алгоритмах будут новые данные
        }

        private void CheckMissingIndicatorParameterRange() //при добавлении шаблона параметра индикатору, в алгоритме, запись со значениями данного параметра не добавляется. Эта функция определяет диапазоны значений которых "недостаёт" и добавляет их.
        {
            //проходим по алгоритмам, далее по диапазонам параметров и формируем список с шаблонами параметров соответствующих диапазонам. Далее проходимся по полученному списку шаблонов параметров и формируем список индикаторов для данного набора шаблонов параметров. Далее проходим по индикаторам, и для каждого находим полный список шаблонов параметров. Далее ищем id шаблона параметров в списке диапазонов параметров, и если не находим добавляем в список диапазонов значений которые нужно будет добавить. Далее проходимся по этому списку и выполняем операацию insert для каждого.
            List<IndicatorParameterRange> indicatorParameterRangesForInsert = new List<IndicatorParameterRange>(); //список с диапазонами значений которые необходимо добавить в БД
            foreach(Algorithm algorithm in Algorithms)
            {
                List<IndicatorParameterTemplate> indicatorParameterTemplates = new List<IndicatorParameterTemplate>(); //список с шаблонами параметров, соответствующих диапазонам значений
                foreach(IndicatorParameterRange indicatorParameterRange in algorithm.IndicatorParameterRanges)
                {
                    foreach(IndicatorParameterTemplate indicatorParameterTemplate in IndicatorParameterTemplates)
                    {
                        if(indicatorParameterRange.IndicatorParameterTemplate.Id == indicatorParameterTemplate.Id)
                        {
                            indicatorParameterTemplates.Add(indicatorParameterTemplate);
                        }
                    }
                }
                List<Indicator> indicators = new List<Indicator>(); //список с индикаторами, которые используются в данном алгоритме
                foreach(IndicatorParameterTemplate itemIPT in indicatorParameterTemplates)
                {
                    //получаем индикатор текущего шаблона параметра
                    Indicator currentIndicator = new Indicator();
                    foreach(Indicator itemIndicator in Indicators)
                    {
                        if (itemIPT.IdIndicator == itemIndicator.Id)
                        {
                            currentIndicator = itemIndicator;
                        }
                    }
                    //проверяем, есть ли текущий индикатор в спискe с индикаторами, которые используются в данном алгоритме. Если нет - то добавляем его в этот список.
                    if (indicators.Contains(currentIndicator) == false)
                    {
                        indicators.Add(currentIndicator);
                    }
                }
                //проходим по индикаторам, и далее по шаблонам параметров индикатора
                foreach(Indicator indicator in indicators)
                {
                    foreach(IndicatorParameterTemplate indicatorParameterTemplate in indicator.IndicatorParameterTemplates)
                    {
                        //ищем id текущего шаблона в диапазонах параметров
                        bool isIdIndicatorFind = false;
                        foreach(IndicatorParameterRange indicatorParameterRange in algorithm.IndicatorParameterRanges)
                        {
                            if(indicatorParameterRange.IndicatorParameterTemplate.Id == indicatorParameterTemplate.Id)
                            {
                                isIdIndicatorFind = true;
                            }
                        }
                        if(isIdIndicatorFind == false) //если параметр не найден, формируем IndicatorParameterRange и добавляем в список с диапазонами значений которые необходимо вставить
                        {
                            IndicatorParameterRange indicatorParameterRangeForInsert = new IndicatorParameterRange { IdAlgorithm = algorithm.Id, IndicatorParameterTemplate = indicatorParameterTemplate };
                            indicatorParameterRangesForInsert.Add(indicatorParameterRangeForInsert);
                        }
                    }
                }
            }
            //вставляем диапазоны значений параметров
            foreach(IndicatorParameterRange indicatorParameterRange1 in indicatorParameterRangesForInsert)
            {
                _database.InsertIndicatorParameterRange(indicatorParameterRange1);
            }
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
            DataTable dataIndicatorParameterRanges = _database.QuerySelect("SELECT ipr.*, ipt.idIndicator AS iptIdIndicator, ipt.id AS iptId, ipt.name AS iptName FROM IndicatorParameterRanges AS ipr LEFT JOIN IndicatorParameterTemplates AS ipt ON ipr.idIndicatorParameterTemplate = ipt.id     UNION ALL     SELECT ipr.*, ipt.idIndicator AS iptIdIndicator, ipt.id AS iptId, ipt.name AS iptName FROM IndicatorParameterTemplates AS ipt LEFT JOIN IndicatorParameterRanges AS ipr ON ipr.idIndicatorParameterTemplate = ipt.id     WHERE ipr.idIndicatorParameterTemplate IS NULL AND ipr.id NOTNULL     ORDER BY idIndicator");
            foreach (DataRow row in dataIndicatorParameterRanges.Rows)
            {
                int idIndicatorParameterTemplate = (int)row.Field<long>("idIndicatorParameterTemplate");
                IndicatorParameterTemplate indicatorParameterTemplate1 = new IndicatorParameterTemplate();
                Indicator indicator = new Indicator();
                foreach (IndicatorParameterTemplate indicatorParameterTemplate in IndicatorParameterTemplates)
                {
                    if (indicatorParameterTemplate.Id == idIndicatorParameterTemplate)
                    {
                        indicator = indicatorParameterTemplate.Indicator;
                        indicatorParameterTemplate1 = indicatorParameterTemplate;
                    }
                }
                IndicatorParameterRange indicatorParameterRange = new IndicatorParameterRange { Id = (int)row.Field<long>("id"), MinValue = row.Field<double>("minValue"), MaxValue = row.Field<double>("maxValue"), Step = row.Field<double>("step"), IdAlgorithm = (int)row.Field<long>("idAlgorithm"), IndicatorParameterTemplate = indicatorParameterTemplate1, Indicator = indicator };
                if((int?)row.Field<long?>("isStepPercent") != null)
                {
                    if((int?)row.Field<long?>("isStepPercent") == 1)
                    {
                        indicatorParameterRange.IsStepPercent = true;
                    }
                    else
                    {
                        indicatorParameterRange.IsStepPercent = false;
                    }
                }
                
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
                int idParameterValueType = (int)row.Field<long>("idParameterValueType");
                ParameterValueType parameterValueType = idParameterValueType == ParameterValueTypes[0].Id ? ParameterValueTypes[0] : ParameterValueTypes[1];
                AlgorithmParameter algorithmParameter = new AlgorithmParameter { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), Description = row.Field<string>("description"), MinValue = row.Field<double>("minValue"), MaxValue = row.Field<double>("maxValue"), Step = row.Field<double>("step"), IsStepPercent = isStepPercent, IdAlgorithm = (int)row.Field<long>("idAlgorithm"), ParameterValueType = parameterValueType };
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
