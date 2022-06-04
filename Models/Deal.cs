using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.Text.Json.Serialization;

namespace ktradesystem.Models
{
    [Serializable]
    public class Deal
    {
        public int Number { get; set; }
        public int IdDataSource { get; set; }
        [NonSerialized]
        public DataSource DataSource;

        public int OrderNumber { get; set; } //номер заявки
        [NonSerialized]
        public Order Order;
        public bool Direction { get; set; } //true - купить, false - продать
        public double Price { get; set; }
        public decimal Count { get; set; }
        public DateTime DateTime { get; set; }
    }
}
