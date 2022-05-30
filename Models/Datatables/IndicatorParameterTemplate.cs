using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ktradesystem.Models.Datatables
{
    [Serializable]
    public class IndicatorParameterTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int IdIndicator { get; set; }
        public ParameterValueType ParameterValueType { get; set; }
        [NonSerialized]
        public Indicator Indicator;
    }
}
