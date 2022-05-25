using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    //класс описывает источник данных, индекс файла а так же индексы начальной и конечной свечки, который используется в секции, и индекс текущей свечки
    class SectionDataSourcePageTradeChart
    {
        public DataSourceAccordance DataSourceAccordance { get; set; } //источник данных
        public int FileIndex { get; set; } //индекс файла у источника данных
        public int CurrentCandleIndex { get; set; } //индекс текущей свечки
        public int StartCandleIndex { get; set; } //индекс начальной свечки
        public int EndCandleIndex { get; set; } //индекс конечной свечки
    }
}
