using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class Scaler
    {
        public int Id { get; set; }
        public bool IsMinZero { get; set; }
        public List<DataSourceTemplate> Min { get; set; }
        public List<DataSourceTemplate> Max { get; set; }
    }
}
