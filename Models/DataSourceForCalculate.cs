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
        public DataSource DataSource { get; set; }
        public double[] IndicatorsValues { get; set; }
        public double Price { get; set; } //средняя цена позиции для данного источника данных
        public decimal CountBuy { get; set; } //количество купленных лотов для данного источника данных
        public decimal CountSell { get; set; } //количество проданных лотов для данного источника данных
        public Candle[] Candles { get; set; }
        public int CurrentCandleIndex { get; set; }
    }
}
