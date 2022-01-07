using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class DataSourceGroupView //выбранные источники данных для макетов источников данных
    {
        public int Number { get; set; }
        public List<DataSourceAccordance> DataSourcesAccordances { get; set; } //соответствие шаблонов источников данных и источников данных
    }
}
