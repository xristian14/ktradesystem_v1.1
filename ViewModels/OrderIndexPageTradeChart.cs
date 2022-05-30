using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class OrderIndexPageTradeChart
    {
        public bool IsStart { get; set; } //флаг того что заявка выставлена в данном сегменте
        public bool isEnd { get; set; } //флаг того что заявка снята/исполнена в данном сегменте
        public int OrderIndex { get; set; } //индекс заявки
    }
}
