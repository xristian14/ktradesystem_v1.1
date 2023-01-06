using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private NumericUpDown _inputLayerCandleCount;
        public NumericUpDown InputLayerCandleCount
        {
            get { return _inputLayerCandleCount; }
            set
            {
                _inputLayerCandleCount = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _lastCandleOffset;
        public NumericUpDown LastCandleOffset
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
        private bool _isMaxMinCandleNeuron;
        public bool IsMaxMinCandleNeuron
        {
            get { return _isMaxMinCandleNeuron; }
            set
            {
                _isMaxMinCandleNeuron = value;
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
        private bool _isVolumeCandleNeuron;
        public bool IsVolumeCandleNeuron
        {
            get { return _isVolumeCandleNeuron; }
            set
            {
                _isVolumeCandleNeuron = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ScalerView> _scalers = new ObservableCollection<ScalerView>();
        public ObservableCollection<ScalerView> Scalers
        {
            get { return _scalers; }
            set
            {
                _scalers = value;
                OnPropertyChanged();
            }
        }
        private ScalerView _selectedScaler;
        public ScalerView SelectedScaler
        {
            get { return _selectedScaler; }
            set
            {
                _selectedScaler = value;
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
