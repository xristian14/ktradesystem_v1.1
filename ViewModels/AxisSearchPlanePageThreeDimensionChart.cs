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
    public class AxisSearchPlanePageThreeDimensionChart
    {
        private AxisSearchPlanePageThreeDimensionChart()
        {

        }
        public static AxisSearchPlanePageThreeDimensionChart CreateButtonReset(PropertyChanged propertyChanged) //возвращает объект, содержащий кнопку: сбросить оси плоскоти поиска топ-модели
        {
            AxisSearchPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart = new AxisSearchPlanePageThreeDimensionChart();
            axisSearchPlanePageThreeDimensionChart.ButtonResetVisibility = Visibility.Visible;
            axisSearchPlanePageThreeDimensionChart.DataAxesSearchPlaneVisibility = Visibility.Visible;
            axisSearchPlanePageThreeDimensionChart.IsButtonResetChecked = false;
            axisSearchPlanePageThreeDimensionChart.UpdateProperty += propertyChanged;
            return axisSearchPlanePageThreeDimensionChart;
        }
        public static AxisSearchPlanePageThreeDimensionChart CreateAxisSearchPlane(PropertyChanged propertyChanged, List<AlgorithmParameter> algorithmParameters, int indexSelectedAlgorithmParameter) //возвращает объект: ось плоскости поиска топ-модели
        {
            AxisSearchPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart = new AxisSearchPlanePageThreeDimensionChart();
            axisSearchPlanePageThreeDimensionChart.ButtonResetVisibility = Visibility.Collapsed;
            axisSearchPlanePageThreeDimensionChart.DataAxesSearchPlaneVisibility = Visibility.Visible;
            axisSearchPlanePageThreeDimensionChart.AlgorithmParameters = algorithmParameters;
            axisSearchPlanePageThreeDimensionChart.SelectedAlgorithmParameter = algorithmParameters[indexSelectedAlgorithmParameter];
            axisSearchPlanePageThreeDimensionChart.UpdateProperty += propertyChanged;
            return axisSearchPlanePageThreeDimensionChart;
        }
        public delegate void PropertyChanged(AxisSearchPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart, string propertyName);
        public PropertyChanged UpdateProperty; //метод, вызывающийся при обновлении свойства
        public Visibility ButtonResetVisibility { get; set; } //видимость кнопки сбросить критерии оценки
        private bool _isButtonResetChecked;
        public bool IsButtonResetChecked //нажата ли кнопка сбросить критерии оценки
        {
            get { return _isButtonResetChecked; }
            set
            {
                _isButtonResetChecked = value;
                UpdateProperty?.Invoke(this, "IsButtonResetChecked"); //вызываем метод, который должен вызываться при обновлении свойства
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
                UpdateProperty?.Invoke(this, "SelectedAlgorithmParameter"); //вызываем метод, который должен вызываться при обновлении свойства
            }
        }
    }
}
