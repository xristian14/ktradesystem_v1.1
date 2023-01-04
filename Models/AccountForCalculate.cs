using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    public class AccountForCalculate
    {
        public double FreeMoney { get; set; } //свободные средства
        public double TakenMoney { get; set; } //занятые средства
        public bool IsForwardDepositTrading { get; set; } //если это форвардный тест с торговлей депозитом
        public AccountVariables AccountVariables { get; set; } //переменные в которых пользователь может хранить данные
    }
}
