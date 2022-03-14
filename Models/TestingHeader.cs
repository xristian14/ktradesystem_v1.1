using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.Models
{
    //класс содержит название результата тестирование и дату выполнения тестирования
    public class TestingHeader
    {
        public bool IsHistory { get; set; } //результат тестирования из истории или из сохраненных
        public string TestingName { get; set; }
        public DateTime DateTimeSimulationEnding { get; set; }
    }
}
