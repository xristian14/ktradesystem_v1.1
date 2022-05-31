using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    [Serializable]
    public class CandleIndex
    {
        public int DataSourceIndex { get; set; } //индекс источника данных в DataSourcesCandles
        public int FileIndex { get; set; } //индекс файла
        public int IndexCandle { get; set; } //индекс свечки
    }
}
