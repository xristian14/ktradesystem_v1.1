using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ktradesystem.ViewModels
{
    class NumericUpDown : ViewModelBase
    {
        public NumericUpDown(int startValue, bool isNegative)
        {
            _value = startValue.ToString();
            _isNegative = isNegative;
        }
        private bool _isNegative;
        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                if(int.TryParse(value, out int res))
                {
                    _value = value;
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
                if(parsedValue > 0)
                {
                    Value = (parsedValue - 1).ToString();
                }
                else if (_isNegative)
                {
                    Value = (parsedValue - 1).ToString();
                }
                OnPropertyChanged();
            }
        }
    }
}
