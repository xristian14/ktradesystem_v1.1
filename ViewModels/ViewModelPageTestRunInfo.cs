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
    class ViewModelPageTestRunInfo : ViewModelBase
    {
        public ViewModelPageTestRunInfo()
        {
            ViewModelPageTestingResult.TestRunsUpdatePages += UpdatePage;
            _viewModelPageTradeChart = ViewModelPageTradeChart.getInstance();
        }
        private ViewModelPageTradeChart _viewModelPageTradeChart;
        private TestRun _testRun;
        private string _algorithmParameterValuesText = "";
        public string AlgorithmParameterValuesText //значения параметров алгоритма в текстовом виде
        {
            get { return _algorithmParameterValuesText; }
            set
            {
                _algorithmParameterValuesText = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<EvaluationCriteriaValue> _evaluationCriteriaValuesOne = new ObservableCollection<EvaluationCriteriaValue>();
        public ObservableCollection<EvaluationCriteriaValue> EvaluationCriteriaValuesOne //критерии оценки для первой колонки
        {
            get { return _evaluationCriteriaValuesOne; }
            set
            {
                _evaluationCriteriaValuesOne = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<EvaluationCriteriaValue> _evaluationCriteriaValuesTwo = new ObservableCollection<EvaluationCriteriaValue>();
        public ObservableCollection<EvaluationCriteriaValue> EvaluationCriteriaValuesTwo //критерии оценки для второй колонки
        {
            get { return _evaluationCriteriaValuesTwo; }
            set
            {
                _evaluationCriteriaValuesTwo = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<EvaluationCriteriaValue> _evaluationCriteriaValuesThree = new ObservableCollection<EvaluationCriteriaValue>();
        public ObservableCollection<EvaluationCriteriaValue> EvaluationCriteriaValuesThree //критерии оценки для третей колонки
        {
            get { return _evaluationCriteriaValuesThree; }
            set
            {
                _evaluationCriteriaValuesThree = value;
                OnPropertyChanged();
            }
        }

        private void CreateAlgorithmParameterValuesText() //формирует текст со значениями параметров алгоритма
        {
            AlgorithmParameterValuesText = "";
            foreach(AlgorithmParameterValue algorithmParameterValue in _testRun.AlgorithmParameterValues)
            {
                AlgorithmParameterValuesText += algorithmParameterValue.AlgorithmParameter.Name + "=";
                if (algorithmParameterValue.AlgorithmParameter.ParameterValueType.Id == 1) //параметр типа int
                {
                    AlgorithmParameterValuesText += algorithmParameterValue.IntValue.ToString();
                }
                else //параметр типа double
                {
                    AlgorithmParameterValuesText += algorithmParameterValue.DoubleValue.ToString();
                }
                AlgorithmParameterValuesText += ", ";
            }
            AlgorithmParameterValuesText = AlgorithmParameterValuesText.Substring(0, AlgorithmParameterValuesText.Length - 2); //удаляем последние два символа
        }

        private void CreateEvaluationCriteriaValues() //формирует три списка с критериями оценки
        {
            EvaluationCriteriaValuesOne.Clear();
            EvaluationCriteriaValuesTwo.Clear();
            EvaluationCriteriaValuesThree.Clear();
            int i = 0;
            while(i < (int)Math.Truncate(_testRun.EvaluationCriteriaValues.Count / 3.0))
            {
                EvaluationCriteriaValuesOne.Add(_testRun.EvaluationCriteriaValues[i]);
                i++;
            }
            while(i < (int)Math.Truncate(_testRun.EvaluationCriteriaValues.Count / 3.0 * 2.0))
            {
                EvaluationCriteriaValuesTwo.Add(_testRun.EvaluationCriteriaValues[i]);
                i++;
            }
            while(i < _testRun.EvaluationCriteriaValues.Count)
            {
                EvaluationCriteriaValuesThree.Add(_testRun.EvaluationCriteriaValues[i]);
                i++;
            }
        }
        public void UpdatePage() //обновляет страницу на новый источник данных
        {
            if (ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null && ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox != null)
            {
                _testRun = ViewModelPageTestingResult.getInstance().SelectedTestRunTestingResultCombobox.TestRun;
                CreateAlgorithmParameterValuesText(); //формируем текст со значениями параметров алгоритма
                CreateEvaluationCriteriaValues(); //формируем три списка с критериями оценки
            }
        }
    }
}
