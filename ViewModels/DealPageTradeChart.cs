using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ktradesystem.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace ktradesystem.ViewModels
{
    class DealPageTradeChart : ViewModelBase
    {
        public int IdDataSource { get; set; }
        public SolidColorBrush StrokeColor { get; set; } //цвет линии
        public SolidColorBrush FillColor { get; set; } //цвет заливки
        public Deal Deal { get; set; }
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
        private int _triangleWidth;
        public int TriangleWidth //ширина треугольника
        {
            get { return _triangleWidth; }
            set
            {
                _triangleWidth = value;
                OnPropertyChanged();
            }
        }
        private int _triangleHeight;
        public int TriangleHeight //высота треугольника
        {
            get { return _triangleHeight; }
            set
            {
                _triangleHeight = value;
                OnPropertyChanged();
            }
        }
        private PointCollection _points = new PointCollection();
        public PointCollection Points //точки по которым будет строиться треугольник
        {
            get { return _points; }
            set
            {
                _points = value;
                OnPropertyChanged();
            }
        }/*
        private ObservableCollection<Point> _points = new ObservableCollection<Point>();
        public ObservableCollection<Point> Points //точки по которым будет строиться треугольник
        {
            get { return _points; }
            set
            {
                _points = value;
                OnPropertyChanged();
            }
        }*/
    }
}
