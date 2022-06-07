using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ktradesystem.ViewModels
{
    class ScaleValuePageTradeChart : ViewModelBase
    {
        public SolidColorBrush StrokeLineColor { get; set; } //цвет линии
        public SolidColorBrush TextColor { get; set; } //цвет текста
        public int FontSize { get; set; }
        private string _price;
        public string Price //значение цены
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }
        private double _priceLeft;
        public double PriceLeft //отступ сверху цены
        {
            get { return _priceLeft; }
            set
            {
                _priceLeft = value;
                OnPropertyChanged();
            }
        }
        private double _priceTop;
        public double PriceTop //отступ сверху цены
        {
            get { return _priceTop; }
            set
            {
                _priceTop = value;
                OnPropertyChanged();
            }
        }
        private double _lineTop;
        public double LineTop //отступ сверху
        {
            get { return _lineTop; }
            set
            {
                _lineTop = value;
                OnPropertyChanged();
            }
        }
        private double _lineLeft;
        public double LineLeft //отступ слева
        {
            get { return _lineLeft; }
            set
            {
                _lineLeft = value;
                OnPropertyChanged();
            }
        }
        private double _x1;
        public double X1 //координата точки линии
        {
            get { return _x1; }
            set
            {
                _x1 = value;
                OnPropertyChanged();
            }
        }
        private double _y1;
        public double Y1 //координата точки линии
        {
            get { return _y1; }
            set
            {
                _y1 = value;
                OnPropertyChanged();
            }
        }
        private double _x2;
        public double X2 //координата точки линии
        {
            get { return _x2; }
            set
            {
                _x2 = value;
                OnPropertyChanged();
            }
        }
        private double _y2;
        public double Y2 //координата точки линии
        {
            get { return _y2; }
            set
            {
                _y2 = value;
                OnPropertyChanged();
            }
        }
    }
}
