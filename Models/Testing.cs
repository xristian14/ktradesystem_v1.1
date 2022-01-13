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
        public List<TestBatch> TestBatches { get; set; } //тестовые прогоны (серия оптимизационных тестов за период + форвардный тест)
        private dynamic[] IndicatorsScripts { get; set; } //объекты, содержащие метод, выполняющий расчет индикатора
        private dynamic AlgorithmScript { get; set; } //объект, содержащий метод, вычисляющий работу алгоритма
        private dynamic EvaluationCriteriasScripts { get; set; } //объекты, содержащие метод, выполняющий расчет критерия оценки тестового прогона

        public void LaunchTesting()
        {
            //определяем списки со значениями параметров
            List<int>[] IndicatorsParametersAllIntValues = new List<int>[Algorithm.IndicatorParameterRanges.Count]; //массив со всеми возможными целочисленными значениями параметров индикаторов
            List<double>[] IndicatorsParametersAllDoubleValues = new List<double>[Algorithm.IndicatorParameterRanges.Count]; //массив со всеми возможными дробными значениями параметров индикаторов

            List<int>[] AlgorithmParametersAllIntValues = new List<int>[Algorithm.AlgorithmParameters.Count]; //массив со всеми возможными целочисленными значениями параметров алгоритма
            List<double>[] AlgorithmParametersAllDoubleValues = new List<double>[Algorithm.AlgorithmParameters.Count]; //массив со всеми возможными дробными значениями параметров алгоритма

            //параметры будут передаваться в индикаторы и алгоритм в качестве параметров методов, при описании методов индикатора или алгоритма я укажу тип принимаемого параметра int или double в зависимости от типа в шаблоне параметра, и после проверки типа параметра, решу из какого списка передавать, со значениями double, или со значениями int
            //вносим значения параметров индикаторов
            for (int i = 0; i < Algorithm.IndicatorParameterRanges.Count; i++)
            {
                IndicatorsParametersAllIntValues[i] = new List<int>();
                IndicatorsParametersAllDoubleValues[i] = new List<double>();

            }

            //проходим по всем группам источников данных
            foreach(DataSourceGroup dataSourceGroup in DataSourceGroups)
            {
                //формируем серии оптимизационных тестов для данного источника данных для каждого периода (для форвардного нужно смотреть, помещается ли форвардный тест в оставшееся время, и если нет то не создавать оптимизационные тесты)

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

                //определяем начальную и конечную даты тестирования данной группы источников данных
                DateTime startDate = DateTime.Compare(startAvailableDate, StartPeriod) < 0 ? StartPeriod : startAvailableDate;
                DateTime endDate = DateTime.Compare(endAvailableDate, EndPeriod) > 0 ? EndPeriod : endAvailableDate;

                DateTime currentDate = startDate; //текущая дата
                while(DateTime.Compare(currentDate.AddYears(DurationOptimizationTests.Years).AddMonths(DurationOptimizationTests.Months).AddDays(DurationOptimizationTests.Days), EndPeriod) <= 0) //пока текущая дата + длительность оптимизации, раньше или равна дате окончания, формируем оптимизационные тесты
                {
                    TestBatch testBatch = new TestBatch();

                    //формируем TestRuns
                    List<TestRun> testRuns = new List<TestRun>();




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
