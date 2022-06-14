using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;

namespace ktradesystem.ViewModels
{
    class ViewModelPageDataSourceGroupInfo : ViewModelBase
    {
        public ViewModelPageDataSourceGroupInfo()
        {
            ViewModelPageTestingResult.TestBatchesUpdatePages += UpdatePage;
        }
        private Testing _testing; //результат тестирования
        private DataSourceGroup _dataSourceGroup;

        private ObservableCollection<ForwardTestInfo> _forwardTestsInfo = new ObservableCollection<ForwardTestInfo>();
        public ObservableCollection<ForwardTestInfo> ForwardTestsInfo //информация о форвардных тестах
        {
            get { return _forwardTestsInfo; }
            private set
            {
                _forwardTestsInfo = value;
                OnPropertyChanged();
            }
        }
        private void CreateForwardTestsInfo() //создает информацию о форвардных тестах
        {
            ForwardTestsInfo.Clear();
            ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "20.02.2020-20.08.2020", NetProfitLoss = "20 000", MaxDropdown = "5000", NumberTrades = "352", PercentWin = "55.20", AveWinDivAveLoss = "2.03", AverageTrade = "262", ProfitRisk = "11.23", Wfe = "80.22%", Prom = "52.03%" });
        }

        public void UpdatePage()
        {
            if (ViewModelPageTestingResult.getInstance().SelectedDataSourceGroupTestingResultCombobox != null)
            {
                _testing = ViewModelPageTestingResult.getInstance().TestingResult;
                _dataSourceGroup = ViewModelPageTestingResult.getInstance().SelectedDataSourceGroupTestingResultCombobox.DataSourceGroup;
                CreateForwardTestsInfo(); //создаем информацию о форвардных тестах
            }
        }
    }
}
