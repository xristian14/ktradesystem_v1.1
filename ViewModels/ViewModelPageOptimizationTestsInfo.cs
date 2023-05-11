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
                    double totalAnnualNetOnMargin = 0;
                    double totalMaxDropdownPercent = 0;
                    double totalAnnualTradesNumber = 0;
                    double totalWinPercent = 0;
                    double totalTopModelAnnualNetOnMargin = 0;
                    bool isTopModelFind = false;

                    //проходим по всем TestBatch текущей групы источников данных
                    foreach (TestBatch testBatch in dataSourceGroupTestBatches)
                    {
                        numberTestBatches++;

                        double totalTestBatchAnnualNetOnMargin = 0;
                        double totalTestBatchMaxDropdownPercent = 0;
                        double totalTestBatchAnnualTradesNumber = 0;
                        double totalTestBatchWinPercent = 0;
                        //проходим по всем тестовым прогонам и определяем показатели
                        foreach (TestRun testRun in testBatch.OptimizationTestRuns)
                        {
                            totalTestBatchAnnualNetOnMargin += testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 6).DoubleValue;
                            totalTestBatchMaxDropdownPercent += testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 8).DoubleValue;
                            totalTestBatchAnnualTradesNumber += (int)testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 9).DoubleValue;
                            totalTestBatchWinPercent += testRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 14).DoubleValue;
                        }

                        double averageAnnualNetOnMargin = totalTestBatchAnnualNetOnMargin / testBatch.OptimizationTestRuns.Count;
                        double averageMaxDropdownPercent = totalTestBatchMaxDropdownPercent / testBatch.OptimizationTestRuns.Count;
                        double averageAnnualTradesNumber = totalTestBatchAnnualTradesNumber / testBatch.OptimizationTestRuns.Count;
                        double averageWinPercent = totalTestBatchWinPercent / testBatch.OptimizationTestRuns.Count;
                        double topModelAnnualNetOnMargin = testBatch.IsTopModelWasFind ? testBatch.TopModelTestRun.EvaluationCriteriaValues.Find(a => a.EvaluationCriteria.Id == 6).DoubleValue : 0;

                        totalAnnualNetOnMargin += averageAnnualNetOnMargin;
                        totalMaxDropdownPercent += averageMaxDropdownPercent;
                        totalAnnualTradesNumber += averageAnnualTradesNumber;
                        totalWinPercent += averageWinPercent;
                        totalTopModelAnnualNetOnMargin += topModelAnnualNetOnMargin;

                        DateTime dateTimeStart = testBatch.OptimizationTestRuns[0].StartPeriod; //начало периода тестирования
                        DateTime dateTimeEnd = testBatch.OptimizationTestRuns[0].EndPeriod; //окончание периода тестирования
                        string dateTimeStartStr = dateTimeStart.Day.ToString().Length == 1 ? "0" + dateTimeStart.Day.ToString() : dateTimeStart.Day.ToString();
                        dateTimeStartStr += dateTimeStart.Month.ToString().Length == 1 ? ".0" + dateTimeStart.Month.ToString() : "." + dateTimeStart.Month.ToString();
                        dateTimeStartStr += "." + dateTimeStart.Year.ToString();

                        string dateTimeEndStr = dateTimeEnd.Day.ToString().Length == 1 ? "0" + dateTimeEnd.Day.ToString() : dateTimeEnd.Day.ToString();
                        dateTimeEndStr += dateTimeEnd.Month.ToString().Length == 1 ? ".0" + dateTimeEnd.Month.ToString() : "." + dateTimeEnd.Month.ToString();
                        dateTimeEndStr += "." + dateTimeEnd.Year.ToString();

                        OptimizationTestsInfo optimizationTestsInfo = new OptimizationTestsInfo { TradeWindow = dateTimeStartStr + "-" + dateTimeEndStr, AverageAnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(averageAnnualNetOnMargin, 1, " ") + " %", AverageMaxDropdownPercent = ModelFunctions.SplitDigitsDouble(averageMaxDropdownPercent, 1) + " %", AverageAnnualTradesNumber = ModelFunctions.SplitDigitsDouble(averageAnnualTradesNumber, 1), AverageWinPercent = ModelFunctions.SplitDigitsDouble(averageWinPercent, 1) + " %" };
                        if (testBatch.IsTopModelWasFind)
                        {
                            isTopModelFind = true;
                            optimizationTestsInfo.TopModelAnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(topModelAnnualNetOnMargin, 1, " ") + " %";
                        }
                        OptimizationsTestsInfo.Add(optimizationTestsInfo);
                    }
                    //добавляем строку со средним значением
                    OptimizationTestsInfo optimizationTestsInfo2 = new OptimizationTestsInfo { TradeWindow = "СРЕДНЕЕ", AverageAnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(totalAnnualNetOnMargin / numberTestBatches, 1, " ") + " %", AverageMaxDropdownPercent = ModelFunctions.SplitDigitsDouble(totalMaxDropdownPercent / numberTestBatches, 1) + " %", AverageAnnualTradesNumber = ModelFunctions.SplitDigitsDouble(totalAnnualTradesNumber / numberTestBatches, 1), AverageWinPercent = Math.Round(totalWinPercent / numberTestBatches, 1) + " %" };
                    if (isTopModelFind)
                    {
                        optimizationTestsInfo2.TopModelAnnualNetOnMargin = ModelFunctions.SplitDigitsDouble(totalTopModelAnnualNetOnMargin / numberTestBatches, 1, " ") + " %";
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
