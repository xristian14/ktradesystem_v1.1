using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ktradesystem.ViewModels
{
    class ForwardTestInfo : ViewModelBase
    {
        public string TradeWindow { get; set; }
        public string NetOnMargin { get; set; }
        public string AnnualNetOnMargin { get; set; } //годовая доходность на маржу
        public string TopModelAnnualNetOnMargin { get; set; } //годовая доходность на маржу топ-модели
        public string PromMinusBiggestWinSeries { get; set; } //пессимистическая доходность на маржу минус наибольшая выигрышная серия
        public string MaxDropdownPercent { get; set; }
        public string TradesNumber { get; set; }
        public string WinPercent { get; set; }
        public string AveWinDivAveLoss { get; set; } //отношение среднего выигрыша к среднему проигрышу
        public string AverageTrade { get; set; }
        public string ProfitRisk { get; set; } //отношение доходность/риск
        public string Wfe { get; set; } //форвардный показатель эффективности
    }
}
