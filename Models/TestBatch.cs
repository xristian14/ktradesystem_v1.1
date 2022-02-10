using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class TestBatch //тестовая связка
    {
        public DataSourceGroup DataSourceGroup { get; set; }
        public List<TestRun> OptimizationTestRuns { get; set; }
        public List<AxesParameter> AxesTopModelSearchPlane { get; set; } //оси плоскости для поиска топ-модели с соседями
        public TestRun TopModelTestRun { get; set; } //ссылка на testRun, определенный как лучшая модель
        public TestRun ForwardTestRun { get; set; }
        public TestRun ForwardTestRunDepositTrading { get; set; }
        public List<string[]> StatisticalSignificance { get; set; }
    }
}
