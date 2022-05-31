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
        public Segment[] Segments { get; set; }
    }
}
