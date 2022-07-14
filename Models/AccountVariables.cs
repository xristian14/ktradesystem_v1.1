using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    [Serializable]
    public class AccountVariables //класс для переменных в которых пользователь может хранить данные
    {
        public static AccountVariables GetAccountVariables()
        {
            return new AccountVariables { IntVar1 = 0, IntVar2 = 0, IntVar3 = 0, IntVar4 = 0, IntVar5 = 0, DoubleVar1 = 0, DoubleVar2 = 0, DoubleVar3 = 0, DoubleVar4 = 0, DoubleVar5 = 0 };
        }
        public int IntVar1 = 0;
        public int IntVar2 = 0;
        public int IntVar3 = 0;
        public int IntVar4 = 0;
        public int IntVar5 = 0;
        public double DoubleVar1 = 0;
        public double DoubleVar2 = 0;
        public double DoubleVar3 = 0;
        public double DoubleVar4 = 0;
        public double DoubleVar5 = 0;
    }
}
