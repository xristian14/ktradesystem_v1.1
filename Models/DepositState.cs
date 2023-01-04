using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    [Serializable]
    public class DepositState
    {
        public double Deposit { get; set; }
        public DateTime DateTime { get; set; }
    }
}
