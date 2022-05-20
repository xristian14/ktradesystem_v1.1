using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTradeChart : ViewModelBase
    {
        public ViewModelPageTradeChart()
        {
            ViewModelPageTestingResult.TestRunsUpdatePages += UpdatePage;
            Candles.Clear();
            double bodyWidth = 4;
            for(int i = 0; i < 20; i++)
            {
                Candles.Add(new CandlePageTradeChart { BodyLeft = i * bodyWidth + i, BodyTop = 10, BodyHeight = 20, BodyWidth = bodyWidth, StickLeft = i * bodyWidth + i + Math.Truncate(bodyWidth / 2), StickTop = 4, StickHeight = 30, StickWidth = 1 });
            }
        }

        private ObservableCollection<CandlePageTradeChart> _candles = new ObservableCollection<CandlePageTradeChart>();
        public ObservableCollection<CandlePageTradeChart> Candles
        {
            get { return _candles; }
            set
            {
                _candles = value;
                OnPropertyChanged();
            }
        }

        public void UpdatePage() //обновляет страницу на новый источник данных
        {

        }
        public ICommand Button1_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    foreach(CandlePageTradeChart candle in Candles)
                    {
                        candle.BodyLeft -= 1;
                        candle.StickLeft -= 1;
                    }
                }, (obj) => true);
            }
        }
    }
}
