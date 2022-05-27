using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    //класс описывает индекс заявки и индекс сегмента с данной заявкой, чтобы можно было быстро найти сегмент с заявкой
    class SegmentOrderIndexPageTradeChart
    {
        public int SegmentIndex { get; set; }
        public int OrderIndex { get; set; }
    }
}
