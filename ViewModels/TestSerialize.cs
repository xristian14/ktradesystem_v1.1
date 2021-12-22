using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    [Serializable]
    class TestSerialize
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ParameterTemplate> ParameterTemplates { get; set; }
        public string CalculateText { get; set; }
        public delegate double Calculate(Candle[] candles);

        public double TestCalc(Candle[] candles)
        {
            double a = 0;
            return a;
        }
    }
}
