using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;
using System.Windows;

namespace ktradesystem.ViewModels
{
    class ViewModelPageDataSourceGroupInfo : ViewModelBase
    {
        public ViewModelPageDataSourceGroupInfo()
        {
            ViewModelPageTestingResult.DataSourceGroupsUpdatePages += UpdatePage;
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

            string currencyName = dataSourceGroupTestBatches[0].OptimizationTestRuns[0].Account.DefaultCurrency.Name;
            TimeSpan totalForwardDuration = TimeSpan.Zero; //суммарная длительность форвардных тестов
            int numberForwardTests = 0;
            TimeSpan totalTopModelDuration = TimeSpan.Zero; //суммарная длительность тестов топ-моделей

            double totalNet = 0;
            double totalTopModelNet = 0;
            
            double totalNetOnMargin = 0;
            double totalAnnualNetOnMargin = 0;
            double totalTopModelAnnualNetOnMargin = 0;
            double totalPromMinusBiggestWinSeries = 0;
            double totalMaxDropdownPercent = 0;
            int totalTradesNumber = 0;
            double totalWinPercent = 0;
            double totalAveWinDivAveLoss = 0;
            double totalAverageTrade = 0;
            double totalProfitRisk = 0;
            double totalWfe = 0;

            double maxNetOnMargin = 0;
            double maxAnnualNetOnMargin = 0;
            double maxTopModelAnnualNetOnMargin = 0;
            double maxPromMinusBiggestWinSeries = 0;
            double maxMaxDropdownPercent = 0;
            int maxTradesNumber = 0;
            double maxWinPercent = 0;
            double maxAveWinDivAveLoss = 0;
            double maxAverageTrade = 0;
            double maxProfitRisk = 0;
            double maxWfe = 0;

            double minNetOnMargin = 0;
            double minAnnualNetOnMargin = 0;
            double minTopModelAnnualNetOnMargin = 0;
            double minPromMinusBiggestWinSeries = 0;
            double minMaxDropdownPercent = 0;
            int minTradesNumber = 0;
            double minWinPercent = 0;
            double minAveWinDivAveLoss = 0;
            double minAverageTrade = 0;
            double minProfitRisk = 0;
            double minWfe = 0;

            bool isFirstIteration = true;

            foreach (TestBatch testBatch in dataSourceGroupTestBatches)
            {
                if (testBatch.ForwardTestRun != null)
                {
                    totalForwardDuration = totalForwardDuration.Add(testBatch.ForwardTestRun.EndPeriod - testBatch.ForwardTestRun.StartPeriod);
                    numberForwardTests++;
                    totalTopModelDuration = totalTopModelDuration.Add(testBatch.TopModelTestRun.EndPeriod - testBatch.TopModelTestRun.StartPeriod);

                    double currentNet = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue;
                    double currentTopModelNet = testBatch.TopModelTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue;

                    double currentNetOnMargin = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 34).DoubleValue;
                    double currentAnnualNetOnMargin = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 6).DoubleValue;
                    double currentTopModelAnnualNetOnMargin = testBatch.TopModelTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 6).DoubleValue;
                    double currentPromMinusBiggestWinSeries = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 4).DoubleValue;
                    double currentMaxDropdownPercent = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 8).DoubleValue;
                    int currentTradesNumber = (int)testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 35).DoubleValue;
                    double currentWinPercent = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 14).DoubleValue;
                    double currentAveWinDivAveLoss = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 16).DoubleValue / testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 19).DoubleValue;
                    double currentAverageTrade = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 36).DoubleValue;
                    double currentProfitRisk = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 10).DoubleValue;
                    double currentWfe = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 6).DoubleValue / testBatch.TopModelTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 6).DoubleValue * 100;

                    if (isFirstIteration)
                    {
                        isFirstIteration = false;

                        maxNetOnMargin = currentNetOnMargin;
                        maxAnnualNetOnMargin = currentAnnualNetOnMargin;
                        maxTopModelAnnualNetOnMargin = currentTopModelAnnualNetOnMargin;
                        maxPromMinusBiggestWinSeries = currentPromMinusBiggestWinSeries;
                        maxMaxDropdownPercent = currentMaxDropdownPercent;
                        maxTradesNumber = currentTradesNumber;
                        maxWinPercent = currentWinPercent;
                        maxAveWinDivAveLoss = currentAveWinDivAveLoss;
                        maxAverageTrade = currentAverageTrade;
                        maxProfitRisk = currentProfitRisk;
                        maxWfe = currentWfe;

                        minNetOnMargin = currentNetOnMargin;
                        minAnnualNetOnMargin = currentAnnualNetOnMargin;
                        minTopModelAnnualNetOnMargin = currentTopModelAnnualNetOnMargin;
                        minPromMinusBiggestWinSeries = currentPromMinusBiggestWinSeries;
                        minMaxDropdownPercent = currentMaxDropdownPercent;
                        minTradesNumber = currentTradesNumber;
                        minWinPercent = currentWinPercent;
                        minAveWinDivAveLoss = currentAveWinDivAveLoss;
                        minAverageTrade = currentAverageTrade;
                        minProfitRisk = currentProfitRisk;
                        minWfe = currentWfe;
                    }
                    else
                    {
                        maxNetOnMargin = currentNetOnMargin > maxNetOnMargin ? currentNetOnMargin : maxNetOnMargin;
                        maxAnnualNetOnMargin = currentAnnualNetOnMargin > maxAnnualNetOnMargin ? currentAnnualNetOnMargin : maxAnnualNetOnMargin;
                        maxTopModelAnnualNetOnMargin = currentTopModelAnnualNetOnMargin > maxTopModelAnnualNetOnMargin ? currentTopModelAnnualNetOnMargin : maxTopModelAnnualNetOnMargin;
                        maxPromMinusBiggestWinSeries = currentPromMinusBiggestWinSeries > maxPromMinusBiggestWinSeries ? currentPromMinusBiggestWinSeries : maxPromMinusBiggestWinSeries;
                        maxMaxDropdownPercent = currentMaxDropdownPercent > maxMaxDropdownPercent ? currentMaxDropdownPercent : maxMaxDropdownPercent;
                        maxTradesNumber = currentTradesNumber > maxTradesNumber ? currentTradesNumber : maxTradesNumber;
                        maxWinPercent = currentWinPercent > maxWinPercent ? currentWinPercent : maxWinPercent;
                        maxAveWinDivAveLoss = currentAveWinDivAveLoss > maxAveWinDivAveLoss ? currentAveWinDivAveLoss : maxAveWinDivAveLoss;
                        maxAverageTrade = currentAverageTrade > maxAverageTrade ? currentAverageTrade : maxAverageTrade;
                        maxProfitRisk = currentProfitRisk > maxProfitRisk ? currentProfitRisk : maxProfitRisk;
                        maxWfe = currentWfe > maxWfe ? currentWfe : maxWfe;

                        minNetOnMargin = currentNetOnMargin < minNetOnMargin ? currentNetOnMargin : minNetOnMargin;
                        minAnnualNetOnMargin = currentAnnualNetOnMargin < minAnnualNetOnMargin ? currentAnnualNetOnMargin : minAnnualNetOnMargin;
                        minTopModelAnnualNetOnMargin = currentTopModelAnnualNetOnMargin < minTopModelAnnualNetOnMargin ? currentTopModelAnnualNetOnMargin : minTopModelAnnualNetOnMargin;
                        minPromMinusBiggestWinSeries = currentPromMinusBiggestWinSeries < minPromMinusBiggestWinSeries ? currentPromMinusBiggestWinSeries : minPromMinusBiggestWinSeries;
                        minMaxDropdownPercent = currentMaxDropdownPercent < minMaxDropdownPercent ? currentMaxDropdownPercent : minMaxDropdownPercent;
                        minTradesNumber = currentTradesNumber < minTradesNumber ? currentTradesNumber : minTradesNumber;
                        minWinPercent = currentWinPercent < minWinPercent ? currentWinPercent : minWinPercent;
                        minAveWinDivAveLoss = currentAveWinDivAveLoss < minAveWinDivAveLoss ? currentAveWinDivAveLoss : minAveWinDivAveLoss;
                        minAverageTrade = currentAverageTrade < minAverageTrade ? currentAverageTrade : minAverageTrade;
                        minProfitRisk = currentProfitRisk < minProfitRisk ? currentProfitRisk : minProfitRisk;
                        minWfe = currentWfe < minWfe ? currentWfe : minWfe;
                    }
                    totalNet += currentNet;
                    totalTopModelNet += currentTopModelNet;

                    totalNetOnMargin += currentNetOnMargin;
                    totalAnnualNetOnMargin += currentAnnualNetOnMargin;
                    totalTopModelAnnualNetOnMargin += currentTopModelAnnualNetOnMargin;
                    totalPromMinusBiggestWinSeries += currentPromMinusBiggestWinSeries;
                    totalMaxDropdownPercent += currentMaxDropdownPercent;
                    totalTradesNumber += currentTradesNumber;
                    totalWinPercent += currentWinPercent;
                    totalAveWinDivAveLoss += currentAveWinDivAveLoss;
                    totalAverageTrade += currentAverageTrade;
                    totalProfitRisk += currentProfitRisk;
                    totalWfe += currentWfe;

                    DateTime dateTimeStart = testBatch.ForwardTestRun.StartPeriod; //начало периода форвардного теста
                    DateTime dateTimeEnd = testBatch.ForwardTestRun.EndPeriod; //окончание периода тестирования форвардного теста
                    string dateTimeStartStr = dateTimeStart.Day.ToString().Length == 1 ? "0" + dateTimeStart.Day.ToString() : dateTimeStart.Day.ToString();
                    dateTimeStartStr += dateTimeStart.Month.ToString().Length == 1 ? ".0" + dateTimeStart.Month.ToString() : "." + dateTimeStart.Month.ToString();
                    dateTimeStartStr += "." + dateTimeStart.Year.ToString();

                    string dateTimeEndStr = dateTimeEnd.Day.ToString().Length == 1 ? "0" + dateTimeEnd.Day.ToString() : dateTimeEnd.Day.ToString();
                    dateTimeEndStr += dateTimeEnd.Month.ToString().Length == 1 ? ".0" + dateTimeEnd.Month.ToString() : "." + dateTimeEnd.Month.ToString();
                    dateTimeEndStr += "." + dateTimeEnd.Year.ToString();

                    ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = dateTimeStartStr + "-" + dateTimeEndStr, NetOnMargin = ModelFunctions.SplitDigitsDouble(currentNetOnMargin, 1, " ") + " %", AnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(currentAnnualNetOnMargin, 1, " ") + " %", TopModelAnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(currentTopModelAnnualNetOnMargin, 1, " ") + " %", PromMinusBiggestWinSeries = ModelFunctions.SplitDigitsDouble(currentPromMinusBiggestWinSeries, 1, " ") + " %", MaxDropdownPercent = ModelFunctions.SplitDigitsDouble(currentMaxDropdownPercent, 1) + " %", TradesNumber = currentTradesNumber.ToString(), WinPercent = ModelFunctions.SplitDigitsDouble(currentWinPercent, 1), AveWinDivAveLoss = ModelFunctions.SplitDigitsDouble(currentAveWinDivAveLoss, 1), AverageTrade = ModelFunctions.SplitDigitsDouble(currentAverageTrade, 2, " ") + " " + currencyName, ProfitRisk = ModelFunctions.SplitDigitsDouble(currentProfitRisk, 2, " "), Wfe = ModelFunctions.SplitDigitsDouble(currentWfe, 1, " ") + " %" });
                }
            }
            //добавляем строки с общей информацией о тестах
            if(numberForwardTests > 0)
            {
                ForwardTestsInfo.Add(new ForwardTestInfo());

                ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "СРЕДНЕЕ", NetOnMargin = ModelFunctions.SplitDigitsDouble(totalNetOnMargin / numberForwardTests, 1, " ") + " %", AnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(totalAnnualNetOnMargin / numberForwardTests, 1, " ") + " %", TopModelAnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(totalTopModelAnnualNetOnMargin / numberForwardTests, 1, " ") + " %", PromMinusBiggestWinSeries = ModelFunctions.SplitDigitsDouble(totalPromMinusBiggestWinSeries / numberForwardTests, 1, " ") + " %", MaxDropdownPercent = ModelFunctions.SplitDigitsDouble(totalMaxDropdownPercent / numberForwardTests, 1) + " %", TradesNumber = ModelFunctions.SplitDigitsDouble(totalTradesNumber / numberForwardTests, 1), WinPercent = ModelFunctions.SplitDigitsDouble(totalWinPercent / numberForwardTests, 1), AveWinDivAveLoss = ModelFunctions.SplitDigitsDouble(totalAveWinDivAveLoss / numberForwardTests, 1), AverageTrade = ModelFunctions.SplitDigitsDouble(totalAverageTrade / numberForwardTests, 2, " ") + " " + currencyName, ProfitRisk = ModelFunctions.SplitDigitsDouble(totalProfitRisk / numberForwardTests, 2, " "), Wfe = ModelFunctions.SplitDigitsDouble(totalWfe / numberForwardTests, 1, " ") + " %" });

                ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "НАИБОЛЬШЕЕ", NetOnMargin = ModelFunctions.SplitDigitsDouble(maxNetOnMargin, 1, " ") + " %", AnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(maxAnnualNetOnMargin, 1, " ") + " %", TopModelAnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(maxTopModelAnnualNetOnMargin, 1, " ") + " %", PromMinusBiggestWinSeries = ModelFunctions.SplitDigitsDouble(maxPromMinusBiggestWinSeries, 1, " ") + " %", MaxDropdownPercent = ModelFunctions.SplitDigitsDouble(maxMaxDropdownPercent, 1) + " %", TradesNumber = maxTradesNumber.ToString(), WinPercent = ModelFunctions.SplitDigitsDouble(maxWinPercent, 1), AveWinDivAveLoss = ModelFunctions.SplitDigitsDouble(maxAveWinDivAveLoss, 1), AverageTrade = ModelFunctions.SplitDigitsDouble(maxAverageTrade, 2, " ") + " " + currencyName, ProfitRisk = ModelFunctions.SplitDigitsDouble(maxProfitRisk, 2, " "), Wfe = ModelFunctions.SplitDigitsDouble(maxWfe, 1, " ") + " %" });

                ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "НАИМЕНЬШЕЕ", NetOnMargin = ModelFunctions.SplitDigitsDouble(minNetOnMargin, 1, " ") + " %", AnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(minAnnualNetOnMargin, 1, " ") + " %", TopModelAnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(minTopModelAnnualNetOnMargin, 1, " ") + " %", PromMinusBiggestWinSeries = ModelFunctions.SplitDigitsDouble(minPromMinusBiggestWinSeries, 2, " ") + " %", MaxDropdownPercent = ModelFunctions.SplitDigitsDouble(minMaxDropdownPercent, 1) + " %", TradesNumber = minTradesNumber.ToString(), WinPercent = ModelFunctions.SplitDigitsDouble(minWinPercent, 1), AveWinDivAveLoss = ModelFunctions.SplitDigitsDouble(minAveWinDivAveLoss, 1), AverageTrade = ModelFunctions.SplitDigitsDouble(minAverageTrade, 2, " ") + " " + currencyName, ProfitRisk = ModelFunctions.SplitDigitsDouble(minProfitRisk, 2, " "), Wfe = ModelFunctions.SplitDigitsDouble(minWfe, 1, " ") + " %" });
            }
            
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
