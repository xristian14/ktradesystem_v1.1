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
            List<TestBatch> dataSourceGroupTestBatches = new List<TestBatch>(); //список с TestBatch с текущей группой источников данных
            foreach (TestBatch testBatch in _testing.TestBatches){
                bool isAllContains = true;
                foreach(DataSourceAccordance dataSourceAccordance in testBatch.DataSourceGroup.DataSourceAccordances)
                {
                    if(_dataSourceGroup.DataSourceAccordances.Where(a=>a.DataSource.Id == dataSourceAccordance.DataSource.Id).Any() == false) //если текущий источник данных не найден в группе источников данных
                    {
                        isAllContains = false;
                    }
                }
                if (isAllContains)
                {
                    dataSourceGroupTestBatches.Add(testBatch);
                }
            }
            for(int i = 0; i < 100; i++)
            {
                foreach (TestBatch testBatch in dataSourceGroupTestBatches)
                {
                    if (testBatch.ForwardTestRun != null)
                    {
                        DateTime dateTimeStart = testBatch.ForwardTestRun.StartPeriod; //начало периода форвардного теста
                        DateTime dateTimeEnd = testBatch.ForwardTestRun.EndPeriod; //окончание периода тестирования форвардного теста
                        string dateTimeStartStr = dateTimeStart.Day.ToString().Length == 1 ? "0" + dateTimeStart.Day.ToString() : dateTimeStart.Day.ToString();
                        dateTimeStartStr += dateTimeStart.Month.ToString().Length == 1 ? ".0" + dateTimeStart.Month.ToString() : "." + dateTimeStart.Month.ToString();
                        dateTimeStartStr += "." + dateTimeStart.Year.ToString();

                        string dateTimeEndStr = dateTimeEnd.Day.ToString().Length == 1 ? "0" + dateTimeEnd.Day.ToString() : dateTimeEnd.Day.ToString();
                        dateTimeEndStr += dateTimeEnd.Month.ToString().Length == 1 ? ".0" + dateTimeEnd.Month.ToString() : "." + dateTimeEnd.Month.ToString();
                        dateTimeEndStr += "." + dateTimeEnd.Year.ToString();

                        string aveWinDivAveLoss = Math.Round(testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 16).DoubleValue / testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 19).DoubleValue, 2).ToString();
                        string wfe = Math.Round(testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue / testBatch.TopModelTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue * 100, 2).ToString() + " %";

                        ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = dateTimeStartStr + "-" + dateTimeEndStr, NetProfitLoss = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).StringValue, MaxDropdown = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 7).StringValue, NumberTrades = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 35).StringValue, PercentWin = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 14).StringValue, AveWinDivAveLoss = aveWinDivAveLoss, AverageTrade = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 36).StringValue, ProfitRisk = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 10).StringValue, Wfe = wfe, Prom = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 2).StringValue });
                    }

                }
            }
            
            /*
            DateTime dateTimeStart = testBatch.OptimizationTestRuns[0].StartPeriod; //начало периода тестирования в тестовой связке
                        DateTime dateTimeEnd = testBatch.ForwardTestRun == null ? testBatch.OptimizationTestRuns[0].EndPeriod : testBatch.ForwardTestRun.EndPeriod; //окончание периода тестирования в тестовой связке (если есть форвардное тестирование, то окончание возьмется из него)
                        string dateTimeStartStr = dateTimeStart.Day.ToString().Length == 1 ? "0" + dateTimeStart.Day.ToString() : dateTimeStart.Day.ToString();
                        dateTimeStartStr += dateTimeStart.Month.ToString().Length == 1 ? ".0" + dateTimeStart.Month.ToString() : "." + dateTimeStart.Month.ToString();
                        dateTimeStartStr += "." + dateTimeStart.Year.ToString();

                        string dateTimeEndStr = dateTimeEnd.Day.ToString().Length == 1 ? "0" + dateTimeEnd.Day.ToString() : dateTimeEnd.Day.ToString();
                        dateTimeEndStr += dateTimeEnd.Month.ToString().Length == 1 ? ".0" + dateTimeEnd.Month.ToString() : "." + dateTimeEnd.Month.ToString();
                        dateTimeEndStr += "." + dateTimeEnd.Year.ToString();
             */
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
