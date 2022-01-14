﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

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
        private dynamic[] IndicatorsScripts { get; set; } //объекты, содержащие метод, выполняющий расчет индикатора
        private dynamic AlgorithmScript { get; set; } //объект, содержащий метод, вычисляющий работу алгоритма
        private dynamic EvaluationCriteriasScripts { get; set; } //объекты, содержащие метод, выполняющий расчет критерия оценки тестового прогона

        private ModelData _modelData;

        public Testing()
        {
            _modelData = ModelData.getInstance();
        }

        public void LaunchTesting()
        {
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

                while(currentValue + step < Algorithm.IndicatorParameterRanges[i].MaxValue)
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

                while (currentValue + step < Algorithm.AlgorithmParameters[i].MaxValue)
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

            /*
            //определяем минимально допустимую длительность теста
            TimeSpan acceptableOptimizationDuration = new TimeSpan(); //минимально допустимая длительность оптимизационного теста
            TimeSpan acceptableForwardDuration = new TimeSpan(); //минимально допустимая длительность форвардного теста

            double totalDays = DurationOptimizationTests.Years * 365 + DurationOptimizationTests.Months * 30 + DurationOptimizationTests.Days;
            acceptableOptimizationDuration = TimeSpan.FromDays(totalDays * (_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100));

            totalDays = IsForwardTesting ? DurationForwardTest.Years * 365 + DurationForwardTest.Months * 30 + DurationForwardTest.Days : 0; //если это не форвардное тестирование, длительность будет 0, и на длительность оптимизационного теста она влиять не будет
            acceptableForwardDuration = TimeSpan.FromDays(totalDays * (_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100));
            */

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

                /*
                //находим дату начала тестирования данной группы источников данных (нужно чтобы в следующем за этой датой промежутоком была минимально допустимая длительность для оптимизационного или форвардного тестирования, если например, дата начала 1-е число, доступные данные начинаются с 5-го, длительность 1 месяц, минимально допустимая 27 дней, в таком случае дата перейдет на 1 число следующего месяца, и там будет проверяться наличие минимально допустимых данных)
                DateTime currentDate = StartPeriod; //текущая дата
                //пока длительность от доступных данных до текущей даты + длительность оптимизации < минимально допустимой длительности, переходим на следующую дату, пока не будет найдена дата начала тестирования (пока дата доступных данных + минимально допустимая длительность > текущей даты + длительность оптимизации)
                while (DateTime.Compare(availableDateStart.Add(acceptableOptimizationDuration), currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days)) > 0)
                {
                    currentDate = currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days);
                }

                
                */

                //в цикле определяется минимально допустимая длительность для следующей проверки, исходя из разности дат текущей и текущей + требуемой
                //цикл проверяет, помещается ли минимум в оставшийся период, так же в нем идет проверка на то, текущая раньше доступной или нет. Если да, то проверяется, помещается ли в период с доступной по текущая + промежуток, минимальная длительность. Если да, то текущая для расчетов устанавливается в начало доступной. Все даты определяются из текущей для расчетов, а не из текущей. Поэтому после установки текущей для расчетов в доступную, можно дальше расчитывать даты тем же алгоритмом что и для варианта когда текущая позже или равна доступной. Если же с доступной до текущей + промежуток минимальная длительность не помещается, цикл переходит на следующую итерацию.
                
                //определяем дату окончания тестирования
                DateTime endDate = DateTime.Compare(availableDateEnd, EndPeriod) > 0 ? EndPeriod : availableDateEnd;

                DateTime currentDate = StartPeriod; //текущая дата

                //определяем минимально допустимую длительность оптимизационного теста ((текущая дата + оптимизация  -  текущая) * % из настроек)
                TimeSpan minimumAllowedOptimizationDuration = TimeSpan.FromMinutes((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days) - currentDate).TotalMinutes * (_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100));
                minimumAllowedOptimizationDuration = minimumAllowedOptimizationDuration.TotalDays < 1 ? TimeSpan.FromDays(1) : minimumAllowedOptimizationDuration; //если менее одного дня, устанавливаем в один день
                //определяем минимально допустимую длительность форвардного теста ((текущая дата + оптимизация + форвардный  -  текущая + оптимизация) * % из настроек)
                TimeSpan minimumAllowedForwardDuration = TimeSpan.FromMinutes((currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days) - currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days)).TotalMinutes * (_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100));
                minimumAllowedForwardDuration = minimumAllowedForwardDuration.TotalDays < 1 && IsForwardTesting ? TimeSpan.FromDays(1) : minimumAllowedForwardDuration; //если менее одного дняи это форвардное тестирование, устанавливаем в один день (при не форвардном будет 0)

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
                        if()
                    }

                    TestBatch testBatch = new TestBatch();

                    /*
                    //если полная длительность оптимизационного и форвардного тестов не помещается в оставшийся период(А В НАЧАЛЬНЫЙ??), находим наибольшую, помещающуюся длительность
                    if(DateTime.Compare(currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days).Date, endDate.Date) > 0)
                    {
                        int durationPercent = 99;
                        double totalOptimizationDays = DurationOptimizationTests.Years * 365 + DurationOptimizationTests.Months * 30 + DurationOptimizationTests.Days;
                        TimeSpan minimumOptimizationDuration = TimeSpan.FromDays(totalOptimizationDays * (durationPercent / 100));
                        double totaForwardDays = IsForwardTesting ? DurationForwardTest.Years * 365 + DurationForwardTest.Months * 30 + DurationForwardTest.Days : 0; //если это не форвардное тестирование, длительность будет 0, и на длительность оптимизационного теста она влиять не будет
                        TimeSpan minimumForwardDuration = TimeSpan.FromDays(totaForwardDays * (durationPercent / 100));
                        //пока длительность не помещается, уменьшаем durationPercent
                        while (DateTime.Compare(currentDate.Add(minimumOptimizationDuration).Add(minimumForwardDuration).Date, endDate.Date) > 0)
                        {
                            durationPercent--;
                            minimumOptimizationDuration = TimeSpan.FromDays(totalOptimizationDays * (durationPercent / 100));
                            minimumForwardDuration = TimeSpan.FromDays(totaForwardDays * (durationPercent / 100));
                        }
                        //если первая доступная дата позже даты начала тестирования, начало тестирования устанавливаем на первую доступную дату, иначе на текущую
                        startOptimization = DateTime.Compare(availableDateStart, currentDate) > 0 ? availableDateStart.Date : currentDate.Date;
                        endOptimization = currentDate.Add(minimumOptimizationDuration).Date;
                        startForward = IsForwardTesting ? endOptimization.AddDays(1).Date : startForward;
                        endForward = IsForwardTesting ? currentDate.Add(minimumOptimizationDuration).Add(minimumForwardDuration).Date : endForward;
                    }
                    else
                    {//что будет если начало форвардного окажется раньше доступной даты?
                        startOptimization = DateTime.Compare(availableDateStart, currentDate) > 0 ? availableDateStart.Date : currentDate.Date;
                        endOptimization = currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).Date;
                        startForward = IsForwardTesting ? endOptimization.AddDays(1).Date : startForward;
                        endForward = IsForwardTesting ? currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days).AddYears(DurationForwardTest.Years).AddMonths(DurationForwardTest.Months).AddDays(DurationForwardTest.Days).Date : endForward;
                    }
                    */

                    //формируем список со всеми комбинациями параметров
                    List<int[]> allCombinations = new List<int[]>();
                    //сначала проходим по параметрам индикаторов
                    for(int ind = 0; ind < Algorithm.IndicatorParameterRanges.Count; ind++)
                    {
                        bool isIndicatorParameterIntValueType = (Algorithm.IndicatorParameterRanges[ind].IndicatorParameterTemplate.ParameterValueType.Id == 1) ? true : false;
                        
                        if (allCombinations.Count == 0) //формируем начальный список всех комбинаций при первом прохождении
                        {
                            if (isIndicatorParameterIntValueType)
                            {
                                allCombinations.Add(new int[IndicatorsParametersAllIntValues.Length]);
                                for(int indValIndex = 0; indValIndex < IndicatorsParametersAllIntValues.Length; indValIndex++)
                                {
                                    allCombinations[ind][indValIndex] = indValIndex;
                                }
                            }
                            else
                            {
                                allCombinations.Add(new int[IndicatorsParametersAllDoubleValues.Length]);
                                for (int indValIndex = 0; indValIndex < IndicatorsParametersAllDoubleValues.Length; indValIndex++)
                                {
                                    allCombinations[ind][indValIndex] = indValIndex;
                                }
                            }
                        }
                        else
                        {
                            List<int> indexes = new List<int>();
                            if (isIndicatorParameterIntValueType)
                            {
                                for (int indValIndex = 0; indValIndex < IndicatorsParametersAllIntValues.Length; indValIndex++)
                                {
                                    indexes.Add(indValIndex);
                                }
                            }
                            else
                            {
                                for (int indValIndex = 0; indValIndex < IndicatorsParametersAllDoubleValues.Length; indValIndex++)
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
                                allCombinations.Add(new int[AlgorithmParametersAllIntValues.Length]);
                                for (int algValIndex = 0; algValIndex < AlgorithmParametersAllIntValues.Length; algValIndex++)
                                {
                                    allCombinations[alg][algValIndex] = algValIndex;
                                }
                            }
                            else
                            {
                                allCombinations.Add(new int[AlgorithmParametersAllDoubleValues.Length]);
                                for (int algValIndex = 0; algValIndex < AlgorithmParametersAllDoubleValues.Length; algValIndex++)
                                {
                                    allCombinations[alg][algValIndex] = algValIndex;
                                }
                            }
                        }
                        else
                        {
                            List<int> indexes = new List<int>();
                            if (isAlgorithmParameterIntValueType)
                            {
                                for (int algValIndex = 0; algValIndex < AlgorithmParametersAllIntValues.Length; algValIndex++)
                                {
                                    indexes.Add(algValIndex);
                                }
                            }
                            else
                            {
                                for (int algValIndex = 0; algValIndex < AlgorithmParametersAllDoubleValues.Length; algValIndex++)
                                {
                                    indexes.Add(algValIndex);
                                }
                            }
                            allCombinations = CreateCombinations(allCombinations, indexes);
                        }
                    }

                    //формируем TestRuns
                    List<TestRun> testRuns = new List<TestRun>();
                    for(int i = 0; i < allCombinations.Count; i++)
                    {
                        Account account = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>() };
                        Account accountDepositTrading = new Account { Orders = new List<Order>(), AllOrders = new List<Order>(), CurrentPosition = new List<Deal>(), AllDeals = new List<Deal>(), ForwardDepositCurrencies = ForwardDepositCurrencies };
                    }

                    //прибавляем к текущей дате временной промежуток между оптимизационными тестами
                    currentDate = currentDate.AddYears(OptimizationTestSpacing.Years).AddMonths(OptimizationTestSpacing.Months).AddDays(OptimizationTestSpacing.Days);
                }
            }
        }

        private void TestRunExecute(TestRun testRun)
        {

        }

        private TestRun DeterminingTopModel(TestBatch testBatch) //определение топ-модели среди оптимизационных тестов
        {
            return new TestRun();
        }

        private List<int[]> CreateCombinations(List<int[]> combination, List<int> indexes) //принимает 2 списка, 1-й - содержит массив с комбинации индексов параметров: {[0,0],[0,1],[1,0],[1,1]}, второй только индексы: {0,1}, фунция перебирает все комбинации элементов обоих списков и возвращает новый список в котором индексы 2-го списка добавлены в комбинацию 1-го: {[0,0,0],[0,0,1],[0,1,0]..}
        {
            List<int[]> newCombination = new List<int[]>();
            for(int i = 0; i < combination.Count; i++)
            {
                for(int k = 0; k < indexes.Count; k++)
                {
                    int[] arr = new int[combination[i].Length + 1];
                    for(int n = 0; n < combination[i].Length; n++)
                    {
                        arr[n] = combination[i][n];
                    }
                    arr[combination[i].Length] = indexes[k];
                    newCombination.Add(arr);
                }
            }
            return newCombination;
        }
    }
}
