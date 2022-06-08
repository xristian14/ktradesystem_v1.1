using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ktradesystem.ViewModels
{
    class TimeLinePageTradeChart : ViewModelBase
    {
        public DateTime DateTime { get; set; }
        public SolidColorBrush StrokeLineColor { get; set; } //цвет линии
        public SolidColorBrush TextColor { get; set; } //цвет текста
        public int FontSize { get; set; }
        private string _text;
        public string Text //дата и время
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
        private double _textLeft;
        public double TextLeft //отступ слева
        {
            get { return _textLeft; }
            set
            {
                _textLeft = value;
                OnPropertyChanged();
            }
        }
        private double _textTop;
        public double TextTop //отступ сверху
        {
            get { return _textTop; }
            set
            {
                _textTop = value;
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
