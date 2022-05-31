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
        [NonSerialized]
        public DataSourceGroup DataSourceGroup;
        public int DataSourceGroupIndex { get; set; } //индекс текущей группы источников данных в testing.DataSourceGroups
        public List<TestRun> OptimizationTestRuns { get; set; }
        public List<AxesParameter> AxesTopModelSearchPlane { get; set; } //оси плоскости для поиска топ-модели с соседями
        public TestRun TopModelTestRun { get; set; } //ссылка на testRun, определенный как лучшая модель
        public TestRun ForwardTestRun { get; set; }
        public TestRun ForwardTestRunDepositTrading { get; set; }
        public List<string[]> StatisticalSignificance { get; set; }
        public bool IsTopModelDetermining { get; set; } //определена топ-модель для данной тестовой связки
        public bool IsTopModelWasFind { get; set; } //была ли найдена топ-модель (из-за фильтров может не быть топ-модели)
        public void SetTopModel(TestRun testRun)
        {
            TopModelTestRun = testRun;
            IsTopModelWasFind = true; //отмечаем что была найдена топ-модель
        }
    }
}
