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
        public List<TestRun> TestRuns { get; set; }
        public TestRun ForwardTestRun { get; set; }
        public List<string[]> StatisticalSignificance { get; set; }
    }
}
