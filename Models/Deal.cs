using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.Text.Json.Serialization;

namespace ktradesystem.Models
{
    public class Deal
    {
        public int Number { get; set; }
        public int IdDataSource { get; set; }
        [JsonIgnore]
        public DataSource DataSource { get; set; }
        public int OrderNumber { get; set; } //номер заявки
        [JsonIgnore]
        public Order Order { get; set; }
        public double Price { get; set; }
        public decimal Count { get; set; }
        public DateTime DateTime { get; set; }
    }
}
