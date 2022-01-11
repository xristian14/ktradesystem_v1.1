using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    class TopModelCriteria
    {
        public EvaluationCriteria EvaluationCriteria { get; set; }
        public string CompareSign { get; set; }
        public List<TopModelFilter> TopModelFilters { get; set; }
    }
}
