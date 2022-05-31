using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    [Serializable]
    public class AlgorithmIndicatorValue //значение индикатора алгоритма
    {
        public bool IsNotOverIndex { get; set; } //не было ли превышение индекса при вычислении этого значения. Если было то будет false
        public double Value { get; set; }
    }
}
