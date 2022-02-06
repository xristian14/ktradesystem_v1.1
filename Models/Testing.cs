﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.Diagnostics;
using System.Reflection;
using ktradesystem.CommunicationChannel;
using System.IO;
using System.Globalization;

namespace ktradesystem.Models
{
    class Testing
    {
        public Algorithm Algorithm { get; set; }
        public List<DataSourceGroup> DataSourceGroups { get; set; }
        public TopModelCriteria TopModelCriteria { get; set; } //критерии оценки топ-модели
        public bool IsConsiderNeighbours { get; set; } //оценивать топ-модель с учетом соседей
        public double SizeNeighboursGroupPercent { get; set; } //размер группы соседних тестов
        public bool IsAxesSpecified { get; set; } //указаны ли оси плоскости для поиска топ-модели с соседями
        public List<AxesParameter> AxesTopModelSearchPlane { get; set; } //оси плоскости для поиска топ-модели с соседями
        public bool IsForwardTesting { get; set; } //проводить ли форвардное тестирование
        public bool IsForwardDepositTrading { get; set; } //добавить ли для форвардного тестирования торговлю депозитом
        public List<DepositCurrency> ForwardDepositCurrencies { get; set; } //размер депозита форвардного тестирования во всех валютах
        public DateTime StartPeriod { get; set; } //дата начала тестирования
        public DateTime EndPeriod { get; set; } //дата окончания тестирования
        public DateTimeDuration DurationOptimizationTests { get; set; } //длительность оптимизационных тестов
        public DateTimeDuration OptimizationTestSpacing { get; set; } //временной промежуток между оптимизационными тестами
        public DateTimeDuration DurationForwardTest { get; set; } //длительность форвардного тестирования
        public List<TestBatch> TestBatches { get; set; } //тестовые связки (серия оптимизационных тестов за период + форвардный тест)
        private dynamic[] CompiledIndicators { get; set; } //объекты, содержащие метод, выполняющий расчет индикатора
        private dynamic CompiledAlgorithm { get; set; } //объект, содержащий метод, вычисляющий работу алгоритма
        private dynamic[] CompiledEvaluationCriterias { get; set; } //объекты, содержащие метод, выполняющий расчет критерия оценки тестового прогона
        public List<DataSourceCandles> DataSourcesCandles { get; set; } //список с массивами свечек (для файлов) для источников данных (от сюда же будут браться данные для отображения графиков)

        private ModelData _modelData;
        private ModelTesting _modelTesting;
        private MainCommunicationChannel _mainCommunicationChannel;

        public Testing()
        {
            _modelData = ModelData.getInstance();
            _modelTesting = ModelTesting.getInstance();
            _mainCommunicationChannel = MainCommunicationChannel.getInstance();
        }

