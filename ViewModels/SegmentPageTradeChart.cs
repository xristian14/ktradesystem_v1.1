using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    //класс описывает элемент для одной даты на таймлайне. В нем содержаться разрывы, индексы свечкек для источников данных, индексы заявок и сделок
    class SegmentPageTradeChart
    {
        bool IsDivide { get; set; } //данный сегмент содержит разрыв или свечки
        List<int> DivideDataSourceIndexes { get; set; } //индексы источников данных в DataSourceCandles для которых происходит разрыв
        List<CandleIndexPageTradeChart> CandleIndexes { get; set; } //индексы свечек которые имеются в текущем сегменте
        List<OrderIndexPageTradeChart> OrderIndexes { get; set; } //индексы заявок которые имеются в текущем сегменте
        List<int> DealIndexes { get; set; } //индексы заявок которые имеются в текущем сегменте
    }
}
