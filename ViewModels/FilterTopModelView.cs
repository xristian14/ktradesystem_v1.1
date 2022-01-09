using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.Collections.ObjectModel;

namespace ktradesystem.ViewModels
{
    class FilterTopModelView
    {
        public ObservableCollection<EvaluationCriteriaView> EvaluationCriteriasView { get; set; }
        public EvaluationCriteriaView SelectedEvaluationCriteriaView { get; set; }
        public ObservableCollection<string> CompareSings { get; set; }
        public string SelectedCompareSing { get; set; }
        public string FilterValue { get; set; }
    }
}
