using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ktradesystem.Models
{
    public class TestRun
    {
        [JsonIgnore]
        public TestBatch TestBatch { get; set; }
        public Account Account { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public List<AlgorithmParameterValue> AlgorithmParameterValues { get; set; }
        public List<EvaluationCriteriaValue> EvaluationCriteriaValues { get; set; }
        public List<string> DealsDeviation { get; set; }
        public List<string> LoseDeviation { get; set; }
        public List<string> ProfitDeviation { get; set; }
        public List<string> LoseSeriesDeviation { get; set; }
        public List<string> ProfitSeriesDeviation { get; set; }
        private readonly object locker = new object();
        private bool _isComplete { get; set; } //завершен ли тест
        public bool IsComplete //реализация потокобезопасного получения и установки свойства
        {
            get
            {
                lock (locker)
                {
                    return _isComplete;
                }
            }
            set
            {
                lock (locker)
                {
                    _isComplete = value;
                }
            }
        }
    }
}
