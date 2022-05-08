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
    class LevelPageThreeDimensionChart
    {
        private LevelPageThreeDimensionChart()
        {

        }
        public static LevelPageThreeDimensionChart CreateButtonAddLevel(PropertyChanged propertyChanged) //возвращает объект, содержащий кнопку: добавить уровень
        {
            LevelPageThreeDimensionChart levelPageThreeDimensionChart = new LevelPageThreeDimensionChart();
            levelPageThreeDimensionChart.ButtonAddLevelVisibility = Visibility.Visible;
            levelPageThreeDimensionChart.DataLevelVisibility = Visibility.Collapsed;
            levelPageThreeDimensionChart.IsButtonAddLevelChecked = false;
            levelPageThreeDimensionChart.UpdateProperty += propertyChanged;
            return levelPageThreeDimensionChart;
        }
        public static LevelPageThreeDimensionChart CreateLevel(PropertyChanged propertyChanged, double minValue, double maxValue, double value) //возвращает объект: уровень
        {
            LevelPageThreeDimensionChart levelPageThreeDimensionChart = new LevelPageThreeDimensionChart();
            levelPageThreeDimensionChart.ButtonAddLevelVisibility = Visibility.Collapsed;
            levelPageThreeDimensionChart.DataLevelVisibility = Visibility.Visible;
            levelPageThreeDimensionChart.IsButtonAddLevelChecked = false;
            levelPageThreeDimensionChart.MinValue = minValue;
            levelPageThreeDimensionChart.MaxValue = maxValue;
            levelPageThreeDimensionChart.Value = value;
            levelPageThreeDimensionChart.IsDeleteChecked = false;
            levelPageThreeDimensionChart.UpdateProperty += propertyChanged;
            return levelPageThreeDimensionChart;
        }
        public delegate void PropertyChanged(LevelPageThreeDimensionChart levelPageThreeDimensionChart, string propertyName, string propertyValue);
        public PropertyChanged UpdateProperty; //метод, вызывающийся при обновлении свойства
        private Visibility _buttonAddLevelVisibility;
        public Visibility ButtonAddLevelVisibility //видимость кнопки добавить уровень
        {
            get { return _buttonAddLevelVisibility; }
            set
            {
                _buttonAddLevelVisibility = value;
                UpdateProperty?.Invoke(this, "ButtonAddLevelVisibility", value.ToString()); //вызываем метод, который должен вызываться при обновлении свойства
            }
        }
        private bool _isButtonAddLevelChecked;
        public bool IsButtonAddLevelChecked //нажата ли кнопка добавить уровень
        {
            get { return _isButtonAddLevelChecked; }
            set
            {
                _isButtonAddLevelChecked = value;
                UpdateProperty?.Invoke(this, "IsButtonAddLevelChecked", value.ToString()); //вызываем метод, который должен вызываться при обновлении свойства
            }
        }
        private Visibility _dataLevelVisibility;
        public Visibility DataLevelVisibility //видимость данных уровня
        {
            get { return _dataLevelVisibility; }
            set
            {
                _dataLevelVisibility = value;
                UpdateProperty?.Invoke(this, "DataLevelVisibility", value.ToString()); //вызываем метод, который должен вызываться при обновлении свойства
            }
        }
        private double _minValue;
        public double MinValue //миниальное значение уровня
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                UpdateProperty?.Invoke(this, "MinValue", value.ToString()); //вызываем метод, который должен вызываться при обновлении свойства
            }
        }
        public double _maxValue;
        public double MaxValue //максимальное значение уровня
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                UpdateProperty?.Invoke(this, "MaxValue", value.ToString()); //вызываем метод, который должен вызываться при обновлении свойства
            }
        }
        private double _value;
        public double Value //текущее значение уровня
        {
            get { return _value; }
            set
            {
                _value = value;
                UpdateProperty?.Invoke(this, "Value", value.ToString()); //вызываем метод, который должен вызываться при обновлении свойства
            }
        }
        private bool _isDeleteChecked;
        public bool IsDeleteChecked //нажата ли кнопка удалить
        {
            get { return _isDeleteChecked; }
            set
            {
                _isDeleteChecked = value;
                UpdateProperty?.Invoke(this, "IsDeleteChecked", value.ToString()); //вызываем метод, который должен вызываться при обновлении свойства
            }
        }
    }
}
