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
    class EvaluationCriteriaPageThreeDimensionChart : ViewModelBase
    {
        private EvaluationCriteriaPageThreeDimensionChart()
        {

        }
        public static EvaluationCriteriaPageThreeDimensionChart CreateButtonReset(PropertyChangedAction propertyChangedAction) //возвращает объект, содержащий кнопку: сбросить критерии оценки
        {
            EvaluationCriteriaPageThreeDimensionChart evaluationCriteriaPageThreeDimensionChart = new EvaluationCriteriaPageThreeDimensionChart();
            evaluationCriteriaPageThreeDimensionChart.ButtonResetVisibility = Visibility.Visible;
            evaluationCriteriaPageThreeDimensionChart.CheckBoxVisibility = Visibility.Collapsed;
            evaluationCriteriaPageThreeDimensionChart.IsButtonResetChecked = false;
            evaluationCriteriaPageThreeDimensionChart.UpdatePropertyAction += propertyChangedAction;
            return evaluationCriteriaPageThreeDimensionChart;
        }
        public static EvaluationCriteriaPageThreeDimensionChart CreateEvaluationCriteria(PropertyChangedAction propertyChangedAction, EvaluationCriteria evaluationCriteria) //возвращает объект: критерий оценки
        {
            EvaluationCriteriaPageThreeDimensionChart evaluationCriteriaPageThreeDimensionChart = new EvaluationCriteriaPageThreeDimensionChart();
            evaluationCriteriaPageThreeDimensionChart.ButtonResetVisibility = Visibility.Collapsed;
            evaluationCriteriaPageThreeDimensionChart.CheckBoxVisibility = Visibility.Visible;
            evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria = evaluationCriteria;
            evaluationCriteriaPageThreeDimensionChart.IsChecked = false;
            evaluationCriteriaPageThreeDimensionChart.UpdatePropertyAction += propertyChangedAction;
            return evaluationCriteriaPageThreeDimensionChart;
        }
        public delegate void PropertyChangedAction(EvaluationCriteriaPageThreeDimensionChart evaluationCriteriaPageThreeDimensionChart, string propertyName);
        public PropertyChangedAction UpdatePropertyAction; //метод, обрабатывающий обновления в свойствах объекта
        public Visibility ButtonResetVisibility { get; set; } //видимость кнопки сбросить критерии оценки
        private bool _isButtonResetChecked;
        public bool IsButtonResetChecked //нажата ли кнопка сбросить критерии оценки
        {
            get { return _isButtonResetChecked; }
            set
            {
                _isButtonResetChecked = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "IsButtonResetChecked"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
        public Visibility CheckBoxVisibility { get; set; } //видимость CheckBox
        public EvaluationCriteria EvaluationCriteria { get; set; }
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "IsChecked"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
    }
}
