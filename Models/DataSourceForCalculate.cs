using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class DataSourceForCalculate //источники данных, которые передаются как параметр в алгоритм, содержат поля источника данных, к которым обращается пользователь при описании алгоритма
    {
        public int idDataSource { get; set; }
        public double[] IndicatorsValues { get; set; }
        public double PriceStep { get; set; } //шаг цены для 1 пункта
        public double CostPriceStep { get; set; } //стоимость шага цены в 1 пункт
        public double OneLotCost { get; set; } //стоимость 1 лота
        public double Price { get; set; } //средняя цена позиции для данного источника данных
        public decimal CountBuy { get; set; } //количество купленных лотов для данного источника данных
        public decimal CountSell { get; set; } //количество проданных лотов для данного источника данных
        public TimeSpan TimeInCandle { get; set; } //время в свечке
        public DateTime TradingStartTimeOfDay { get; set; } //время начала торгов дня
        public DateTime TradingEndTimeOfDay { get; set; } //время окончания торгов дня
        public Candle[] Candles { get; set; }
        public int CurrentCandleIndex { get; set; }
    }
}
