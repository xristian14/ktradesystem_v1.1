using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    public class Setting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int IdSettingType { get; set; }
        public bool BoolValue { get; set; }
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
    }
}
