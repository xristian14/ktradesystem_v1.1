using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using ktradesystem.Models.Datatables;
using System.Threading;

namespace ktradesystem.Models
{
    [Serializable]
    public class TestRun
    {
        [NonSerialized]
        public TestBatch TestBatch;
        public bool IsOptimizationTestRun { get; set; } //тестовый прогон оптимизационны или форвардный
        public int Number { get; set; } //номер тестового прогона
        public Account Account { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public List<AlgorithmParameterValue> AlgorithmParameterValues { get; set; }
        public List<EvaluationCriteriaValue> EvaluationCriteriaValues { get; set; }
        public List<string> DealsDeviation { get; set; }
        public List<string> LoseDeviation { get; set; }
        public List<string> ProfitDeviation { get; set; }
        public List<string> LoseSeriesDeviation { get; set; }
        public List<string> ProfitSeriesDeviation { get; set; }
        private readonly object locker = new object();
        private bool _isComplete { get; set; } //завершен ли тест
        public bool IsComplete //реализация потокобезопасного получения и установки свойства
        {
            get
            {
                lock (locker)
                {
                    return _isComplete;
                }
            }
            set
            {
                lock (locker)
                {
                    _isComplete = value;
                }
            }
        }

        [NonSerialized]
        private ModelData _modelData;
        private void TestRunExecute(TestRun testRun, Testing testing, List<AlgorithmIndicator> algorithmIndicators, CancellationToken cancellationToken)
        {
            _modelData = ModelData.getInstance();
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            //формируем массивы с int и double значениями параметров для каждого индикатора
            int[][] indicatorParametersIntValues = new int[algorithmIndicators.Count][];
            double[][] indicatorParametersDoubleValues = new double[algorithmIndicators.Count][];
            //новая версия
            for (int i = 0; i < algorithmIndicators.Count; i++)
            {
                indicatorParametersIntValues[i] = new int[algorithmIndicators[i].IndicatorParameterRanges.Count];
                indicatorParametersDoubleValues[i] = new double[algorithmIndicators[i].IndicatorParameterRanges.Count];
                for (int k = 0; k < algorithmIndicators[i].IndicatorParameterRanges.Count; k++)
                {
                    indicatorParametersIntValues[i][k] = testRun.AlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmIndicators[i].IndicatorParameterRanges[k].AlgorithmParameter.Id).First().IntValue;
                    indicatorParametersDoubleValues[i][k] = testRun.AlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmIndicators[i].IndicatorParameterRanges[k].AlgorithmParameter.Id).First().DoubleValue;
                }
            }

            //формируем массивы с int и double значениями параметров для алгоритма
            int[] algorithmParametersIntValues = new int[testRun.AlgorithmParameterValues.Count];
            double[] algorithmParametersDoubleValues = new double[testRun.AlgorithmParameterValues.Count];
            for (int i = 0; i < testRun.AlgorithmParameterValues.Count; i++)
            {
                algorithmParametersIntValues[i] = testRun.AlgorithmParameterValues[i].IntValue;
                algorithmParametersDoubleValues[i] = testRun.AlgorithmParameterValues[i].DoubleValue;
            }

