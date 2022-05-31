using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    //класс описывает элемент для одной даты на таймлайне. В нем содержатся индексы свечкек источников данных
    [Serializable]
    public class Segment
    {
        public bool IsPresent { get; set; } //сегмент находится в настоящем, или на дате которая уже была (если в следующем файле есть даты которые уже были, то для такого сегмента будет false)
        public List<CandleIndex> CandleIndexes { get; set; } //индексы свечек которые имеются в текущем сегменте для источников данных
    }
}
