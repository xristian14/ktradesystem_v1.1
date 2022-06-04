using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    class IndicatorPolylinePageTradeChart : ViewModelBase
    {
        public int IdDataSource { get; set; }
        public int IdIndicator { get; set; }
        public SolidColorBrush StrokeColor { get; set; } //цвет линии
        private double _left;
        public double Left //отступ слева
        {
            get { return _left; }
            set
            {
                _left = value;
                OnPropertyChanged();
            }
        }
        private double _top;
        public double Top //отступ сверху
        {
            get { return _top; }
            set
            {
                _top = value;
                OnPropertyChanged();
            }
        }
        private PointCollection _points;
        public PointCollection Points //точки линии
        {
            get { return _points; }
            set
            {
                _points = value;
                OnPropertyChanged();
            }
        }
        private List<double> _pointsPrices;
        public List<double> PointsPrices //цены, соответствующие точкам линии
        {
            get { return _pointsPrices; }
            set
            {
                _pointsPrices = value;
                OnPropertyChanged();
            }
        }
    }
}
