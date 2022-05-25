using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    //класс описывает секции, из которых состоит график. Имеющиеся в секции источники данных отображаются на графике одновременно. Для источников данных указан индекс файла, свечки которого будут браться для данной секции, индексы начальной и конечной свечки которые учавствуют в данной секции, а так же индекс текущей свечки в данной секции у данного источника данных
    class SectionPageTradeChart
    {
        public List<SectionDataSourcePageTradeChart> SectionDataSources { get; set; } //источники данных у данной секции
    }
}
