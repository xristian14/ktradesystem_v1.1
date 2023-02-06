using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class NnTesting
    {
        public List<DataSourceGroup> DataSourceGroups { get; set; }
        public List<NnDataSourceTemplate> NnDataSourceTemplates { get; set; }
        public NnDataSourceTemplate TradingNnDataSourceTemplate { get; set; }
        public List<DataSourcePrognosisFile> DataSourcePrognosisFiles { get; set; }
        public Currency DefaultCurrency { get; set; }
        public List<DataSourceCandles> DataSourceCandles { get; set; }
        public List<Candle[][][]> PrognosisCandles { get; set; }
        public NnManager[][] DataSourceGroupNnManagers { get; set; }
    }
}
