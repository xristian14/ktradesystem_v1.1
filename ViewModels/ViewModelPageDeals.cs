using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    class ViewModelPageDeals : ViewModelBase
    {
        public ViewModelPageDeals()
        {
            ViewModelPageTestingResult.TestRunsUpdatePages += UpdatePage;
            _viewModelPageTradeChart = ViewModelPageTradeChart.getInstance();
        }
        private ViewModelPageTradeChart _viewModelPageTradeChart;
        private TestRun _testRun;

        private ObservableCollection<Deal> _deals = new ObservableCollection<Deal>();
        public ObservableCollection<Deal> Deals //сделки
        {
            get { return _deals; }
            set
            {
                _deals = value;
                OnPropertyChanged();
            }
        }
        private void CreateDeals()
        {
            Deals.Clear();
            foreach (Deal deal in _testRun.Account.AllDeals)
            {
                Deals.Add(deal);
            }
        }

        private Deal _selectedDeal;
        public Deal SelectedDeal //выбранная сделка
        {
            get { return _selectedDeal; }
            set
            {
                _selectedDeal = value;
                OnPropertyChanged();
            }
        }
        public void UpdatePage() //обновляет страницу на новый источник данных
        {
            if (ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null && ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox != null)
            {
                _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
                CreateDeals();
            }
        }
        public ICommand MoveToDeal_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _viewModelPageTradeChart.GoToDeal(Deals.IndexOf(SelectedDeal));
                }, (obj) => SelectedDeal != null);
            }
        }
    }
}
