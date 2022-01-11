using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    class Order
    {
        public int Number { get; set; }
        public DataSource DataSource { get; set; }
        public TypeOrder TypeOrder { get; set; }
        public bool Direction { get; set; } //true - купить, false - продать
        public double price { get; set; }
        public double Count { get; set; }
        public DateTime DateTimeSubmit { get; set; }
        public DateTime DateTimeRemove { get; set; }
    }
}
