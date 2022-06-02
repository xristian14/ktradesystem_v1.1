using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class CandleIndexPageTradeChart
    {
        public int DataSourceCandlesIndex { get; set; } //индекс источника данных в DataSourcesCandles
        public int FileIndex { get; set; } //индекс файла
        public int CandleIndex { get; set; } //индекс свечки
        public List<OrderIndexPageTradeChart> OrderIndexes { get; set; } //индексы заявок которые имеются в текущем источнике данных на текущей свечке
        public List<int> DealIndexes { get; set; } //индексы сделок которые имеются в текущем источнике данных на текущей свечке
    }
}
