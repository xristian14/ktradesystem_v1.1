using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class DataSourceCandles //класс содержит источник данных, и массив массивов свечек которые содержат данные файлов
    {
        public DataSource DataSource { get; set; }
        public Candle[][] Candles { get; set; } //массив содержит массивы свечек, соответствующие файлам источника данных
        public IndicatorValues[] IndicatorsValues { get; set; } //массив содержит значения индикаторов
    }
}
