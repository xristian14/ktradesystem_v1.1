using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    public class AlgorithmIndicatorView
    {
        public int Id { get; set; }
        public Algorithm Algorithm { get; set; }
        public Indicator Indicator { get; set; }
        public List<IndicatorParameterRangeView> IndicatorParameterRangesView { get; set; }
        public string Ending { get; set; }
    }
}
