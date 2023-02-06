using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class NnDataSourceTemplate
    {
        public DataSourceTemplate DataSourceTemplate { get; set; }
        public string Name { get; set; }
        public bool IsLimitPrognosisCandles { get; set; }
        public int LimitPrognosisCandles { get; set; }
        public bool IsOpen { get; set; }
        public bool IsHighLow { get; set; }
        public bool IsClose { get; set; }
        public bool IsVolume { get; set; }
    }
}
