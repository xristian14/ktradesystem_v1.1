using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    /// <summary>
    /// Путь к файлу с спрогнозированными свечками для источника данных
    /// </summary>
    public class DataSourcePrognosisFile
    {
        public DataSource DataSource { get; set; }
        public string Path { get; set; }
    }
}
