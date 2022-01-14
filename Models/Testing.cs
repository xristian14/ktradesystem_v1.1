using System;
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

            //определяем минимально допустимую длительность теста
            TimeSpan acceptableOptimizationDuration = new TimeSpan(); //минимально допустимая длительность оптимизационного теста
            TimeSpan acceptableForwardDuration = new TimeSpan(); //минимально допустимая длительность форвардного теста
            if (IsForwardTesting)
            {
                double totalDays = DurationForwardTest.Years * 365 + DurationForwardTest.Months * 30 + DurationForwardTest.Days;
                acceptableForwardDuration = TimeSpan.FromDays(totalDays * (_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100));
            }
            else
            {
                double totalDays = DurationOptimizationTests.Years * 365 + DurationOptimizationTests.Months * 30 + DurationOptimizationTests.Days;
                acceptableOptimizationDuration = TimeSpan.FromDays(totalDays * (_modelData.Settings.Where(i => i.Id == 2).First().IntValue / 100));
            }

            //формируем тестовые связки
            foreach (DataSourceGroup dataSourceGroup in DataSourceGroups)
            {
                //формируем серии оптимизационных тестов для данного источника данных для каждого периода

                //определяем диапазон доступных дат для данной группы источников данных (начальная и конечная даты которые есть во всех источниках данных)
                DateTime startAvailableDate = new DateTime();
                DateTime endAvailableDate = new DateTime();
                for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
                {
                    if(i == 0)
                    {
                        startAvailableDate = dataSourceGroup.DataSourceAccordances[i].DataSource.StartDate;
                        endAvailableDate = dataSourceGroup.DataSourceAccordances[i].DataSource.EndDate;
                    }
                    else
                    {
                        if(DateTime.Compare(startAvailableDate, dataSourceGroup.DataSourceAccordances[i].DataSource.StartDate) < 0)
                        {
                            startAvailableDate = dataSourceGroup.DataSourceAccordances[i].DataSource.StartDate;
                        }
                        if (DateTime.Compare(endAvailableDate, dataSourceGroup.DataSourceAccordances[i].DataSource.EndDate) > 0)
                        {
                            endAvailableDate = dataSourceGroup.DataSourceAccordances[i].DataSource.EndDate;
                        }
                    }
                }

                //находим дату начала тестирования данной группы источников данных (нужно чтобы в следующем за этой датой промежутоком была минимально допустимая длительность для оптимизационного или форвардного тестирования, если например, дата начала 1-е число, доступные данные начинаются с 5-го, длительность 1 месяц, минимально допустимая 27 дней, в таком случае дата перейдет на 1 число следующего месяца, и там будет проверяться наличие минимально допустимых данных)
                DateTime currentDate = StartPeriod; //текущая дата
                //пока длительность от доступных данных до текущей даты + длительность оптимизации < минимально допустимой длительности, переходим на следующую дату, пока не будет найдена дата начала тестирования (пока дата доступных данных + минимально допустимая длительность > текущей даты + длительность оптимизации)
                while (DateTime.Compare(startAvailableDate.Add(acceptableOptimizationDuration), currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days)) > 0)
                {
                    currentDate = currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days);
                }

                //определяем дату окончания тестирования
                DateTime endDate = DateTime.Compare(endAvailableDate, EndPeriod) > 0 ? EndPeriod : endAvailableDate;

                //пока истинно одно из условий:
                // 1) это не форвардное тестирование, и текущая дата + минимально допустимая длительность оптимизационного теста <= конечной дате тестирования
                // 2) это форвардное тестирование, и текущая дата + минимально допустимая длительность оптимизационного теста + минимально допустимая длительность форвардного теста <= конечной дате тестирования
                while (IsForwardTesting == false && DateTime.Compare(currentDate.Add(acceptableOptimizationDuration), endDate) <= 0     ||     IsForwardTesting == true && DateTime.Compare(currentDate.Add(acceptableOptimizationDuration).Add(acceptableForwardDuration), endDate) <= 0)
                {
                    TestBatch testBatch = new TestBatch();

                    //формируем список со всеми комбинациями параметров

                    //формируем TestRuns
                    List<TestRun> testRuns = new List<TestRun>();

                    //прибавляем к текущей дате временной промежуток между оптимизационными тестами
                    currentDate = currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days);
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
    }
}
