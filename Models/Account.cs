using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class Account
    {
        public List<Order> Orders { get; set; }
        public List<Order> AllOrders { get; set; }
        public List<Deal> CurrentPosition { get; set; } //копии сделок (т.к. при ссылке на оригинальную, при закрытии части позиции, будут изменяться сделки из истории сделок), количество лотов в этих сделках будет уменьшаться при закрытии части позиции
        public List<Deal> AllDeals { get; set; }
        public bool IsForwardDepositTrading { get; set; } //если это форвардный тест с торговлей депозитом
        public List<DepositCurrency> FreeForwardDepositCurrencies { get; set; } //свободные средства
        public List<DepositCurrency> TakenForwardDepositCurrencies { get; set; } //занятые средства (в открытых позициях)
        public List<List<DepositCurrency>> DepositCurrenciesChanges { get; set; } //изменения депозита в разных валютах
        public Currency DefaultCurrency { get; set; } //валюта по умолчанию
        public double Margin { get; set; } //значение маржи. Вычисляется в конце выполнения тестового прогона, и используется для вычисления критериев оценки. Т.к. вычислять каждый раз в критериях оценки займет больше времени
    }
}
