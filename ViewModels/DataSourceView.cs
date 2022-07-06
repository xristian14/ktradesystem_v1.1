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
        public MarginType MarginType { get; set; }
        public string Interval { get; set; } //временной интервал
        public Currency Currency { get; set; } //валюта
        public double MarginCost { get; set; } //стоимость маржи
        public decimal MinLotCount { get; set; } //минимальное количество лотов
        public decimal MinLotMarginPartCost { get; set; } //стоимость минимального количества лотов относительно маржи
        public Comissiontype Comissiontype { get; set; } //тип комисси (денежный, процентный)
        public decimal Comission { get; set; } //комиссия на одну операцию, куплю или продажу
        public string ComissionView { get; set; } //комиссия с типом комиссии для показа пользователю
        public decimal PriceStep { get; set; } //шаг цены для 1 пункта
        public decimal CostPriceStep { get; set; } //стоимость шага цены в 1 пункт
        public int PointsSlippage { get; set; } //проскальзывание в пунктах
        public string DatePeriod { get; set; }
        public List<string> Files { get; set; } //пути к файлам источника данных
    }
}
