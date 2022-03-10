using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class AxesParameterSelectView //класс для выбора осей при определении параметров тестирования
    {
        public string Axis { get; set; }
        public List<AlgorithmParameterView> AlgorithmParametersView { get; set; }
        public AlgorithmParameterView SelectedAlgorithmParameterView { get; set; }
    }
}
