using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class AlgorithmCalculateResult //возвращаемое значение алгоритма
    {
        public List<Order> Orders { get; set; }
        public int OverIndex { get; set; } //число, на которое требуемый индекс массива candles превышает максимально доступный индекс
    }
}
