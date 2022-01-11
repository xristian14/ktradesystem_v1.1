using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class DataSourceAccordanceView //соответствие между шаблоном источника данных, и источником данных, для тестирования
    {
        public DataSourceTemplate DataSourceTemplate { get; set; }
        public DataSource DataSource { get; set; }
    }
}
