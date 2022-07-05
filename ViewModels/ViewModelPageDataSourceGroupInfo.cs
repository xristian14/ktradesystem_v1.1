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

            TimeSpan totalForwardDuration = TimeSpan.Zero; //суммарная длительность форвардных тестов
            int numberForwardTests = 0;
            double totalTopModelNet = 0; //суммарная прибыль топ-моделей
            TimeSpan totalTopModelDuration = TimeSpan.Zero; //суммарная длительность тестов топ-моделей

            double totalNet = 0;
            double totalDropdown = 0;
            int totalNumberTrades = 0;
            double totalPercentWin = 0;
            double totalAveWinDivAveLoss = 0;
            double totalAverageTrade = 0;
            double totalProfitRisk = 0;
            double totalWfe = 0;
            double totalProm = 0;

            double maxNet = 0;
            double maxDropdown = 0;
            int maxNumberTrades = 0;
            double maxPercentWin = 0;
            double maxAveWinDivAveLoss = 0;
            double maxAverageTrade = 0;
            double maxProfitRisk = 0;
            double maxWfe = 0;
            double maxProm = 0;

            double minNet = 0;
            double minDropdown = 0;
            int minNumberTrades = 0;
            double minPercentWin = 0;
            double minAveWinDivAveLoss = 0;
            double minAverageTrade = 0;
            double minProfitRisk = 0;
            double minWfe = 0;
            double minProm = 0;

            bool isFirstIteration = true;

            foreach (TestBatch testBatch in dataSourceGroupTestBatches)
            {
                if (testBatch.ForwardTestRun != null)
                {
                    totalForwardDuration = totalForwardDuration.Add(testBatch.ForwardTestRun.EndPeriod - testBatch.ForwardTestRun.StartPeriod);
                    numberForwardTests++;
                    totalTopModelNet += testBatch.TopModelTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue;
                    totalTopModelDuration = totalTopModelDuration.Add(testBatch.TopModelTestRun.EndPeriod - testBatch.TopModelTestRun.StartPeriod);

                    double currentNet = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue;
                    double currentDropdown = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 7).DoubleValue;
                    int currentNumberTrades = (int)testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 35).DoubleValue;
                    double currentPercentWin = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 14).DoubleValue;
                    double currentAveWinDivAveLoss = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 16).DoubleValue / testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 19).DoubleValue;
                    double currentAverageTrade = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 36).DoubleValue;
                    double currentProfitRisk = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 10).DoubleValue;
                    double currentWfe = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue / testBatch.TopModelTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue;
                    double currentProm = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 2).DoubleValue;

                    if (isFirstIteration)
                    {
                        isFirstIteration = false;

                        maxNet = currentNet;
                        maxDropdown = currentDropdown;
                        maxNumberTrades = currentNumberTrades;
                        maxPercentWin = currentPercentWin;
                        maxAveWinDivAveLoss = currentAveWinDivAveLoss;
                        maxAverageTrade = currentAverageTrade;
                        maxProfitRisk = currentProfitRisk;
                        maxWfe = currentWfe;
                        maxProm = currentProm;

                        minNet = currentNet;
                        minDropdown = currentDropdown;
                        minNumberTrades = currentNumberTrades;
                        minPercentWin = currentPercentWin;
                        minAveWinDivAveLoss = currentAveWinDivAveLoss;
                        minAverageTrade = currentAverageTrade;
                        minProfitRisk = currentProfitRisk;
                        minWfe = currentWfe;
                        minProm = currentProm;
                    }
                    else
                    {
                        maxNet = currentNet > maxNet ? currentNet : maxNet;
                        maxDropdown = currentDropdown > maxDropdown ? currentDropdown : maxDropdown;
                        maxNumberTrades = currentNumberTrades > maxNumberTrades ? currentNumberTrades : maxNumberTrades;
                        maxPercentWin = currentPercentWin > maxPercentWin ? currentPercentWin : maxPercentWin;
                        maxAveWinDivAveLoss = currentAveWinDivAveLoss > maxAveWinDivAveLoss ? currentAveWinDivAveLoss : maxAveWinDivAveLoss;
                        maxAverageTrade = currentAverageTrade > maxAverageTrade ? currentAverageTrade : maxAverageTrade;
                        maxProfitRisk = currentProfitRisk > maxProfitRisk ? currentProfitRisk : maxProfitRisk;
                        maxWfe = currentWfe > maxWfe ? currentWfe : maxWfe;
                        maxProm = currentProm > maxProm ? currentProm : maxProm;

                        minNet = currentNet < minNet ? currentNet : minNet;
                        minDropdown = currentDropdown < minDropdown ? currentDropdown : minDropdown;
                        minNumberTrades = currentNumberTrades < minNumberTrades ? currentNumberTrades : minNumberTrades;
                        minPercentWin = currentPercentWin < minPercentWin ? currentPercentWin : minPercentWin;
                        minAveWinDivAveLoss = currentAveWinDivAveLoss < minAveWinDivAveLoss ? currentAveWinDivAveLoss : minAveWinDivAveLoss;
                        minAverageTrade = currentAverageTrade < minAverageTrade ? currentAverageTrade : minAverageTrade;
                        minProfitRisk = currentProfitRisk < minProfitRisk ? currentProfitRisk : minProfitRisk;
                        minWfe = currentWfe < minWfe ? currentWfe : minWfe;
                        minProm = currentProm < minProm ? currentProm : minProm;
                    }
                    totalNet += currentNet;
                    totalDropdown += currentDropdown;
                    totalNumberTrades += currentNumberTrades;
                    totalPercentWin += currentPercentWin;
                    totalAveWinDivAveLoss += currentAveWinDivAveLoss;
                    totalAverageTrade += currentAverageTrade;
                    totalProfitRisk += currentProfitRisk;
                    totalWfe += currentWfe;
                    totalProm += currentProm;

                    DateTime dateTimeStart = testBatch.ForwardTestRun.StartPeriod; //начало периода форвардного теста
                    DateTime dateTimeEnd = testBatch.ForwardTestRun.EndPeriod; //окончание периода тестирования форвардного теста
                    string dateTimeStartStr = dateTimeStart.Day.ToString().Length == 1 ? "0" + dateTimeStart.Day.ToString() : dateTimeStart.Day.ToString();
                    dateTimeStartStr += dateTimeStart.Month.ToString().Length == 1 ? ".0" + dateTimeStart.Month.ToString() : "." + dateTimeStart.Month.ToString();
                    dateTimeStartStr += "." + dateTimeStart.Year.ToString();

                    string dateTimeEndStr = dateTimeEnd.Day.ToString().Length == 1 ? "0" + dateTimeEnd.Day.ToString() : dateTimeEnd.Day.ToString();
                    dateTimeEndStr += dateTimeEnd.Month.ToString().Length == 1 ? ".0" + dateTimeEnd.Month.ToString() : "." + dateTimeEnd.Month.ToString();
                    dateTimeEndStr += "." + dateTimeEnd.Year.ToString();

                    ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = dateTimeStartStr + "-" + dateTimeEndStr, NetProfitLoss = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).StringValue, MaxDropdown = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 7).StringValue, NumberTrades = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 35).StringValue, PercentWin = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 14).StringValue, AveWinDivAveLoss = ModelFunctions.SplitDigitsDouble(currentAveWinDivAveLoss, 1), AverageTrade = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 36).StringValue, ProfitRisk = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 10).StringValue, Wfe = ModelFunctions.SplitDigitsDouble(currentWfe * 100, 1) + " %", Prom = testBatch.ForwardTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 2).StringValue });
                }
            }
            //добавляем строки с общей информацией о тестах
            if(numberForwardTests > 0)
            {
                ForwardTestsInfo.Add(new ForwardTestInfo());
                ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "ОБЩЕЕ", NetProfitLoss = ModelFunctions.SplitDigitsDouble(totalNet, 0) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, NumberTrades = totalNumberTrades.ToString() });
                
                ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "Наибольшее", NetProfitLoss = ModelFunctions.SplitDigitsDouble(maxNet, 0) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, MaxDropdown = ModelFunctions.SplitDigitsDouble(maxDropdown, 1) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, NumberTrades = maxNumberTrades.ToString(), PercentWin = Math.Round(maxPercentWin, 1) + " %", AveWinDivAveLoss = ModelFunctions.SplitDigitsDouble(maxAveWinDivAveLoss, 1), AverageTrade = ModelFunctions.SplitDigitsDouble(maxAverageTrade, 1) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, ProfitRisk = Math.Round(maxProfitRisk, 1).ToString(), Wfe = ModelFunctions.SplitDigitsDouble(maxWfe * 100, 1) + " %", Prom = ModelFunctions.SplitDigitsDouble(maxProm, 1) + " %" });

                ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "Наименьшее", NetProfitLoss = ModelFunctions.SplitDigitsDouble(minNet, 0) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, MaxDropdown = ModelFunctions.SplitDigitsDouble(minDropdown, 1) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, NumberTrades = minNumberTrades.ToString(), PercentWin = Math.Round(minPercentWin, 1) + " %", AveWinDivAveLoss = ModelFunctions.SplitDigitsDouble(minAveWinDivAveLoss, 1), AverageTrade = ModelFunctions.SplitDigitsDouble(minAverageTrade, 1) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, ProfitRisk = Math.Round(minProfitRisk, 1).ToString(), Wfe = ModelFunctions.SplitDigitsDouble(minWfe * 100, 1) + " %", Prom = ModelFunctions.SplitDigitsDouble(minProm, 1) + " %" });

                ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "Среднее", NetProfitLoss = ModelFunctions.SplitDigitsDouble(totalNet / numberForwardTests, 0) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, MaxDropdown = ModelFunctions.SplitDigitsDouble(totalDropdown / numberForwardTests, 1) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, NumberTrades = Math.Round(totalNumberTrades / (double)numberForwardTests, 1).ToString(), PercentWin = Math.Round(totalPercentWin / numberForwardTests, 1) + " %", AveWinDivAveLoss = ModelFunctions.SplitDigitsDouble(totalAveWinDivAveLoss / numberForwardTests, 1), AverageTrade = ModelFunctions.SplitDigitsDouble(totalAverageTrade / numberForwardTests, 1) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name, ProfitRisk = Math.Round(totalProfitRisk / numberForwardTests, 1).ToString(), Wfe = ModelFunctions.SplitDigitsDouble(totalWfe / numberForwardTests * 100, 1) + " %", Prom = ModelFunctions.SplitDigitsDouble(totalProm / numberForwardTests, 1) + " %" });

                double forwardAnnualNet = totalNet / (totalForwardDuration.TotalDays / 365); //годовая форвардная прибыль за все форвардные тесты
                double topModelAnnualNet = totalTopModelNet / (totalTopModelDuration.TotalDays / 365); //годовая прибыль топ-моделей за все оптимизационные тесты

                ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "Годовые прибыль и убытки", NetProfitLoss = ModelFunctions.SplitDigitsDouble(forwardAnnualNet, 0) + " " + dataSourceGroupTestBatches[0].ForwardTestRun.Account.DefaultCurrency.Name });

                ForwardTestsInfo.Add(new ForwardTestInfo { TradeWindow = "Форвардная эфективность WFE", NetProfitLoss = ModelFunctions.SplitDigitsDouble(forwardAnnualNet / topModelAnnualNet * 100.0, 1) + " %" });
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