            TimeSpan intervalDuration = testRun.TestBatch.DataSourceGroup.DataSourceAccordances[0].DataSource.Interval.Duration; //длительность интервала
            DataSourceCandles[] dataSourceCandles = new DataSourceCandles[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //массив с ссылками на DataSourceCandles, соответствующими источникам данных группы источников данных
            for (int i = 0; i < dataSourceCandles.Length; i++)
            {
                dataSourceCandles[i] = testing.DataSourcesCandles.Where(j => j.DataSource == testRun.TestBatch.DataSourceGroup.DataSourceAccordances[i].DataSource).First(); //DataSourceCandles для источника данных из DataSourceAccordances с индексом i в dataSourceCandles
            }
            int[] fileIndexes = new int[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //индексы (для всех источников данных группы) элемента массива Candle[][] Candles в DataSourcesCandles, соответствующий файлу источника данных
            int[] candleIndexes = new int[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //индексы (для всех источников данных группы) элемента массива Candles[], сответствующий свечке
            int[] gapIndexes = new int[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //индексы (для всех источников данных группы) элемента списка , содержащего индекс свечки с гэпом
            bool[] gaps = new bool[gapIndexes.Length]; //является ли текущая свечка гэпом, для каждого источника данных группы

            //определяем индексы элемента каталога в AlgorithmIndicatorCatalog со значениями индикатора для всех индикаторов во всех источниках данных
            int[][] algorithmIndicatorCatalogElementIndexes = new int[dataSourceCandles.Length][]; //индексы элемента каталога в AlgorithmIndicatorCatalog со значениями индикатора для всех индикаторов во всех источниках данных
            //algorithmIndicatorCatalogElementIndexes[0] - соответствует источнику данных dataSourceCandles[0]
            //algorithmIndicatorCatalogElementIndexes[0][0] - соответствует индикатору dataSourceCandles[0].AlgorithmIndicatorCatalogs[0]. И содержит индекс элемента каталога со значениями для даннного индикатора
            for (int i = 0; i < dataSourceCandles.Length; i++)
            {
                algorithmIndicatorCatalogElementIndexes[i] = new int[dataSourceCandles[i].AlgorithmIndicatorCatalogs.Length]; //индексы для всех индикаторов в данном источнике данных
                for (int k = 0; k < algorithmIndicatorCatalogElementIndexes[i].Length; k++)
                {
                    //определяем индекс элемента каталога с текущей комбинацией значений параметров алгоритма
                    bool isFind = false;
                    int catalogElementIndex = 0;
                    while (isFind == false && catalogElementIndex < dataSourceCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements.Count)
                    {
                        bool isAllParameterValuesEqual = true; //совпадают ли все значения параметров алгоритма со значениями в элементе каталога
                        //проходим по всем занчениями параметров алгоритма в элементе каталога
                        foreach (AlgorithmParameterValue algorithmParameterValue in dataSourceCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements[catalogElementIndex].AlgorithmParameterValues)
                        {
                            AlgorithmParameterValue algorithmParameterValueTestRun = testRun.AlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmParameterValue.AlgorithmParameter.Id).First(); //значение параметра алгоритма с таким же параметром алгоритма как и текущий параметр из элемента каталога
                            if (algorithmParameterValue.AlgorithmParameter.ParameterValueType.Id == 1) //параметр типа int
                            {
                                if (algorithmParameterValue.IntValue != algorithmParameterValueTestRun.IntValue)
                                {
                                    isAllParameterValuesEqual = false;
                                }
                            }
                            else //параметр типа double
                            {
                                if (algorithmParameterValue.DoubleValue != algorithmParameterValueTestRun.DoubleValue)
                                {
                                    isAllParameterValuesEqual = false;
                                }
                            }
                        }
                        if (isAllParameterValuesEqual)
                        {
                            algorithmIndicatorCatalogElementIndexes[i][k] = catalogElementIndex; //запоминаем индекс элемента каталога со значенями индикатора
                            isFind = true;
                        }
                        catalogElementIndex++;
                    }
                }
            }

            //устанавливаем начальные индексы файла и свечки для источников данных
            for (int i = 0; i < fileIndexes.Length; i++)
            {
                fileIndexes[i] = 0;
                candleIndexes[i] = 0;
                gapIndexes[i] = 0;
                gaps[i] = false;
            }

            //находим индексы файлов и свечек, дата и время которых позже или равняется дате и времени начала тестирования
            for (int i = 0; i < dataSourceCandles.Length; i++)
            {
                //находим индекс файла в текущем dataSourceCandles, дата последней свечки которого позже даты начала тестирования
                int fileIndex = 0;
                bool isFindFile = false;
                //пока не вышли за пределы массива файлов, и пока не нашли файл, дата последней свечки которого позже даты начала тестирования
                while (fileIndex < dataSourceCandles[i].Candles.Length && isFindFile == false)
                {
                    if (DateTime.Compare(dataSourceCandles[i].Candles[fileIndex].Last().DateTime, testRun.StartPeriod) > 0)
                    {
                        isFindFile = true;
                    }
                    else
                    {
                        fileIndex++;
                    }
                }
                fileIndexes[i] = fileIndex;
                //если нашли файл, последняя свечка которого позже даты начала тестирования, находим индекс свечки, дата которой позже или равняется дате начала тестирования
                int candleIndex = 0;
                if (isFindFile)
                {
                    bool isFindCandle = false;
                    while (isFindCandle == false)
                    {
                        if (DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndex].DateTime, testRun.StartPeriod) >= 0)
                        {
                            isFindCandle = true;
                        }
                        else
                        {
                            candleIndex++;
                        }
                    }
                }
                candleIndexes[i] = candleIndex;
            }

            bool isOverFileIndex = false; //вышел ли какой-либо из индексов файлов за границы массива файлов источника данных
            for (int i = 0; i < fileIndexes.Length; i++)
            {
                if (fileIndexes[i] >= dataSourceCandles[i].Candles.Length)
                {
                    isOverFileIndex = true; //отмечаем что индекс файла вышел за границы массива
                }
            }

            //устанавливаем текущую дату, взяв самую позднюю дату текущей свечки среди источников данных
            DateTime currentDateTime = new DateTime(); //текущие дата и время
            if (isOverFileIndex == false) //если не было превышений индекса файла, ищем текущую дату
            {
                currentDateTime = dataSourceCandles[0].Candles[fileIndexes[0]][candleIndexes[0]].DateTime; //текущие дата и время
                for (int i = 1; i < dataSourceCandles.Length; i++)
                {
                    if (DateTime.Compare(currentDateTime, dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime) < 0)
                    {
                        currentDateTime = dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                    }
                }
            }

            //копируем объект скомпилированного алгоритма, чтобы из разных потоков не обращаться к одному объекту и к одним свойствам объекта
            dynamic CompiledAlgorithmCopy = testing.CompiledAlgorithm.Clone();

            //проходим по всем свечкам источников данных, пока не достигнем времени окончания теста, не выйдем за границы имеющихся файлов, или не получим запрос на отмену тестирования
            while (DateTime.Compare(currentDateTime, testRun.EndPeriod) < 0 && isOverFileIndex == false && cancellationToken.IsCancellationRequested == false)
            {
                //определяем гэпы для текущих свечек
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    gaps[i] = false;
                    if (gapIndexes[i] < dataSourceCandles[i].GapIndexes[fileIndexes[i]].Count) //проверяем, не вышли ли за границы списка с индексами свечек с гэпом
                    {
                        if (candleIndexes[i] == dataSourceCandles[i].GapIndexes[fileIndexes[i]][gapIndexes[i]]) //равняется ли индекс свечки, индексу свечки с гэпом
                        {
                            gaps[i] = true;
                            gapIndexes[i]++; //переходим на следующий индекс свечки с гэпом
                        }
                    }
                }
                //обрабатываем текущие заявки (только тех источников данных, текущие свечки которых равняются текущей дате)
                //формируем список источников данных для которых будут проверяться заявки на исполнение (те, даты которых равняются текущей дате)
                List<DataSource> approvedDataSources = new List<DataSource>();
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    if (DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) == 0)
                    {
                        approvedDataSources.Add(dataSourceCandles[i].DataSource);
                    }
                }
                //проверяем заявки на исполнение
                bool isWereDeals = CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, gaps, false, true, true); //были ли совершены сделки при проверке исполнения заявок

                //если были совершены сделки на текущей свечке, дважды выполняем алгоритм: первый раз обновляем заявки и проверяем на исполнение стоп-заявки (если была открыта позиция на текущей свечке, нужно выставить стоп и проверить мог ли он на этой же свечке исполнится), и если были сделки то выполняем алгоритм еще раз и обновляем заявки, после чего переходим на следующую свечку

                bool IsOverIndex = false; //было ли превышение индекса в индикаторах и алгоритме
                double[][] indicatorsValues = new double[dataSourceCandles.Length][];
                //проходим по всем источникам данных и формируем значения всех индикаторов для каждого источника данных
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    indicatorsValues[i] = new double[algorithmIndicators.Count];
                    for (int k = 0; k < indicatorsValues[i].Length; k++)
                    {
                        indicatorsValues[i][k] = dataSourceCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements[algorithmIndicatorCatalogElementIndexes[i][k]].AlgorithmIndicatorValues.Values[fileIndexes[i]][candleIndexes[i]].Value;
                        if (dataSourceCandles[i].AlgorithmIndicatorCatalogs[k].AlgorithmIndicatorCatalogElements[algorithmIndicatorCatalogElementIndexes[i][k]].AlgorithmIndicatorValues.Values[fileIndexes[i]][candleIndexes[i]].IsNotOverIndex == false) //если при вычислении данного значения индикатора было превышение индекса свечки, отмечаем что было превышение индекса
                        {
                            IsOverIndex = true;
                        }
                    }
                }

                //если были сделки на этой свечке, то для того чтобы проверить мог ли исполниться стоп-лосс на текущей свечке, выполняем алгоритм (после чего для открытой позиции будет выставлен стоп-лосс) и проверяем исполнение стоп-заявок. Если в процессе выполнения стоп-заявок были совершены сделки, еще раз выполняем алгоритм, обновляем заявки и переходим на следующую свечку
                int iteration = 0; //номер итерации
                bool isWereDealsStopLoss = false; //были ли совешены сделки при проверки стоп-заявок на исполнение
                do
                {
                    iteration++;
                    //вычисляем алгоритм
                    //формируем dataSourcesForCalculate
                    DataSourceForCalculate[] dataSourcesForCalculate = new DataSourceForCalculate[dataSourceCandles.Length];
                    for (int i = 0; i < dataSourceCandles.Length; i++)
                    {
                        //определяем среднюю цену и объем позиции
                        double averagePricePosition = 0; //средняя цена позиции
                        decimal volumePosition = 0; //объем позиции
                        bool isBuyDirection = false;
                        foreach (Deal deal in testRun.Account.CurrentPosition)
                        {
                            if (deal.DataSource == dataSourceCandles[i].DataSource) //если сделка относится к текущему источнику данных
                            {
                                if (volumePosition == 0) //если это первая сделка по данному источнику данных, запоминаем цену и объем
                                {
                                    averagePricePosition = deal.Price;
                                    volumePosition = deal.Count;
                                }
                                else //если это не первая сделка по данному источнику данных, определяем среднюю цену и обновляем объем
                                {
                                    averagePricePosition = (double)(((decimal)averagePricePosition * volumePosition + (decimal)deal.Price * deal.Count) / (volumePosition + deal.Count)); //(средняя цена * объем средней цены + текущая цена * текущий объем)/(объем средней цены + текущий объем)
                                    averagePricePosition = ModelFunctions.RoundToIncrement(averagePricePosition, deal.DataSource.PriceStep); //округляем среднюю цену позиции до шага 1 пункта цены данного инструмента
                                    volumePosition += deal.Count;
                                }
                                if (deal.Order.Direction)
                                {
                                    isBuyDirection = true;
                                }
                            }
                        }
                        DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod = dataSourceCandles[i].DataSource.DataSourceFiles[fileIndexes[i]].DataSourceFileWorkingPeriods.Where(j => DateTime.Compare(currentDateTime, j.StartPeriod) >= 0).Last(); //последний период, дата начала которого раньше или равняется текущей дате

                        dataSourcesForCalculate[i] = new DataSourceForCalculate();
                        dataSourcesForCalculate[i].idDataSource = dataSourceCandles[i].DataSource.Id;
                        dataSourcesForCalculate[i].IndicatorsValues = indicatorsValues[i];
                        dataSourcesForCalculate[i].MinLotCount = dataSourceCandles[i].DataSource.MinLotCount;
                        dataSourcesForCalculate[i].PriceStep = dataSourceCandles[i].DataSource.PriceStep;
                        dataSourcesForCalculate[i].CostPriceStep = dataSourceCandles[i].DataSource.CostPriceStep;
                        dataSourcesForCalculate[i].MinLotsCost = dataSourceCandles[i].DataSource.MarginType.Id == 2 ? dataSourceCandles[i].DataSource.MarginCost * dataSourceCandles[i].DataSource.MinLotMarginPartCost : dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].C * dataSourceCandles[i].DataSource.MinLotMarginPartCost;
                        dataSourcesForCalculate[i].Price = averagePricePosition;
                        dataSourcesForCalculate[i].CountBuy = isBuyDirection ? volumePosition : 0;
                        dataSourcesForCalculate[i].CountSell = isBuyDirection ? 0 : volumePosition;
                        dataSourcesForCalculate[i].TimeInCandle = dataSourceCandles[i].DataSource.Interval.Duration;
                        dataSourcesForCalculate[i].TradingStartTimeOfDay = dataSourceFileWorkingPeriod.TradingStartTime;
                        dataSourcesForCalculate[i].TradingEndTimeOfDay = dataSourceFileWorkingPeriod.TradingEndTime;
                        dataSourcesForCalculate[i].Candles = dataSourceCandles[i].Candles[fileIndexes[i]];
                        dataSourcesForCalculate[i].CurrentCandleIndex = candleIndexes[i];
                    }

                    AccountForCalculate accountForCalculate = new AccountForCalculate { FreeRubleMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, FreeDollarMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit, TakenRubleMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, TakenDollarMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit, IsForwardDepositTrading = testRun.Account.IsForwardDepositTrading, AccountVariables = testRun.Account.AccountVariables };
                    AlgorithmCalculateResult algorithmCalculateResult = CompiledAlgorithmCopy.Calculate(accountForCalculate, dataSourcesForCalculate, algorithmParametersIntValues, algorithmParametersDoubleValues);

                    if (IsOverIndex == false) //если не был превышен допустимый индекс при вычислении индикаторов и алгоритма, обрабатываем заявки
                    {
                        //удаляем заявки, количество лотов в которых равно 0
                        for (int i = algorithmCalculateResult.Orders.Count - 1; i >= 0; i--)
                        {
                            if (algorithmCalculateResult.Orders[i].Count == 0)
                            {
                                algorithmCalculateResult.Orders.RemoveAt(i);
                            }
                        }
                        //если это не форвардное тестирование с торговлей депозитом, устанавливаем размер заявок в минимальное количество лотов, а так же устанавливаем DateTimeSubmit для заявок
                        foreach (Order order in algorithmCalculateResult.Orders)
                        {
                            if (testRun.Account.IsForwardDepositTrading == false)
                            {
                                order.Count = order.DataSource.MinLotCount;
                                order.StartCount = order.DataSource.MinLotCount;
                            }
                            order.DateTimeSubmit = currentDateTime;
                        }
                        //приводим заявки к виду который прислал пользователь в алгоритме
                        List<Order> accountOrders = new List<Order>(); //список с текущими выставленными заявками
                        accountOrders.AddRange(testRun.Account.Orders);

                        List<Order> userOrders = new List<Order>(); //список с заявками пользователя
                        userOrders.AddRange(algorithmCalculateResult.Orders);

                        List<Order> newOrders = new List<Order>(); //список с новыми выствленными заявками
                        newOrders.AddRange(algorithmCalculateResult.Orders);

                        //обрабатываем все заявки в accountOrders
                        int countAccountOrders = accountOrders.Count;
                        while (countAccountOrders > 0)
                        {
                            Order accountOrder = accountOrders[0]; //текущая заявка из accountOrders
                            Order userOrder = null; //совпадающая с accountOrder заявка из userOrders
                            //ищем в userOrders совпадающую с accountOrder заявку
                            int userOrderIndex = 0;
                            while (userOrderIndex < userOrders.Count && userOrder == null)
                            {
                                bool isEqual = accountOrder.DataSource == userOrders[userOrderIndex].DataSource && accountOrder.TypeOrder == userOrders[userOrderIndex].TypeOrder && accountOrder.Direction == userOrders[userOrderIndex].Direction && accountOrder.Price == userOrders[userOrderIndex].Price && accountOrder.Count == userOrders[userOrderIndex].Count; //проверка на соответстве источника данных, типа заявки, направления, цены, количества
                                isEqual = isEqual && ((accountOrder.LinkedOrder != null && userOrders[userOrderIndex].LinkedOrder != null) || (accountOrder.LinkedOrder == null && userOrders[userOrderIndex].LinkedOrder == null)); //проверка на соответствие наличия/отсутствия связаной заявки
                                if (isEqual)
                                {
                                    userOrder = userOrders[userOrderIndex]; //запоминаем совпадающую с accountOrder заявку
                                }
                                else
                                {
                                    userOrderIndex++; //увеличиваем индекс заявок пользователя
                                }
                            }
                            //если в userOrders есть совпадающая, удаляем совпадающую из userOrders и newOrders, и вставляем в newOrders из accountOrders
                            if (userOrder != null)
                            {
                                userOrders.Remove(userOrder);
                                newOrders.Remove(userOrder);
                                newOrders.Add(accountOrder);
                                //если у accountOrder есть связанная заявка, сравниваем accountOrder.LinkedOrder и userOrder.LinkedOrder
                                if (accountOrder.LinkedOrder != null)
                                {
                                    bool isEqual = accountOrder.LinkedOrder.DataSource == userOrder.LinkedOrder.DataSource && accountOrder.LinkedOrder.TypeOrder == userOrder.LinkedOrder.TypeOrder && accountOrder.LinkedOrder.Direction == userOrder.LinkedOrder.Direction && accountOrder.LinkedOrder.Price == userOrder.LinkedOrder.Price && accountOrder.LinkedOrder.Count == userOrder.LinkedOrder.Count; //проверка на соответстве источника данных, типа заявки, направления, цены, количества
                                    if (isEqual)
                                    {
                                        //если совпадают удаляем из userOrders и newOrders userOrder.LinkedOrder, вставляем в newOrders accountOrder.LinkedOrder, удаляем из accountOrders accountOrder.LinkedOrder
                                        userOrders.Remove(userOrder.LinkedOrder);
                                        newOrders.Remove(userOrder.LinkedOrder);
                                        newOrders.Add(accountOrder.LinkedOrder);
                                        accountOrders.Remove(accountOrder.LinkedOrder);
                                    }
                                    else
                                    {
                                        //если не совпадают, значит свзяанная с accountOrder будет взята из userOrder, и нужно проставить связи между ними (т.к. userOrder.LinkedOrder уже имеется, а accountOrder только что добавлена)
                                        accountOrder.LinkedOrder.DateTimeRemove = currentDateTime; //т.к. accountOrder.LinkedOrder не совпадает с userOrder.LinkedOrder, accountOrder.LinkedOrder снята, и нужно установить дату снятия
                                        accountOrder.LinkedOrder = userOrder.LinkedOrder;
                                        accountOrder.LinkedOrder.LinkedOrder = accountOrder;
                                    }
                                }
                            }
                            else
                            {
                                //т.к. в userOrders не была найдена такая заявка, она снята, и нужно установить дату снятия
                                accountOrder.DateTimeRemove = currentDateTime;
                            }
                            //удаляем из accountOrders accountOrder, т.к. мы её обработали
                            accountOrders.Remove(accountOrder);
                            countAccountOrders = accountOrders.Count;
                        }

                        //проставляем номера новым заявкам и добавляем их в testRun.Account.AllOrders
                        int lastNumber = testRun.Account.AllOrders.Count == 0 ? 0 : testRun.Account.AllOrders.Last().Number; //номер последней заявки
                        foreach (Order order in newOrders)
                        {
                            if (order.Number == 0) //если номер заявки равен 0, значит она новая
                            {
                                lastNumber++;
                                order.Number = lastNumber;
                                testRun.Account.AllOrders.Add(order);
                            }
                        }
                        //устанавливаем текущие выставленные заявки в newOrders
                        testRun.Account.Orders = newOrders;

                        //проверяем исполнение рыночных заявок, выставленных на текущей свечке
                        if (iteration == 1)
                        {
                            CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, gaps, true, false, false);
                        }

                        //если на текущей свечке были совершены сделки, проверяем стоп-заявки на исполнение (чтобы если на текущей свечке была открыта позиция, после выставления стоп-заявки проверить её на исполнение на текущей свечке)
                        if (isWereDeals && iteration == 1)
                        {
                            isWereDealsStopLoss = CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, gaps, false, true, false); //были ли совершены сделки при проверке исполнения стоп-заявок
                        }
                    }
                }
                while (isWereDealsStopLoss && iteration == 1); //если этой первое исполнение алгоритма, и при проверке стоп-заявок были сделки, еще раз прогоняем алгоритм чтобы обновить заявки

                //находим среди следующих свечек, свечку с саммой ранней датой. Обновляем текущую дату на эту дату. Затем переходим на следующую свечку, если она раньше или равняется текущей дате, и переходим на следующий файл источника данных только если следующий файл имеется, т.к. мы будем обращаться по текущему индексу и свечки и файла к тем источникам данных которые закончились, во время прохода по тем источникам данных которые еще не закончились (например файл с недельными свечками закончился, а часовые еще есть, и мы еще 7 дней можем торговать на часовых свечках, обращаясь к информации последней недельной свечки)
                //ищем среди следующих свечек самую раннюю дату
                int[] nextCandleIndexes = new int[candleIndexes.Length]; //индексы свечек, свечки, которая позже текущей
                int[] nextFileIndexes = new int[fileIndexes.Length]; //индексы файлов, свечки, которая позже текущей
                for (int i = 0; i < nextFileIndexes.Length; i++)
                {
                    nextFileIndexes[i] = -1; //если свечки, которая позже текущей не найдно, значение индекса файла равняется -1
                }
                DateTime nextEarliestDateTime = dataSourceCandles[0].Candles[fileIndexes[0]][candleIndexes[0]].DateTime; //присваиваем любое значение
                bool isEndAllDataSources = true;
                bool isInitNextDateTime = false;
                for (int i = 0; i < dataSourceCandles.Length; i++)
                {
                    //доходим до свечки, которая позже текущей свечки в данном источнике данных
                    //переходим на следующую свечку, пока не дойдем до даты которая позже текущей свечки
                    bool isOverDate = fileIndexes[i] < dataSourceCandles[i].Candles.Length ? DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) > 0 : false; //дошли ли до даты которая позже текущей
                    int candleIndex = candleIndexes[i];
                    int fileIndex = fileIndexes[i];
                    //переходим на следующую свечку, пока не дойдем до даты которая позже текущей или пока не выйдем за пределы файлов
                    while (isOverDate == false && fileIndex < dataSourceCandles[i].Candles.Length)
                    {
                        candleIndex++;
                        //если массив со свечками файла подошел к концу, переходим на следующий файл
                        if (candleIndex >= dataSourceCandles[i].Candles[fileIndex].Length)
                        {
                            candleIndex = 0;
                            fileIndex++;
                        }
                        //если индекс файла не вышел за пределы массива, проверяем, дошли ли до даты которая позже текущей свечки
                        if (fileIndex < dataSourceCandles[i].Candles.Length)
                        {
                            isOverDate = DateTime.Compare(dataSourceCandles[i].Candles[fileIndex][candleIndex].DateTime, dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime) > 0;
                        }
                    }
                    //если не вышли за пределы файлов, значит нашли дату которая позже текущей свечки
                    if (fileIndex < dataSourceCandles[i].Candles.Length)
                    {
                        nextCandleIndexes[i] = candleIndex; //запоминаем индексы файла и свечки, следующей по дате свечки, у данного источника данных
                        nextFileIndexes[i] = fileIndex;
                        isEndAllDataSources = false; //отмечаем что не все файлы закончились
                        if (!isInitNextDateTime)
                        {
                            isInitNextDateTime = true;
                            nextEarliestDateTime = dataSourceCandles[i].Candles[fileIndex][candleIndex].DateTime;
                        }
                        else
                        {
                            if (DateTime.Compare(nextEarliestDateTime, dataSourceCandles[i].Candles[fileIndex][candleIndex].DateTime) > 0)
                            {
                                nextEarliestDateTime = dataSourceCandles[i].Candles[fileIndex][candleIndex].DateTime;
                            }
                        }
                    }
                }
                //если не все файлы закончились, переходим на следующую свечку
                if (!isEndAllDataSources)
                {
                    currentDateTime = nextEarliestDateTime;
                    for (int i = 0; i < dataSourceCandles.Length; i++)
                    {
                        if (nextFileIndexes[i] > -1) //если найдена свечка, которая позже текущей
                        {
                            //если свечка, которая позже текущей раньше, или равняется обновленной текущей дате, значит переходим на неё (свечка не в будущем)
                            if (DateTime.Compare(dataSourceCandles[i].Candles[nextFileIndexes[i]][nextCandleIndexes[i]].DateTime, currentDateTime) <= 0)
                            {
                                if (nextFileIndexes[i] > fileIndexes[i]) //если перешли на следующий файл, обнуляем индекс гэпа
                                {
                                    gapIndexes[i] = 0;
                                }
                                candleIndexes[i] = nextCandleIndexes[i];
                                fileIndexes[i] = nextFileIndexes[i];
                            }
                        }
                    }
                }
                else
                {
                    isOverFileIndex = true;
                }
            }

            //устанавливаем значение маржи
            testRun.Account.Margin = ModelFunctions.MarginCalculate(testRun);
            //рассчитываем критерии оценки для данного testRun
            for (int i = 0; i < _modelData.EvaluationCriterias.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break; //если был запрос на отмену тестирования, завершаем цикл
                }
                //копируем объект скомпилированного критерия оценки, чтобы из разных потоков не обращаться к одному объекту и к одним свойствам объекта
                dynamic CompiledEvaluationCriteriaCopy = testing.CompiledEvaluationCriterias[i].Clone();
                EvaluationCriteriaValue evaluationCriteriaValue = CompiledEvaluationCriteriaCopy.Calculate(dataSourceCandles, testRun, _modelData.Settings);
                evaluationCriteriaValue.EvaluationCriteria = _modelData.EvaluationCriterias[i];
                testRun.EvaluationCriteriaValues.Add(evaluationCriteriaValue);
            }

            //ModelFunctions.TestEvaluationCriteria(testRun); //так я отлаживаю критерии оценки

            testRun.IsComplete = true;
        }
        public bool CheckOrdersExecution(DataSourceCandles[] dataSourcesCandles, Account account, List<DataSource> approvedDataSources, int[] fileIndexes, int[] candleIndexes, bool[] gaps, bool isMarket, bool isStop, bool isLimit) //функция проверяет заявки на их исполнение в текущей свечке, возвращает false если не было сделок, и true если были совершены сделки. approvedDataSources - список с источниками данных, заявки которых будут проверяться на исполнение. isMarket, isStop, isLimit - если true, будут проверяться на исполнение эти заявки
        {
            bool isMakeADeals = false; //были ли совершены сделки
            //исполняем рыночные заявки
            if (isMarket)
            {
                List<Order> ordersToRemove = new List<Order>(); //заявки которые нужно удалить из заявок
                List<DateTime> ordersToRemoveDateTime = new List<DateTime>(); //дата снятия заявок
                foreach (Order order in account.Orders)
                {
                    if (order.TypeOrder.Id == 2 && approvedDataSources.Contains(order.DataSource)) //рыночная заявка
                    {
                        //определяем индекс источника данных со свечками текущей заявки
                        int dataSourcesCandlesIndex = 0;
                        for (int i = 0; i < dataSourcesCandles.Length; i++)
                        {
                            if (dataSourcesCandles[i].DataSource == order.DataSource)
                            {
                                dataSourcesCandlesIndex = i;
                            }
                        }
                        int slippage = gaps[dataSourcesCandlesIndex] ? 0 : dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PointsSlippage; //если текущая свечка - гэп, убираем базовое проскальзывание, и оставляем только вычисляемое, чтобы цена исполнения заявки была по худщей цене в свечке, и если объем большой то и проскальзывание было
                        slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                        slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                        double dealPrice = dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].C;
                        if (gaps[dataSourcesCandlesIndex]) //если текущая свечка - гэп, устанавливаем худшую цену
                        {
                            dealPrice = order.Direction ? dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H : dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L;
                        }
                        isMakeADeals = MakeADeal(account, order, order.Count, dealPrice + slippage * dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PriceStep, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
                        if (order.Count == 0)
                        {
                            ordersToRemove.Add(order);
                            ordersToRemoveDateTime.Add(dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime);
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for (int i = 0; i < ordersToRemove.Count; i++)
                {
                    ordersToRemove[i].DateTimeRemove = ordersToRemoveDateTime[i];
                    account.Orders.Remove(ordersToRemove[i]);
                    if (ordersToRemove[i].LinkedOrder != null)
                    {
                        ordersToRemove[i].LinkedOrder.DateTimeRemove = ordersToRemoveDateTime[i];
                        account.Orders.Remove(ordersToRemove[i].LinkedOrder);
                    }
                }
            }

            //проверяем стоп-заявки на исполнение
            if (isStop)
            {
                List<Order> ordersToRemove = new List<Order>(); //заявки которые нужно удалить из заявок
                List<DateTime> ordersToRemoveDateTime = new List<DateTime>(); //дата снятия заявок
                foreach (Order order in account.Orders)
                {
                    if (order.TypeOrder.Id == 3 && approvedDataSources.Contains(order.DataSource)) //стоп-заявка
                    {
                        //определяем индекс источника данных со свечками текущей заявки
                        int dataSourcesCandlesIndex = 0;
                        for (int i = 0; i < dataSourcesCandles.Length; i++)
                        {
                            if (dataSourcesCandles[i].DataSource == order.DataSource)
                            {
                                dataSourcesCandlesIndex = i;
                            }
                        }
                        //проверяем, зашла ли цена в текущей свечке за цену заявки
                        bool isStopExecute = (order.Direction == true && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H >= order.Price) || (order.Direction == false && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L <= order.Price); //заявка на покупку, и верхняя цена выше цены заявки или заявка на продажу, и нижняя цена ниже цены заявки
                        if (isStopExecute)
                        {
                            int slippage = gaps[dataSourcesCandlesIndex] ? 0 : dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PointsSlippage; //если текущая свечка - гэп, убираем базовое проскальзывание, и оставляем только вычисляемое, чтобы цена исполнения заявки была по худщей цене в свечке, и если объем большой то и проскальзывание было
                            slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                            slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                            double dealPrice = order.Price;
                            if (gaps[dataSourcesCandlesIndex]) //если текущая свечка - гэп, устанавливаем худшую цену
                            {
                                dealPrice = order.Direction ? dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H : dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L;
                            }
                            isMakeADeals = MakeADeal(account, order, order.Count, dealPrice + slippage * dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PriceStep, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
                            if (order.Count == 0)
                            {
                                ordersToRemove.Add(order);
                                ordersToRemoveDateTime.Add(dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime);
                            }
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for (int i = 0; i < ordersToRemove.Count; i++)
                {
                    ordersToRemove[i].DateTimeRemove = ordersToRemoveDateTime[i];
                    account.Orders.Remove(ordersToRemove[i]);
                    if (ordersToRemove[i].LinkedOrder != null)
                    {
                        ordersToRemove[i].LinkedOrder.DateTimeRemove = ordersToRemoveDateTime[i];
                        account.Orders.Remove(ordersToRemove[i].LinkedOrder);
                    }
                }
            }

            //проверяем лимитные заявки на исполнение
            if (isLimit)
            {
                List<Order> ordersToRemove = new List<Order>(); //заявки которые нужно удалить из заявок
                List<DateTime> ordersToRemoveDateTime = new List<DateTime>(); //дата снятия заявок
                foreach (Order order in account.Orders)
                {
                    if (order.TypeOrder.Id == 1 && approvedDataSources.Contains(order.DataSource)) //лимитная заявка
                    {
                        //определяем индекс источника данных со свечками текущей заявки
                        int dataSourcesCandlesIndex = 0;
                        for (int i = 0; i < dataSourcesCandles.Length; i++)
                        {
                            if (dataSourcesCandles[i].DataSource == order.DataSource)
                            {
                                dataSourcesCandlesIndex = i;
                            }
                        }
                        //проверяем, зашла ли цена в текущей свечке за цену заявки
                        bool isLimitExecute = (order.Direction == true && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L < order.Price) || (order.Direction == false && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H > order.Price); //заявка на покупку, и нижняя цена ниже цены покупки или заявка на продажу, и верхняя цена выше цены продажи
                        if (isLimitExecute)
                        {
                            //определяем количество лотов, которое находится за ценой заявки, и которое могло быть куплено/продано на текущей свечке
                            int stepCount = (int)Math.Round((dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H - dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L + order.DataSource.PriceStep) / order.DataSource.PriceStep); //количество пунктов цены
                            decimal stepLots = (decimal)dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].V / stepCount; //среднее количество лотов на 1 пункт цены
                            int stepsOver = order.Direction ? (int)Math.Round((order.Price - dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L) / order.DataSource.PriceStep) : (int)Math.Round((dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H - order.Price) / order.DataSource.PriceStep); //количество пунктов за ценой заявки
                            decimal overLots = stepLots * stepsOver / 2; //количество лотов которое могло быть куплено/продано на текущей свечке (делить на 2 т.к. лишь половина от лотов - это сделки в нужной нам операции (купить или продать))
                            if (overLots > 0) //если есть лоты которые могли быть исполнены на текущей свечке, совершаем сделку
                            {
                                decimal dealCount = order.Count <= overLots ? order.Count : overLots;
                                double dealPrice = order.Price;

                                //если цена лимитной заявки находится вне цены свечки (цена покупки выше самой худшей цены свечки, или цена продажи ниже самой худшей цены свечки), устанавливаем цену исполнения как у рыночной заявки
                                if ((order.Direction == true && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H < order.Price) || (order.Direction == false && dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L > order.Price))
                                {
                                    int slippage = gaps[dataSourcesCandlesIndex] ? 0 : dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PointsSlippage; //если текущая свечка - гэп, убираем базовое проскальзывание, и оставляем только вычисляемое, чтобы цена исполнения заявки была по худщей цене в свечке, и если объем большой то и проскальзывание было
                                    slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                                    slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                                    dealPrice = dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].C + slippage * dataSourcesCandles[dataSourcesCandlesIndex].DataSource.PriceStep;
                                }

                                if (gaps[dataSourcesCandlesIndex]) //если текущая свечка - гэп, устанавливаем худшую цену
                                {
                                    dealPrice = order.Direction ? dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H : dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L;
                                }
                                isMakeADeals = MakeADeal(account, order, dealCount, dealPrice, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
                                if (order.Count == 0)
                                {
                                    ordersToRemove.Add(order);
                                    ordersToRemoveDateTime.Add(dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime);
                                }
                            }
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for (int i = 0; i < ordersToRemove.Count; i++)
                {
                    ordersToRemove[i].DateTimeRemove = ordersToRemoveDateTime[i];
                    account.Orders.Remove(ordersToRemove[i]);
                    if (ordersToRemove[i].LinkedOrder != null)
                    {
                        ordersToRemove[i].LinkedOrder.DateTimeRemove = ordersToRemoveDateTime[i];
                        account.Orders.Remove(ordersToRemove[i].LinkedOrder);
                    }
                }
            }
            return isMakeADeals;
        }

        private bool MakeADeal(Account account, Order order, decimal lotsCount, double price, DateTime dateTime) //совершает сделку, возвращает false если сделка не была совершена, и true если была совершена. Закрывает открытые позиции если они есть, и открывает новые если заявка не была исполнена полностью на закрытие позиций, высчитывает результат трейда, обновляет занятые и свободные средства во всех валютах, удаляет закрытые сделки в открытых позициях
        {
            bool isMakeADeal = false; //была ли совершена сделка
            //определяем хватает ли средств на минимальное количество лотов, если да, определяем хватает ли средств на lotsCount, если нет устанавливаем минимально доступное количество
            //стоимость минимального количества лотов
            double minLotsCost = order.DataSource.MarginType.Id == 2 ? order.DataSource.MarginCost * order.DataSource.MinLotMarginPartCost : price * order.DataSource.MinLotMarginPartCost; //для фиксированной маржи, устанавливаем фиксированную маржу источника данных, помноженную на часть стоимости минимального количества лотов относительно маржи, для маржи с графика, устанавливаем стоимость с график, помноженную на часть стоимости минимального количества лотов относительно маржи
            double minLotsComission = order.DataSource.Comissiontype.Id == 2 ? minLotsCost * (order.DataSource.Comission / 100) : order.DataSource.Comission; //комиссия на минимальное количество лотов
            double freeDeposit = account.FreeForwardDepositCurrencies.Where(i => i.Currency == order.DataSource.Currency).First().Deposit; //свободный остаток в валюте источника данных
            double takenDeposit = account.TakenForwardDepositCurrencies.Where(i => i.Currency == order.DataSource.Currency).First().Deposit; //занятые средства на открытые позиции в валюте источника данных
            //определяем максимально доступное количество лотов
            decimal maxLotsCount = (decimal)ModelFunctions.TruncateToIncrement(freeDeposit / (minLotsCost + minLotsComission), (double)order.DataSource.MinLotCount);
            decimal reverseDirectionLotsCount = 0;//количество лотов в открытой позиции с обратным направлением
            decimal currentDirectionLotsCount = 0;//количество лотов в открытой позиции с текущим направлением
            foreach (Deal deal in account.CurrentPosition)
            {
                if (deal.DataSource == order.DataSource && deal.Order.Direction != order.Direction) //если сделка совершена по тому же источнику данных что и заявка, но отличается с ней в направлении
                {
                    if (deal.Order.Direction != order.Direction)
                    {
                        reverseDirectionLotsCount += deal.Count;
                    }
                    else
                    {
                        currentDirectionLotsCount += deal.Count;
                    }
                }
            }
            maxLotsCount += reverseDirectionLotsCount; //прибавляем к максимально доступному количеству лотов, количество лотов в открытой позиции с обратным направлением
            //если это не форвардное тестирование с торговлей депозитом, устанавливаем максимально доступное количество лотов в минимальное количество лотов
            if (account.IsForwardDepositTrading == false)
            {
                maxLotsCount = currentDirectionLotsCount == 0 ? order.DataSource.MinLotCount : 0; //если количество лотов в открытых сделках с текущим направлением равно нулю, устанавливаем доступное количество лотов в минимальное, если же есть открытые позиции с текущим направлением, устанавливаем в ноль
            }
            if (maxLotsCount > 0) //если максимально доступное количество лотов для совершения сделки по заявке > 0, совершаем сделку
            {
                decimal dealLotsCount = lotsCount > maxLotsCount ? maxLotsCount : lotsCount; //если количество лотов для сделки больше максимально доступного, устанавливаем в максимально доступное
                //вычитаем из неисполненных лотов заявки dealLotsCount
                order.Count -= dealLotsCount;
                if (order.LinkedOrder != null)
                {
                    order.LinkedOrder.Count = order.Count;
                }
                //записываем сделку
                account.AllDeals.Add(new Deal { Number = account.AllDeals.Count, IdDataSource = order.DataSource.Id, DataSource = order.DataSource, OrderNumber = order.Number, Order = order, Direction = order.Direction, Price = price, Count = dealLotsCount, DateTime = dateTime });
                Deal currentDeal = new Deal { Number = account.AllDeals.Count, IdDataSource = order.DataSource.Id, DataSource = order.DataSource, OrderNumber = order.Number, Order = order, Direction = order.Direction, Price = price, Count = dealLotsCount, DateTime = dateTime };
                account.CurrentPosition.Add(currentDeal);
                isMakeADeal = true; //запоминаем что была совершена сделка
                //вычитаем комиссию на сделку из свободных средств
                double comission = (double)((decimal)minLotsComission * (dealLotsCount / order.DataSource.MinLotCount));
                freeDeposit -= (double)((decimal)minLotsComission * (dealLotsCount / order.DataSource.MinLotCount));
                account.Totalcomission += comission;
                //закрываем открытые позиции которые были закрыты данной сделкой
                int i = 0;
                while (i < account.CurrentPosition.Count - 1 && currentDeal.Count > 0) //проходим по всем сделкам кроме последней (только что добавленной)
                {
                    if (account.CurrentPosition[i].DataSource == currentDeal.DataSource && account.CurrentPosition[i].Order.Direction != currentDeal.Order.Direction) //если совпадает источник данных, но отличается направление сделки
                    {
                        decimal decrementCount = account.CurrentPosition[i].Count > currentDeal.Count ? currentDeal.Count : account.CurrentPosition[i].Count; //количество для уменьшения лотов в сделке
                        //определяем денежный результат трейда и прибавляем его к свободным средствам
                        double priceSell = account.CurrentPosition[i].Order.Direction == false ? account.CurrentPosition[i].Price : currentDeal.Price; //цена продажи в трейде
                        double priceBuy = account.CurrentPosition[i].Order.Direction == true ? account.CurrentPosition[i].Price : currentDeal.Price; //цена покупки в трейде
                        double resultMoney = (double)((decimal)(priceSell - priceBuy) / (decimal)account.CurrentPosition[i].DataSource.PriceStep * (decimal)account.CurrentPosition[i].DataSource.CostPriceStep * (decrementCount / account.CurrentPosition[i].DataSource.MinLotCount)); //количество пунктов трейда * стоимость 1 пункта * количество минимального количества лотов
                        freeDeposit += resultMoney;
                        //определяем стоимость закрытых лотов в открытой позиции, вычитаем её из занятых средств и прибавляем к свободным
                        double closedCost = account.CurrentPosition[i].Order.DataSource.MarginType.Id == 2 ? account.CurrentPosition[i].Order.DataSource.MarginCost * order.DataSource.MinLotMarginPartCost : account.CurrentPosition[i].Price * order.DataSource.MinLotMarginPartCost;
                        closedCost = (double)((decimal)closedCost * (decrementCount / account.CurrentPosition[i].DataSource.MinLotCount)); //умножаем стоимость на количество
                        takenDeposit -= closedCost; //вычитаем из занятых на открытые позиции средств
                        freeDeposit += closedCost; //прибавляем к свободным средствам
                        //вычитаем закрытое количесво из открытых позиций
                        account.CurrentPosition[i].Count -= decrementCount;
                        currentDeal.Count -= decrementCount;
                    }
                    i++;
                }
                //определяем стоимость занятых средств на оставшееся (незакрытое) количество лотов текущей сделки, вычитаем её из сободных средств и добавляем к занятым
                double currentCost = currentDeal.DataSource.MarginType.Id == 2 ? currentDeal.DataSource.MarginCost * order.DataSource.MinLotMarginPartCost : currentDeal.Price * order.DataSource.MinLotMarginPartCost;
                currentCost = (double)((decimal)currentCost * (currentDeal.Count / order.DataSource.MinLotCount)); //умножаем стоимость на количество
                freeDeposit -= currentCost; //вычитаем из свободных средств
                takenDeposit += currentCost; //добавляем к занятым на открытые позиции средствам
                //удаляем из открытых позиций сделки с нулевым количеством
                for (int j = account.CurrentPosition.Count - 1; j >= 0; j--)
                {
                    if (account.CurrentPosition[j].Count == 0)
                    {
                        account.CurrentPosition.RemoveAt(j);
                    }
                }
                //обновляем средства во всех валютах
                account.FreeForwardDepositCurrencies = CalculateDepositCurrrencies(freeDeposit, order.DataSource.Currency, dateTime);
                account.TakenForwardDepositCurrencies = CalculateDepositCurrrencies(takenDeposit, order.DataSource.Currency, dateTime);
                //если открытые позиции пусты и была совершена сделка, записываем состояние депозита
                if (account.CurrentPosition.Count == 0 && isMakeADeal)
                {
                    account.DepositCurrenciesChanges.Add(account.FreeForwardDepositCurrencies);
                }
            }
            return isMakeADeal;
        }
        private int Slippage(DataSourceCandles dataSourceCandles, int fileIndex, int candleIndex, decimal lotsCountInOrder) //возвращает размер проскальзывания в пунктах
        {
            int candleCount = 20; //количество свечек для определения среднего количества лотов на 1 пункт цены
            candleCount = candleIndex < candleCount ? candleIndex + 1 : candleCount; //чтобы избежать обращения к несуществующему индексу
            decimal lotsCount = 0; //количество лотов
            int pointsCount = 0; //количество пунктов цены на которых было куплено/продано данное количество лотов
            for (int i = 0; i < candleCount; i++)
            {
                lotsCount += (decimal)dataSourceCandles.Candles[fileIndex][candleIndex - i].V;
                pointsCount += (int)Math.Round((dataSourceCandles.Candles[fileIndex][candleIndex - i].H - dataSourceCandles.Candles[fileIndex][candleIndex - i].L) / dataSourceCandles.DataSource.CostPriceStep); //делим high - low на стоимость 1 пункта цены и получаем количество пунктов
            }
            pointsCount = pointsCount == 0 ? 1 : pointsCount; //чтобы избежать деления на 0
            decimal lotsInOnePoint = lotsCount / pointsCount; //количество лотов в 1 пункте цены
            lotsInOnePoint = lotsInOnePoint == 0 ? (decimal)0.001 : lotsInOnePoint; //чтобы избежать деления на 0
            int slippage = (int)Math.Round(lotsCountInOrder / lotsInOnePoint / 2); //делю количество лотов в заявке на количество лотов в 1 пункте и получаю количество пунктов на которое размажется цена, поделив это количество на 2 получаю среднее значение проскальзывания
            return slippage;
        }
        private List<DepositCurrency> CalculateDepositCurrrencies(double deposit, Currency inputCurrency, DateTime dateTime) //возвращает значения депозита во всех валютах
        {
            List<DepositCurrency> depositCurrencies = new List<DepositCurrency>();

            double dollarCostDeposit = deposit / inputCurrency.DollarCost; //определяем долларовую стоимость
            foreach (Currency currency in _modelData.Currencies)
            {
                //переводим доллоровую стоимость в валютную, умножая на стоимость 1 доллара
                double cost = dollarCostDeposit * currency.DollarCost;
                depositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = cost, DateTime = dateTime });
            }

            return depositCurrencies;
        }
    }
}
