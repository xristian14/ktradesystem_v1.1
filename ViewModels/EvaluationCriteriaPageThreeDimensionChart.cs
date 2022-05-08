using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    //класс содержит критерий оценки и выбран ли он. Используется в трехмерном графике для определяния выбранных для отображения критериев оценки на графике
    public class EvaluationCriteriaPageThreeDimensionChart
    {
        public Visibility ButtonResetVisibility { get; set; } //видимость кнопки сбросить критерии оценки
        public Visibility CheckBoxVisibility { get; set; } //видимость CheckBox
        public EvaluationCriteria EvaluationCriteria { get; set; }
        public bool IsChecked { get; set; }
    }
}
