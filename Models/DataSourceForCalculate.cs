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
        public Candle[] Candles { get; set; }
        public int CurrentCandleIndex { get; set; }
    }
}
