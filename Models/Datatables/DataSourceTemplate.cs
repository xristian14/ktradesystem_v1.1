using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    [Serializable]
    public class DataSourceTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int IdAlgorithm { get; set; }
        public DataSourceTemplate Clone()
        {
            DataSourceTemplate dataSourceTemplate = new DataSourceTemplate { Id = Id, Name = Name, Description = Description, IdAlgorithm = IdAlgorithm };
            return dataSourceTemplate;
        }
    }
}
