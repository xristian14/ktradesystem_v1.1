using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ktradesystem.Models.Datatables
{
    public class IndicatorParameterRange
    {
        public int Id { get; set; }
        public int IdAlgorithmIndicator { get; set; }
        public IndicatorParameterTemplate IndicatorParameterTemplate { get; set; }
        public AlgorithmParameter AlgorithmParameter { get; set; }
        [JsonIgnore]
        public AlgorithmIndicator AlgorithmIndicator { get; set; }
    }
}
