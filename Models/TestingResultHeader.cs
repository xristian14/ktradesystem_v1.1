using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    //класс содержит название результата тестирование и дату выполнения тестирования
    public class TestingResultHeader
    {
        public string TestingResultName { get; set; }
        public DateTime DateTimeSimulationEnding { get; set; }
    }
}
