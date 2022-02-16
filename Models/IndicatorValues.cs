using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    [Serializable]
    public class IndicatorValues //индикатор и значения данного индикатора, используется как свойство в DataSourceCandles
    {
        public Indicator Indicator { get; set; } //индикатор, для которого текущие значения
        public double[][] Values { get; set; } //массив из массивов значений, элементы которого соответствуют файлам источника данных
    }
}
