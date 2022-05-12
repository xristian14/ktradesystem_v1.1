using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    //класс содержит описание объекта: отображаемого уровня, на графике
    class LevelPageThreeDimensionChart : ViewModelBase
    {
        private LevelPageThreeDimensionChart()
        {

        }
        public static LevelPageThreeDimensionChart CreateButtonAddLevel(PropertyChangedAction propertyChangedAction) //возвращает объект, содержащий кнопку: добавить уровень
        {
            LevelPageThreeDimensionChart levelPageThreeDimensionChart = new LevelPageThreeDimensionChart();
            levelPageThreeDimensionChart.ButtonAddLevelVisibility = Visibility.Visible;
            levelPageThreeDimensionChart.DataLevelVisibility = Visibility.Collapsed;
            levelPageThreeDimensionChart.IsButtonAddLevelChecked = false;
            levelPageThreeDimensionChart.UpdatePropertyAction += propertyChangedAction;
            return levelPageThreeDimensionChart;
        }
        public static LevelPageThreeDimensionChart CreateLevel(PropertyChangedAction propertyChangedAction, double minValue, double maxValue, double value) //возвращает объект: уровень
        {
            LevelPageThreeDimensionChart levelPageThreeDimensionChart = new LevelPageThreeDimensionChart();
            levelPageThreeDimensionChart.ButtonAddLevelVisibility = Visibility.Collapsed;
            levelPageThreeDimensionChart.DataLevelVisibility = Visibility.Visible;
            levelPageThreeDimensionChart.IsButtonAddLevelChecked = false;
            levelPageThreeDimensionChart.MinValue = minValue;
            levelPageThreeDimensionChart.MaxValue = maxValue;
            levelPageThreeDimensionChart.Value = value;
            levelPageThreeDimensionChart.IsDeleteChecked = false;
            levelPageThreeDimensionChart.UpdatePropertyAction += propertyChangedAction;
            return levelPageThreeDimensionChart;
        }
        public delegate void PropertyChangedAction(LevelPageThreeDimensionChart levelPageThreeDimensionChart, string propertyName);
        public PropertyChangedAction UpdatePropertyAction; //метод, обрабатывающий обновления в свойствах объекта
        public Visibility ButtonAddLevelVisibility { get; set; } //видимость кнопки добавить уровень
        private bool _isButtonAddLevelChecked;
        public bool IsButtonAddLevelChecked //нажата ли кнопка добавить уровень
        {
            get { return _isButtonAddLevelChecked; }
            set
            {
                _isButtonAddLevelChecked = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "IsButtonAddLevelChecked"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
        public Visibility DataLevelVisibility { get; set; } //видимость данных уровня
        private double _minValue;
        public double MinValue //миниальное значение уровня
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "MinValue"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
        public double _maxValue;
        public double MaxValue //максимальное значение уровня
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "MaxValue"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
        private double _value;
        public double Value //текущее значение уровня
        {
            get { return _value; }
            set
            {
                _value = Math.Round(value, 2); //округляем до 2-х знаков после запятой
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "Value"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
        private bool _isDeleteChecked;
        public bool IsDeleteChecked //нажата ли кнопка удалить
        {
            get { return _isDeleteChecked; }
            set
            {
                _isDeleteChecked = value;
                OnPropertyChanged();
                UpdatePropertyAction?.Invoke(this, "IsDeleteChecked"); //вызываем метод, обрабатывающий обновления в свойствах объекта
            }
        }
    }
}
