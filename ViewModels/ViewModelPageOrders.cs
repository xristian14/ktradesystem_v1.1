using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class ViewModelPageOrders : ViewModelBase
    {
        public ViewModelPageOrders()
        {
            ViewModelPageTestingResult.TestRunsUpdatePages += UpdatePage;
            _viewModelPageTradeChart = ViewModelPageTradeChart.getInstance();
        }
        private ViewModelPageTradeChart _viewModelPageTradeChart;
        private TestRun _testRun;

        private ObservableCollection<Order> _orders = new ObservableCollection<Order>();
        public ObservableCollection<Order> Orders //заявки
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged();
            }
        }
        private void CreateOrders()
        {
            Orders.Clear();
            foreach (Order order in _testRun.Account.AllOrders)
            {
                Orders.Add(order);
            }
        }

        private Order _selectedOrder;
        public Order SelectedOrder //выбранная заявка
        {
            get { return _selectedOrder; }
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
            }
        }
        public void UpdatePage() //обновляет страницу на новый источник данных
        {
            if (ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null && ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox != null)
            {
                _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
                CreateOrders();
            }
        }
        public ICommand MoveToOrder_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    _viewModelPageTradeChart.GoToOrder(Orders.IndexOf(SelectedOrder));
                }, (obj) => SelectedOrder != null);
            }
        }
    }
}
