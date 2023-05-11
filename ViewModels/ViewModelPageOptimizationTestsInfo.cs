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
                    double totalNetOnMargin = 0;
                    double totalMaxDropdown = 0;
                    double totalTradesNumber = 0;
                    double totalWinPercent = 0;
                    double totalNetOnMarginTopModel = 0;
                    bool isTopModelFind = false;

                    //проходим по всем TestBatch текущей групы источников данных
                    foreach (TestBatch testBatch in dataSourceGroupTestBatches)
                    {
                        numberTestBatches++;

                        double totalTestRunNetOnMargin = 0;
                        double totalTestRunMaxDropdown = 0;
                        int totalTestRunTradesNumber = 0;
                        double totalTestRunWinPercent = 0;
                        //проходим по всем тестовым прогонам и определяем показатели
                        foreach (TestRun testRun in testBatch.OptimizationTestRuns)
                        {
                            totalTestRunNetOnMargin += testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 34).DoubleValue;
                            totalTestRunMaxDropdown += testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 7).DoubleValue;
                            totalTestRunTradesNumber += (int)testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 35).DoubleValue;
                            totalTestRunWinPercent += testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 14).DoubleValue;
                        }

                        double averageNetOnMargin = totalTestRunNetOnMargin / testBatch.OptimizationTestRuns.Count;
                        double averageMaxDropdown = totalTestRunMaxDropdown / testBatch.OptimizationTestRuns.Count;
                        double averageTradesNumber = (double)totalTestRunTradesNumber / testBatch.OptimizationTestRuns.Count;
                        double averageWinPercent = totalTestRunWinPercent / testBatch.OptimizationTestRuns.Count;
                        double netOnMarginTopModel = testBatch.IsTopModelWasFind ? testBatch.TopModelTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 1).DoubleValue : 0;

                        totalNetOnMargin += averageNetOnMargin;
                        totalMaxDropdown += averageMaxDropdown;
                        totalTradesNumber += averageTradesNumber;
                        totalWinPercent += averageWinPercent;
                        totalNetOnMarginTopModel += netOnMarginTopModel;

                        DateTime dateTimeStart = testBatch.OptimizationTestRuns[0].StartPeriod; //начало периода тестирования
                        DateTime dateTimeEnd = testBatch.OptimizationTestRuns[0].EndPeriod; //окончание периода тестирования
                        string dateTimeStartStr = dateTimeStart.Day.ToString().Length == 1 ? "0" + dateTimeStart.Day.ToString() : dateTimeStart.Day.ToString();
                        dateTimeStartStr += dateTimeStart.Month.ToString().Length == 1 ? ".0" + dateTimeStart.Month.ToString() : "." + dateTimeStart.Month.ToString();
                        dateTimeStartStr += "." + dateTimeStart.Year.ToString();

                        string dateTimeEndStr = dateTimeEnd.Day.ToString().Length == 1 ? "0" + dateTimeEnd.Day.ToString() : dateTimeEnd.Day.ToString();
                        dateTimeEndStr += dateTimeEnd.Month.ToString().Length == 1 ? ".0" + dateTimeEnd.Month.ToString() : "." + dateTimeEnd.Month.ToString();
                        dateTimeEndStr += "." + dateTimeEnd.Year.ToString();

                        OptimizationTestsInfo optimizationTestsInfo = new OptimizationTestsInfo { TradeWindow = dateTimeStartStr + "-" + dateTimeEndStr, AverageNetOnMargin = ModelFunctions.SplitDigitsDouble(averageNetOnMargin, 2, " ") + " " + currencyName, AverageMaxDropdown = ModelFunctions.SplitDigitsDouble(averageMaxDropdown, 2) + " " + currencyName, AverageTradesNumber = ModelFunctions.SplitDigitsDouble(averageTradesNumber, 1), AverageWinPercent = ModelFunctions.SplitDigitsDouble(averageWinPercent, 1) + " %" };
                        if (testBatch.IsTopModelWasFind)
                        {
                            isTopModelFind = true;
                            optimizationTestsInfo.NetOnMarginTopModel = ModelFunctions.SplitDigitsDouble(netOnMarginTopModel, 2, " ") + " " + currencyName;
                        }
                        OptimizationsTestsInfo.Add(optimizationTestsInfo);
                    }
                    //добавляем строку со средним значением
                    OptimizationTestsInfo optimizationTestsInfo2 = new OptimizationTestsInfo { TradeWindow = "Среднее", AverageNetOnMargin = ModelFunctions.SplitDigitsDouble(totalNetOnMargin / numberTestBatches, 2, " ") + " " + currencyName, AverageMaxDropdown = ModelFunctions.SplitDigitsDouble(totalMaxDropdown / numberTestBatches, 2) + " " + currencyName, AverageTradesNumber = ModelFunctions.SplitDigitsDouble((double)totalTradesNumber / numberTestBatches, 1), AverageWinPercent = Math.Round(totalWinPercent / numberTestBatches, 1) + " %" };
                    if (isTopModelFind)
                    {
                        optimizationTestsInfo2.NetOnMarginTopModel = ModelFunctions.SplitDigitsDouble(totalNetOnMarginTopModel / numberTestBatches, 2, " ") + " " + currencyName;
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
