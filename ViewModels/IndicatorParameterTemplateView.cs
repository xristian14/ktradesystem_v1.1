using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class IndicatorParameterTemplateView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int IdIndicator { get; set; }
        public ParameterValueType ParameterValueType { get; set; }
        public Indicator Indicator { get; set; }
    }
}
