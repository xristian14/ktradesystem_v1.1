using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class AlgorithmParameterView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string Step { get; set; }
        public bool IsStepPercent { get; set; }
        public int IdAlgorithm { get; set; }
        public string RangeValuesView { get; set; }
        public string StepView { get; set; }
    }
}