        public void LaunchTesting()
        {
            TestBatches = new List<TestBatch>();

            //определяем списки со значениями параметров
            List<int>[] IndicatorsParametersAllIntValues = new List<int>[Algorithm.IndicatorParameterRanges.Count]; //массив со всеми возможными целочисленными значениями параметров индикаторов
            List<double>[] IndicatorsParametersAllDoubleValues = new List<double>[Algorithm.IndicatorParameterRanges.Count]; //массив со всеми возможными дробными значениями параметров индикаторов

            List<int>[] AlgorithmParametersAllIntValues = new List<int>[Algorithm.AlgorithmParameters.Count]; //массив со всеми возможными целочисленными значениями параметров алгоритма
            List<double>[] AlgorithmParametersAllDoubleValues = new List<double>[Algorithm.AlgorithmParameters.Count]; //массив со всеми возможными дробными значениями параметров алгоритма

            //параметры будут передаваться в индикаторы и алгоритм в качестве параметров методов, при описании методов индикатора или алгоритма я укажу тип принимаемого параметра int или double в зависимости от типа в шаблоне параметра, и после проверки типа параметра, решу из какого списка передавать, со значениями double, или со значениями int
            
            //генерируем все значения параметров индикаторов
            for (int i = 0; i < Algorithm.IndicatorParameterRanges.Count; i++)
            {
                IndicatorsParametersAllIntValues[i] = new List<int>();
                IndicatorsParametersAllDoubleValues[i] = new List<double>();
                //определяем, какой список формировать, целых или дробных чисел
                bool isIntValueType = (Algorithm.IndicatorParameterRanges[i].IndicatorParameterTemplate.ParameterValueType.Id == 1)? true : false;
                //определяем шаг
                double step = Algorithm.IndicatorParameterRanges[i].Step;
                if (Algorithm.IndicatorParameterRanges[i].IsStepPercent)
                {
                    step = (Algorithm.IndicatorParameterRanges[i].MaxValue - Algorithm.IndicatorParameterRanges[i].MinValue) * (Algorithm.IndicatorParameterRanges[i].Step / 100);
                }

                double currentValue = Algorithm.IndicatorParameterRanges[i].MinValue; //текущее значение

                if (isIntValueType)
                {
                    IndicatorsParametersAllIntValues[i].Add((int)Math.Round(currentValue));
                }
                else
                {
                    IndicatorsParametersAllDoubleValues[i].Add(currentValue);
                }
                currentValue += step;

                while (currentValue <= Algorithm.IndicatorParameterRanges[i].MaxValue)
                {
                    if (isIntValueType)
                    {
                        int intCurrentValue = (int)Math.Round(currentValue);
                        if(intCurrentValue != IndicatorsParametersAllIntValues[i].Last()) //если текущее значение отличается от предыдущего, добавляем его в целочисленные значения
                        {
                            IndicatorsParametersAllIntValues[i].Add(intCurrentValue);
                        }
                    }
                    else
                    {
                        IndicatorsParametersAllDoubleValues[i].Add(currentValue);
                    }

                    currentValue += step;
                }
            }

            //генерируем все значения параметров алгоритма
            for (int i = 0; i < Algorithm.AlgorithmParameters.Count; i++)
            {
                AlgorithmParametersAllIntValues[i] = new List<int>();
                AlgorithmParametersAllDoubleValues[i] = new List<double>();
                //определяем, какой список формировать, целых или дробных чисел
                bool isIntValueType = (Algorithm.AlgorithmParameters[i].ParameterValueType.Id == 1) ? true : false;
                //определяем шаг
                double step = Algorithm.AlgorithmParameters[i].Step;
                if (Algorithm.AlgorithmParameters[i].IsStepPercent)
                {
                    step = (Algorithm.AlgorithmParameters[i].MaxValue - Algorithm.AlgorithmParameters[i].MinValue) * (Algorithm.AlgorithmParameters[i].Step / 100);
                }

                double currentValue = Algorithm.AlgorithmParameters[i].MinValue; //текущее значение

                if (isIntValueType)
                {
                    AlgorithmParametersAllIntValues[i].Add((int)Math.Round(currentValue));
                }
                else
                {
                    AlgorithmParametersAllDoubleValues[i].Add(currentValue);
                }
                currentValue += step;

                while (currentValue <= Algorithm.AlgorithmParameters[i].MaxValue)
                {
                    if (isIntValueType)
                    {
                        int intCurrentValue = (int)Math.Round(currentValue);
                        if (intCurrentValue != AlgorithmParametersAllIntValues[i].Last()) //если текущее значение отличается от предыдущего, добавляем его в целочисленные значения
                        {
                            AlgorithmParametersAllIntValues[i].Add(intCurrentValue);
                        }
                    }
                    else
                    {
                        AlgorithmParametersAllDoubleValues[i].Add(currentValue);
                    }

                    currentValue += step;
                }
            }

            //формируем список со всеми комбинациями параметров
            List<int[]> allCombinations = new List<int[]>();
            //сначала проходим по параметрам индикаторов
            for (int ind = 0; ind < Algorithm.IndicatorParameterRanges.Count; ind++)
            {
                bool isIndicatorParameterIntValueType = (Algorithm.IndicatorParameterRanges[ind].IndicatorParameterTemplate.ParameterValueType.Id == 1) ? true : false;

                if (allCombinations.Count == 0) //формируем начальный список всех комбинаций при первом прохождении
                {
                    if (isIndicatorParameterIntValueType)
                    {
                        for (int indValIndex = 0; indValIndex < IndicatorsParametersAllIntValues[ind].Count; indValIndex++)
                        {
                            allCombinations.Add(new int[1] { indValIndex });
                        }
                    }
                    else
                    {
                        for (int indValIndex = 0; indValIndex < IndicatorsParametersAllDoubleValues[ind].Count; indValIndex++)
                        {
                            allCombinations.Add(new int[1] { indValIndex });
                        }
                    }
                }
                else
                {
                    List<int> indexes = new List<int>();
                    if (isIndicatorParameterIntValueType)
                    {
                        for (int indValIndex = 0; indValIndex < IndicatorsParametersAllIntValues[ind].Count; indValIndex++)
                        {
                            indexes.Add(indValIndex);
                        }
                    }
                    else
                    {
                        for (int indValIndex = 0; indValIndex < IndicatorsParametersAllDoubleValues[ind].Count; indValIndex++)
                        {
                            indexes.Add(indValIndex);
                        }
                    }
                    allCombinations = CreateCombinations(allCombinations, indexes);
                }
            }
            //теперь проходим по параметрам алгоритма
            for (int alg = 0; alg < Algorithm.AlgorithmParameters.Count; alg++)
            {
                bool isAlgorithmParameterIntValueType = (Algorithm.AlgorithmParameters[alg].ParameterValueType.Id == 1) ? true : false;

                if (allCombinations.Count == 0) //если комбинации пустые (не было параметров индикаторов), создаем комбинации
                {
                    if (isAlgorithmParameterIntValueType)
                    {
                        for (int algValIndex = 0; algValIndex < AlgorithmParametersAllIntValues[alg].Count; algValIndex++)
                        {
                            allCombinations.Add(new int[1] { algValIndex });
                        }
                    }
                    else
                    {
                        for (int algValIndex = 0; algValIndex < AlgorithmParametersAllDoubleValues[alg].Count; algValIndex++)
                        {
                            allCombinations.Add(new int[1] { algValIndex });
                        }
                    }
                }
                else
                {
                    List<int> indexes = new List<int>();
                    if (isAlgorithmParameterIntValueType)
                    {
                        for (int algValIndex = 0; algValIndex < AlgorithmParametersAllIntValues[alg].Count; algValIndex++)
                        {
                            indexes.Add(algValIndex);
                        }
                    }
                    else
                    {
                        for (int algValIndex = 0; algValIndex < AlgorithmParametersAllDoubleValues[alg].Count; algValIndex++)
                        {
                            indexes.Add(algValIndex);
                        }
                    }
                    allCombinations = CreateCombinations(allCombinations, indexes);
                }
            }

            //формируем тестовые связки
            foreach (DataSourceGroup dataSourceGroup in DataSourceGroups)
            {
                //формируем серии оптимизационных тестов для данного источника данных для каждого периода

                //определяем диапазон доступных дат для данной группы источников данных (начальная и конечная даты которые есть во всех источниках данных группы)
                DateTime availableDateStart = new DateTime();
                DateTime availableDateEnd = new DateTime();
                for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                {
                    if(i == 0)
                    {
                        availableDateStart = dataSourceGroup.DataSourceAccordances[i].DataSource.StartDate.Date;
                        availableDateEnd = dataSourceGroup.DataSourceAccordances[i].DataSource.EndDate.Date;
                    }
                    else
                    {
                        if(DateTime.Compare(availableDateStart, dataSourceGroup.DataSourceAccordances[i].DataSource.StartDate) < 0)
                        {
                            availableDateStart = dataSourceGroup.DataSourceAccordances[i].DataSource.StartDate.Date;
                        }
                        if (DateTime.Compare(availableDateEnd, dataSourceGroup.DataSourceAccordances[i].DataSource.EndDate) > 0)
                        {
                            availableDateEnd = dataSourceGroup.DataSourceAccordances[i].DataSource.EndDate.Date;
                        }
                    }
                }
                availableDateEnd = availableDateEnd.AddDays(1); //прибавляем 1 день, т.к. в расчетах последний день является днем окончания и не торговым днем, а здесь последний день вычисляется как торговый

                //определяем дату окончания тестирования
                DateTime endDate = DateTime.Compare(availableDateEnd, EndPeriod) > 0 ? EndPeriod : availableDateEnd;

                DateTime currentDate = StartPeriod; //текущая дата

                //определяем минимально допустимую длительность оптимизационного теста ((текущая дата + оптимизация  -  текущая) * % из настроек)
                TimeSpan minimumAllowedOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days) - currentDate).TotalDays * ((double)_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100)));
                minimumAllowedOptimizationDuration = minimumAllowedOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : minimumAllowedOptimizationDuration; //если менее одного дня, устанавливаем в один день
                //определяем минимально допустимую длительность форвардного теста ((текущая дата + оптимизация + форвардный  -  текущая + оптимизация) * % из настроек)
                TimeSpan minimumAllowedForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days) - currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days)).TotalDays * ((double)_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100)));
                minimumAllowedForwardDuration = minimumAllowedForwardDuration.TotalDays < 1 && IsForwardTesting ? TimeSpan.FromDays(1) : minimumAllowedForwardDuration; //если менее одного дня и это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)

                //в цикле определяется минимально допустимая длительность для следующей проверки, исходя из разности дат текущей и текущей + требуемой
                //цикл проверяет, помещается ли минимум в оставшийся период, так же в нем идет проверка на то, текущая раньше доступной или нет. Если да, то проверяется, помещается ли в период с доступной по текущая + промежуток, минимальная длительность. Если да, то текущая для расчетов устанавливается в начало доступной. Все даты определяются из текущей для расчетов, а не из текущей. Поэтому после установки текущей для расчетов в доступную, можно дальше расчитывать даты тем же алгоритмом что и для варианта когда текущая позже или равна доступной. Если же с доступной до текущей + промежуток минимальная длительность не помещается, цикл переходит на следующую итерацию.
                while (DateTime.Compare(currentDate.Add(minimumAllowedOptimizationDuration).Add(minimumAllowedForwardDuration).Date, endDate) <= 0)
                {
                    DateTime currentDateForCalculate = currentDate; //текущая дата для расчетов, в неё будет попадать доступная дата начала, если текущая раньше доступной

                    bool isSkipIteration = false; //пропустить итерацию или нет
                    //проверяем, текущая дата раньше доступной даты начала или нет
                    if(DateTime.Compare(currentDate, availableDateStart) < 0)
                    {
                        //проверяем, помещается ли минимальная длительность оптимизационного и форвардного тестов в промежуток с доступной даты начала по текущая + промежуток
                        if(DateTime.Compare(availableDateStart.Add(minimumAllowedOptimizationDuration).Add(minimumAllowedForwardDuration).Date, currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days)) < 0)
                        {
                            currentDateForCalculate = availableDateStart;
                        }
                        else
                        {
                            isSkipIteration = true; //т.к. минимально допустимая длительность не помещается в текущий промежуток, переходим на следующую итерацию цикла
                        }
                    }

                    if(isSkipIteration == false) //если минимальная длительность помещается в доступную, создаем тесты
                    {
                        //определяем начальные и конечные даты оптимизационного и форвардного тестов
                        DateTime optimizationStartDate = new DateTime();
                        DateTime optimizationEndDate = new DateTime(); //дата, на которой заканчивается тест, этот день не торговый
                        DateTime forwardStartDate = new DateTime();
                        DateTime forwardEndDate = new DateTime(); //дата, на которой заканчивается тест, этот день не торговый

                        //проверяем, помещается ли полная оптимизационная и форвардная длительность в доступный промежуток
                        if(DateTime.Compare(currentDateForCalculate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days), currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days)) > 0) //если текущая дата для расчетов + полная длительность позже текущей даты + полная длительность, значит не помещается
                        {
                            //определяем максимальную длительность, которая помещается в доступный промежуток
                            double currentDurationPercent = 99.75;
                            TimeSpan currentOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days) - currentDate).TotalDays * (currentDurationPercent / 100)));
                            currentOptimizationDuration = currentOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : currentOptimizationDuration; //если менее одного дня, устанавливаем в один день
                            TimeSpan currentForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days) - currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days)).TotalDays * (currentDurationPercent / 100)));
                            currentForwardDuration = currentForwardDuration.TotalDays < 1 && IsForwardTesting ? TimeSpan.FromDays(1) : currentForwardDuration; //если менее одного дня и это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)
                            //пока период с уменьшенной длительностью не поместится, уменьшаем длительность (пока текущая дата для расчетов + уменьшенная длительность больше текущей даты + полная длительность)
                            while (DateTime.Compare(currentDateForCalculate.Add(currentOptimizationDuration).Add(currentForwardDuration).Date, currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days)) > 0)
                            {
                                currentDurationPercent -= 0.25;
                                currentOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days) - currentDate).TotalDays * (currentDurationPercent / 100)));
                                currentOptimizationDuration = currentOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : currentOptimizationDuration; //если менее одного дня, устанавливаем в один день
                                currentForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days) - currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days)).TotalDays * (currentDurationPercent / 100)));
                                currentForwardDuration = currentForwardDuration.TotalDays < 1 && IsForwardTesting ? TimeSpan.FromDays(1) : currentForwardDuration; //если менее одного дня и это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)
                            }

                            //устанавливаем начальные и конечные даты оптимизационного и форвардного тестов
                            optimizationStartDate = currentDateForCalculate;
                            optimizationEndDate = currentDateForCalculate.Add(currentOptimizationDuration).Date;
                            forwardStartDate = optimizationEndDate;
                            forwardEndDate = currentDateForCalculate.Add(currentOptimizationDuration).Add(currentForwardDuration).Date;
                        }
                        else
                        {
                            //устанавливаем начальные и конечные даты оптимизационного и форвардного тестов
                            optimizationStartDate = currentDateForCalculate;
                            optimizationEndDate = currentDateForCalculate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).Date;
                            forwardStartDate = optimizationEndDate;
                            forwardEndDate = currentDateForCalculate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days).Date;
                        }

                        //создаем testBatch
                        TestBatch testBatch = new TestBatch { DataSourceGroup = dataSourceGroup, StatisticalSignificance = new List<string[]>() };

                        //формируем оптимизационные тесты
                        List<TestRun> optimizationTestRuns = new List<TestRun>();
                        for (int i = 0; i < allCombinations.Count; i++)
                        {
                            Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>() };
                            //формируем список со значениями параметров индикаторов
                            List<IndicatorParameterValue> indicatorParameterValues = new List<IndicatorParameterValue>();
                            int ind = 0;
                            while(ind < Algorithm.IndicatorParameterRanges.Count)
                            {
                                IndicatorParameterValue indicatorParameterValue = new IndicatorParameterValue { IndicatorParameterTemplate = Algorithm.IndicatorParameterRanges[ind].IndicatorParameterTemplate };
                                if(Algorithm.IndicatorParameterRanges[ind].IndicatorParameterTemplate.ParameterValueType.Id == 1)
                                {
                                    indicatorParameterValue.IntValue = IndicatorsParametersAllIntValues[ind][allCombinations[i][ind]];
                                }
                                else
                                {
                                    indicatorParameterValue.DoubleValue = IndicatorsParametersAllDoubleValues[ind][allCombinations[i][ind]];
                                }
                                indicatorParameterValues.Add(indicatorParameterValue);
                                ind++;
                            }
                            //формируем список со значениями параметров алгоритма
                            List<AlgorithmParameterValue> algorithmParameterValues = new List<AlgorithmParameterValue>();
                            int alg = 0;
                            while (alg < Algorithm.AlgorithmParameters.Count)
                            {
                                AlgorithmParameterValue algorithmParameterValue = new AlgorithmParameterValue { AlgorithmParameter = Algorithm.AlgorithmParameters[alg] };
                                if (Algorithm.AlgorithmParameters[alg].ParameterValueType.Id == 1)
                                {
                                    algorithmParameterValue.IntValue = AlgorithmParametersAllIntValues[alg][allCombinations[i][ind + alg]];
                                }
                                else
                                {
                                    algorithmParameterValue.DoubleValue = AlgorithmParametersAllDoubleValues[alg][allCombinations[i][ind + alg]];
                                }
                                algorithmParameterValues.Add(algorithmParameterValue);
                                alg++;
                            }
                            TestRun testRun = new TestRun { TestBatch = testBatch, Account = account, StartPeriod = optimizationStartDate, EndPeriod = optimizationEndDate, IndicatorParameterValues = indicatorParameterValues, AlgorithmParameterValues = algorithmParameterValues, EvaluationCriteriaValues = new List<EvaluationCriteriaValue>(), DealsDeviation = new List<string>(), LoseDeviation = new List<string>(), ProfitDeviation = new List<string>(), LoseSeriesDeviation = new List<string>(), ProfitSeriesDeviation = new List<string>() };
                            optimizationTestRuns.Add(testRun);
                        }
                        testBatch.OptimizationTestRuns = optimizationTestRuns;

                        //формируем форвардный тест
                        if (IsForwardTesting)
                        {
                            List<DepositCurrency> takenForwardDepositCurrencies = new List<DepositCurrency>(); //средства в открытых позициях
                            foreach(DepositCurrency depositCurrency in ForwardDepositCurrencies)
                            {
                                takenForwardDepositCurrencies.Add(new DepositCurrency { Currency = depositCurrency.Currency, Deposit = 0 });
                            }

                            Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>() };
                            Account accountDepositTrading = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>(), FreeForwardDepositCurrencies = ForwardDepositCurrencies, TakenForwardDepositCurrencies = takenForwardDepositCurrencies };
                            TestRun testRun = new TestRun { TestBatch = testBatch, Account = account, AccountDepositTrading = accountDepositTrading, StartPeriod = forwardStartDate, EndPeriod = forwardEndDate, EvaluationCriteriaValues = new List<EvaluationCriteriaValue>(), DealsDeviation = new List<string>(), LoseDeviation = new List<string>(), ProfitDeviation = new List<string>(), LoseSeriesDeviation = new List<string>(), ProfitSeriesDeviation = new List<string>() };
                            //добавляем форвардный тест в testBatch
                            testBatch.ForwardTestRun = testRun;
                        }

                        TestBatches.Add(testBatch);
                    }

                    //прибавляем к текущей дате временной промежуток между оптимизационными тестами
                    currentDate = currentDate.AddYears(OptimizationTestSpacing.Years).AddMonths(OptimizationTestSpacing.Months).AddDays(OptimizationTestSpacing.Days);

                    //определяем минимально допустимую длительность оптимизационного теста ((текущая дата + оптимизация  -  текущая) * % из настроек)
                    minimumAllowedOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days) - currentDate).TotalDays * ((double)_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100)));
                    minimumAllowedOptimizationDuration = minimumAllowedOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : minimumAllowedOptimizationDuration; //если менее одного дня, устанавливаем в один день
                    //определяем минимально допустимую длительность форвардного теста ((текущая дата + оптимизация + форвардный  -  текущая + оптимизация) * % из настроек)
                    minimumAllowedForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days) - currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days)).TotalDays * ((double)_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100)));
                    minimumAllowedForwardDuration = minimumAllowedForwardDuration.TotalDays < 1 && IsForwardTesting ? TimeSpan.FromDays(1) : minimumAllowedForwardDuration; //если менее одного дня и это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)
                }
            }

            //выполняем тесты
            bool isErrorCompile = false; //были ли ошибки при компиляции
            //создаем классы индикаторов, алгоритма, и  критериев оценки
            //создаем классы индикаторов
            //определяем список используемых индикаторов
            List<Indicator> indicators = new List<Indicator>();
            for (int i = 0; i < Algorithm.IndicatorParameterRanges.Count; i++)
            {
                if(indicators.Contains(Algorithm.IndicatorParameterRanges[i].Indicator) == false)
                {
                    indicators.Add(Algorithm.IndicatorParameterRanges[i].Indicator);
                }
            }

            CompiledIndicators = new dynamic[indicators.Count];
            for(int i = 0; i < indicators.Count; i++)
            {
                string variablesParameters = ""; //инициализация и присвоение значений переменным, в которых хранятся значения параметров индикатора
                int currentIndicatorParameterIndex = -1; //номер параметра для текущего индикатора (это число используется как индекс в массиве параметров который принимает скомпилированный индикатор)
                for(int k = 0; k < Algorithm.IndicatorParameterRanges.Count; k++)
                {
                    if(Algorithm.IndicatorParameterRanges[k].Indicator == indicators[i])
                    {
                        currentIndicatorParameterIndex++;
                        variablesParameters += Algorithm.IndicatorParameterRanges[k].IndicatorParameterTemplate.ParameterValueType.Id == 1 ? "int " : "double ";
                        variablesParameters += "Parameter_" + Algorithm.IndicatorParameterRanges[k].IndicatorParameterTemplate.Name;
                        variablesParameters += Algorithm.IndicatorParameterRanges[k].IndicatorParameterTemplate.ParameterValueType.Id == 1 ? " = indicatorParametersIntValues[" + currentIndicatorParameterIndex + "]; " : " = indicatorParametersDoubleValues[" + currentIndicatorParameterIndex + "]; ";
                    }
                }

                //добавляем в текст скрипта приведение к double переменных и чисел в операциях деления и умножения (т.к. при делении типов int на int получится тип int и дробная часть потеряется), а так же возвращаемого значения
                //удаляем дублирующиеся пробелы
                StringBuilder script = RemoveDuplicateSpaces(indicators[i].Script);
                //добавляем приведение к double правой части операции деления, чтобы результат int/int был с дробной частью
                script.Replace("/", "/(double)");
                //удаляем пробел между Candles и [
                script.Replace("Candles [", "Candles[");
                //определяем для всех обращений к Candles[]: индекс начала ключевого слова Candles[ и индекс закрывающей квардратной скобки, если внутри были еще квадратные скобки, их нужно пропустить и дойти до закрывающей
                string scriptIndicatorString = script.ToString();
                List<int> indexesCandles = new List<int>(); //индексы всех вхождений подстроки "Candles["
                if (scriptIndicatorString.IndexOf("Candles[") != -1)
                {
                    indexesCandles.Add(scriptIndicatorString.IndexOf("Candles["));
                    while (scriptIndicatorString.IndexOf("Candles[", indexesCandles.Last() + 1) != -1)
                    {
                        indexesCandles.Add(scriptIndicatorString.IndexOf("Candles[", indexesCandles.Last() + 1));
                    }
                }
                //проходим по всем indexesCandles с конца к началу, и заменяем все закрывающие квадратные скобки которые закрывают Candles[ на круглые, при этом внутренние квадратные скобки будет игнорироваться, и заменена будет только закрывающая Candles[
                for(int k = indexesCandles.Count - 1; k >= 0; k--)
                {
                    int countOpen = 1; //количество найденных открывающих скобок на текущий момент
                    int countClose = 0; //количество найденных закрывающих скобок на текущий момент
                    int currentIndex = indexesCandles[k] + 8; //индекс текущего символа
                    //пока количество открывающи не будет равно количеству закрывающих, или пока не превысим длину строки
                    while(countOpen != countClose && currentIndex < scriptIndicatorString.Length)
                    {
                        if(scriptIndicatorString[currentIndex] == '[')
                        {
                            countOpen++;
                        }
                        if (scriptIndicatorString[currentIndex] == ']')
                        {
                            countClose++;
                        }
                        currentIndex++;
                    }
                    if(countOpen != countClose) //не найдена закрывающия скобка, выводим сообщение
                    {
                        isErrorCompile = true;
                        _modelData.DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка в скрипте индикатора: отсутствует закрывающая скобка \"]\" при обращении к массиву Candles."); }));
                    }
                    //заменяем закрывающую квадратную скобку на круглую
                    scriptIndicatorString = scriptIndicatorString.Remove(currentIndex - 1, 1);
                    scriptIndicatorString = scriptIndicatorString.Insert(currentIndex - 1, ")");
                }

                script = new StringBuilder(scriptIndicatorString);
                //заменяем все Canldes[ на GetCandle(
                script.Replace("Candles[", "GetCandle(");


                Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
                System.CodeDom.Compiler.CompilerParameters param = new System.CodeDom.Compiler.CompilerParameters();
                param.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
                param.GenerateExecutable = false;
                param.GenerateInMemory = true;

                var compiled = provider.CompileAssemblyFromSource(param, new string[]
                {
                    @"
                    using System;
                    using ktradesystem.Models;
                    public class CompiledIndicator_" + indicators[i].Name +
                    @"{
                        public int MaxOverIndex;
                        public Candle[] Candles;
                        public int CurrentCandleIndex;
                        public IndicatorCalculateResult Calculate(Candle[] inputCandles, int currentCandleIndex, int[] indicatorParametersIntValues, double[] indicatorParametersDoubleValues)
                        {
                            " + variablesParameters + @"
                            MaxOverIndex = 0;
                            Candles = inputCandles;
                            CurrentCandleIndex = currentCandleIndex;
                            double Indicator = 0;
                            " + script +
                            @"return new IndicatorCalculateResult { Value = Indicator, OverIndex = MaxOverIndex };
                        }
                        public Candle GetCandle(int userIndex)
                        {
                            int realIndex = CurrentCandleIndex - userIndex;
                            Candle result;
                            if(realIndex < 0)
                            {
                                MaxOverIndex = - realIndex > MaxOverIndex? - realIndex: MaxOverIndex;
                                result = new Candle { DateTime = Candles[0].DateTime, O = Candles[0].O, H = Candles[0].H, L = Candles[0].L, C = Candles[0].C, V = Candles[0].V };
                            }
                            else
                            {
                                result = new Candle { DateTime = Candles[realIndex].DateTime, O = Candles[realIndex].O, H = Candles[realIndex].H, L = Candles[realIndex].L, C = Candles[realIndex].C, V = Candles[realIndex].V };
                            }
                            return result;
                        }
                    }" //MaxOverIndex - максимальное превышение индекса массива со свечками; GetCandle() создает и возвращает новый объект Candle для того чтобы в скрипте нельзя было переопределить значения свечки
                });
                if (compiled.Errors.Count == 0)
                {
                    CompiledIndicators[i] = compiled.CompiledAssembly.CreateInstance("CompiledIndicator_" + indicators[i].Name);
                }
                else
                {
                    isErrorCompile = true;
                    //отправляем пользователю сообщения об ошибке
                    for (int r = 0; r < compiled.Errors.Count; r++)
                    {
                        _modelData.DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка при компиляции индикатора " + indicators[i].Name + ": " + compiled.Errors[0].ErrorText); }));
                    }
                }
            }

            //создаем класс алгоритма
            string dataSourcesForCalculateVariables = ""; //объявление переменных для класса, в которых будут храниться DataSourcesForCalculate
            List<string> dsVariablesNames = new List<string>(); //список с названиями переменных dataSourceForCalculate
            for (int k = 0; k < Algorithm.DataSourceTemplates.Count; k++)
            {
                dsVariablesNames.Add("Datasource_" + Algorithm.DataSourceTemplates[k].Name);
                dataSourcesForCalculateVariables += "public DataSourceForCalculate " + dsVariablesNames[k] + "; ";
            }

            string algorithmVariables = ""; //инициализация и присвоение значений переменным, с которыми будет работать пользователь
            //присваиваем переменным источников данных значения
            for (int k = 0; k < Algorithm.DataSourceTemplates.Count; k++)
            {
                algorithmVariables += dsVariablesNames[k] + " = dataSourcesForCalculate[" + k +"]; ";
            }
            //формируем параметры индикаторов для источников данных
            for(int i = 0; i < Algorithm.DataSourceTemplates.Count; i++)
            {
                for (int k = 0; k < indicators.Count; k++)
                {
                    algorithmVariables += "double " + dsVariablesNames[i] + "_Indicator_" + indicators[k].Name + " = dataSourcesForCalculate[" + i + "].IndicatorsValues[" + k + "]; ";
                }
            }
            //формируем параметры алгоритма
            for (int k = 0; k < Algorithm.AlgorithmParameters.Count; k++)
            {
                algorithmVariables += Algorithm.AlgorithmParameters[k].ParameterValueType.Id == 1 ? "int " : "double ";
                algorithmVariables += "Parameter_" + Algorithm.AlgorithmParameters[k].Name + " = ";
                algorithmVariables += Algorithm.AlgorithmParameters[k].ParameterValueType.Id == 1 ? "algorithmParametersIntValues[" + k + "]; " : "algorithmParametersDoubleValues[" + k + "]; ";
            }
            //удаляем дублирующиеся пробелы
            StringBuilder scriptAlgorithm = RemoveDuplicateSpaces(Algorithm.Script);
            //добавляем приведение к double правой части операции деления, чтобы результат int/int был с дробной частью
            scriptAlgorithm.Replace("/", "/(double)");
            //заменяем все обращения к конкретному индикатору типа: Datasource_maket.Indicator_sma на Datasource_maket_Indicator_sma
            for (int i = 0; i < Algorithm.DataSourceTemplates.Count; i++)
            {
                for (int k = 0; k < indicators.Count; k++)
                {
                    scriptAlgorithm.Replace(dsVariablesNames[i] + ".Indicator_" + indicators[k].Name, dsVariablesNames[i] + "_Indicator_" + indicators[k].Name);
                }
            }
            //удаляем пробелы до и после открывающейся скобки после ключевого слова на создание заявки
            string[] orderLetters = new string[] { "Order_LimitSell", "Order_LimitBuy", "Order_StopSell", "Order_StopBuy", "Order_MarketSell", "Order_MarketBuy", "Order_StopTakeBuy", "Order_StopTakeSell" }; //слова создания заявок
            foreach(string str in orderLetters)
            {
                scriptAlgorithm.Replace(str + " (", str + "("); //удаляем пробел перед открывающейся скобкой
                scriptAlgorithm.Replace(str + "( ", str + "("); //удаляем пробел после открывающейся скобки
            }
            //заменяем ключевые слова на создание заявок, на функцию добавления объекта типа Order в список orders
            string[] orderCorrectLetters = new string[] { "orders.Add(new Order(1, false,", "orders.Add(new Order(1, true,", "orders.Add(new Order(3, false,", "orders.Add(new Order(3, true,", "orders.Add(new Order(2, false,", "orders.Add(new Order(2, true,", "orders.AddRange(GetStopTake(true,", "orders.AddRange(GetStopTake(false," };
            for(int k = 0; k < orderLetters.Length; k++)
            {
                scriptAlgorithm.Replace(orderLetters[k] + "(", orderCorrectLetters[k]);
            }
            //добавляем закрывающую скобку для добавлений заявок
            List<int> indexesSemicolon = new List<int>(); //список с индексами точек с запятой, следующих за словом добавления заявки
            //находим индексы слов добавления заявок
            bool isSemicolonFind = true; //найдена ли точка с запятой после слова на создание заявки
            string scriptAlgorithmString = scriptAlgorithm.ToString(); //текст скрипта в формате string
            //ищем вхождения всех ключевых слов
            for (int k = 0; k < orderCorrectLetters.Length; k++)
            {
                int indexFindLetter = scriptAlgorithmString.IndexOf(orderCorrectLetters[k]); //индекс найденного слова
                while(indexFindLetter != -1)
                {
                    int indexSemicolon = scriptAlgorithmString.IndexOf(";", indexFindLetter); //индекс первой найденной точки запятой, от индекса слова добавления заявки
                    if(indexSemicolon != -1)
                    {
                        indexesSemicolon.Add(indexSemicolon);
                    }
                    else
                    {
                        isSemicolonFind = false; //указываем что после описания добавления заявки не найден символ точки с запятой
                    }

                    indexFindLetter = scriptAlgorithmString.IndexOf(orderCorrectLetters[k], indexFindLetter + 1); //ищем следующее вхождение данного слова
                }
            }
            if (isSemicolonFind == false)
            {
                isErrorCompile = true;
                //отправляем пользователю сообщения об ошибке
                _modelData.DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Синтаксическая ошибка в скрипте алгоритма: не найдена \";\" после добавления заявки."); }));
            }
            //вставляем закрывающую скобку
            for (int k = indexesSemicolon.Count - 1; k >= 0; k--)
            {
                scriptAlgorithm.Insert(indexesSemicolon[k], ")");
            }
            //заменяем для всех источников данных обращение типа: Datasource_maket.Candles[5] на GetCandle(Datasource_maket, 5)
            scriptAlgorithm.Replace("Candles [", "Candles["); //удаляем пробел
            scriptAlgorithmString = scriptAlgorithm.ToString(); //текст скрипта в формате string
            //заменяем все закрывающие скобки "]" при обращении к Candles на круглые ")"
            List<int> algorithmIndexesCandles = new List<int>(); //индексы всех вхождений подстроки "Candles["
            if (scriptAlgorithmString.IndexOf("Candles[") != -1)
            {
                algorithmIndexesCandles.Add(scriptAlgorithmString.IndexOf("Candles["));
                while (scriptAlgorithmString.IndexOf("Candles[", algorithmIndexesCandles.Last() + 1) != -1)
                {
                    algorithmIndexesCandles.Add(scriptAlgorithmString.IndexOf("Candles[", algorithmIndexesCandles.Last() + 1));
                }
            }
            //проходим по всем indexesCandles с конца к началу, и заменяем все закрывающие квадратные скобки которые закрывают Candles[ на круглые, при этом внутренние квадратные скобки будет игнорироваться, и заменена будет только закрывающая Candles[
            for(int k = algorithmIndexesCandles.Count - 1; k >= 0; k--)
            {
                int countOpen = 1; //количество найденных открывающих скобок на текущий момент
                int countClose = 0; //количество найденных закрывающих скобок на текущий момент
                int currentIndex = algorithmIndexesCandles[k] + 8; //индекс текущего символа
                //пока количество открывающи не будет равно количеству закрывающих, или пока не превысим длину строки
                while(countOpen != countClose && currentIndex < scriptAlgorithmString.Length)
                {
                    if(scriptAlgorithmString[currentIndex] == '[')
                    {
                        countOpen++;
                    }
                    if (scriptAlgorithmString[currentIndex] == ']')
                    {
                        countClose++;
                    }
                    currentIndex++;
                }
                if(countOpen != countClose) //не найдена закрывающия скобка, выводим сообщение
                {
                    isErrorCompile = true;
                    _modelData.DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка в скрипте алгоритма: отсутствует закрывающая скобка \"]\" при обращении к массиву Candles."); }));
                }
                //заменяем закрывающую квадратную скобку на круглую
                scriptAlgorithmString = scriptAlgorithmString.Remove(currentIndex - 1, 1);
                scriptAlgorithmString = scriptAlgorithmString.Insert(currentIndex - 1, ")");
            }
            scriptAlgorithm = new StringBuilder(scriptAlgorithmString);
            //заменяем все обращения типа: Datasource_maket.Candles[ на GetCandle(Datasource_maket, 
            for (int k = 0; k < dsVariablesNames.Count; k++)
            {
                //находим индексы начала обращения к Datasource_maket.Candles[
                scriptAlgorithm.Replace(dsVariablesNames[k] + ".Candles[", "GetCandle(" + dsVariablesNames[k] + ", ");
            }
            
            Microsoft.CSharp.CSharpCodeProvider providerAlgorithm = new Microsoft.CSharp.CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerParameters paramAlgorithm = new System.CodeDom.Compiler.CompilerParameters();
            paramAlgorithm.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            paramAlgorithm.GenerateExecutable = false;
            paramAlgorithm.GenerateInMemory = true;

            var compiledAlgorithm = providerAlgorithm.CompileAssemblyFromSource(paramAlgorithm, new string[]
            {
                @"
                using System;
                using System.Collections.Generic;
                using ktradesystem.Models;
                public class CompiledAlgorithm
                {
                    " + dataSourcesForCalculateVariables + @"
                    int MaxOverIndex;
                    public AlgorithmCalculateResult Calculate(AccountForCalculate accountForCalculate, DataSourceForCalculate[] dataSourcesForCalculate, int[] algorithmParametersIntValues, double[] algorithmParametersDoubleValues)
                    {
                        " + algorithmVariables + @"
                        MaxOverIndex = 0;
                        List<Order> orders = new List<Order>();
                        " + scriptAlgorithm +
                        @"return new AlgorithmCalculateResult { Orders = orders, OverIndex = MaxOverIndex };
                    }
                    public Candle GetCandle(DataSourceForCalculate dataSourcesForCalculate, int userIndex)
                    {
                        int realIndex = dataSourcesForCalculate.CurrentCandleIndex - userIndex;
                        Candle result;
                        if(realIndex < 0)
                        {
                            MaxOverIndex = - realIndex > MaxOverIndex? - realIndex: MaxOverIndex;
                            result = new Candle { DateTime = dataSourcesForCalculate.Candles[0].DateTime, O = dataSourcesForCalculate.Candles[0].O, H = dataSourcesForCalculate.Candles[0].H, L = dataSourcesForCalculate.Candles[0].L, C = dataSourcesForCalculate.Candles[0].C, V = dataSourcesForCalculate.Candles[0].V };
                        }
                        else
                        {
                            result = new Candle { DateTime = dataSourcesForCalculate.Candles[realIndex].DateTime, O = dataSourcesForCalculate.Candles[realIndex].O, H = dataSourcesForCalculate.Candles[realIndex].H, L = dataSourcesForCalculate.Candles[realIndex].L, C = dataSourcesForCalculate.Candles[realIndex].C, V = dataSourcesForCalculate.Candles[realIndex].V };
                        }
                        return result;
                    }
                    public List<Order> GetStopTake(bool direction, DataSourceForCalculate dataSourceForCalculate, double stopPrice, double takePrice, decimal count)
                    {
                        Order stopOrder = new Order(3, direction, dataSourceForCalculate, stopPrice, count);
                        Order takeOrder = new Order(1, direction, dataSourceForCalculate, takePrice, count);
                        stopOrder.LinkedOrder = takeOrder;
                        takeOrder.LinkedOrder = stopOrder;
                        return new List<Order> { stopOrder, takeOrder };
                    }
                }"
            });
            if (compiledAlgorithm.Errors.Count == 0)
            {
                CompiledAlgorithm = compiledAlgorithm.CompiledAssembly.CreateInstance("CompiledAlgorithm");
            }
            else
            {
                isErrorCompile = true;
                //отправляем пользователю сообщения об ошибке
                for (int r = 0; r < compiledAlgorithm.Errors.Count; r++)
                {
                    _modelData.DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка при компиляции алгоритма: " + compiledAlgorithm.Errors[0].ErrorText); }));
                }
            }

            //создаем классы критериев оценки
            CompiledEvaluationCriterias = new dynamic[_modelData.EvaluationCriterias.Count];
            for(int i = 0; i < _modelData.EvaluationCriterias.Count; i++)
            {
                string script = _modelData.EvaluationCriterias[i].Script;

                Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
                System.CodeDom.Compiler.CompilerParameters param = new System.CodeDom.Compiler.CompilerParameters();
                param.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
                param.GenerateExecutable = false;
                param.GenerateInMemory = true;
                
                var compiled = provider.CompileAssemblyFromSource(param, new string[]
                {
                    @"
                    using System;
                    using ktradesystem.Models;
                    public class CompiledEvaluationCriteria_" + _modelData.EvaluationCriterias[i].Name +
                    @"{
                        public EvaluationCriteriaValue Calculate(DataSourceCandles dataSourceCandles, List<EvaluationCriteriaValue> evaluationCriteriaValues, ObservableCollection<Setting> settings)
                        {
                            double doubleValue = 0;
                            string stringValue = """";
                            " + script +
                            @"return new EvaluationCriteriaValue { DoubleValue = doubleValue, StringValue = stringValue };
                        }
                    }"
                });
                if (compiled.Errors.Count == 0)
                {
                    CompiledEvaluationCriterias[i] = compiled.CompiledAssembly.CreateInstance("CompiledEvaluationCriteria_" + _modelData.EvaluationCriterias[i].Name);
                }
                else
                {
                    isErrorCompile = true;
                    //отправляем пользователю сообщения об ошибке
                    for (int r = 0; r < compiled.Errors.Count; r++)
                    {
                        _modelData.DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Ошибка при компиляции критерия оценки " + _modelData.EvaluationCriterias[i].Name + ": " + compiled.Errors[0].ErrorText); }));
                    }
                }
            }


            if (isErrorCompile == false)
            {
                //определяем количество testRun без учета форвардных
                int countTestRuns = 0;
                foreach (TestBatch testBatch1 in TestBatches)
                {
                    foreach (TestRun testRun in testBatch1.OptimizationTestRuns)
                    {
                        countTestRuns++;
                    }
                }
                if (countTestRuns > 0) //если количество тестов больше нуля, переходим на создание задач и выполнение тестов
                {
                    CancellationToken cancellationToken = _modelTesting.CancellationTokenTesting.Token;
                    //определяем количество используемых потоков
                    int processorCount = Environment.ProcessorCount;
                    processorCount -= _modelData.Settings.Where(i => i.Id == 1).First().BoolValue ? 1 : 0; //если в настройках выбрано оставлять один поток, вычитаем из количества потоков
                    if (countTestRuns < processorCount) //если тестов меньше чем число доступных потоков, устанавливаем количество потоков на количество тестов, т.к. WaitAll ругается если задача в tasks null
                    {
                        processorCount = countTestRuns;
                    }
                    if (processorCount < 1)
                    {
                        processorCount = 1;
                    }


                    NumberFormatInfo nfiComma = CultureInfo.GetCultureInfo("ru-RU").NumberFormat;
                    NumberFormatInfo nfiDot = (NumberFormatInfo)nfiComma.Clone();
                    nfiDot.NumberDecimalSeparator = nfiDot.CurrencyDecimalSeparator = nfiDot.PercentDecimalSeparator = "."; //эту переменнную нужно указать в методе double.Parse(string, nfiDot), чтобы преобразовался формат строки с разделителем дробной части в виде точки а не запятой

                    //считываем свечки всех источников данных
                    DataSourcesCandles = new List<DataSourceCandles>(); //инициализируем массив со всеми свечками источников данных
                    foreach (DataSourceGroup dataSourceGroup in DataSourceGroups)
                    {
                        for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                        {
                            //если такого источника данных еще нет в DataSourcesCandles, считываем его файлы
                            if(DataSourcesCandles.Where(j => j.DataSource == dataSourceGroup.DataSourceAccordances[i].DataSource).Any() == false)
                            {
                                DataSourcesCandles.Add(new DataSourceCandles { DataSource = dataSourceGroup.DataSourceAccordances[i].DataSource, Candles = new Candle[dataSourceGroup.DataSourceAccordances[i].DataSource.DataSourceFiles.Count][], IndicatorsValues = new IndicatorValues[indicators.Count] });
                                //проходим по всем файлам источника данных
                                for (int k = 0; k < dataSourceGroup.DataSourceAccordances[i].DataSource.DataSourceFiles.Count; k++)
                                {
                                    string fileName = dataSourceGroup.DataSourceAccordances[i].DataSource.DataSourceFiles[k].Path;
                                    //определяем размер массива (исходя из количества строк в файле)
                                    FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                                    StreamReader streamReader = new StreamReader(fileStream);
                                    string line = streamReader.ReadLine(); //пропускаем шапку файла
                                    line = streamReader.ReadLine();
                                    int count = 0;
                                    while (line != null)
                                    {
                                        count++;
                                        line = streamReader.ReadLine();
                                    }
                                    streamReader.Close();
                                    fileStream.Close();

                                    //создаем массив
                                    Candle[] candles = new Candle[count];
                                    //заполняем массив
                                    fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                                    streamReader = new StreamReader(fileStream);
                                    line = streamReader.ReadLine(); //пропускаем шапку файла
                                    line = streamReader.ReadLine(); //счиытваем 1-ю строку с данными
                                    int r = 0;
                                    while (line != null)
                                    {
                                        string[] lineArr = line.Split(',');
                                        string dateTimeFormated = lineArr[2].Insert(6, "-").Insert(4, "-") + " " + lineArr[3].Insert(4, ":").Insert(2, ":");
                                        candles[r] = new Candle { DateTime = DateTime.Parse(dateTimeFormated), O = double.Parse(lineArr[4], nfiDot), H = double.Parse(lineArr[5], nfiDot), L = double.Parse(lineArr[6], nfiDot), C = double.Parse(lineArr[7], nfiDot), V = double.Parse(lineArr[8], nfiDot) };
                                        line = streamReader.ReadLine();
                                        r++;
                                    }
                                    streamReader.Close();
                                    fileStream.Close();

                                    DataSourcesCandles.Last().Candles[k] = candles;
                                }
                            }
                            
                        }
                    }

                    //заполняем элементы массива IndicatorsValues объектами IndicatorValues, указываем размерность Values исходя из количества файлов. Размер массива со значениями для файла будет определен при заполнении значений в потоке.
                    for(int i = 0; i < DataSourcesCandles.Count; i++)
                    {
                        for(int k = 0; k < DataSourcesCandles[i].IndicatorsValues.Length; k++)
                        {
                            DataSourcesCandles[i].IndicatorsValues[k] = new IndicatorValues { Indicator = indicators[i], Values = new double[DataSourcesCandles[i].Candles.Length][] };
                        }
                    }

                    //вычисляем идеальную прибыль дял каждого DataSourceCandles
                    foreach(DataSourceCandles dataSourceCandles in DataSourcesCandles)
                    {
                        double pricesAmount = 0; //сумма разности цен закрытия, взятой по модулю
                        int fileIndex = 0;
                        int candleIndex = 0;
                        DateTime currentDateTime = dataSourceCandles.Candles[fileIndex][candleIndex].DateTime;
                        bool isOverFileIndex = false; //вышел ли какой-либо из индексов файлов за границы массива файлов источника данных
                        while(isOverFileIndex == false)
                        {
                            if(candleIndex > 0) //чтобы не обращатсья к прошлой свечке при смне файла
                            {
                                currentDateTime = dataSourceCandles.Candles[fileIndex][candleIndex].DateTime;
                                pricesAmount += Math.Abs(dataSourceCandles.Candles[fileIndex][candleIndex].C - dataSourceCandles.Candles[fileIndex][candleIndex - 1].C); //прибавляем разность цен закрытия, взятую по модулю
                            }
                            //переходим на следующую свечку, пока не дойдем до даты которая позже текущей или пока не выйдем за пределы файлов
                            bool isOverDate = DateTime.Compare(dataSourceCandles.Candles[fileIndex][candleIndex].DateTime, currentDateTime) > 0; //дошли ли до даты которая позже текущей
                            while (isOverDate == false && isOverFileIndex == false)
                            {
                                candleIndex++;
                                //если массив со свечками файла подошел к концу, переходим на следующий файл
                                if (candleIndex >= dataSourceCandles.Candles[fileIndex].Length)
                                {
                                    fileIndex++;
                                    candleIndex = 0;
                                }
                                //если индекс файла не вышел за пределы массива, проверяем, дошли ли до даты которая позже текущей
                                if (fileIndex < dataSourceCandles.Candles.Length)
                                {
                                    isOverDate = DateTime.Compare(dataSourceCandles.Candles[fileIndex][candleIndex].DateTime, currentDateTime) > 0;
                                }
                                else
                                {
                                    isOverFileIndex = true;
                                }
                            }
                        }
                        dataSourceCandles.PerfectProfit = pricesAmount / dataSourceCandles.DataSource.PriceStep * dataSourceCandles.DataSource.CostPriceStep; //записываем идеальную прибыль
                    }


                    //выполняем тестирование для всех TestBatches
                    Task[] tasks = new Task[processorCount]; //задачи
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    int testBatchIndex = 0; //индекс тестовой связки, testRun-ы которой отправляются в задачи
                    int testRunIndex = 0; //индекс testRun-а, который отправляется в задачи
                    int[][] tasksExecutingTestRuns = new int[processorCount][]; //массив, в котором хранится индекс testBatch-а (в 0-м индексе) и testRuna (из OptimizationTestRuns) (в 1-м индексе), который выполняется в задаче с таким же индексом в массиве задач (если это форвардный тест массив бдет состоять только из 1 элемента: индекса testBatch-а)
                    int[][] testRunsStatus = new int[TestBatches.Count - 1][]; //статусы выполненности testRun-ов в testBatch-ах. Первый индекс - индекс testBatch-а, второй - индекс testRun-a. У невыполненного значение 0, у выполненного 1
                    //создаем для каждого testBatch массив равный количеству testRun
                    for (int k = 0; k < TestBatches.Count; k++)
                    {
                        testRunsStatus[k] = new int[TestBatches[k].OptimizationTestRuns.Count];
                        for (int y = 0; y < testRunsStatus[k].Length; y++) { testRunsStatus[k][y] = 0; } //заполняем статусы testRun нулями
                    }
                    int n = 0; //номер прохождения цикла
                    while (testBatchIndex < TestBatches.Count && cancellationToken.IsCancellationRequested == false)
                    {
                        //если пока еще не заполнен массив с задачами, заполняем его
                        if (tasks[tasks.Length - 1] == null)
                        {
                            Task task = tasks[n];
                            TestRun testRun = TestBatches[testBatchIndex].OptimizationTestRuns[testRunIndex];
                            task = Task.Run(() => TestRunExecute(testRun, indicators));
                            tasksExecutingTestRuns[n] = new int[2] { testBatchIndex, testRunIndex }; //запоминаем индексы testBatch и testRun, который выполняется в текущей задачи (в элементе массива tasks с индексом n)
                            testRunIndex++;
                            testBatchIndex += TestBatches[testBatchIndex].OptimizationTestRuns.Count > testRunIndex ? 1 : 0; //если индекс testRun >= количеству OptimizationTestRuns, переходим на следующий testBatch
                        }
                        else //иначе обрабатываем выполненную задачу
                        {
                            int completedTaskIndex = Task.WaitAny(tasks);
                            //если это форвардное тестирование, проверяем, выполнены ли все testRun-ы этого testBatch-а и форвардный не запущен, если да - определяем топ-модель и запускаем форвардный тест
                            //отмечаем testRun как выполненный (если это не форвардный тест)
                            if (tasksExecutingTestRuns[completedTaskIndex].Length == 2) //если в массиве 2 элемента, зачит это не форвардный тест, и его нужно записать
                            {
                                testRunsStatus[tasksExecutingTestRuns[completedTaskIndex][0]][tasksExecutingTestRuns[completedTaskIndex][1]] = 1; //присваиваем статусу с сохраненным для этой задачи индексами testBatch и testRun, значение 1 (то есть выполнено)

                                //проверяем, если все testRun для данного testBatch (к которому принадлежит выполненный) выполненны, определяем топ-модель и статистическую значимость
                                bool isOptimizationTestsComplete = true;
                                foreach (int a in testRunsStatus[tasksExecutingTestRuns[completedTaskIndex][0]])
                                {
                                    if (a == 0)
                                    {
                                        isOptimizationTestsComplete = false;
                                    }
                                }
                                if (isOptimizationTestsComplete)
                                {
                                    //определяем топ-модель и статистичекую значимость
                                    //определяем оси двумерной плоскости поиска топ-модели с соседями, для которых волатильность критерия оценки максимальная
                                    //рассматриваются только те пары осей, площадь которых не менее 60% от максимальной площади. Чтобы исключить выбор осей с небольшой площадью но большой средней волатильностью
                                    //формируем список со всеми параметрами
                                    List<int[]> indicatorsAndAlgorithmParameters = new List<int[]>(); //список с параметрами (0-й элемент массива - тип параметра: 1-индикатор, 2-алгоритм, 1-й элемент массива - индекс параметра)
                                    for(int i = 0; i < IndicatorsParametersAllIntValues.Length; i++)
                                    {
                                        indicatorsAndAlgorithmParameters.Add(new int[2] { 1, i }); //запоминаем что параметр индикатор с индексом i
                                    }
                                    for(int i = 0; i < AlgorithmParametersAllIntValues.Length; i++)
                                    {
                                        indicatorsAndAlgorithmParameters.Add(new int[2] { 2, i }); //запоминаем что параметр индикатор с индексом i
                                    }
                                    //находим максимальную площадь плоскости
                                    int maxArea = 0;
                                    int axisX = 0; //одна ось плоскости
                                    int axisY = 0; //вторая ось плоскости
                                    for(int i = 0; i < indicatorsAndAlgorithmParameters.Count; i++)
                                    {
                                        for(int k = 0; k < indicatorsAndAlgorithmParameters.Count; k++)
                                        {
                                            if (i != k)
                                            {
                                                int iCount = 0; //количество элементов в параметре с индексом i
                                                if(indicatorsAndAlgorithmParameters[i][0] == 1) //если параметр индикатор
                                                {
                                                    iCount = IndicatorsParametersAllIntValues[indicatorsAndAlgorithmParameters[i][1]].Count > 0 ? IndicatorsParametersAllIntValues[indicatorsAndAlgorithmParameters[i][1]].Count : IndicatorsParametersAllDoubleValues[indicatorsAndAlgorithmParameters[i][1]].Count; //если количество элементов в int values больше нуля, присваиваем количеству параметра количество int values элементов, индаче количество double values элементов
                                                }
                                                else //если параметр алгоритм
                                                {
                                                    iCount = AlgorithmParametersAllIntValues[indicatorsAndAlgorithmParameters[i][1]].Count > 0 ? AlgorithmParametersAllIntValues[indicatorsAndAlgorithmParameters[i][1]].Count : AlgorithmParametersAllDoubleValues[indicatorsAndAlgorithmParameters[i][1]].Count; //если количество элементов в int values больше нуля, присваиваем количеству параметра количество int values элементов, индаче количество double values элементов
                                                }
                                                int kCount = 0; //количество элементов в параметре с индексом k
                                                if (indicatorsAndAlgorithmParameters[k][0] == 1) //если параметр индикатор
                                                {
                                                    kCount = IndicatorsParametersAllIntValues[indicatorsAndAlgorithmParameters[k][1]].Count > 0 ? IndicatorsParametersAllIntValues[indicatorsAndAlgorithmParameters[k][1]].Count : IndicatorsParametersAllDoubleValues[indicatorsAndAlgorithmParameters[k][1]].Count; //если количество элементов в int values больше нуля, присваиваем количеству параметра количество int values элементов, индаче количество double values элементов
                                                }
                                                else //если параметр алгоритм
                                                {
                                                    kCount = AlgorithmParametersAllIntValues[indicatorsAndAlgorithmParameters[k][1]].Count > 0 ? AlgorithmParametersAllIntValues[indicatorsAndAlgorithmParameters[k][1]].Count : AlgorithmParametersAllDoubleValues[indicatorsAndAlgorithmParameters[k][1]].Count; //если количество элементов в int values больше нуля, присваиваем количеству параметра количество int values элементов, индаче количество double values элементов
                                                }
                                                if(iCount * kCount > maxArea) //если площадь данной комбинации больше максимальной, запоминаем площадь плоскости и её оси
                                                {
                                                    maxArea = iCount * kCount;
                                                    axisX = i;
                                                    axisY = k;
                                                }
                                            }
                                        }
                                    }
                                    //формируем


                                }
                            }
                        }
                        n++;
                    }











                    
                }
            }

            
            




        }

        private List<int[]> CreateCombinations(List<int[]> combination, List<int> indexes) //принимает 2 списка, 1-й - содержит массив с комбинации индексов параметров: {[0,0],[0,1],[1,0],[1,1]}, второй только индексы: {0,1}, функция перебирает все комбинации элементов обоих списков и возвращает новый список в котором индексы 2-го списка добавлены в комбинацию 1-го: {[0,0,0],[0,0,1],[0,1,0]..}
        {
            List<int[]> newCombination = new List<int[]>();
            for(int i = 0; i < combination.Count; i++)
            {
                for(int k = 0; k < indexes.Count; k++)
                {
                    int[] arr = new int[combination[i].Length + 1]; //создаем новый массив с комбинацией индексов параметров, превышающий старый на один элемент
                    for(int n = 0; n < combination[i].Length; n++) //заносим в новый массив все элементы старого массива
                    {
                        arr[n] = combination[i][n];
                    }
                    arr[combination[i].Length] = indexes[k]; //помещаем в последний элемент нового массива индекс из списка индексов
                    newCombination.Add(arr); //добавляем новую созданную комбинацию в список новых комбинаций
                }
            }
            return newCombination;
        }

        private StringBuilder RemoveDuplicateSpaces(string str) //удаляет дублирующиеся пробелы в строке, и возвращает объект StringBuilder
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(str[0]); //добавляем первый символ, т.к. stringBuilder[stringBuilder.Length-1] в цикле будет обращаться к -1 индексу
            for (int k = 1; k < str.Length; k++)
            {
                if ((stringBuilder[stringBuilder.Length - 1] == ' ' && str[k] == ' ') == false) //если последний символ в stringBuilder = пробел и добавляемый = пробел, пропускаем, если же это ложно то добавляем символ в stringBuilder
                {
                    stringBuilder.Append(str[k]);
                }
            }
            return stringBuilder;
        }

        private void IndicatorValuesForFileCalculate(DataSourceCandles dataSourceCandles, int indicatorIndex, int fileIndex) //выполняет расчет значений индикатора на основе массива свечек для файла, и помещает в элемент элемента массива IndicatorsValues по индексу файла
        {
            dataSourceCandles.IndicatorsValues[indicatorIndex].Values[fileIndex] = new double[dataSourceCandles.Candles[fileIndex].Length]; //создаем массив размерностью с количество свечек для значений индикатора
            int i = 0;
            while (i < dataSourceCandles.Candles[fileIndex].Length) //проходим по всем свечкам
            {
                //вычисляем значение индикатора
                //IndicatorCalculateResult indicatorCalculateResult = CompiledIndicators[indicatorIndex].Calculate(dataSourceCandles.Candles[fileIndex], i, )
                //Calculate(Candle[] inputCandles, int currentCandleIndex, int[] indicatorParametersIntValues, double[] indicatorParametersDoubleValues)
                i++;
            }
        }

        public void IndicatorValuesWithParameterCalculate(TestRun testRun) //вычисляет индикаторы для всех источников данных testRun-а, с параметрами индикатора, взятыми из testRun-а, и помещает их в IndicatorsValues объекта DataSourceCandles, теперь для свечек файлов будут так же значения индикаторов для каждой свечки, эта функция будет использоваться для вычисления индикаторов которые будут отображаться на графике конкретного testRun-а
        {
            //вычисляем индикаторы для всех DataSourcesCandles, делаем это несколькими потоками
            /*int processorCount = Environment.ProcessorCount;
            processorCount -= _modelData.Settings.Where(i => i.Id == 1).First().BoolValue ? 1 : 0; //если в настройках выбрано оставлять один поток, вычитаем из количества потоков
            if (countTestRuns < processorCount) //если тестов меньше чем число доступных потоков, устанавливаем количество потоков на количество тестов, т.к. WaitAll ругается если задача в tasks null
            {
                processorCount = countTestRuns;
            }
            if (processorCount < 1)
            {
                processorCount = 1;
            }
            Task[] tasksIndicator = new Task[processorCount]; //задачи
            Stopwatch stopwatchIndicator = new Stopwatch();
            stopwatchIndicator.Start();
            int dataSourceIndex = 0; //индекс DataSourcesCandles
            int indicatorIndex = 0; //индекс индиктора в массиве IndicatorsValues
            int fileIndex = 0; //индекс массива Candles[], соответствующего файлу
            int m = 0; //номер прохождения цикла
                       //вычисляем индикаторы в следующей последовательности: проходим по DataSourcesCandles => проходим по IndicatorsValues, и для каждого перебираем все файлы, после чего присваиваем элементу массива IndicatorsValues созданный объект IndicatorValues
            while (dataSourceIndex < DataSourcesCandles.Count)
            {
                //если пока еще не заполнен массив с задачами, заполняем его
                if (tasksIndicator[tasksIndicator.Length - 1] == null)
                {
                    Task task = tasksIndicator[m];
                    DataSourceCandles dataSourceCandles = DataSourcesCandles[dataSourceIndex];
                    int iIndex = indicatorIndex;
                    int fIndex = fileIndex;
                    task = Task.Run(() => IndicatorValuesForFileCalculate(dataSourceCandles, iIndex, fIndex));
                    //переходим на следующий элемент
                    fileIndex++;
                    if (fileIndex >= DataSourcesCandles[dataSourceIndex].DataSource.DataSourceFiles.Count)
                    {
                        fileIndex = 0;
                        indicatorIndex++;
                    }
                    if (indicatorIndex >= DataSourcesCandles[dataSourceIndex].IndicatorsValues.Length)
                    {
                        indicatorIndex = 0;
                        dataSourceIndex++;
                    }
                }
                else
                {
                    int completedTaskIndex = Task.WaitAny(tasksIndicator);

                    Task task = tasksIndicator[completedTaskIndex];
                    DataSourceCandles dataSourceCandles = DataSourcesCandles[dataSourceIndex];
                    int iIndex = indicatorIndex;
                    int fIndex = fileIndex;
                    task = Task.Run(() => IndicatorValuesForFileCalculate(dataSourceCandles, iIndex, fIndex));
                    //переходим на следующий элемент
                    fileIndex++;
                    if (fileIndex >= DataSourcesCandles[dataSourceIndex].DataSource.DataSourceFiles.Count)
                    {
                        fileIndex = 0;
                        indicatorIndex++;
                    }
                    if (indicatorIndex >= DataSourcesCandles[dataSourceIndex].IndicatorsValues.Length)
                    {
                        indicatorIndex = 0;
                        dataSourceIndex++;
                    }
                }
                m++;
            }
            Task.WaitAll(tasksIndicator);
            stopwatchIndicator.Stop();*/
        }

        private void TestRunExecute(TestRun testRun, List<Indicator> indicators)
        {
            //формируем массивы с int и double значениями параметров для каждого индикатора
            int[][] indicatorParametersIntValues = new int[indicators.Count][];
            double[][] indicatorParametersDoubleValues = new double[indicators.Count][];
            for (int i = 0; i < indicators.Count; i++)
            {
                List<int> indexesThisIndicatorInIndicatorParameterValues = new List<int>(); //список с индексами параметров, которые относятся к текущему индикатору
                for(int k = 0; k < testRun.IndicatorParameterValues.Count; k++)
                {
                    if(testRun.IndicatorParameterValues[k].IndicatorParameterTemplate.Indicator == indicators[i]) //если индикатор параметра совпадает с текущим
                    {
                        indexesThisIndicatorInIndicatorParameterValues.Add(k); //добавляем индекс этого параметра в список параметров текущего индикатора
                    }
                }
                indicatorParametersIntValues[i] = new int[indexesThisIndicatorInIndicatorParameterValues.Count];
                indicatorParametersDoubleValues[i] = new double[indexesThisIndicatorInIndicatorParameterValues.Count];
                for(int y = 0; y < indexesThisIndicatorInIndicatorParameterValues.Count; y++)
                {
                    indicatorParametersIntValues[i][y] = testRun.IndicatorParameterValues[indexesThisIndicatorInIndicatorParameterValues[y]].IntValue;
                    indicatorParametersDoubleValues[i][y] = testRun.IndicatorParameterValues[indexesThisIndicatorInIndicatorParameterValues[y]].DoubleValue;
                }
            }

            //формируем массивы с int и double значениями параметров для алгоритма
            int[] algorithmParametersIntValues = new int[testRun.AlgorithmParameterValues.Count];
            double[] algorithmParametersDoubleValues = new double[testRun.AlgorithmParameterValues.Count];
            for(int i = 0; i < testRun.AlgorithmParameterValues.Count; i++)
            {
                algorithmParametersIntValues[i] = testRun.AlgorithmParameterValues[i].IntValue;
                algorithmParametersDoubleValues[i] = testRun.AlgorithmParameterValues[i].DoubleValue;
            }

            TimeSpan intervalDuration = testRun.TestBatch.DataSourceGroup.DataSourceAccordances[0].DataSource.Interval.Duration; //длительность интервала
            DataSourceCandles[] dataSourceCandles = new DataSourceCandles[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //массив с ссылками на DataSourceCandles, соответствующими источникам данных группы источников данных
            for(int i = 0; i < dataSourceCandles.Length; i++)
            {
                dataSourceCandles[i] = DataSourcesCandles.Where(j => j.DataSource == testRun.TestBatch.DataSourceGroup.DataSourceAccordances[i].DataSource).First(); //DataSourceCandles для источника данных из DataSourceAccordances с индексом i в dataSourceCandles
            }
            int[] fileIndexes = new int[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //индексы (для всех источников данных группы) элемента массива Candle[][] Candles в DataSourcesCandles, соответствующий файлу источника данных
            int[] candleIndexes = new int[testRun.TestBatch.DataSourceGroup.DataSourceAccordances.Count]; //индексы (для всех источников данных группы) элемента массива Candles[], сответствующий свечке
            //устанавливаем начальные индексы файла и свечки для источников данных
            for (int i = 0; i < fileIndexes.Length; i++)
            {
                fileIndexes[i] = 0;
                candleIndexes[i] = 0;
            }

            //находим индексы файлов и свечек, дата и время которых позже или равняется дате и времени начала теста
            for (int i = 0; i < dataSourceCandles.Length; i++)
            {
                //ищем индекс первого файла источника данных, дата начала которого позже текущей даты, если такой файл не найден, то индекс = -1
                int fileIndex = -1;
                for(int k = 0; k < dataSourceCandles[i].DataSource.DataSourceFiles.Count; k++)
                {
                    if(DateTime.Compare(dataSourceCandles[i].DataSource.DataSourceFiles[k].DataSourceFileWorkingPeriods[0].StartPeriod, testRun.StartPeriod) > 0)
                    {
                        fileIndex = k; //запоминаем индекс файла, дата начала которого позже даты начала тестирования
                    }
                }
                if(fileIndex > 0)
                {
                    fileIndexes[i] = fileIndex - 1; //сохраняем индекс первого файла, дата начала которго <= дате начала теста
                    //находим индекс свечки, дата и время которой >= дате начала теста
                    DateTime candleDateTime = dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                    while(DateTime.Compare(candleDateTime, testRun.StartPeriod) < 0 && fileIndexes[i] < dataSourceCandles[i].Candles.Length)
                    {
                        candleIndexes[i]++;
                        //если массив со свечками файла подошел к концу, переходим на следующий файл
                        if (candleIndexes[i] >= dataSourceCandles[i].Candles[fileIndexes[i]].Length)
                        {
                            fileIndexes[i]++;
                            candleIndexes[i] = 0;
                        }
                        //если индекс файла не вышел за пределы массива, находим дату для текущей свечки
                        if (fileIndexes[i] < dataSourceCandles[i].Candles.Length)
                        {
                            candleDateTime = dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                        }
                    }
                }
                else
                {
                    //доступных данных нет, присваиваем индексу файла значение, выходящее за пределы массива
                    fileIndexes[i] = dataSourceCandles[i].Candles.Length;
                }
            }


            //определяем текущую дату и время, взяв самую раннюю дату и время свечки (среди источников данных)
            DateTime currentDateTime = dataSourceCandles[0].Candles[fileIndexes[0]][candleIndexes[0]].DateTime; //текущие дата и время
            for(int i = 1; i < dataSourceCandles.Length; i++)
            {
                if(DateTime.Compare(currentDateTime, dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime) > 0)
                {
                    currentDateTime = dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime; //самая ранняя дата и время свечки (которая >= дате начала теста) (среди источников данных)
                }
            }

            bool isOverFileIndex = false; //вышел ли какой-либо из индексов файлов за границы массива файлов источника данных
            for(int i = 0; i < fileIndexes.Length; i++)
            {
                if(fileIndexes[i] >= dataSourceCandles[i].Candles.Length)
                {
                    isOverFileIndex = true; //отмечаем что индекс файла вышел за границы массива
                }
            }
            //проходим по всем свечкам источников данных, пока не достигнем времени окончания теста, или пока не выйдем за границы имеющихся файлов
            while(DateTime.Compare(currentDateTime, testRun.EndPeriod) < 0 && isOverFileIndex == false)
            {
                //обрабатываем текущие заявки (только тех источников данных, текущие свечки которых равняются текущей дате)
                //формируем список источников данных для которых будут проверяться заявки на исполнение (те, даты которых равняются текущей дате)
                List<DataSource> approvedDataSources = new List<DataSource>();
                for(int i = 0; i < dataSourceCandles.Length; i++)
                {
                    if(DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) == 0)
                    {
                        approvedDataSources.Add(dataSourceCandles[i].DataSource);
                    }
                }
                //проверяем заявки на исполнение
                bool isWereDeals = CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, true, true, true); //были ли совершены сделки при проверке исполнения заявок

                //проверяем, равняются ли все свечки источников данных текущей дате
                bool isCandlesDateTimeEqual = true;
                for(int i = 0; i < dataSourceCandles.Length; i++)
                {
                    if(DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) != 0)
                    {
                        isCandlesDateTimeEqual = false;
                    }
                }
                //если свечки всех источников данных равняются текущей дате, вычисляем индикаторы и алгоритм
                if (isCandlesDateTimeEqual)
                {
                    //если были совершены сделки на текущей свечке, дважды выполняем алгоритм: первый раз обновляем заявки и проверяем на исполнение стоп-заявки (если была открыта позиция на текущей свечке, нужно выставить стоп и проверить мог ли он на этой же свечке исполнится), и если были сделки то выполняем алгоритм еще раз и обновляем заявки, после чего переходим на следующую свечку

                    int maxOverIndex = 0; //максимальное превышение индекса в индикаторах и алгоритме
                    double[][] indicatorsValues = new double[fileIndexes.Length][];
                    //проходим по всем файлам и вычисляем значения всех индикаторов для каждого файла
                    for(int i = 0; i < fileIndexes.Length; i++)
                    {
                        //вычисляем значения всех индикаторов
                        indicatorsValues[i] = new double[indicators.Count];
                        for (int k = 0; k < indicatorsValues.Length; k++)
                        {
                            //вычисляем значение индикатора
                            IndicatorCalculateResult indicatorCalculateResult = CompiledIndicators[k].Calculate(dataSourceCandles[fileIndexes[i]].Candles[fileIndexes[i]], candleIndexes[i], indicatorParametersIntValues[k], indicatorParametersDoubleValues[k]); //indicatorParametersIntValues[индекс_индикатора]
                            //Calculate(Candle[] inputCandles, int currentCandleIndex, int[] indicatorParametersIntValues, double[] indicatorParametersDoubleValues)
                            maxOverIndex = indicatorCalculateResult.OverIndex > maxOverIndex ? indicatorCalculateResult.OverIndex : maxOverIndex; //если превышение индекса больше максимального, обновляем его максимальное значение
                            indicatorsValues[i][k] = indicatorCalculateResult.Value; //запоминаем значение индикатора для файла i и индикатора k
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
                                        averagePricePosition = RoundToIncrement(averagePricePosition, deal.DataSource.PriceStep); //округляем среднюю цену позиции до шага 1 пункта цены данного инструмента
                                        volumePosition += deal.Count;
                                    }
                                    if (deal.Order.Direction)
                                    {
                                        isBuyDirection = true;
                                    }
                                }
                            }
                            dataSourcesForCalculate[i] = new DataSourceForCalculate();
                            dataSourcesForCalculate[i].DataSource = dataSourceCandles[i].DataSource;
                            dataSourcesForCalculate[i].IndicatorsValues = indicatorsValues[i];
                            dataSourcesForCalculate[i].Price = averagePricePosition;
                            dataSourcesForCalculate[i].CountBuy = isBuyDirection ? volumePosition : 0;
                            dataSourcesForCalculate[i].CountSell = isBuyDirection ? 0 : volumePosition;
                            dataSourcesForCalculate[i].Candles = dataSourceCandles[i].Candles[fileIndexes[i]];
                            dataSourcesForCalculate[i].CurrentCandleIndex = candleIndexes[i];
                        }

                        AccountForCalculate accountForCalculate = new AccountForCalculate { FreeRubleMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, FreeDollarMoney = testRun.Account.FreeForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit, TakenRubleMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 1).First().Deposit, TakenDollarMoney = testRun.Account.TakenForwardDepositCurrencies.Where(j => j.Currency.Id == 2).First().Deposit };
                        AlgorithmCalculateResult algorithmCalculateResult = CompiledAlgorithm.Calculate(accountForCalculate, dataSourcesForCalculate, algorithmParametersIntValues, algorithmParametersDoubleValues);
                        maxOverIndex = algorithmCalculateResult.OverIndex > maxOverIndex ? algorithmCalculateResult.OverIndex : maxOverIndex; //если првышение индекса больше максимального, обновляем его максимальное значение
                        if (maxOverIndex == 0) //если не был превышен допустимый индекс при вычислении индикаторов и алгоритма, обрабатываем заявки
                        {
                            //устанавливаем DateTimeSubmit для заявок пользователя
                            foreach (Order order in algorithmCalculateResult.Orders)
                            {
                                order.DateTimeSubmit = currentDateTime;
                            }
                            //приводим заявки к виду который прислал пользователь в алгоритме
                            List<Order> newAccountOrders = new List<Order>(); //новый список с текущими заявками
                                                                              //проходим по заявкам пользователя, и для каждой ищем совпадение в текущих заявках. Если находим, то добавляем эту заявку в newAccountOrders, и удаляем из заявок пользователя и текущих заявок
                            for (int i = algorithmCalculateResult.Orders.Count - 1; i >= 0; i--)
                            {
                                bool isFindInOrders = false;
                                int orderIndex = 0;
                                //проходим по всем текущим заявкам
                                while (isFindInOrders == false && orderIndex < testRun.Account.Orders.Count)
                                {
                                    //если заявка пользователя соответствует заявке из текущих заявок
                                    bool isEqual = testRun.Account.Orders[orderIndex].DataSource == algorithmCalculateResult.Orders[i].DataSource && testRun.Account.Orders[orderIndex].TypeOrder == algorithmCalculateResult.Orders[i].TypeOrder && testRun.Account.Orders[orderIndex].Direction == algorithmCalculateResult.Orders[i].Direction && testRun.Account.Orders[orderIndex].Price == algorithmCalculateResult.Orders[i].Price && testRun.Account.Orders[orderIndex].Count == algorithmCalculateResult.Orders[i].Count; //проверка на соответстве источника данных, типа заявки, направления, цены, количества
                                    isEqual = isEqual && ((testRun.Account.Orders[orderIndex].LinkedOrder != null && algorithmCalculateResult.Orders[i].LinkedOrder != null) || (testRun.Account.Orders[orderIndex].LinkedOrder == null && algorithmCalculateResult.Orders[i].LinkedOrder == null)); //проверка на соответствие наличия/отсутствия связаной заявки
                                    if (isEqual)
                                    {
                                        isFindInOrders = true;
                                    }
                                    else
                                    {
                                        orderIndex++;
                                    }
                                }
                                //если такая же заявка найдена в текущих, добавляем её в newAccountOrders, и удаляем из заявок пользователя и текущих заявок
                                if (isFindInOrders)
                                {
                                    newAccountOrders.Add(testRun.Account.Orders[orderIndex]); //добавляем заявку из текущих в новые текущие заявки
                                    if (testRun.Account.Orders[orderIndex].LinkedOrder != null)
                                    {
                                        newAccountOrders.Add(testRun.Account.Orders[orderIndex].LinkedOrder); //добавляем связанную заявку из текущих в новые текущие
                                        testRun.Account.Orders.Remove(testRun.Account.Orders[orderIndex].LinkedOrder); //удаляем связанную заявку из текущих
                                        algorithmCalculateResult.Orders.Remove(algorithmCalculateResult.Orders[i].LinkedOrder); //удаляем связанную заявку из заявок пользователя
                                    }
                                    testRun.Account.Orders.RemoveAt(orderIndex); //удаляем заявку из текущих
                                    algorithmCalculateResult.Orders.RemoveAt(i); //удаляем заявку из заявок пользователя
                                }
                            }
                            //устанавливаем дату снятия заявок для текущих которые не соответствуют заявкам пользователя
                            foreach (Order order in testRun.Account.Orders)
                            {
                                order.DateTimeRemove = currentDateTime;
                            }
                            //добавляем оставшиеся заявки пользователя к новым текущим
                            newAccountOrders.AddRange(algorithmCalculateResult.Orders);
                            testRun.Account.Orders = newAccountOrders; //обновляем текущие заявки

                            //если на текущей свечке были совершены сделки, проверяем стоп-заявки на исполнение (чтобы если на текущей свечке была открыта позиция, после выставления стоп-заявки проверить её на исполнение на текущей свечке)
                            if (isWereDeals && iteration == 1)
                            {
                                isWereDealsStopLoss = CheckOrdersExecution(dataSourceCandles, testRun.Account, approvedDataSources, fileIndexes, candleIndexes, false, true, false); //были ли совершены сделки при проверке исполнения стоп-заявок
                            }
                        }
                    }
                    while (isWereDealsStopLoss && iteration == 1); //если этой первое исполнение алгоритма, и при проверке стоп-заявок были сделки, еще раз прогоняем алгоритм чтобы обновить заявки
                }

                //для каждого источника данных доходим до даты, которая позже текущей
                for(int i = 0; i < dataSourceCandles.Length; i++)
                {
                    //переходим на следующую свечку, пока не дойдем до даты которая позже текущей
                    bool isOverDate = DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) > 0; //дошли ли до даты которая позже текущей

                    //переходим на следующую свечку, пока не дойдем до даты которая позже текущей или пока не выйдем за пределы файлов
                    while (isOverDate == false && isOverFileIndex == false)
                    {
                        candleIndexes[i]++;
                        //если массив со свечками файла подошел к концу, переходим на следующий файл
                        if (candleIndexes[i] >= dataSourceCandles[i].Candles[fileIndexes[i]].Length)
                        {
                            fileIndexes[i]++;
                            candleIndexes[i] = 0;
                        }
                        //если индекс файла не вышел за пределы массива, проверяем, дошли ли до даты которая позже текущей
                        if (fileIndexes[i] < dataSourceCandles[i].Candles.Length)
                        {
                            isOverDate = DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) > 0;
                        }
                        else
                        {
                            isOverFileIndex = true;
                        }
                    }
                }

                //обновляем текущую дату (берем самую раннюю дату из источников данных)
                if(isOverFileIndex == false)
                {
                    currentDateTime = dataSourceCandles[0].Candles[fileIndexes[0]][candleIndexes[0]].DateTime;
                    for(int i = 1; i < dataSourceCandles.Length; i++)
                    {
                        if(DateTime.Compare(dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime, currentDateTime) < 0)
                        {
                            currentDateTime = dataSourceCandles[i].Candles[fileIndexes[i]][candleIndexes[i]].DateTime;
                        }
                    }
                }
            }
            //рассчитываем критерии оценки для данного testRun
            for (int i = 0; i < _modelData.EvaluationCriterias.Count; i++)
            {
                //CompiledEvaluationCriterias[i].Calculate(DataSourceCandles dataSourceCandles, List < EvaluationCriteriaValue > evaluationCriteriaValues)
                //определяем индекс источника данных, с наибольшей идеальной прибылью
                int index = 0;
                for(int k = 1; k < dataSourceCandles.Length; k++)
                {
                    if(dataSourceCandles[index].PerfectProfit < dataSourceCandles[k].PerfectProfit)
                    {
                        index = k;
                    }
                }
                EvaluationCriteriaValue evaluationCriteriaValue = CompiledEvaluationCriterias[i].Calculate(dataSourceCandles[index], testRun.EvaluationCriteriaValues, _modelData.Settings);
                evaluationCriteriaValue.EvaluationCriteria = _modelData.EvaluationCriterias[i];
                testRun.EvaluationCriteriaValues.Add(evaluationCriteriaValue);
            }
        }

        public double RoundToIncrement(double x, double m) //функция округляет число до определенного множителя, например, RoundToIncrement(3.14, 0.2) вернет 3.2
        {
            return Math.Round(x / m) * m;
        }

        public bool CheckOrdersExecution(DataSourceCandles[] dataSourcesCandles, Account account, List<DataSource> approvedDataSources, int[] fileIndexes, int[] candleIndexes, bool isMarket, bool isStop, bool isLimit) //функция проверяет заявки на их исполнение в текущей свечке, возвращает false если не было сделок, и true если были совершены сделки. approvedDataSources - список с источниками данных, заявки которых будут проверяться на исполнение. isMarket, isStop, isLimit - если true, будут проверяться на исполнение эти заявки
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
                        int slippage = _modelData.Settings.Where(i => i.Id == 3).First().IntValue; //количество пунктов на которое цена исполнения рыночной заявки будет хуже
                        slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                        slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                        isMakeADeals = MakeADeal(account, order, order.Count, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].C + slippage, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
                        if (order.Count == 0)
                        {
                            ordersToRemove.Add(order);
                            ordersToRemoveDateTime.Add(dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime);
                        }
                    }
                }
                //снимаем полностью исполненные заявки
                for(int i = 0; i < ordersToRemove.Count; i++)
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
                            int slippage = _modelData.Settings.Where(i => i.Id == 3).First().IntValue; //количество пунктов на которое цена исполнения рыночной заявки будет хуже
                            slippage += Slippage(dataSourcesCandles[dataSourcesCandlesIndex], fileIndexes[dataSourcesCandlesIndex], candleIndexes[dataSourcesCandlesIndex], order.Count); //добавляем проскальзывание
                            slippage = order.Direction == true ? slippage : -slippage; //для покупки проскальзывание идет вверх, для продажи вниз
                            isMakeADeals = MakeADeal(account, order, order.Count, order.Price + slippage, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
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
                            int stepCount = (int)Math.Round((dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H - dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L) / order.DataSource.PriceStep); //количество пунктов цены
                            decimal stepLots = (decimal)dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].V / stepCount; //среднее количество лотов на 1 пункт цены
                            int stepsOver = order.Direction ? (int)Math.Round((order.Price - dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].L) / order.DataSource.PriceStep) : (int)Math.Round((dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].H - order.Price) / order.DataSource.PriceStep); //количество пунктов за ценой заявки
                            decimal overLots = stepLots * stepsOver / 2; //количество лотов которое могло быть куплено/продано на текущей свечке (делить на 2 т.к. лишь половина от лотов - это сделки в нужной нам операции (купить или продать))
                            if (order.DataSource.Instrument.Id != 3) //если это не криптовалюта, округляем количество лотов до целого
                            {
                                overLots = Math.Round(overLots);
                            }
                            if(overLots > 0) //если есть лоты которые могли быть исполнены на текущей свечке, совершаем сделку
                            {
                                decimal dealCount = order.Count >= overLots ? order.Count : overLots;
                                isMakeADeals = MakeADeal(account, order, dealCount, order.Price, dataSourcesCandles[dataSourcesCandlesIndex].Candles[fileIndexes[dataSourcesCandlesIndex]][candleIndexes[dataSourcesCandlesIndex]].DateTime) ? true : isMakeADeals;
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
            //определяем хватает ли средств на 1 лот, если да, определяем хватает ли средств на lotsCount, если нет устанавливаем минимально доступное количество
            //стоимость 1 лота
            double lotsCost = order.DataSource.Instrument.Id == 2 ? order.DataSource.Cost : price; //если инструмент источника данных - фьючерс, устанавливаем стоимость в стоимость фьючерса, иначе - устанавливаем стоимость 1 лота в стоимость с графика
            double lotsComission = order.DataSource.Comissiontype.Id == 2 ? lotsCost * order.DataSource.Comission : order.DataSource.Comission; //комиссия на 1 лот
            double freeDeposit = account.FreeForwardDepositCurrencies.Where(i => i.Currency == order.DataSource.Currency).First().Deposit; //свободный остаток в валюте источника данных
            double takenDeposit = account.TakenForwardDepositCurrencies.Where(i => i.Currency == order.DataSource.Currency).First().Deposit; //занятые средства на открытые позиции в валюте источника данных
            //определяем максимально доступное количество лотов
            decimal maxLotsCount = Math.Truncate((decimal)freeDeposit / (decimal)(lotsCost + lotsComission));
            decimal reverseLotsCount = 0;//количество лотов в открытой позиции с обратным направлением
            foreach(Deal deal in account.CurrentPosition)
            {
                if(deal.DataSource == order.DataSource && deal.Order.Direction != order.Direction) //если сделка совершена по тому же источнику данных что и заявка, но отличается с ней в направлении
                {
                    reverseLotsCount += deal.Count;
                }
            }
            maxLotsCount += reverseLotsCount; //прибавляем к максимально доступному количеству лотов, количество лотов в открытой позиции с обратным направлением
            if (maxLotsCount > 0) //если максимально доступное количество лотов для совершения сделки по заявке > 0, совершаем сделку
            {
                decimal dealLotsCount = lotsCount > maxLotsCount ? maxLotsCount : lotsCount; //если количество лотов для сделки больше максимально доступного, устанавливаем в максимально доступное
                //вычитаем из неисполненных лотов заявки dealLotsCount
                order.Count -= dealLotsCount;
                if(order.LinkedOrder != null)
                {
                    order.LinkedOrder.Count = order.Count;
                }
                //записываем сделку
                account.AllDeals.Add(new Deal { Number = account.AllDeals.Count + 1, DataSource = order.DataSource, Order = order, Price = price, Count = dealLotsCount, DateTime = dateTime });
                Deal currentDeal = new Deal { Number = account.AllDeals.Count + 1, DataSource = order.DataSource, Order = order, Price = price, Count = dealLotsCount, DateTime = dateTime };
                account.CurrentPosition.Add(currentDeal);
                isMakeADeal = true; //запоминаем что была совершена сделка
                //вычитаем комиссию на сделку из свободных средств
                freeDeposit -= (double)((decimal)lotsComission * dealLotsCount);
                //закрываем открытые позиции которые были закрыты данной сделкой
                int i = 0;
                while(i < account.CurrentPosition.Count - 1 && currentDeal.Count > 0) //проходим по всем сделкам кроме последней (только что добавленной)
                {
                    if(account.CurrentPosition[i].DataSource == currentDeal.DataSource && account.CurrentPosition[i].Order.Direction != currentDeal.Order.Direction) //если совпадает источник данных, но отличается направление сделки
                    {
                        decimal decrementCount = account.CurrentPosition[i].Count > currentDeal.Count ? currentDeal.Count : account.CurrentPosition[i].Count; //количество для уменьшения лотов в сделке
                        //определяем денежный результат трейда и прибавляем его к свободным средствам
                        double priceSell = account.CurrentPosition[i].Order.Direction == false ? account.CurrentPosition[i].Price : currentDeal.Price; //цена продажи в трейде
                        double priceBuy = account.CurrentPosition[i].Order.Direction == true ? account.CurrentPosition[i].Price : currentDeal.Price; //цена покупки в трейде
                        double resultMoney = (double)(decrementCount * (decimal)(((priceSell - priceBuy) / account.CurrentPosition[i].DataSource.PriceStep) * account.CurrentPosition[i].DataSource.CostPriceStep)); //определяю разность цен между проджей и покупкой, делю на шаг цены и умножаю на стоимость шага цены, получаю денежное значение для 1 лота, и умножаю на количество лотов
                        freeDeposit += resultMoney;
                        //определяем стоимость закрытых лотов в открытой позиции, вычитаем её из занятых средств и прибавляем к свободным
                        double closedCost = account.CurrentPosition[i].Order.DataSource.Instrument.Id == 2 ? account.CurrentPosition[i].Order.DataSource.Cost : account.CurrentPosition[i].Price;
                        closedCost = (double)((decimal)closedCost * decrementCount); //умножаем стоимость на количество
                        takenDeposit -= closedCost; //вычитаем из занятых на открытые позиции средств
                        freeDeposit += closedCost; //прибавляем к свободным средствам
                        //вычитаем закрытое количесво из открытых позиций
                        account.CurrentPosition[i].Count -= decrementCount;
                        currentDeal.Count -= decrementCount;
                    }
                    i++;
                }
                //определяем стоимость занятых средств на оставшееся (незакрытое) количество лотов текущей сделки, вычитаем её из сободных средств и добавляем к занятым
                double currentCost = currentDeal.DataSource.Instrument.Id == 2 ? currentDeal.DataSource.Cost : currentDeal.Price;
                currentCost = (double)((decimal)currentCost * currentDeal.Count); //умножаем стоимость на количество
                freeDeposit -= currentCost; //вычитаем из свободных средств
                takenDeposit += currentCost; //добавляем к занятым на открытые позиции средствам
                //удаляем из открытых позиций сделки с нулевым количеством
                for (int j = account.CurrentPosition.Count - 1; j >= 0; j--)
                {
                    if(account.CurrentPosition[j].Count == 0)
                    {
                        account.CurrentPosition.RemoveAt(j);
                    }
                }
                //обновляем средства во всех валютах
                account.FreeForwardDepositCurrencies = CalculateDepositCurrrencies(freeDeposit, order.DataSource.Currency);
                account.TakenForwardDepositCurrencies = CalculateDepositCurrrencies(takenDeposit, order.DataSource.Currency);
                //если открытые позиции пусты, записываем состояние депозита
                if(account.CurrentPosition.Count == 0)
                {
                    account.DepositCurrenciesChanges.Add(account.FreeForwardDepositCurrencies);
                }
            }
            return isMakeADeal;
        }

        private List<DepositCurrency> CalculateDepositCurrrencies(double deposit, Currency inputCurrency) //возвращает значения депозита во всех валютах
        {
            List<DepositCurrency> depositCurrencies = new List<DepositCurrency>();

            double dollarCostDeposit = deposit / inputCurrency.DollarCost; //определяем долларовую стоимость
            foreach (Currency currency in _modelData.Currencies)
            {
                //переводим доллоровую стоимость в валютную, умножая на стоимость 1 доллара
                double cost = Math.Round(dollarCostDeposit * currency.DollarCost, 2);
                depositCurrencies.Add(new DepositCurrency { Currency = currency, Deposit = cost });
            }

            return depositCurrencies;
        }

        private int Slippage(DataSourceCandles dataSourceCandles, int fileIndex, int candleIndex, decimal lotsCountInOrder) //возвращает размер проскальзывания
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

        private TestRun DeterminingTopModel(List<TestRun> testRuns) //определение топ-модели среди оптимизационных тестов
        {
            return new TestRun();
        }
    }
}
