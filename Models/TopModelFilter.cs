using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    [Serializable]
    public class TopModelFilter
    {
        public EvaluationCriteria EvaluationCriteria { get; set; }
        public int EvaluationCriteriaIndex { get; set; }
        public string CompareSign { get; set; }
        public double Value { get; set; }
    }
}
