using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class NumericUpDown : ViewModelBase
    {
        public NumericUpDown(int startValue, bool isMin, int min = 0)
        {
            _value = startValue.ToString();
            _isMin = isMin;
            _min = min;
        }
        private bool _isMin;
        private int _min;
        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                if(int.TryParse(value, out int res))
                {
                    _value = res.ToString();
                }
                OnPropertyChanged();
            }
        }
        private bool _upClick = false;
        public bool UpClick
        {
            get { return _upClick; }
            set
            {
                Value = (int.Parse(Value) + 1).ToString();
                OnPropertyChanged();
            }
        }
        private bool _downClick = false;
        public bool DownClick
        {
            get { return _downClick; }
            set
            {
                int parsedValue = int.Parse(Value);
                if (_isMin)
                {
                    if(parsedValue > _min)
                    {
                        Value = (parsedValue - 1).ToString();
                    }
                }
                else
                {
                    Value = (parsedValue - 1).ToString();
                }
                OnPropertyChanged();
            }
        }
    }
}
