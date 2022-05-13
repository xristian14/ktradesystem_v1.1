using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTheeDimensionChart : ViewModelBase
    {
        public ViewModelPageTheeDimensionChart()
        {
            ViewModelPageTestingResult.TestBatchesUpdatePages += UpdatePage;
            CreateEvaluationCriteriasPageThreeDimensionChart();
        }

        private Testing _testing; //результат тестирования
        private TestBatch _testBatch; //тестовая связка, на основании которой будет строиться график
        private List<AlgorithmParameter> _leftAxisParameters; //параметры алгоритма, которые будут на левой стороне квадрата
        private List<AlgorithmParameter> _topAxisParameters; //параметры алгоритма, которые будут на верхней стороне квадрата. В каждой клетке квадрата будет комбинация из параметров, в этой клетке должен находится тестовый прогон с данной комбинацией параметров
        private TestRun[][] testRunsMatrix; //двумерный массив с тестовыми прогонами. График будет строиться по этому массиву
        private double _sizeChartSide = 1; //размер стороны куба, в который вписывается график
        private float _cameraDistance = 1; //растояние от центра куба до камеры
        private int _countScaleValues = 7; //количество отрезков на шкале значений по оси критерия оценки. К этому значению будет стремиться количество отрезков
        private double _scaleValuesPlaneOffset = 0.005; //величина смещения плоскости от размера стороны _sizeChartSide. Плоскость будет смещена на _sizeChartSide*_scaleValuesPlaneOffset относительно линий на шкале значений
        private double _levelsTotalOpacity = 0.3; //суммарная прозрачность для уровней
        private double _searchPlanesTotalOpacity = 0.3; //суммарная прозрачность для плоскостей поиска

        private Model3D _parametersPlanesModel3D;
        public Model3D ParametersPlanesModel3D //плоскости, на которых отображается ось с параметрами алгоритма
        {
            get { return _parametersPlanesModel3D; }
            private set
            {
                _parametersPlanesModel3D = value;
                OnPropertyChanged();
            }
        }

        private Model3D _scaleValuesModel3D;
        public Model3D ScaleValuesModel3D //плоскости, на которых отображается ось с шкалой значений
        {
            get { return _scaleValuesModel3D; }
            private set
            {
                _scaleValuesModel3D = value;
                OnPropertyChanged();
            }
        }

        private Model3D _surfacesModel3D;
        public Model3D SurfacesModel3D //поверхности графиков
        {
            get { return _surfacesModel3D; }
            private set
            {
                _surfacesModel3D = value;
                OnPropertyChanged();
            }
        }

        private Model3D _levelsModel3D;
        public Model3D LevelsModel3D //уровни
        {
            get { return _levelsModel3D; }
            private set
            {
                _levelsModel3D = value;
                OnPropertyChanged();
            }
        }

        private Model3D _searchPlanesModel3D;
        public Model3D SearchPlanesModel3D //плоскости поиска
        {
            get { return _searchPlanesModel3D; }
            private set
            {
                _searchPlanesModel3D = value;
                OnPropertyChanged();
            }
        }

        private double _min = 0; //минимальное значение на графике
        private double _max = 0; //максимальное значение на графике

        private void UpdateMinAndMaxValuesInEvaluationCriterias() //обновляет минимальное и максимальное значения у выбранных критериев оценки
        {
            _min = _testBatch.OptimizationTestRuns.First().EvaluationCriteriaValues.Where(j => j.EvaluationCriteria.Id == EvaluationCriteriasPageThreeDimensionChart.Where(jj => jj.IsChecked == true).First().EvaluationCriteria.Id).First().DoubleValue; //получили значение первого выбранного критерия оценки для первого тестового прогона
            _max = _min;
            //проходим по всем выбранным критериям оценки
            foreach (EvaluationCriteriaPageThreeDimensionChart evaluationCriteriaPageThreeDimensionChart in EvaluationCriteriasPageThreeDimensionChart.Where(j => j.IsChecked == true))
            {
                //определяем индекс текущего критерия оценки
                int evaluationCriteriaIndex = -1;
                int i = 0;
                while (i < _testBatch.OptimizationTestRuns.First().EvaluationCriteriaValues.Count && evaluationCriteriaIndex == -1)
                {
                    if (evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.Id == _testBatch.OptimizationTestRuns.First().EvaluationCriteriaValues[i].EvaluationCriteria.Id)
                    {
                        evaluationCriteriaIndex = i;
                    }
                    i++;
                }
                //проходим по всем тестовым прогонам данной тестовой связки, и ищем в них минимальное и максимальное значения критерия оценки
                foreach (TestRun testRun in _testBatch.OptimizationTestRuns)
                {
                    if (testRun.EvaluationCriteriaValues[evaluationCriteriaIndex].DoubleValue < _min)
                    {
                        _min = testRun.EvaluationCriteriaValues[evaluationCriteriaIndex].DoubleValue;
                    }
                    if (testRun.EvaluationCriteriaValues[evaluationCriteriaIndex].DoubleValue > _max)
                    {
                        _max = testRun.EvaluationCriteriaValues[evaluationCriteriaIndex].DoubleValue;
                    }
                }
            }
        }

        private MeshGeometry3D CreateMeshGeometry3D(Point3D[] positions, int[] triangleIndices)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            for(int i = 0; i < positions.Length; i++)
            {
                mesh.Positions.Add(positions[i]);
            }
            for(int i = 0; i < triangleIndices.Length; i++)
            {
                mesh.TriangleIndices.Add(triangleIndices[i]);
            }
            return mesh;
        }

        private void UpdateSearchPlanes() //обновляет поисковые плоскости на выбранные, формирует двумерный массив с тестовыми прогонами на основе выбранных осей поисковой плоскости
        {

        }

        public void BuildChart() //строит график
        {
            if(_testBatch != null)
            {
                Model3DGroup chartModelGroup = new Model3DGroup(); //группа с моделями графика
                
                //определяем минимальное и максимальное значение на графике
                double min = _testBatch.OptimizationTestRuns.First().EvaluationCriteriaValues.Where(j => j.EvaluationCriteria.Id == EvaluationCriteriasPageThreeDimensionChart.Where(jj => jj.IsChecked == true).First().EvaluationCriteria.Id).First().DoubleValue; //получили значение первого выбранного критерия оценки для первого тестового прогона
                double max = min;
                //проходим по всем выбранным критериям оценки
                foreach(EvaluationCriteriaPageThreeDimensionChart evaluationCriteriaPageThreeDimensionChart in EvaluationCriteriasPageThreeDimensionChart.Where(j => j.IsChecked == true))
                {
                    //определяем индекс текущего критерия оценки
                    int evaluationCriteriaIndex = -1;
                    int i = 0;
                    while(i < _testBatch.OptimizationTestRuns.First().EvaluationCriteriaValues.Count && evaluationCriteriaIndex == -1)
                    {
                        if(evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.Id == _testBatch.OptimizationTestRuns.First().EvaluationCriteriaValues[i].EvaluationCriteria.Id)
                        {
                            evaluationCriteriaIndex = i;
                        }
                        i++;
                    }
                    //проходим по всем тестовым прогонам данной тестовой связки, и ищем в них минимальное и максимальное значения критерия оценки
                    foreach (TestRun testRun in _testBatch.OptimizationTestRuns)
                    {
                        if(testRun.EvaluationCriteriaValues[evaluationCriteriaIndex].DoubleValue < min)
                        {
                            min = testRun.EvaluationCriteriaValues[evaluationCriteriaIndex].DoubleValue;
                        }
                        if(testRun.EvaluationCriteriaValues[evaluationCriteriaIndex].DoubleValue > max)
                        {
                            max = testRun.EvaluationCriteriaValues[evaluationCriteriaIndex].DoubleValue;
                        }
                    }
                }
                
                //формируем двумерный массив с тестовыми прогонами, на основе выбранных осей поисковой плоскости
                //определяем значения оптимизируемых переменных для вертикальной и горизонтальной оси двумерного массива, на персечении комбинации должен находится тестовый прогон с данной комбинацией параметров

                //определяем значения на шкале по оси критерия оценки





                //ChartModel.Content = chartModelGroup;






                //создаем переднюю шкалу со значениями
                /*ChartScaleValuesFront = new Model3DGroup();
                GeometryModel3D frontPlane = new GeometryModel3D(); //определяем плоскость
                MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
                double frontPlaneZ = _sizeChartSide / 2 + _sizeChartSide * _countScaleValues;
                frontPlane.Geometry = CreateMeshGeometry3D(new Point3D[4] { new Point3D(-_sizeChartSide / 2, -_sizeChartSide / 2, frontPlaneZ), new Point3D(-_sizeChartSide / 2, _sizeChartSide / 2, frontPlaneZ), new Point3D(_sizeChartSide / 2, _sizeChartSide / 2, frontPlaneZ), new Point3D(_sizeChartSide / 2, -_sizeChartSide / 2, frontPlaneZ) }, new int[6] { 0, 1, 2, 3, 4, 5 });
                ChartScaleValuesFront.Children.Add(frontPlane);
                GeometryModel3D[] frontScales = new GeometryModel3D[_countScaleValues]; //плоскости, которые рисуют линии для значений
                for(int i = 0; i < _countScaleValues; i++)
                {
                    GeometryModel3D frontScale = new GeometryModel3D(); //определяем плоскость, которая отобразит линию для значения шкалы

                    frontPlane.Geometry = CreateMeshGeometry3D(new Point3D[4] { new Point3D(-_sizeChartSide / 2, -_sizeChartSide / 2, _sizeChartSide / 2), new Point3D(-_sizeChartSide / 2, _sizeChartSide / 2, _sizeChartSide / 2), new Point3D(_sizeChartSide / 2, _sizeChartSide / 2, _sizeChartSide / 2), new Point3D(_sizeChartSide / 2, -_sizeChartSide / 2, _sizeChartSide / 2) }, new int[6] { 0, 1, 2, 3, 4, 5 });
                }*/
            }
        }

        private ObservableCollection<EvaluationCriteriaPageThreeDimensionChart> _evaluationCriteriasPageThreeDimensionChart = new ObservableCollection<EvaluationCriteriaPageThreeDimensionChart>(); //критерии оценки для checkbox
        public ObservableCollection<EvaluationCriteriaPageThreeDimensionChart> EvaluationCriteriasPageThreeDimensionChart
        {
            get { return _evaluationCriteriasPageThreeDimensionChart; }
            private set
            {
                _evaluationCriteriasPageThreeDimensionChart = value;
                OnPropertyChanged();
            }
        }
        public void EvaluationCriteriasPageThreeDimensionChart_PropertyChanged(EvaluationCriteriaPageThreeDimensionChart evaluationCriteriaPageThreeDimensionChart, string propertyName) //обработчик изменения свойств у объектов в EvaluationCriteriasPageThreeDimensionChart
        {
            if(propertyName == "IsButtonResetChecked") //если была переключена кнопка: Сбросить критерии оценки
            {
                if (evaluationCriteriaPageThreeDimensionChart.IsButtonResetChecked)
                {
                    ResetEvaluationCriteriasPageThreeDimensionChart(); //сбрасываем выбранные критерии оценки на тот что используется для определения топ-модели
                    evaluationCriteriaPageThreeDimensionChart.IsButtonResetChecked = false;
                }
            }
            if(propertyName == "IsChecked") //если был переключен чекбокс
            {
                
            }
        }
        private void CreateEvaluationCriteriasPageThreeDimensionChart() //создает критерии оценки для представления. Добавляет только те, которые имеют числовое значение
        {
            EvaluationCriteriasPageThreeDimensionChart.Clear();
            EvaluationCriteriasPageThreeDimensionChart.Add(EvaluationCriteriaPageThreeDimensionChart.CreateButtonReset(EvaluationCriteriasPageThreeDimensionChart_PropertyChanged)); //добавляем кнопку сбросить критерии оценки

            //добавляем критерии оценки
            foreach (EvaluationCriteria evaluationCriteria in ViewModelPageTesting.getInstance().EvaluationCriterias)
            {
                if (evaluationCriteria.IsDoubleValue)
                {
                    EvaluationCriteriasPageThreeDimensionChart.Add(EvaluationCriteriaPageThreeDimensionChart.CreateEvaluationCriteria(EvaluationCriteriasPageThreeDimensionChart_PropertyChanged, evaluationCriteria));
                }
            }
        }

        private void ResetEvaluationCriteriasPageThreeDimensionChart() //сбрасывает выбранные критерии оценки на тот что используется для определения топ-модели
        {
            for(int i = 1; i < EvaluationCriteriasPageThreeDimensionChart.Count; i++)
            {
                if (EvaluationCriteriasPageThreeDimensionChart[i].EvaluationCriteria.Id == _testing.TopModelCriteria.EvaluationCriteria.Id) //если это критерий оценки для оределения топ-модели
                {
                    if(EvaluationCriteriasPageThreeDimensionChart[i].IsChecked == false) //если он не выбран, выбираем его
                    {
                        EvaluationCriteriasPageThreeDimensionChart[i].IsChecked = true;
                    }
                }
                else if (EvaluationCriteriasPageThreeDimensionChart[i].IsChecked) //иначе, это не критерий оценки топ-модели, и если он выбран, снимаем выбор
                {
                    EvaluationCriteriasPageThreeDimensionChart[i].IsChecked = false;
                }
            }
        }

        private ObservableCollection<LevelPageThreeDimensionChart> _levelsPageThreeDimensionChart = new ObservableCollection<LevelPageThreeDimensionChart>(); //уровни на графике
        public ObservableCollection<LevelPageThreeDimensionChart> LevelsPageThreeDimensionChart
        {
            get { return _levelsPageThreeDimensionChart; }
            private set
            {
                _levelsPageThreeDimensionChart = value;
                OnPropertyChanged();
            }
        }
        public void LevelPageThreeDimensionChart_PropertyChanged(LevelPageThreeDimensionChart levelPageThreeDimensionChart, string propertyName)//обработчик изменения свойств у объектов в LevelsPageThreeDimensionChart
        {
            if(propertyName == "IsButtonAddLevelChecked") //если была переключена кнопка: Добавить уровень
            {
                if(levelPageThreeDimensionChart.IsButtonAddLevelChecked) //если кнопка в состоянии true, - добавляем уровень
                {
                    LevelsPageThreeDimensionChart.Add(LevelPageThreeDimensionChart.CreateLevel(LevelPageThreeDimensionChart_PropertyChanged, -50, 50, 0));
                    levelPageThreeDimensionChart.IsButtonAddLevelChecked = false;

                    for(int i = 1; i < LevelsPageThreeDimensionChart.Count; i++)
                    {
                        LevelsPageThreeDimensionChart[i].Value = 1.5;
                    }
                }
            }
            if(propertyName == "IsDeleteChecked") //если была переключена кнопка: Удалить, - удаляем уровень
            {
                LevelsPageThreeDimensionChart.Remove(levelPageThreeDimensionChart);
            }
        }

        private ObservableCollection<AxisSearchPlanePageThreeDimensionChart> _axesSearchPlanePageThreeDimensionChart = new ObservableCollection<AxisSearchPlanePageThreeDimensionChart>(); //оси плоскости поиска топ-модели
        public ObservableCollection<AxisSearchPlanePageThreeDimensionChart> AxesSearchPlanePageThreeDimensionChart
        {
            get { return _axesSearchPlanePageThreeDimensionChart; }
            private set
            {
                _axesSearchPlanePageThreeDimensionChart = value;
                OnPropertyChanged();
            }
        }
        public void AxesSearchPlanePageThreeDimensionChart_PropertyChanged(AxisSearchPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart, string propertyName) //обработчик изменения свойств у объектов в AxesSearchPlanePageThreeDimensionChart
        {
            if (propertyName == "IsButtonResetChecked") //если была переключена кнопка: Сбросить
            {
                if (axisSearchPlanePageThreeDimensionChart.IsButtonResetChecked)
                {
                    axisSearchPlanePageThreeDimensionChart.IsButtonResetChecked = false;
                }
            }
            if (propertyName == "SelectedAlgorithmParameter") //если был выбран другой параметр алгоритма
            {
                if(AxesSearchPlanePageThreeDimensionChart.Count > 2) //если параметров 2 и более, для того который не был переключен обновляем доступные для выбора параметры алгоритма
                {
                    AxisSearchPlanePageThreeDimensionChart axisSearchPlanePageThreeDimensionChart2 = axisSearchPlanePageThreeDimensionChart == AxesSearchPlanePageThreeDimensionChart[1] ? AxesSearchPlanePageThreeDimensionChart[2] : AxesSearchPlanePageThreeDimensionChart[1]; //получаем объект у которого не был изменен выбранный параметр
                    axisSearchPlanePageThreeDimensionChart2.AlgorithmParameters = GetAlgorithmParameters();
                    axisSearchPlanePageThreeDimensionChart2.AlgorithmParameters.Remove(axisSearchPlanePageThreeDimensionChart.SelectedAlgorithmParameter); //удаляем из доступных для выбора параметров, параметр который был выбран
                }
            }
        }
        private ObservableCollection<AlgorithmParameter> GetAlgorithmParameters() //возвращает параметры алгоритма
        {
            ObservableCollection<AlgorithmParameter> algorithmParameters = new ObservableCollection<AlgorithmParameter>();
            foreach (AlgorithmParameter algorithmParameter in _testing.Algorithm.AlgorithmParameters)
            {
                algorithmParameters.Add(algorithmParameter);
            }
            return algorithmParameters;
        }
        private void CreateAxesSearchPlanePageThreeDimensionChart() //создает элементы для меню выбора осей плоскости поиска топ-модели
        {
            AxesSearchPlanePageThreeDimensionChart.Clear();
            AxesSearchPlanePageThreeDimensionChart.Add(AxisSearchPlanePageThreeDimensionChart.CreateButtonReset(AxesSearchPlanePageThreeDimensionChart_PropertyChanged)); //добавляем кнопку сбросить

            //добавляем чекбоксы для осей
            ObservableCollection<AlgorithmParameter> algorithmParametersFirst = GetAlgorithmParameters(); //получаем список с параметрами алгоритма в текущем результате тестирования
            ObservableCollection<AlgorithmParameter> algorithmParametersSecond = GetAlgorithmParameters(); //получаем список с параметрами алгоритма в текущем результате тестирования

            if (_testing.Algorithm.AlgorithmParameters.Count == 1) //если параметр алгоритма только один
            {
                AxesSearchPlanePageThreeDimensionChart.Add(AxisSearchPlanePageThreeDimensionChart.CreateAxisSearchPlane(AxesSearchPlanePageThreeDimensionChart_PropertyChanged, algorithmParametersFirst, 0));
            }
            else if(_testing.Algorithm.AlgorithmParameters.Count >= 2) //иначе, если их два или более
            {
                //определяем индекс первого параметра оси плоскости поиска
                int indexFirstParameter = algorithmParametersFirst.IndexOf(algorithmParametersFirst.Where(j => j.Id == _testBatch.AxesTopModelSearchPlane[0].AlgorithmParameter.Id).First());
                AxesSearchPlanePageThreeDimensionChart.Add(AxisSearchPlanePageThreeDimensionChart.CreateAxisSearchPlane(AxesSearchPlanePageThreeDimensionChart_PropertyChanged, algorithmParametersFirst, indexFirstParameter)); //добавляем чекбокс первой оси в меню выбора осей плоскости поиска
                //определяем индекс второго параметра оси плоскости поиска
                int indexSecondParameter = algorithmParametersSecond.IndexOf(algorithmParametersSecond.Where(j => j.Id == _testBatch.AxesTopModelSearchPlane[1].AlgorithmParameter.Id).First());
                AxesSearchPlanePageThreeDimensionChart.Add(AxisSearchPlanePageThreeDimensionChart.CreateAxisSearchPlane(AxesSearchPlanePageThreeDimensionChart_PropertyChanged, algorithmParametersSecond, indexSecondParameter)); //добавляем чекбокс второй оси в меню выбора осей плоскости поиска

                //удаляем из списков с параметрами те что выбраны  в другом списке
                AxesSearchPlanePageThreeDimensionChart[1].AlgorithmParameters.Remove(AxesSearchPlanePageThreeDimensionChart[2].SelectedAlgorithmParameter);
                AxesSearchPlanePageThreeDimensionChart[2].AlgorithmParameters.Remove(AxesSearchPlanePageThreeDimensionChart[1].SelectedAlgorithmParameter);
            }
        }








        public void UpdatePage() //обновляет страницу
        {
            if(ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null)
            {
                _testing = ViewModelPageTestingResult.getInstance().TestingResult;
                _testBatch = ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox.TestBatch;
                ResetEvaluationCriteriasPageThreeDimensionChart(); //сбрасываем выбранные критерии оценки на тот что используется для определения топ-модели
                LevelsPageThreeDimensionChart.Clear();
                LevelsPageThreeDimensionChart.Add(LevelPageThreeDimensionChart.CreateButtonAddLevel(LevelPageThreeDimensionChart_PropertyChanged)); //создаем кнопку добавить для меню уровней
                CreateAxesSearchPlanePageThreeDimensionChart(); //создаем элементы для меню выбора осей плоскости поиска топ-модели
            }
        }
        public ICommand ResetCamera_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    
                }, (obj) => true);
            }
        }
        public ICommand ResetEvaluationCriterias_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    
                }, (obj) => true);
            }
        }
    }
}
