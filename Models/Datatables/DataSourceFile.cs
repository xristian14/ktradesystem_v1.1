using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    [Serializable]
    public class DataSourceFile
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public List<DataSourceFileWorkingPeriod> DataSourceFileWorkingPeriods { get; set; }
        public int IdDataSource { get; set; }
    }
}
