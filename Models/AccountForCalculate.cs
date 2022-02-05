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
        public double FreeDollarMoney { get; set; } //свободные средства в долларах
        public double TakenRubleMoney { get; set; } //занятые средства в рублях
        public double TakenDollarMoney { get; set; } //занятые средства в долларах
    }
}
