using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    [Serializable]
    public class IndicatorParameterRange
    {
        public int Id { get; set; }
        public IndicatorParameterTemplate IndicatorParameterTemplate { get; set; }
        public AlgorithmParameter AlgorithmParameter { get; set; }
        public AlgorithmIndicator AlgorithmIndicator { get; set; }
    }
}
