using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ktradesystem.Models.Datatables;
using System.Collections.ObjectModel;

namespace ktradesystem.ViewModels
{
    //класс содержит оси плоскости поиска топ-модели. Используется в трехмерном графике для определяния осей полскости поиска топ-модели
    class AxisPlanePageThreeDimensionChart : ViewModelBase
    {
        private AxisPlanePageThreeDimensionChart()
        {

        }
        public static AxisPlanePageThreeDimensionChart CreateButtonReset(PropertyChangedAction propertyChangedAction) //возвращает объект, содержащий кнопку: сбросить оси плоскоти поиска топ-модели
        {
            AxisPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart = new AxisPlanePageThreeDimensionChart();
            axisSearchPlanePageThreeDimensionChart.ButtonResetVisibility = Visibility.Visible;
            axisSearchPlanePageThreeDimensionChart.ComboBoxAxesPlaneVisibility = Visibility.Collapsed;
            axisSearchPlanePageThreeDimensionChart.TextBlockAxesPlaneVisibility = Visibility.Collapsed;
            axisSearchPlanePageThreeDimensionChart.IsButtonResetChecked = false;
            axisSearchPlanePageThreeDimensionChart.UpdatePropertyAction += propertyChangedAction;
            return axisSearchPlanePageThreeDimensionChart;
        }
        public static AxisPlanePageThreeDimensionChart CreateAxisPlaneComboBox(PropertyChangedAction propertyChangedAction, ObservableCollection<AlgorithmParameter> algorithmParameters, int indexSelectedAlgorithmParameter) //возвращает объект: ось плоскости с комбобоксом для выбора оси
        {
            AxisPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart = new AxisPlanePageThreeDimensionChart();
            axisSearchPlanePageThreeDimensionChart.ButtonResetVisibility = Visibility.Collapsed;
            axisSearchPlanePageThreeDimensionChart.ComboBoxAxesPlaneVisibility = Visibility.Visible;
            axisSearchPlanePageThreeDimensionChart.TextBlockAxesPlaneVisibility = Visibility.Collapsed;
            axisSearchPlanePageThreeDimensionChart.AlgorithmParameters = algorithmParameters;
            axisSearchPlanePageThreeDimensionChart.SelectedAlgorithmParameter = algorithmParameters[indexSelectedAlgorithmParameter];
            axisSearchPlanePageThreeDimensionChart.UpdatePropertyAction += propertyChangedAction;
            return axisSearchPlanePageThreeDimensionChart;
        }
        public static AxisPlanePageThreeDimensionChart CreateAxisPlaneTextBlock(PropertyChangedAction propertyChangedAction, AlgorithmParameter algorithmParameter) //возвращает объект: ось плоскости с текстовым блоком без возможности выбора для параметра с одним значением
        {
            AxisPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart = new AxisPlanePageThreeDimensionChart();
            axisSearchPlanePageThreeDimensionChart.ButtonResetVisibility = Visibility.Collapsed;
            axisSearchPlanePageThreeDimensionChart.ComboBoxAxesPlaneVisibility = Visibility.Collapsed;
            axisSearchPlanePageThreeDimensionChart.TextBlockAxesPlaneVisibility = Visibility.Visible;
            axisSearchPlanePageThreeDimensionChart.SelectedAlgorithmParameter = algorithmParameter;
            axisSearchPlanePageThreeDimensionChart.UpdatePropertyAction += propertyChangedAction;
            return axisSearchPlanePageThreeDimensionChart;
        }
        public delegate void PropertyChangedAction(AxisPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart, string propertyName);
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
        public Visibility ComboBoxAxesPlaneVisibility { get; set; } //видимость комбобоксов с выбранным параметром для оси
        public Visibility TextBlockAxesPlaneVisibility { get; set; } //видимость текстового поля с названием параметра
        private ObservableCollection<AlgorithmParameter> _algorithmParameters;
        public ObservableCollection<AlgorithmParameter> AlgorithmParameters //список с параметрами для combobox
        {
            get { return _algorithmParameters; }
            set
            {
                _algorithmParameters = value;
                OnPropertyChanged();
            }
        }
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
