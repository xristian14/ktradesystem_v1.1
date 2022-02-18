using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models.Datatables
{
    public class AlgorithmIndicator
    {
        public int Id { get; set; }
        public Algorithm Algorithm { get; set; }
        public Indicator Indicator { get; set; }
        public string Ending { get; set; }
    }
}
