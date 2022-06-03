using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    class CandlePageTradeChart : ViewModelBase
    {
        public int IdDataSource { get; set; }
        public SolidColorBrush StrokeColor { get; set; } //цвет обводки
        public SolidColorBrush FillColor { get; set; } //цвет заливки
        public Candle Candle { get; set; }
        private double _bodyLeft;
        public double BodyLeft //отступ слева для тела свечки
        {
            get { return _bodyLeft; }
            set
            {
                _bodyLeft = value;
                OnPropertyChanged();
            }
        }
        private double _bodyTop;
        public double BodyTop //отступ сверху для тела свечки
        {
            get { return _bodyTop; }
            set
            {
                _bodyTop = value;
                OnPropertyChanged();
            }
        }
        private double _bodyWidth;
        public double BodyWidth //ширина тела свечки
        {
            get { return _bodyWidth; }
            set
            {
                _bodyWidth = value;
                OnPropertyChanged();
            }
        }
        private double _bodyHeight;
        public double BodyHeight //высота тела свечки
        {
            get { return _bodyHeight; }
            set
            {
                _bodyHeight = value;
                OnPropertyChanged();
            }
        }
        private double _stickLeft;
        public double StickLeft //отступ слева для линии свечки
        {
            get { return _stickLeft; }
            set
            {
                _stickLeft = value;
                OnPropertyChanged();
            }
        }
        private double _stickTop;
        public double StickTop //отступ сверху для линии свечки
        {
            get { return _stickTop; }
            set
            {
                _stickTop = value;
                OnPropertyChanged();
            }
        }
        private double _stickWidth;
        public double StickWidth //ширина линии свечки
        {
            get { return _stickWidth; }
            set
            {
                _stickWidth = value;
                OnPropertyChanged();
            }
        }
        private double _stickHeight;
        public double StickHeight //ширина линии свечки
        {
            get { return _stickHeight; }
            set
            {
                _stickHeight = value;
                OnPropertyChanged();
            }
        }
        private double _volumeTop;
        public double VolumeTop //отступ сверху для Volume
        {
            get { return _volumeTop; }
            set
            {
                _volumeTop = value;
                OnPropertyChanged();
            }
        }
        private double _volumeHeigh;
        public double VolumeHeigh //высота Volume
        {
            get { return _volumeHeigh; }
            set
            {
                _volumeHeigh = value;
                OnPropertyChanged();
            }
        }
    }
}