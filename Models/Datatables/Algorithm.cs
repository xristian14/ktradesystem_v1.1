using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    class Algorithm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AlgorithmParamter> AlgorithmParamters { get; set; }
        public string Script { get; set; }
        public bool IsStandart { get; set; }
    }
}
