using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ktradesystem.Models;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTestBatchInfo : ViewModelBase
    {
        public ViewModelPageTestBatchInfo()
        {
            ViewModelPageTestingResult.TestBatchesUpdatePages += UpdatePage;
        }
        private Testing _testing; //результат тестирования
        private TestBatch _testBatch; //тестовая связка

        private string _totalCount;
        public string TotalCount
        {
            get { return _totalCount; }
            private set
            {
                _totalCount = value;
                OnPropertyChanged();
            }
        }
        private string _totalCountPercent;
        public string TotalCountPercent
        {
            get { return _totalCountPercent; }
            private set
            {
                _totalCountPercent = value;
                OnPropertyChanged();
            }
        }
        private string _totalNet;
        public string TotalNet
        {
            get { return _totalNet; }
            private set
            {
                _totalNet = value;
                OnPropertyChanged();
            }
        }
        private string _profitCount;
        public string ProfitCount
        {
            get { return _profitCount; }
            private set
            {
                _profitCount = value;
                OnPropertyChanged();
            }
        }
        private string _profitCountPercent;
        public string ProfitCountPercent
        {
            get { return _profitCountPercent; }
            private set
            {
                _profitCountPercent = value;
                OnPropertyChanged();
            }
        }
        private string _profitNet;
        public string ProfitNet
        {
            get { return _profitNet; }
            private set
            {
                _profitNet = value;
                OnPropertyChanged();
            }
        }
        private string _lossCount;
        public string LossCount
        {
            get { return _lossCount; }
            private set
            {
                _lossCount = value;
                OnPropertyChanged();
            }
        }
        private string _lossCountPercent;
        public string LossCountPercent
        {
            get { return _lossCountPercent; }
            private set
            {
                _lossCountPercent = value;
                OnPropertyChanged();
            }
        }
        private string _lossNet;
        public string LossNet
        {
            get { return _lossNet; }
            private set
            {
                _lossNet = value;
                OnPropertyChanged();
            }
        }

        private void CreateStatisticalSignificance() //обновляет статистическую значимость
        {
            int totalCount = (int)(_testBatch.StatisticalSignificance[0] + _testBatch.StatisticalSignificance[2]); //количество прибыльных плюс количество убыточных
            double totalNet = _testBatch.StatisticalSignificance[1] + _testBatch.StatisticalSignificance[3]; //прибыль прибыльных плюс убыток убыточных
            int profitCount = (int)_testBatch.StatisticalSignificance[0];
            double profitCountPercent = Math.Round((double)profitCount / totalCount * 100.0, 1);
            double profitNet = _testBatch.StatisticalSignificance[1];
            int lossCount = (int)_testBatch.StatisticalSignificance[2];
            double lossCountPercent = Math.Round((double)lossCount / totalCount * 100.0, 1);
            double lossNet = _testBatch.StatisticalSignificance[3];

            TotalCount = totalCount.ToString();
            TotalCountPercent = "100,0 %";
            TotalNet = ModelFunctions.SplitDigitsDouble(Math.Round(totalNet)).ToString() + " " + _testing.DefaultCurrency.Name;

            ProfitCount = profitCount.ToString();
            ProfitCountPercent = profitCountPercent.ToString();
            if(ProfitCountPercent.Contains(",") == false)
            {
                ProfitCountPercent += ",0";
            }
            ProfitCountPercent += " %";
            ProfitNet = ModelFunctions.SplitDigitsDouble(Math.Round(profitNet)).ToString() + " " + _testing.DefaultCurrency.Name;

            LossCount = lossCount.ToString();
            LossCountPercent = lossCountPercent.ToString();
            if (LossCountPercent.Contains(",") == false)
            {
                LossCountPercent += ",0";
            }
            LossCountPercent += " %";
            LossNet = ModelFunctions.SplitDigitsDouble(Math.Round(lossNet)).ToString() + " " + _testing.DefaultCurrency.Name;
        }

        public void UpdatePage()
        {
            if (ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null)
            {
                _testing = ViewModelPageTestingResult.getInstance().TestingResult;
                _testBatch = ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox.TestBatch;
                CreateStatisticalSignificance(); //обновляем статистическую значимость
            }
        }
    }
}
