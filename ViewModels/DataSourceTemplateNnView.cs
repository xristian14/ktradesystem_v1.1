using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class DataSourceTemplateNnView : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        private string _inputLayerCandleCount;
        public string InputLayerCandleCount
        {
            get { return _inputLayerCandleCount; }
            set
            {
                _inputLayerCandleCount = value;
                OnPropertyChanged();
            }
        }
        private string _lastCandleOffset;
        public string LastCandleOffset
        {
            get { return _lastCandleOffset; }
            set
            {
                _lastCandleOffset = value;
                OnPropertyChanged();
            }
        }
        private bool _isOpenCandleNeuron;
        public bool IsOpenCandleNeuron
        {
            get { return _isOpenCandleNeuron; }
            set
            {
                _isOpenCandleNeuron = value;
                OnPropertyChanged();
            }
        }
        private bool __isMaxMinCandleNeuron;
        public bool IsMaxMinCandleNeuron
        {
            get { return __isMaxMinCandleNeuron; }
            set
            {
                __isMaxMinCandleNeuron = value;
                OnPropertyChanged();
            }
        }
        private bool _isCloseCandleNeuron;
        public bool IsCloseCandleNeuron
        {
            get { return _isCloseCandleNeuron; }
            set
            {
                _isCloseCandleNeuron = value;
                OnPropertyChanged();
            }
        }
        private bool _isScaleShowingNeurons;
        public bool IsScaleShowingNeurons
        {
            get { return _isScaleShowingNeurons; }
            set
            {
                _isScaleShowingNeurons = value;
                OnPropertyChanged();
            }
        }
    }
}
