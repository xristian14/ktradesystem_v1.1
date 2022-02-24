using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    public class IndicatorParameterRangeView
    {
        public int Id { get; set; }
        public int IdAlgorithm { get; set; }
        public IndicatorParameterTemplate IndicatorParameterTemplate { get; set; }
        public AlgorithmIndicatorView AlgorithmIndicatorView { get; set; }
        public string NameAlgorithmIndicator { get; set; }
        public ObservableCollection<AlgorithmParameterView> AlgorithmParametersView { get; set; }
        public AlgorithmParameterView SelectedAlgorithmParameterView { get; set; }
    }
}
