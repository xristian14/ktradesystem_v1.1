using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    //класс описывает элемент разрыва, при смене файла у источника данных
    class DivideTemplatePageTradeChart
    {
        public int Number { get; set; } //номер элемента, под которым он следует на графике
        public List<DataSourceAccordance> DataSourceAccordances { get; set; } //источники данных, которых затрагивает разрыв
    }
}
