using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    class Deal
    {
        public int Number { get; set; }
        public DataSource DataSource { get; set; }
        public Order Order { get; set; }
        public double Price { get; set; }
        public double Count { get; set; }
        public DateTime DateTime { get; set; }
    }
}
