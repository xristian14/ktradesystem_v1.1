using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    public class IndicatorParameterTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int IdIndicator { get; set; }
        public ParameterValueType ParameterValueType { get; set; }
        public Indicator Indicator { get; set; }
    }
}
