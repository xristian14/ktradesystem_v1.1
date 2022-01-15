using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    public class Algorithm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DataSourceTemplate> DataSourceTemplates { get; set; }
        public List<IndicatorParameterRange> IndicatorParameterRanges { get; set; }
        public List<AlgorithmParameter> AlgorithmParameters { get; set; }
        public string Script { get; set; }
    }
}
