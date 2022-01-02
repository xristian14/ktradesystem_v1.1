using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    class DataSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Instrument Instrument { get; set; } //тип инстрмента: фьючерс, акция
        public Interval Interval { get; set; } //временной интервал
        public Currency Currency { get; set; } //валюта
        public double? Cost { get; set; } //стоимость 1 фьючерса (для акций стоимость берется с графика)
        public Comissiontype Comissiontype { get; set; } //тип комисси (денежный, процентный)
        public double Comission { get; set; } //комиссия на одну операцию, куплю или продажу
        public double PriceStep { get; set; } //шаг цены для 1 пункта
        public double CostPriceStep { get; set; } //стоимость шага цены в 1 пункт
        public List<DataSourceFile> DataSourceFiles { get; set; } //файлы
    }
}
