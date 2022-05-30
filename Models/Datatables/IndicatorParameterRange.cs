using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ktradesystem.Models.Datatables
{
    [Serializable]
    public class IndicatorParameterRange
    {
        public int Id { get; set; }
        public int IdAlgorithmIndicator { get; set; }
        public IndicatorParameterTemplate IndicatorParameterTemplate { get; set; }
        public AlgorithmParameter AlgorithmParameter { get; set; }
        [NonSerialized]
        public AlgorithmIndicator AlgorithmIndicator;
    }
}
