using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class SectionPageTradeChart
    {
        public bool IsPresent { get; set; } //секция находится в настоящем, или на дате которая уже была (если в следующем файле есть даты которые уже были, то для такой секции будет false)
        public List<DataSource> DataSources { get; set; } //источники данных у данной секции
    }
}
