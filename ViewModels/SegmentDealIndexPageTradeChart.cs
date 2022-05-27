using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    //класс описывает индекс сделки и индекс сегмента с данной сделкой, чтобы можно было быстро найти сегмент со сделкой
    class SegmentDealIndexPageTradeChart
    {
        public int SegmentIndex { get; set; }
        public int DealIndex { get; set; }
    }
}
