using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;

namespace ktradesystem.Models.Datatables
{
    class Indicator
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<ParameterTemplate> ParameterTemplates { get; set; }
        public string Script { get; set; }
        public bool IsStandart { get; set; }
    }
}
