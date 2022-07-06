using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    [Serializable]
    public class DataSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public MarginType MarginType { get; set; } //тип маржи: фиксированная, с графика
        public Interval Interval { get; set; } //временной интервал
        public Currency Currency { get; set; } //валюта
        public double MarginCost { get; set; } //стоимость маржи
        public decimal MinLotCount { get; set; } //минимальное количество лотов
        public double MinLotMarginPartCost { get; set; } //стоимость минимального количества лотов относительно маржи
        public Comissiontype Comissiontype { get; set; } //тип комисси (денежный, процентный)
        public double Comission { get; set; } //комиссия на одну операцию, куплю или продажу
        public double PriceStep { get; set; } //шаг цены для 1 пункта
        public double CostPriceStep { get; set; } //стоимость шага цены в 1 пункт
        public int PointsSlippage { get; set; } //проскальзывание в пунктах
        public List<DataSourceFile> DataSourceFiles { get; set; } //файлы
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
