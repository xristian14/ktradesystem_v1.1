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
        public int IdDataSourceFile { get; set; }
    }
}
