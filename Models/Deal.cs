using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    [Serializable]
    public class Deal
    {
        public int Number { get; set; }
        public DataSource DataSource { get; set; }
        public Order Order { get; set; }
        public double Price { get; set; }
        public decimal Count { get; set; }
        public DateTime DateTime { get; set; }
    }
}
