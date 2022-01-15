﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.Diagnostics;

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
        private dynamic CompiledEvaluationCriterias { get; set; } //объекты, содержащие метод, выполняющий расчет критерия оценки тестового прогона

        private ModelData _modelData;

        public Testing()
        {
            _modelData = ModelData.getInstance();
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
                            int currentDurationPercent = 99;
                            TimeSpan currentOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days) - currentDate).TotalDays * ((double)currentDurationPercent / 100)));
                            currentOptimizationDuration = currentOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : currentOptimizationDuration; //если менее одного дня, устанавливаем в один день
                            TimeSpan currentForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days) - currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days)).TotalDays * ((double)currentDurationPercent / 100)));
                            currentForwardDuration = currentForwardDuration.TotalDays < 1 && IsForwardTesting ? TimeSpan.FromDays(1) : currentForwardDuration; //если менее одного дня и это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)
                            //пока период с уменьшенной длительностью не поместится, уменьшаем длительность (пока текущая дата для расчетов + уменьшенная длительность больше текущей даты + полная длительность)
                            while (DateTime.Compare(currentDateForCalculate.Add(currentOptimizationDuration).Add(currentForwardDuration).Date, currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days)) > 0)
                            {
                                currentDurationPercent--;
                                currentOptimizationDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days) - currentDate).TotalDays * ((double)currentDurationPercent / 100)));
                                currentOptimizationDuration = currentOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : currentOptimizationDuration; //если менее одного дня, устанавливаем в один день
                                currentForwardDuration = TimeSpan.FromDays(Math.Round((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days) - currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days)).TotalDays * ((double)currentDurationPercent / 100)));
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
                        }
                        testBatch.TestRuns = optimizationTestRuns;

                        //формируем форвардный тест
                        if (IsForwardTesting)
                        {
                            Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>() };
                            Account accountDepositTrading = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>(), ForwardDepositCurrencies = ForwardDepositCurrencies };
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
                Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
                System.CodeDom.Compiler.CompilerParameters param = new System.CodeDom.Compiler.CompilerParameters();
                param.GenerateExecutable = false;
                param.GenerateInMemory = true;

                string indicatorParameters = ""; //описание принимаемых параметров методом индикатора
                for(int k = 0; k < Algorithm.IndicatorParameterRanges.Count; k++)
                {
                    if(Algorithm.IndicatorParameterRanges[k].Indicator == indicators[i])
                    {
                        indicatorParameters += Algorithm.IndicatorParameterRanges[k].IndicatorParameterTemplate.ParameterValueType.Id == 1 ? "int " : "double ";
                        indicatorParameters += Algorithm.IndicatorParameterRanges[k].IndicatorParameterTemplate.Name + ", ";
                    }
                }
                indicatorParameters = indicatorParameters.Length >= 2 ? indicatorParameters.Substring(0, indicatorParameters.Length - 2) : indicatorParameters; //удаляем последние 2 символа

                //редактируем текст скрипта
                string script = indicators[i].Script;

                var compiled = provider.CompileAssemblyFromSource(param, new string[]
                {
                        @"
                        using System;
                        public class CompiledIndicator_" + indicators[i].Name +
                        @"
                        {
                            public double Calculate(" + indicatorParameters + ")" +
                            "{" +
                                script +
                            "}" +
                        @"
                        }"
                });
                /*dynamic test = Result.CompiledAssembly.CreateInstance("Test");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                double b = test.Main();
                stopwatch.Stop();*/

                dynamic[] tests = new dynamic[4];
                tests[0] = compiled.CompiledAssembly.CreateInstance("Test");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                double b = tests[0].Main();
                stopwatch.Stop();
            }
        }

        private void TestRunExecute(TestRun testRun)
        {

        }

        private TestRun DeterminingTopModel(TestBatch testBatch) //определение топ-модели среди оптимизационных тестов
        {
            return new TestRun();
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
    }
}
