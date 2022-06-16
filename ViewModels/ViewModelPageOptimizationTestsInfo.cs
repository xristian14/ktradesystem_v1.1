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
    class ViewModelPageOptimizationTestsInfo : ViewModelBase
    {
        public ViewModelPageOptimizationTestsInfo()
        {
            ViewModelPageTestingResult.TestingResultUpdatePages += UpdatePage;
        }
        private Testing _testing; //результат тестирования

        private ObservableCollection<OptimizationTestsInfo> _optimizationsTestsInfo = new ObservableCollection<OptimizationTestsInfo>();
        public ObservableCollection<OptimizationTestsInfo> OptimizationsTestsInfo //информация об оптимизационных тестах
        {
            get { return _optimizationsTestsInfo; }
            private set
            {
                _optimizationsTestsInfo = value;
                OnPropertyChanged();
            }
        }

        private void CreateOptimizationTestsInfo() //создает информацию об оптимизационных тестах
        {
            OptimizationsTestsInfo.Clear();
            //проходим по всем группам источников данных
            bool isFirstIteration = true;
            foreach(DataSourceGroup dataSourceGroup in _testing.DataSourceGroups)
            {
                List<TestBatch> dataSourceGroupTestBatches = new List<TestBatch>(); //список с TestBatch с текущей группой источников данных
                foreach (TestBatch testBatch in _testing.TestBatches)
                {
                    bool isAllContains = true;
                    foreach (DataSourceAccordance dataSourceAccordance in testBatch.DataSourceGroup.DataSourceAccordances)
                    {
                        if (dataSourceGroup.DataSourceAccordances.Where(a => a.DataSource.Id == dataSourceAccordance.DataSource.Id).Any() == false) //если текущий источник данных не найден в группе источников данных
                        {
                            isAllContains = false;
                        }
                    }
                    if (isAllContains)
                    {
                        dataSourceGroupTestBatches.Add(testBatch);
                    }
                }

                if(dataSourceGroupTestBatches.Count > 0)
                {
                    if (isFirstIteration == false) //для всех кроме первой группы источников данных вставляем пустую строку разделитель
                    {
                        OptimizationsTestsInfo.Add(new OptimizationTestsInfo());
                    }
                    isFirstIteration = false;

                    //добавляем строку с названием группы источников данных
                    string nameDataSourceGroup = "";
                    foreach(DataSourceAccordance dataSourceAccordance in dataSourceGroup.DataSourceAccordances)
                    {
                        nameDataSourceGroup += dataSourceAccordance.DataSource.Name + ", ";
                    }
                    nameDataSourceGroup = nameDataSourceGroup.Substring(0, nameDataSourceGroup.Length - 2);

                    OptimizationsTestsInfo.Add(new OptimizationTestsInfo { TradeWindow = nameDataSourceGroup });

                    string currencyName = dataSourceGroupTestBatches[0].OptimizationTestRuns[0].Account.DefaultCurrency.Name;

                    int numberTestBatches = 0;
                    double totalNet = 0;
                    double totalDropdown = 0;
                    double totalNumberTrades = 0;
                    double totalPercentWin = 0;
                    double totalNetTopModel = 0;
                    bool isTopModelFind = false;

                    //проходим по всем TestBatch текущей групы источников данных
                    foreach (TestBatch testBatch in dataSourceGroupTestBatches)
                    {
                        numberTestBatches++;

                        double totalTestRunNet = 0;
                        double totalTestRunDropdown = 0;
                        int totalTestRunNumberTrades = 0;
                        double totalTestRunPercentWin = 0;
                        //проходим по всем тестовым прогонам и определяем показатели
                        foreach (TestRun testRun in testBatch.OptimizationTestRuns)
                        {
                            totalTestRunNet += testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue;
                            totalTestRunDropdown += testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 7).DoubleValue;
                            totalTestRunNumberTrades += (int)testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 35).DoubleValue;
                            totalTestRunPercentWin += testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 14).DoubleValue;
                        }

                        double currentNet = totalTestRunNet / testBatch.OptimizationTestRuns.Count;
                        double currentDropdown = totalTestRunDropdown / testBatch.OptimizationTestRuns.Count;
                        double currentNumberTrades = (double)totalTestRunNumberTrades / testBatch.OptimizationTestRuns.Count;
                        double currentPercentWin = totalTestRunPercentWin / testBatch.OptimizationTestRuns.Count;
                        double currentNetTopModel = testBatch.IsTopModelWasFind ? testBatch.TopModelTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue : 0;

                        totalNet += currentNet;
                        totalDropdown += currentDropdown;
                        totalNumberTrades += currentNumberTrades;
                        totalPercentWin += currentPercentWin;
                        totalNetTopModel += currentNetTopModel;

                        DateTime dateTimeStart = testBatch.OptimizationTestRuns[0].StartPeriod; //начало периода тестирования
                        DateTime dateTimeEnd = testBatch.OptimizationTestRuns[0].EndPeriod; //окончание периода тестирования
                        string dateTimeStartStr = dateTimeStart.Day.ToString().Length == 1 ? "0" + dateTimeStart.Day.ToString() : dateTimeStart.Day.ToString();
                        dateTimeStartStr += dateTimeStart.Month.ToString().Length == 1 ? ".0" + dateTimeStart.Month.ToString() : "." + dateTimeStart.Month.ToString();
                        dateTimeStartStr += "." + dateTimeStart.Year.ToString();

                        string dateTimeEndStr = dateTimeEnd.Day.ToString().Length == 1 ? "0" + dateTimeEnd.Day.ToString() : dateTimeEnd.Day.ToString();
                        dateTimeEndStr += dateTimeEnd.Month.ToString().Length == 1 ? ".0" + dateTimeEnd.Month.ToString() : "." + dateTimeEnd.Month.ToString();
                        dateTimeEndStr += "." + dateTimeEnd.Year.ToString();

                        OptimizationTestsInfo optimizationTestsInfo = new OptimizationTestsInfo { TradeWindow = dateTimeStartStr + "-" + dateTimeEndStr, NetProfitLoss = ModelFunctions.SplitDigitsDouble(currentNet, 0) + " " + currencyName, MaxDropdown = ModelFunctions.SplitDigitsDouble(currentDropdown, 0) + " " + currencyName, NumberTrades = ModelFunctions.SplitDigitsDouble(currentNumberTrades, 1), PercentWin = Math.Round(currentPercentWin, 1) + " %" };
                        if (testBatch.IsTopModelWasFind)
                        {
                            isTopModelFind = true;
                            optimizationTestsInfo.NetProfitLossTopModel = ModelFunctions.SplitDigitsDouble(currentNetTopModel, 0) + " " + currencyName;
                        }
                        OptimizationsTestsInfo.Add(optimizationTestsInfo);
                    }
                    //добавляем строку со средним значением
                    OptimizationTestsInfo optimizationTestsInfo2 = new OptimizationTestsInfo { TradeWindow = "Среднее", NetProfitLoss = ModelFunctions.SplitDigitsDouble(totalNet / numberTestBatches, 0) + " " + currencyName, MaxDropdown = ModelFunctions.SplitDigitsDouble(totalDropdown / numberTestBatches, 0) + " " + currencyName, NumberTrades = ModelFunctions.SplitDigitsDouble((double)totalNumberTrades / numberTestBatches, 1), PercentWin = Math.Round(totalPercentWin / numberTestBatches, 1) + " %" };
                    if (isTopModelFind)
                    {
                        optimizationTestsInfo2.NetProfitLossTopModel = ModelFunctions.SplitDigitsDouble(totalNetTopModel / numberTestBatches, 0) + " " + currencyName;
                    }
                    OptimizationsTestsInfo.Add(optimizationTestsInfo2);
                }
            }
        }

        public void UpdatePage()
        {
            if (ViewModelPageTestingResult.getInstance().TestingResult != null)
            {
                _testing = ViewModelPageTestingResult.getInstance().TestingResult;
                CreateOptimizationTestsInfo(); //создаем информацию об оптимизационных тестах
            }
        }
    }
}
