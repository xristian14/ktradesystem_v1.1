using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    class OrderPageTradeChart : ViewModelBase
    {
        public int IdDataSource { get; set; }
        public SolidColorBrush FillColor { get; set; } //цвет заливки
        public Order Order { get; set; }
        public bool IsStart { get; set; } //флаг того что заявка выставлена в данном сегменте
        private double _horizontalLineLeft;
        public double HorizontalLineLeft //отступ слева для горизонтальной линии
        {
            get { return _horizontalLineLeft; }
            set
            {
                _horizontalLineLeft = value;
                OnPropertyChanged();
            }
        }
        private double _horizontalLineTop;
        public double HorizontalLineTop //отступ сверху для горизонтальной линии
        {
            get { return _horizontalLineTop; }
            set
            {
                _horizontalLineTop = value;
                OnPropertyChanged();
            }
        }
        private double _horizontalLineWidth;
        public double HorizontalLineWidth //ширина горизонтальной линии
        {
            get { return _horizontalLineWidth; }
            set
            {
                _horizontalLineWidth = value;
                OnPropertyChanged();
            }
        }
        private double _horizontalLineHeight;
        public double HorizontalLineHeight //высота горизонтальной линии
        {
            get { return _horizontalLineHeight; }
            set
            {
                _horizontalLineHeight = value;
                OnPropertyChanged();
            }
        }
        private double _verticalLineLeft;
        public double VerticalLineLeft //отступ слева для вертикальной линии
        {
            get { return _verticalLineLeft; }
            set
            {
                _verticalLineLeft = value;
                OnPropertyChanged();
            }
        }
        private double _verticalLineTop;
        public double VerticalLineTop //отступ сверху для вертикальной линии
        {
            get { return _verticalLineTop; }
            set
            {
                _verticalLineTop = value;
                OnPropertyChanged();
            }
        }
        private double _verticalLineWidth;
        public double VerticalLineWidth //ширина вертикальной линии
        {
            get { return _verticalLineWidth; }
            set
            {
                _verticalLineWidth = value;
                OnPropertyChanged();
            }
        }
        private double _verticalLineHeight;
        public double VerticalLineHeight //высота вертикальной линии
        {
            get { return _verticalLineHeight; }
            set
            {
                _verticalLineHeight = value;
                OnPropertyChanged();
            }
        }
    }
}
