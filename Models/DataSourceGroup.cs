using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    [Serializable]
    public class DataSourceGroup
    {
        public List<DataSourceAccordance> DataSourceAccordances { get; set; }
        public DateTime StartPeriodTesting { get; set; } //начало перида тестирования
        public DateTime EndPeriodTesting { get; set; } //окончание перида тестирования
    }
}
