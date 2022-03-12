using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class AlgorithmIndicatorValues //индикатор алгоритма и значения (для всех свечек), используется как свойство в DataSourceCandles
    {
        public AlgorithmIndicator AlgorithmIndicator { get; set; } //индикатор алгоритма, для которого текущие значения
        public double[][] Values { get; set; } //массив из массивов значений, элементы которого соответствуют файлам источника данных
    }
}
