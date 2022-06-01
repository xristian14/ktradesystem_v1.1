using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    //класс описывает секции, для графика, на одной секции могут быть только определенные источники данных. Например при переходе на следующий файл у которого даты из прошлого создается секция для этого источника данных, которая длится до перехода на новую дату, после чего создается секция со всеми источниками данных
    [Serializable]
    public class Section
    {
        public bool IsPresent { get; set; } //секция находится в настоящем, или на дате которая уже была (если в следующем файле есть даты которые уже были, то для такой секции будет false)
        public List<DataSource> DataSources { get; set; } //источники данных у данной секции
        public List<int> DataSourceCandlesIndexes { get; set; } //индексы объектов со свечками источников данных которые соответствуют источникам данных в DataSources. Нужно чтобы при формировании сегментов для графика в представлении можно было понять к какому источнику данных относится индекс DataSourceCandlesIndex в CandleIndex. В DataSourceCandlesIndexes[0] содержится индекс DataSourceCandles у которого источник данных как в DataSources[0]
    }
}
