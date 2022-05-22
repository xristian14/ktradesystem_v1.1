using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ktradesystem.Models.Datatables
{
    public class AlgorithmIndicator
    {
        public int Id { get; set; }
        public int IdAlgorithm { get; set; }
        public int IdIndicator { get; set; }
        [JsonIgnore]
        public Algorithm Algorithm { get; set; }
        public Indicator Indicator { get; set; }
        public List<IndicatorParameterRange> IndicatorParameterRanges { get; set; }
        public string Ending { get; set; }
    }
}
