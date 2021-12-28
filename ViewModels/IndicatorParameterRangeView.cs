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
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double Step { get; set; }
        public bool IsStepPercent { get; set; }
        public int IdAlgorithm { get; set; }
        public int IdIndicatorParameterTemplate { get; set; }
        public Indicator Indicator { get; set; }
        public string NameIndicator { get; set; }
        public string NameIndicatorParameterTemplate { get; set; }
        public string DescriptionIndicatorParameterTemplate { get; set; }
        public string RangeValuesView { get; set; }
        public string StepView { get; set; }
    }
}
