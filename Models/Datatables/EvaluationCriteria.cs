using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    public class EvaluationCriteria //критерий оценки тестирвоания
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Script { get; set; }
        public bool IsDoubleValue { get; set; } //данный параметр имеет числовое значение или не. Если нет - то значение хранится в строке
    }
}
