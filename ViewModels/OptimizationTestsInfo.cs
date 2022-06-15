using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class OptimizationTestsInfo
    {
        public string TradeWindow { get; set; } //торговое окно
        public string NetProfitLoss { get; set; } //прибыль и убытки
        public string MaxDropdown { get; set; } //максимальное проседание
        public string NumberTrades { get; set; } //количество трейдов
        public string PercentWin { get; set; } //процент выигрышей
        public string NetProfitLossTopModel { get; set; } //прибыль и убытки топ-модели
    }
}
