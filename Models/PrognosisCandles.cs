using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class PrognosisCandles
    {
        public NnSettings NnSettings { get; set; }
        public int DataSourceGroupNumber { get; set; }
        public NnDataSourceTemplate NnDataSourceTemplate { get; set; }
        public string Path { get; set; }
        public Candle[][][] Candles { get; set; }
    }
}
