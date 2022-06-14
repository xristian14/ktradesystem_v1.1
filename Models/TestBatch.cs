using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    [Serializable]
    public class TestBatch //тестовая связка
    {
        public int Number { get; set; } //номер
        public DataSourceGroup DataSourceGroup;
        public int DataSourceGroupIndex { get; set; } //индекс текущей группы источников данных в testing.DataSourceGroups
        [NonSerialized]
        public List<TestRun> OptimizationTestRuns;
        public List<AxesParameter> AxesTopModelSearchPlane { get; set; } //оси плоскости для поиска топ-модели с соседями
        public int TopModelTestRunNumber { get; set; } //номер тестового прогона, определенного как лучшая модель
        [NonSerialized]
        public TestRun TopModelTestRun; //ссылка на testRun, определенный как лучшая модель
        [NonSerialized]
        public TestRun ForwardTestRun;
        [NonSerialized]
        public TestRun ForwardTestRunDepositTrading;
        public List<double> StatisticalSignificance { get; set; } //статистическая значимость. в 0-м элементе количество прибыльных тестов, в 1-м общая прибыль прибыльных тестов, во 2-м количество убыточных тестов, в 3-м общий убыток убыточных убыточных
        public bool IsTopModelDetermining { get; set; } //определена топ-модель для данной тестовой связки
        public bool IsTopModelWasFind { get; set; } //была ли найдена топ-модель (из-за фильтров может не быть топ-модели)
        public List<PerfectProfit> OptimizationPerfectProfits { get; set; } //идеальные прибыли для периода оптимизации, для всех источников данных группы
        public List<PerfectProfit> ForwardPerfectProfits { get; set; } //идеальные прибыли для периода фораврдного теста, для всех источников данных группы
        public void SetTopModel(TestRun testRun)
        {
            TopModelTestRunNumber = testRun.Number;
            TopModelTestRun = testRun;
            IsTopModelWasFind = true; //отмечаем что была найдена топ-модель
        }
    }
}
