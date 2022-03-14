using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    //класс описывает объекты которые будут использоваться как элементы списка в combobox для выбора тестового прогона
    public class TestRunTestingResultCombobox
    {
        public TestRun TestRun { get; set; }
        public string NameTestRun { get; set; } //название, отображаемое в combobox
    }
}
