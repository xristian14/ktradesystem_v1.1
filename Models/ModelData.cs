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
    public class ModelData : ModelBase
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
            DataTable dataEvaluationCriterias = _database.QuerySelect("SELECT * FROM EvaluationCriterias ORDER BY \"orderCalculate\"");
            foreach (DataRow row in dataEvaluationCriterias.Rows)
            {
                EvaluationCriteria evaluationCriteria = new EvaluationCriteria
                {
                    Id = (int)row.Field<long>("id"),
                    OrderCalculate = (int)row.Field<long>("orderCalculate"),
                    OrderView = (int)row.Field<long>("orderView"),
                    Name = row.Field<string>("name"),
                    Description = row.Field<string>("description"),
                    Script = row.Field<string>("script"),
                    IsDoubleValue = (int)row.Field<long>("isDoubleValue") == 1 ? true : false,
                    IsBestPositive = (int)row.Field<long>("isBestPositive") == 1 ? true : false,
                    IsHaveBestAndWorstValue = (int)row.Field<long>("isHaveBestAndWorstValue") == 1 ? true : false
                };
                if (evaluationCriteria.IsHaveBestAndWorstValue)
                {
                    evaluationCriteria.BestValue = row.Field<double>("bestValue");
                    evaluationCriteria.WorstValue = row.Field<double>("worstValue");
                }
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

        private ObservableCollection<AlgorithmIndicator> _algorithmIndicators = new ObservableCollection<AlgorithmIndicator>(); //индикаторы алгоритмов
        public ObservableCollection<AlgorithmIndicator> AlgorithmIndicators
        {
            get { return _algorithmIndicators; }
            private set
            {
                _algorithmIndicators = value;
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
                ds.Cost = row.Field<double>("cost");
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
                ds.Cost = row.Field<double>("cost");
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

            CheckAlgorithmIndicators();

            ReadAndSetAlgorithms(); //считываем алгоритмы, т.к. после изменения индикаторов, при каскадном удалении параметров или индикаторов, в алгоритмах будут новые данные
        }

        private void CheckAlgorithmIndicators() //проверяет, все ли параметры индикаторов имеются в индикаторах алгоритмов, если нет, - добавляются и добавляется параметр алгоритма для них. Так же проверяется, все ли параметры индикаторов алгоритмов имеют тот же тип значения что и выбранный параметр алгоритма, если тип значения не совпадает, - добавляется новый параметр алгоритма и выбирается для параметра индикатора. Добавлено может быть не более 2-х параметров алгоритма для одного алгоритма. Если необходимы параметры под разные нужды (сменился тип + добавился параметр индикатора), тогда в описании созданного параметра алгоритма будут указаны все его функции
        {
            //считываем текущие алгоритмы
            ObservableCollection<DataSourceTemplate> currentDataSourceTemplates = ReadDataSourceTemplates();
            ObservableCollection<AlgorithmParameter> currentAlgorithmParameters = ReadAlgorithmParameters();
            ObservableCollection<AlgorithmIndicator> currentAlgorithmIndicators = ReadAlgorithmIndicators(Indicators);
            ObservableCollection<IndicatorParameterRange> currentIndicatorParameterRanges = ReadIndicatorParameterRanges(IndicatorParameterTemplates, currentAlgorithmParameters, currentAlgorithmIndicators);
            ObservableCollection<Algorithm> currentAlgorithms = ReadAlgorithms(currentDataSourceTemplates, currentAlgorithmParameters, currentAlgorithmIndicators);

            //проходим по алгоритмам
            foreach(Algorithm algorithm in currentAlgorithms)
            {
                ObservableCollection<AlgorithmParameter> newAlgorithmParameters = ReadAlgorithmParameters(); //параметры алгоритма, нужны чтобы при добавлении двух параметров алгоритма можно было подобрать уникальное имя для второго учитывая имя первого
                AlgorithmParameter algorithmParameterInt = new AlgorithmParameter(); //созданный параметр алгоритма с типом int
                AlgorithmParameter algorithmParameterDouble = new AlgorithmParameter(); //созданный параметр алгоритма с типом double
                bool isIntParameterCreated = false; //был ли создан параметр алгоритма с типом int
                bool isDoubleParameterCreated = false; //был ли создан параметр алгоритма с типом double
                string msgIntParameter = ""; //сообщение которое будет в описании созданного параметра алгоритма с типом int, уведомляющее о том для чего был создан данный параметр алгоритма
                string msgDoubleParameter = ""; //сообщение которое будет в описании созданного параметра алгоритма с типом double, уведомляющее о том для чего был создан данный параметр алгоритма
                                                //проходим по всем индикаторам алгоритма
                foreach (AlgorithmIndicator algorithmIndicator in algorithm.AlgorithmIndicators)
                {
                    //проходим по всем шаблонам параметров индикатора
                    foreach(IndicatorParameterTemplate indicatorParameterTemplate in algorithmIndicator.Indicator.IndicatorParameterTemplates)
                    {
                        bool isNeedAlgorithmParameterInt = false; //нужен ли параметр алгоритма с типом int
                        bool isNeedAlgorithmParameterDouble = false; //нужен ли параметр алгоритма с типом double
                        //проходим дважды по циклу: в первый раз определяем все параметры алгоритма которые нужны для данной ситуации и добавляем эти параметры если они не были добавлены, во второй раз добавляем недостающие параметры индикатора алгоритма или меняем для параметра индикатора алгоритма параметр алгоритма
                        int y = 0;
                        do
                        {
                            y++;
                            //если данного шаблона параметра нет в параметрах индикатора алгоритма
                            if (algorithmIndicator.IndicatorParameterRanges.Where(j => j.IndicatorParameterTemplate.Id == indicatorParameterTemplate.Id).Any() == false)
                            {
                                if(y == 1) //для первого прохода отмечаем что нужен параметр алгоритма с определенным типом
                                {
                                    if (indicatorParameterTemplate.ParameterValueType.Id == 1) //int
                                    {
                                        isNeedAlgorithmParameterInt = true;
                                    }
                                    else //double
                                    {
                                        isNeedAlgorithmParameterDouble = true;
                                    }
                                }
                                else //при втором проходе вставляем недостающий параметр индикатора алгоритма
                                {
                                    _database.InsertIndicatorParameterRange(new IndicatorParameterRange { IndicatorParameterTemplate = indicatorParameterTemplate, AlgorithmParameter = indicatorParameterTemplate.ParameterValueType.Id == 1 ? algorithmParameterInt : algorithmParameterDouble, AlgorithmIndicator = algorithmIndicator });
                                    string msg = "- добавленного в индикатор " + algorithmIndicator.Indicator.Name + "_" + algorithmIndicator.Ending + " параметра " + indicatorParameterTemplate.Name + ", ";
                                    msgIntParameter += indicatorParameterTemplate.ParameterValueType.Id == 1 ? msg : "";
                                    msgDoubleParameter += indicatorParameterTemplate.ParameterValueType.Id == 2 ? msg : "";
                                }
                            }
                            //если тип шаблона параметра не соответствует типу выбранного параметра алгоритма
                            else if(algorithmIndicator.IndicatorParameterRanges.Where(j => j.IndicatorParameterTemplate.Id == indicatorParameterTemplate.Id).First().AlgorithmParameter.ParameterValueType.Id != indicatorParameterTemplate.ParameterValueType.Id)
                            {
                                if (y == 1) //для первого прохода отмечаем что нужен параметр алгоритма с определенным типом
                                {
                                    if (indicatorParameterTemplate.ParameterValueType.Id == 1) //int
                                    {
                                        isNeedAlgorithmParameterInt = true;
                                    }
                                    else //double
                                    {
                                        isNeedAlgorithmParameterDouble = true;
                                    }
                                }
                                else //при втором проходе обновляем параметр индикатора алгоритма, устанавливая новый параметр алгоритма
                                {
                                    IndicatorParameterRange oldIndicatorParameterRange = algorithmIndicator.IndicatorParameterRanges.Where(j => j.IndicatorParameterTemplate.Id == indicatorParameterTemplate.Id).First();
                                    _database.UpdateIndicatorParameterRange(new IndicatorParameterRange { Id = oldIndicatorParameterRange.Id, IndicatorParameterTemplate = oldIndicatorParameterRange.IndicatorParameterTemplate, AlgorithmParameter = indicatorParameterTemplate.ParameterValueType.Id == 1 ? algorithmParameterInt : algorithmParameterDouble, AlgorithmIndicator = oldIndicatorParameterRange.AlgorithmIndicator });
                                    string msg = "- изменившего свой тип значения параметра " + indicatorParameterTemplate.Name + " индикатора " + algorithmIndicator.Indicator.Name + "_" + algorithmIndicator.Ending + ", ";
                                    msgIntParameter += indicatorParameterTemplate.ParameterValueType.Id == 1 ? msg : "";
                                    msgDoubleParameter += indicatorParameterTemplate.ParameterValueType.Id == 2 ? msg : "";
                                }
                            }
                            //при первом проходе добавляем в бд требуемые параметры алгоритма если они не были добавлены
                            if(y == 1)
                            {
                                if (isNeedAlgorithmParameterInt)
                                {
                                    if(isIntParameterCreated == false)
                                    {
                                        //находим незанятое имя
                                        string nameAlgPar = "autoInsert";
                                        int u = 1;
                                        while (newAlgorithmParameters.Where(j => j.IdAlgorithm == algorithm.Id && j.Name == nameAlgPar + u.ToString()).Any())
                                        {
                                            u++;
                                        }
                                        _database.InsertAlgorithmParameter(new AlgorithmParameter { Name = nameAlgPar + u.ToString(), MinValue = 1, MaxValue = 2, Step = 500, IsStepPercent = true, IdAlgorithm = algorithm.Id, ParameterValueType = indicatorParameterTemplate.ParameterValueType });
                                        newAlgorithmParameters = ReadAlgorithmParameters(); //считываем добавленный параметр алгоритма из бд
                                        algorithmParameterInt = newAlgorithmParameters.Where(j => j.IdAlgorithm == algorithm.Id && j.Name == nameAlgPar + u.ToString()).First(); //присваиваем добавленный параметр
                                        isIntParameterCreated = true;
                                    }
                                }
                                if (isNeedAlgorithmParameterDouble)
                                {
                                    if(isDoubleParameterCreated == false)
                                    {
                                        //находим незанятое имя
                                        string nameAlgPar = "autoInsert";
                                        int u = 1;
                                        while (newAlgorithmParameters.Where(j => j.IdAlgorithm == algorithm.Id && j.Name == nameAlgPar + u.ToString()).Any())
                                        {
                                            u++;
                                        }
                                        _database.InsertAlgorithmParameter(new AlgorithmParameter { Name = nameAlgPar + u.ToString(), MinValue = 1, MaxValue = 2, Step = 500, IsStepPercent = true, IdAlgorithm = algorithm.Id, ParameterValueType = indicatorParameterTemplate.ParameterValueType });
                                        newAlgorithmParameters = ReadAlgorithmParameters(); //считываем добавленный параметр алгоритма из бд
                                        algorithmParameterDouble = newAlgorithmParameters.Where(j => j.IdAlgorithm == algorithm.Id && j.Name == nameAlgPar + u.ToString()).First(); //присваиваем добавленный параметр
                                        isDoubleParameterCreated = true;
                                    }
                                }
                            }
                        }
                        while (y < 2);
                    }
                }
                //если были добавлены параметры алгоритма, обновляем их описание, указав в них сообщение для чего они были созданы
                string description = "Параметр создан автоматически " + DateTime.Now.ToString("G") + " для: ";
                if (isIntParameterCreated)
                {
                    algorithmParameterInt.Description = description + msgIntParameter.Substring(0, msgIntParameter.Length - 2) + ".";
                    _database.UpdateAlgorithmParameter(algorithmParameterInt);
                }
                if (isDoubleParameterCreated)
                {
                    algorithmParameterDouble.Description = description + msgDoubleParameter.Substring(0, msgDoubleParameter.Length - 2) + ".";
                    _database.UpdateAlgorithmParameter(algorithmParameterDouble);
                }
            }
        }

        public ObservableCollection<DataSourceTemplate> ReadDataSourceTemplates() //возвращает считанные шаблоны источников данных
        {
            ObservableCollection<DataSourceTemplate> dataSourceTemplates = new ObservableCollection<DataSourceTemplate>();

            DataTable dataDataSourceTemplates = _database.QuerySelect("SELECT * FROM DataSourceTemplates");
            foreach (DataRow row in dataDataSourceTemplates.Rows)
            {
                DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), Description = row.Field<string>("description"), IdAlgorithm = (int)row.Field<long>("idAlgorithm") };
                dataSourceTemplates.Add(dataSourceTemplate);
            }

            return dataSourceTemplates;
        }

        public ObservableCollection<AlgorithmParameter> ReadAlgorithmParameters() //возвращает считанные параметры алгоритмов
        {
            ObservableCollection<AlgorithmParameter> algorithmParameters = new ObservableCollection<AlgorithmParameter>();

            DataTable dataAlgorithmParameters = _database.QuerySelect("SELECT * FROM AlgorithmParameters");
            foreach (DataRow row in dataAlgorithmParameters.Rows)
            {
                bool isStepPercent = false;
                if ((int)row.Field<long>("isStepPercent") == 1)
                {
                    isStepPercent = true;
                }
                int idParameterValueType = (int)row.Field<long>("idParameterValueType");
                ParameterValueType parameterValueType = idParameterValueType == ParameterValueTypes[0].Id ? ParameterValueTypes[0] : ParameterValueTypes[1];
                AlgorithmParameter algorithmParameter = new AlgorithmParameter { Id = (int)row.Field<long>("id"), Name = row.Field<string>("name"), Description = row.Field<string>("description"), MinValue = row.Field<double>("minValue"), MaxValue = row.Field<double>("maxValue"), Step = row.Field<double>("step"), IsStepPercent = isStepPercent, IdAlgorithm = (int)row.Field<long>("idAlgorithm"), ParameterValueType = parameterValueType };
                algorithmParameters.Add(algorithmParameter);
            }

            return algorithmParameters;
        }

        public ObservableCollection<AlgorithmIndicator> ReadAlgorithmIndicators(ObservableCollection<Indicator> indicators) //возвращает считанные алгоритмы индикаторов
        {
            ObservableCollection<AlgorithmIndicator> algorithmIndicators = new ObservableCollection<AlgorithmIndicator>();

            DataTable dataAlgorithmIndicators = _database.QuerySelect("SELECT * FROM AlgorithmIndicators ORDER BY idIndicator");
            foreach (DataRow row in dataAlgorithmIndicators.Rows)
            {
                int idIndicator = (int)row.Field<long>("idIndicator"); //id индикатора
                Indicator indicator = new Indicator();
                foreach (Indicator indicator1 in indicators)
                {
                    if (indicator1.Id == idIndicator)
                    {
                        indicator = indicator1;
                    }
                }
                AlgorithmIndicator algorithmIndicator = new AlgorithmIndicator { Id = (int)row.Field<long>("id"), IdAlgorithm = (int)row.Field<long>("idAlgorithm"), IdIndicator = (int)row.Field<long>("idIndicator"), Indicator = indicator, IndicatorParameterRanges = new List<IndicatorParameterRange>(), Ending = row.Field<string>("ending") };
                algorithmIndicators.Add(algorithmIndicator);
            }

            return algorithmIndicators;
        }

        public ObservableCollection<IndicatorParameterRange> ReadIndicatorParameterRanges(ObservableCollection<IndicatorParameterTemplate> indicatorParameterTemplates, ObservableCollection<AlgorithmParameter> algorithmParameters, ObservableCollection<AlgorithmIndicator> algorithmIndicators) //возвращает считанные параметры индикаторов алгоритмов
        {
            ObservableCollection<IndicatorParameterRange> indicatorParameterRanges = new ObservableCollection<IndicatorParameterRange>();

            DataTable dataIndicatorParameterRanges = _database.QuerySelect("SELECT * FROM IndicatorParameterRanges ORDER BY idAlgorithmIndicator");
            foreach (DataRow row in dataIndicatorParameterRanges.Rows)
            {
                int idIndicatorParameterTemplate = (int)row.Field<long>("idIndicatorParameterTemplate"); //id шаблона параметра
                IndicatorParameterTemplate indicatorParameterTemplate = indicatorParameterTemplates.Where(j => j.Id == idIndicatorParameterTemplate).First();
                int idAlgorithmParameter = (int)row.Field<long>("idAlgorithmParameter"); //id параметра алгоритма
                AlgorithmParameter algorithmParameter = algorithmParameters.Where(j => j.Id == idAlgorithmParameter).First();
                int idAlgorithmIndicator = (int)row.Field<long>("idAlgorithmIndicator"); //id индикатора алгоритма
                AlgorithmIndicator algorithmIndicator = algorithmIndicators.Where(j => j.Id == idAlgorithmIndicator).First();

                IndicatorParameterRange indicatorParameterRange = new IndicatorParameterRange { Id = (int)row.Field<long>("id"), IdAlgorithmIndicator = idAlgorithmIndicator, IndicatorParameterTemplate = indicatorParameterTemplate, AlgorithmParameter = algorithmParameter, AlgorithmIndicator = algorithmIndicator };
                //добавляем indicatorParameterRange в IndicatorParameterRanges algorithmIndicator-а
                algorithmIndicator.IndicatorParameterRanges.Add(indicatorParameterRange);

                indicatorParameterRanges.Add(indicatorParameterRange);
            }

            return indicatorParameterRanges;
        }

        public ObservableCollection<Algorithm> ReadAlgorithms(ObservableCollection<DataSourceTemplate> inputDataSourceTemplates, ObservableCollection<AlgorithmParameter> inputAlgorithmParameters, ObservableCollection<AlgorithmIndicator> inputAlgorithmIndicators) //возвращает считанные алгоритмы
        {
            ObservableCollection<Algorithm> algorithms = new ObservableCollection<Algorithm>();

            DataTable dataAlgorithms = _database.QuerySelect("SELECT * FROM Algorithms");
            foreach (DataRow row in dataAlgorithms.Rows)
            {
                int idAlgorithm = (int)row.Field<long>("id");
                Algorithm algorithm = new Algorithm { Id = idAlgorithm, Name = row.Field<string>("name"), Description = row.Field<string>("description"), Script = row.Field<string>("script") };

                List<DataSourceTemplate> dataSourceTemplates = new List<DataSourceTemplate>();
                foreach (DataSourceTemplate dataSourceTemplate in inputDataSourceTemplates)
                {
                    if (dataSourceTemplate.IdAlgorithm == idAlgorithm)
                    {
                        dataSourceTemplates.Add(dataSourceTemplate);
                    }
                }
                algorithm.DataSourceTemplates = dataSourceTemplates;

                List<AlgorithmParameter> algorithmParameters = new List<AlgorithmParameter>();
                foreach (AlgorithmParameter algorithmParameter in inputAlgorithmParameters)
                {
                    if (algorithmParameter.IdAlgorithm == idAlgorithm)
                    {
                        algorithmParameters.Add(algorithmParameter);
                    }
                }
                algorithm.AlgorithmParameters = algorithmParameters;

                List<AlgorithmIndicator> algorithmIndicators = new List<AlgorithmIndicator>();
                foreach (AlgorithmIndicator algorithmIndicator in inputAlgorithmIndicators)
                {
                    if (algorithmIndicator.IdAlgorithm == idAlgorithm)
                    {
                        algorithmIndicators.Add(algorithmIndicator);
                        //устанавливаем Algorithm для algorithmIndicator-а
                        algorithmIndicator.Algorithm = algorithm;
                    }
                }
                algorithm.AlgorithmIndicators = algorithmIndicators;

                algorithms.Add(algorithm);
            }

            return algorithms;
        }

        public void ReadAndSetAlgorithms() //считывает шаблоны источников данных, параметры алгоритмов, индикаторы алгоритмов, параметры индикаторов алгоритмов со значениями, алгоритмы, и устанавливает их в списки на которые оформлена подписка у модели-представления
        {
            //считываем макеты источников данных из базы данных
            DataSourceTemplates.Clear();
            foreach(DataSourceTemplate dataSourceTemplate in ReadDataSourceTemplates())
            {
                DataSourceTemplates.Add(dataSourceTemplate);
            }

            //считываем параметры алгоритмов из базы данных
            AlgorithmParameters.Clear();
            foreach (AlgorithmParameter algorithmParameter in ReadAlgorithmParameters())
            {
                AlgorithmParameters.Add(algorithmParameter);
            }

            //считываем индикаторы алгоритмов (IndicatorParameterRanges установим при их считывании, Algorithm установим при считывании алгоритмов)
            AlgorithmIndicators.Clear();
            foreach (AlgorithmIndicator algorithmIndicator in ReadAlgorithmIndicators(Indicators))
            {
                AlgorithmIndicators.Add(algorithmIndicator);
            }

            //считываем диапазоны значений параметров индикаторов из базы данных
            IndicatorParameterRanges.Clear();
            foreach (IndicatorParameterRange indicatorParameterRange in ReadIndicatorParameterRanges(IndicatorParameterTemplates, AlgorithmParameters, AlgorithmIndicators))
            {
                IndicatorParameterRanges.Add(indicatorParameterRange);
            }

            //считываем алгоритмы из базы данных
            Algorithms.Clear();
            foreach (Algorithm algorithm in ReadAlgorithms(DataSourceTemplates, AlgorithmParameters, AlgorithmIndicators))
            {
                Algorithms.Add(algorithm);
            }
        }
    }
}
