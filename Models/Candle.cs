using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    class Candle
    {
        DateTime dateTime { get; set; }
        double O { get; set; }
        double H { get; set; }
        double L { get; set; }
        double C { get; set; }
        double V { get; set; }
    }
}
