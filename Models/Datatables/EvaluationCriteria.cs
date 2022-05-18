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
        public int OrderCalculate { get; set; } //порядок вычисления
        public int OrderView { get; set; } //порядок отображения пользователю
        public string Name { get; set; }
        public string Description { get; set; }
        public string Script { get; set; }
        public bool IsDoubleValue { get; set; } //данный параметр имеет числовое значение или не. Если нет - то значение хранится в строке
        public bool IsBestPositive { get; set; } //является ли лучшее значение максимальным, если false то лучшее значение минимальное
        public bool IsHaveBestAndWorstValue { get; set; } //имеет ли указанные лучшее и худшее значения. Если да, то на трехмерной диаграмме лучший цвет будет у лучшего значения, а худший у худшего
        public double BestValue { get; set; } //лучшее значение
        public double WorstValue { get; set; } //худшее значение
    }
}
