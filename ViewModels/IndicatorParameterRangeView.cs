using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class IndicatorParameterRangeView
    {
        public int Id { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string Step { get; set; }
        public bool IsStepPercent { get; set; }
        public int IdAlgorithm { get; set; }
        public IndicatorParameterTemplate IndicatorParameterTemplate { get; set; }
        public Indicator Indicator { get; set; }
        public string NameIndicator { get; set; }
        public string NameIndicatorParameterTemplate { get; set; }
        public string DescriptionIndicatorParameterTemplate { get; set; }
        public string RangeValuesView { get; set; }
        public string StepView { get; set; }
    }
}
