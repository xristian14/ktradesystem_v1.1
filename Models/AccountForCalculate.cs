using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class AccountForCalculate
    {
        public double FreeRubleMoney { get; set; } //свободные средства в рублях
        public double TakenRubleMoney { get; set; } //занятые средства в рублях
        public double FreeDollarMoney { get; set; } //свободные средства в долларах
        public double TakenDollarMoney { get; set; } //занятые средства в долларах
        public bool IsForwardDepositTrading { get; set; } //если это форвардный тест с торговлей депозитом
        public AccountVariables AccountVariables { get; set; } //переменные в которых пользователь может хранить данные
    }
}
