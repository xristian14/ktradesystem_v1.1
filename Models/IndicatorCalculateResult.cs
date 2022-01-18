using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class IndicatorCalculateResult //возвращаемое значение индикатора
    {
        public double Value { get; set; }
        public int OverIndex { get; set; } //число, на которое требуемый индекс массива candles превышает максимально доступный индекс
    }
}
