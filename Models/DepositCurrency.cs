using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class DepositCurrency
    {
        public Currency Currency { get; set; }
        public double Deposit { get; set; }
    }
}
