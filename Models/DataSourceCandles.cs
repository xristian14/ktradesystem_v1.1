using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    [Serializable]
    public class DataSourceCandles //класс содержит источник данных, и массив массивов свечек которые содержат данные файлов
    {
        public DataSource DataSource { get; set; }
        public Candle[][] Candles { get; set; } //массив со свечками на каждый файл источника данных
        public AlgorithmIndicatorValues[] AlgorithmIndicatorsValues { get; set; } //массив содержит значения индикаторов
        public double PerfectProfit { get; set; } //идеальная прибыль. Сумма разности цен закрытия всех последовательных по датам свечек (при переходе на следующий файл доходит до даты которая позже текущей, а разница между свечками разных файлов не высчитывается), взятая по модулю, поделенная на шаг цены и умноженная на стоимость пункта цены
    }
}
