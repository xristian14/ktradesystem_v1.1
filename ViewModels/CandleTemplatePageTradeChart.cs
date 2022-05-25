using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    //класс описывает шаблон свечки, на основании которого будет строиться отображаемая свечка
    class CandleTemplatePageTradeChart
    {
        public int Number { get; set; } //номер элемента, под которым он следует на графике
        public DataSourceAccordance DataSourceAccordance { get; set; } //источник даных, свечке которого соответсвтвует данная свечка
        public int FileIndex { get; set; } //индекс файла у источника данных
        public int CandleIndex { get; set; } //индекс свечки
    }
}
