using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class NnSettingsView : ViewModelBase
    {
        private int _number;
        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningOffsetY;
        public NumericUpDown LearningOffsetY
        {
            get { return _learningOffsetY; }
            set
            {
                _learningOffsetY = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningOffsetM;
        public NumericUpDown LearningOffsetM
        {
            get { return _learningOffsetM; }
            set
            {
                _learningOffsetM = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningOffsetD;
        public NumericUpDown LearningOffsetD
        {
            get { return _learningOffsetD; }
            set
            {
                _learningOffsetD = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningPeriodsCount;
        public NumericUpDown LearningPeriodsCount
        {
            get { return _learningPeriodsCount; }
            set
            {
                _learningPeriodsCount = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningDurationY;
        public NumericUpDown LearningDurationY
        {
            get { return _learningDurationY; }
            set
            {
                _learningDurationY = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningDurationM;
        public NumericUpDown LearningDurationM
        {
            get { return _learningDurationM; }
            set
            {
                _learningDurationM = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningDurationD;
        public NumericUpDown LearningDurationD
        {
            get { return _learningDurationD; }
            set
            {
                _learningDurationD = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningDistanceY;
        public NumericUpDown LearningDistanceY
        {
            get { return _learningDistanceY; }
            set
            {
                _learningDistanceY = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningDistanceM;
        public NumericUpDown LearningDistanceM
        {
            get { return _learningDistanceM; }
            set
            {
                _learningDistanceM = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _learningDistanceD;
        public NumericUpDown LearningDistanceD
        {
            get { return _learningDistanceD; }
            set
            {
                _learningDistanceD = value;
                OnPropertyChanged();
            }
        }
        private bool _isForwardTesting;
        public bool IsForwardTesting
        {
            get { return _isForwardTesting; }
            set
            {
                _isForwardTesting = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardOffsetY;
        public NumericUpDown ForwardOffsetY
        {
            get { return _forwardOffsetY; }
            set
            {
                _forwardOffsetY = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardOffsetM;
        public NumericUpDown ForwardOffsetM
        {
            get { return _forwardOffsetM; }
            set
            {
                _forwardOffsetM = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardOffsetD;
        public NumericUpDown ForwardOffsetD
        {
            get { return _forwardOffsetD; }
            set
            {
                _forwardOffsetD = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardPeriodsCount;
        public NumericUpDown ForwardPeriodsCount
        {
            get { return _forwardPeriodsCount; }
            set
            {
                _forwardPeriodsCount = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardDurationY;
        public NumericUpDown ForwardDurationY
        {
            get { return _forwardDurationY; }
            set
            {
                _forwardDurationY = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardDurationM;
        public NumericUpDown ForwardDurationM
        {
            get { return _forwardDurationM; }
            set
            {
                _forwardDurationM = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardDurationD;
        public NumericUpDown ForwardDurationD
        {
            get { return _forwardDurationD; }
            set
            {
                _forwardDurationD = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardDistanceY;
        public NumericUpDown ForwardDistanceY
        {
            get { return _forwardDistanceY; }
            set
            {
                _forwardDistanceY = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardDistanceM;
        public NumericUpDown ForwardDistanceM
        {
            get { return _forwardDistanceM; }
            set
            {
                _forwardDistanceM = value;
                OnPropertyChanged();
            }
        }
        private NumericUpDown _forwardDistanceD;
        public NumericUpDown ForwardDistanceD
        {
            get { return _forwardDistanceD; }
            set
            {
                _forwardDistanceD = value;
                OnPropertyChanged();
            }
        }
    }
}
