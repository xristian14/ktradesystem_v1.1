using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class TestRun
    {
        public TestBatch TestBatch { get; set; }
        public Account Account { get; set; }
        public Account AccountDepositTrading { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public List<IndicatorParameterValue> IndicatorParameterValues { get; set; }
        public List<AlgorithmParameterValue> AlgorithmParameterValues { get; set; }
        public List<EvaluationCriteriaValue> EvaluationCriteriaValues { get; set; }
        public List<string> DealsDeviation { get; set; }
        public List<string> LoseDeviation { get; set; }
        public List<string> ProfitDeviation { get; set; }
        public List<string> LoseSeriesDeviation { get; set; }
        public List<string> ProfitSeriesDeviation { get; set; }
    }
}
