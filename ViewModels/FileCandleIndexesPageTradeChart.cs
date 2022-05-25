using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    //класс описывает индексы файла и свечки источника данных
    class FileCandleIndexesPageTradeChart
    {
        public DataSourceAccordance DataSourceAccordance { get; set; } //источник данных, для которого указаны индексы файла и свечки
        public int FileIndex { get; set; } //индекс файла
        public int CandleIndex { get; set; } //индекс свечки
    }
}
