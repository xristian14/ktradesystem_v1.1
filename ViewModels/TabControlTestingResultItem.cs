using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    public class TabControlTestingResultItem
    {
        public string Header { get; set; } //заголовок вкладки
        public List<StackPanelTestingResult> HorizontalStackPanels { get; set; } //ряды со страницами
        public List<StackPanelTestingResult> VerticalStackPanels { get; set; } //колонки со страницами. Предполагается использовать либо HorizontalStackPanels либо VerticalStackPanels, в зависимости от того как лучше скомпоновать страницы
    }
}
