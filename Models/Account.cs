using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    class Account
    {
        public List<Order> Orders { get; set; }
        public List<Order> AllOrders { get; set; }
        public List<Deal> CurrentPosition { get; set; } //копии сделок (т.к. при ссылке на оригинальную, при закрытии части позиции, будут изменяться сделки из истории сделок), количество лотов в этих сделках будет уменьшаться при закрытии части позиции
        public List<Deal> AllDeals { get; set; }
        public double Deposit { get; set; }
    }
}
