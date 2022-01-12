using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    class DataSourceFileWorkingPeriod
    {
        public int Id { get; set; }
        public DateTime StartPeriod { get; set; } //начало периода
        public DateTime TradingStartTime { get; set; } //время начала торгов
        public DateTime TradingEndTime { get; set; } //время окончания торгов
        public DateTime EndDateTime { get; set; } //добавил сюда это поле чтобы при определении последней даты для источника данных, сохранять где-то эту дату
        public int IdDataSourceFile { get; set; }
    }
}
