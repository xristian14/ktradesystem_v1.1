using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class TradeChartAreaPageTradeChart : ViewModelBase
    {
        private string _name;
        public string Name //название области
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public int _areaHeight;
        public int AreaHeight //высота области
        {
            get { return _areaHeight; }
            set
            {
                _areaHeight = value;
                OnPropertyChanged();
            }
        }
    }
}
