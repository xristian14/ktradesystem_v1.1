using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    class IndicatorParameterRange
    {
        public int Id { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double? Step { get; set; }
        public bool? IsStepPercent { get; set; }
        public int IdAlgorithm { get; set; }
        public int IdIndicatorParameterTemplate { get; set; }
        public Indicator Indicator { get; set; }
    }
}
