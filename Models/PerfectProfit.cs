using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    [Serializable]
    public class PerfectProfit
    {
        public int IdDataSource { get; set; } //id источника данных, для которого вычеслена идеальная прибыль
        public double Value { get; set; } //значение идеальной прибыли
    }
}
