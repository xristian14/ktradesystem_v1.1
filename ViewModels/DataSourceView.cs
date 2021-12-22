using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class DataSourceView //описывает объекты datasource в виде удобном для представления
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Instrument { get; set; } //тип инстрмента: фьючерс, акция
        public string Interval { get; set; } //временной интервал
        public string Currency { get; set; } //валюта
        public double? Cost { get; set; } //стоимость 1 фьючерса (для акций стоимость берется с графика)
        public Comissiontype Comissiontype { get; set; } //тип комисси (денежный, процентный)
        public double Comission { get; set; } //комиссия на одну операцию, куплю или продажу

        public string ComissionView { get; set; } //комиссия с типом комиссии для показа пользователю
        public double PriceStep { get; set; } //шаг цены для 1 пункта
        public double CostPriceStep { get; set; } //стоимость шага цены в 1 пункт
        public List<string> Files { get; set; } //пути к файлам источника данных
    }
}
