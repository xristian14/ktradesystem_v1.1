using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    //класс описывает элемент для одной даты на таймлайне. В нем содержатся индексы свечкек источников данных
    [Serializable]
    public class Segment
    {
        public Section Section { get; set; } //секция, к которой относится сегмент
        public List<CandleIndex> CandleIndexes { get; set; } //индексы свечек которые имеются в текущем сегменте для источников данных
    }
}
