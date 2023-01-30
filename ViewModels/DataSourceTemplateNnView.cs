using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        private bool _isLimitPrognosisCandles;
        public bool IsLimitPrognosisCandles
        {
            get { return _isLimitPrognosisCandles; }
            set
            {
                _isLimitPrognosisCandles = value;
                if (value)
                {
                    LimitPrognosisCandlesVisibility = Visibility.Visible;
                }
                else
                {
                    LimitPrognosisCandlesVisibility = Visibility.Collapsed;
                }
                OnPropertyChanged();
            }
        }
        private Visibility _limitPrognosisCandlesVisibility = Visibility.Collapsed;
        public Visibility LimitPrognosisCandlesVisibility
        {
            get { return _limitPrognosisCandlesVisibility; }
            set
            {
                _limitPrognosisCandlesVisibility = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _limitPrognosisCandles;
        public NumericUpDown LimitPrognosisCandles
        {
            get { return _limitPrognosisCandles; }
            set
            {
                _limitPrognosisCandles = value;
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
    }
}
