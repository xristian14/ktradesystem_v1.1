using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    //класс содержит оси плоскости поиска топ-модели. Используется в трехмерном графике для определяния осей полскости поиска топ-модели
    class AxisSearchPlanePageThreeDimensionChart : ViewModelBase
    {
        private AxisSearchPlanePageThreeDimensionChart()
        {

        }
        public static AxisSearchPlanePageThreeDimensionChart CreateButtonReset(PropertyChangedAction propertyChangedAction) //возвращает объект, содержащий кнопку: сбросить оси плоскоти поиска топ-модели
        {
            AxisSearchPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart = new AxisSearchPlanePageThreeDimensionChart();
            axisSearchPlanePageThreeDimensionChart.ButtonResetVisibility = Visibility.Visible;
            axisSearchPlanePageThreeDimensionChart.DataAxesSearchPlaneVisibility = Visibility.Visible;
            axisSearchPlanePageThreeDimensionChart.IsButtonResetChecked = false;
            axisSearchPlanePageThreeDimensionChart.UpdatePropertyAction += propertyChangedAction;
            return axisSearchPlanePageThreeDimensionChart;
        }
        public static AxisSearchPlanePageThreeDimensionChart CreateAxisSearchPlane(PropertyChangedAction propertyChangedAction, List<AlgorithmParameter> algorithmParameters, int indexSelectedAlgorithmParameter) //возвращает объект: ось плоскости поиска топ-модели
        {
            AxisSearchPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart = new AxisSearchPlanePageThreeDimensionChart();
            axisSearchPlanePageThreeDimensionChart.ButtonResetVisibility = Visibility.Collapsed;
            axisSearchPlanePageThreeDimensionChart.DataAxesSearchPlaneVisibility = Visibility.Visible;
            axisSearchPlanePageThreeDimensionChart.AlgorithmParameters = algorithmParameters;
            axisSearchPlanePageThreeDimensionChart.SelectedAlgorithmParameter = algorithmParameters[indexSelectedAlgorithmParameter];
            axisSearchPlanePageThreeDimensionChart.UpdatePropertyAction += propertyChangedAction;
            return axisSearchPlanePageThreeDimensionChart;
        }
        public delegate void PropertyChangedAction(AxisSearchPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart, string propertyName);
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
        public Visibility DataAxesSearchPlaneVisibility { get; set; } //видимость комбобоксов с выбранным параметром для оси
        public List<AlgorithmParameter> AlgorithmParameters { get; set; } //список с параметрами для combobox
        private AlgorithmParameter _selectedAlgorithmParameter; //выбранный параметр в combobox
        public AlgorithmParameter SelectedAlgorithmParameter
        {
            get { return _selectedAlgorithmParameter; }
            set
            {
                _selectedAlgorithmParameter = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "SelectedAlgorithmParameter"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
    }
}
