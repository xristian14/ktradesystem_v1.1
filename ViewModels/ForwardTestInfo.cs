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
        public string TradeWindow { get; set; } //торговое окно
        public string NetProfitLoss { get; set; } //прибыль и убытки
        public string MaxDropdown { get; set; } //максимальное проседание
        public string NumberTrades { get; set; } //количество трейдов
        public string PercentWin { get; set; } //процент выигрышей
        public string AveWinDivAveLoss { get; set; } //отношение среднего выигрыша к среднему проигрышу
        public string AverageTrade { get; set; } //средний трейд
        public string ProfitRisk { get; set; } //отношение доходность/риск
        public string Wfe { get; set; } //форвардный показатель эффективности
        public string Prom { get; set; } //пессимистическая доходность на маржу
    }
}
