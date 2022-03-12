using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    public class Currency
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double DollarCost { get; set; } //стоимость 1 доллара для данной валюты
    }
}
