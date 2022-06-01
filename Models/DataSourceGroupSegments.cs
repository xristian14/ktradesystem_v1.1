using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    //сегменты для группы источников данных
    [Serializable]
    public class DataSourceGroupSegments
    {
        public DataSourceGroup DataSourceGroup { get; set; }
        public List<Section> Sections { get; set; } //секции для данных сегментов
        public List<Segment> Segments { get; set; } //сегменты, каждый из которых отражает одну дату на таймлайне
        public int LastTradeSegmentIndex { get; set; } //индекс последнего сегмента при котором можно продолжать торговлю. Например один источник данных закончился, а второй продолжается, тогда этот индекс будет показывать индекс на котором закончился один из источников данных, т.к. его свечек больше не будет, а для вычисления алгоритма необходимы все источники данных
    }
}
