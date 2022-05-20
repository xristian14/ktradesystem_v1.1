using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class TradeChartAreaPageTradeChart
    {
        public int AreaHeight { get; set; } //высота области
        public List<Indicator> Indicators { get; set; } //индикаторы в данной области
    }
}
