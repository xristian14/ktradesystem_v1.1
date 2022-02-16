using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    [Serializable]
    public class IndicatorParameterValue
    {
        public IndicatorParameterTemplate IndicatorParameterTemplate { get; set; }
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
    }
}
