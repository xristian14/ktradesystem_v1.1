using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    //класс описывает объекты которые будут использоваться как элементы списка в combobox для выбора тестовой связки
    public class TestBatchTestingResultCombobox
    {
        public TestBatch TestBatch { get; set; }
        public string NameTestBatch { get; set; } //название, отображаемое в combobox
    }
}
