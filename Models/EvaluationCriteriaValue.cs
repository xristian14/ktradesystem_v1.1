using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    [Serializable]
    public class EvaluationCriteriaValue
    {
        public EvaluationCriteria EvaluationCriteria { get; set; }
        public double DoubleValue { get; set; }
        public string StringValue { get; set; }
    }
}
