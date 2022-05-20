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
using System.Windows.Controls;
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
            ResetCameraPosition();
        }

        private bool isLoadingTestResultComplete = false; //загружен ли тестовый результат. Нужно чтобы не допускать посроения поверхности когда не все функции, связанные с формированием матрицы выполнены
        private Testing _testing; //результат тестирования
        private TestBatch _testBatch; //тестовая связка, на основании которой будет строиться график
        private List<AlgorithmParameter> _leftAxisParameters = new List<AlgorithmParameter>(); //параметры алгоритма, которые будут на левой стороне квадрата
        private List<AlgorithmParameter> _topAxisParameters = new List<AlgorithmParameter>(); //параметры алгоритма, которые будут на верхней стороне квадрата. В каждой клетке квадрата будет комбинация из параметров, в этой клетке должен находится тестовый прогон с данной комбинацией параметров
        private TestRun[,] _testRunsMatrix; //двумерный массив с тестовыми прогонами. График будет строиться по этому массиву
        private double _cubeSideSize = 1; //размер стороны куба, в который вписывается график
        private float _cameraDistance = 1; //растояние от центра куба до камеры
        private int _countScaleValues = 5; //количество отрезков на шкале значений по оси критерия оценки
        private double _offsetScaleValues = 0.08; //отступ от графика на котором продолжается отрисовываться линия шкалы значений/параметров алгоритма, и после которого начинают отображаться значения шаклы значений/параметров алгоритма. Значение относительно стороны куба, в который вписывается график
        private double _scaleValueLineWidth = 0.0036; //толщина линии на шкале занчений относительно стороны куба, в который вписывается график

        private bool _isMouseDown = false; //зажата ли левая клавиша мыши
        private Point _mouseDownPosition; //позиция мыши при нажатии мыши
        private double _moveToRotateFactor = 0.01; //скольким радианам вращения соответствует 1 значение движения мыши
        private double _mouseDownСameraArcRotate; //значение угла в момент нажатия левой клавиши мыши
        private double _mouseDownСameraInArcRotate; //значение угла в момент нажатия левой клавиши мыши
        private double _cameraArcRotate = 0; //угол вращения дуги на которой расположена камера, вокруг центральной оси
        private double _cameraInArcRotate = 0; //угол на котором камера распологается на дуге
        public double Viewport3DWidth { get; set; } //ширина вьюпорта 3D, используется для определения двумерных координат на canvas трехмерной координаты 
        public double Viewport3DHeight { get; set; } //высота вьюпорта 3D, используется для определения двумерных координат на canvas трехмерной координаты
        public Canvas CanvasOn3D { get; set; }
        private List<Point3D> ScaleValuesLeftPoints = new List<Point3D>(); //список с коллекцией точек, которые соответствуют месту в котором нужно отрисовывать значение на шкале значений
        private List<Point3D> ScaleValuesRightPoints = new List<Point3D>(); //список с коллекцией точек, которые соответствуют месту в котором нужно отрисовывать значение на шкале значений


        private Point3D _cameraPosition;
        public Point3D CameraPosition
        {
            get { return _cameraPosition; }
            private set
            {
                _cameraPosition = value;
                OnPropertyChanged();
            }
        }
        private Vector3D _cameraLookDirection;
        public Vector3D CameraLookDirection
        {
            get { return _cameraLookDirection; }
            private set
            {
                _cameraLookDirection = value;
                OnPropertyChanged();
            }
        }
        private double _cameraWidth = 2;
        public double CameraWidth
        {
            get { return _cameraWidth; }
            set
            {
                _cameraWidth = value;
                OnPropertyChanged();
            }
        }

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

        private Model3D _scaleValuesLeftModel3D;
        public Model3D ScaleValuesLeftModel3D //плоскости, на которых отображаются шкалы значений
        {
            get { return _scaleValuesLeftModel3D; }
            private set
            {
                _scaleValuesLeftModel3D = value;
                OnPropertyChanged();
            }
        }

        private Model3D _scaleValuesRightModel3D;
        public Model3D ScaleValuesRightModel3D //плоскости, на которых отображаются шкалы значений
        {
            get { return _scaleValuesRightModel3D; }
            private set
            {
                _scaleValuesRightModel3D = value;
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

        public void MouseDown(Point position) //обрабатывает нажатие мыши для вращения камеры
        {
            _isMouseDown = true;
            _mouseDownPosition = position; //запоминаем позицию мыши в момент нажатия клавиши мыши
            _mouseDownСameraArcRotate = _cameraArcRotate; //запоминаем угол в момент нажатия клавиши мыши
            _mouseDownСameraInArcRotate = _cameraInArcRotate; //запоминаем угол в момент нажатия клавиши мыши
        }
        public void MouseMove(Point position) //обрабатывает движение мыши для вращения камеры
        {
            if (_isMouseDown)
            {
                double angleVertikal = (position.Y - _mouseDownPosition.Y) * _moveToRotateFactor;
                double angleHorizontal = (_mouseDownPosition.X - position.X) * _moveToRotateFactor;
                _cameraInArcRotate = _mouseDownСameraInArcRotate + angleVertikal;
                while(Math.Abs(_cameraInArcRotate) > 1.5707963) //ограничиваем вертикальное вращение до 90 градусов
                {
                    _cameraInArcRotate = _cameraInArcRotate > 0 ? 1.5707963 : -1.5707963;
                }
                _cameraArcRotate = _mouseDownСameraArcRotate + angleHorizontal;
                while(Math.Abs(_cameraArcRotate) > 6.28318530718) //если угол поворота превышет 360 градусов, вычитаем 360 градусов, 6.28318530718 радиан = 360 градусов
                {
                    _cameraArcRotate += _cameraArcRotate > 0 ? -6.28318530718 : 6.28318530718;
                }
                UpdateCameraPosition();
            }
        }
        public void MouseUp() //обрабатывает отпускание мыши для вращения камеры
        {
            _isMouseDown = false;
        }

        private void UpdateCameraPosition() //устанавливает позицию камеры в соответствии с выбранными углами _cameraArcRotate и _cameraInArcRotate
        {
            double x = _cameraDistance * Math.Cos(_cameraInArcRotate) * Math.Sin(_cameraArcRotate);
            double y = _cameraDistance * Math.Sin(_cameraInArcRotate);
            double z = _cameraDistance * Math.Cos(_cameraInArcRotate) * Math.Cos(_cameraArcRotate);
            CameraPosition = new Point3D(x, y, z);
            CameraLookDirection = new Vector3D(-x, -y, -z);
            UpdateScaleValuesRotation(); //обновляем угол вращения шкал значений, чтобы они всегда были напротив камеры
            Draw2D(); //отрисовываем 2D информацию
        }
        private void ResetCameraPosition() //сбрасывает положение камеры в значение по умолчанию
        {
            _cameraArcRotate = 0.52;
            _cameraInArcRotate = 0.26;
            UpdateCameraPosition();
            Draw2D(); //отрисовываем 2D информацию
        }

        private void UpdateScaleValuesRotation() //обновляет угол вращения шкал значений, чтобы они всегда были напротив камеры
        {
            if (ScaleValuesLeftModel3D != null)
            {
                double cameraArcRotatePositive = _cameraArcRotate < 0 ? _cameraArcRotate + 6.28318530718 : _cameraArcRotate; //угол в положительном значении
                double angle = Math.Truncate(cameraArcRotatePositive / 1.57079633) * 90;
                ScaleValuesLeftModel3D.Transform = GetRotateTransform3D(new Vector3D(0, 1, 0), angle - 90);
                ScaleValuesRightModel3D.Transform = GetRotateTransform3D(new Vector3D(0, 1, 0), angle - 180);
            }
        }

        private RotateTransform3D GetRotateTransform3D(Vector3D vector3D, double angle)
        {
            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            AxisAngleRotation3D axisAngleRotation3D = new AxisAngleRotation3D();
            axisAngleRotation3D.Axis = vector3D;
            axisAngleRotation3D.Angle = angle;
            rotateTransform3D.Rotation = axisAngleRotation3D;
            return rotateTransform3D;
        }

        private double _min = 0; //минимальное значение на графике
        private double _max = 0; //максимальное значение на графике

        private void DeterminingMinAndMaxValuesInEvaluationCriterias() //определяет минимальное и максимальное значения у выбранных критериев оценки
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

        private void DeterminingAlgorithmParamtersAxes() //определяет параметры алгоритма для левой и верхней оси матрицы тестовых прогонов. Параметры, соответствующие осям поисковой плоскости, находятся в конце списков
        {
            _leftAxisParameters.Clear();
            _topAxisParameters.Clear();
            if(_testing.Algorithm.AlgorithmParameters.Count == 1) //если параметр всего один
            {
                _leftAxisParameters.Add(AxesSearchPlanePageThreeDimensionChart[1].SelectedAlgorithmParameter);
            }
            else if(_testing.Algorithm.AlgorithmParameters.Count > 1) //если параметров два и более
            {
                _leftAxisParameters.Add(AxesSearchPlanePageThreeDimensionChart[1].SelectedAlgorithmParameter);
                _topAxisParameters.Add(AxesSearchPlanePageThreeDimensionChart[2].SelectedAlgorithmParameter);
                foreach(AlgorithmParameter algorithmParameter in _testing.Algorithm.AlgorithmParameters) //проходим по всем параметрам алгоритма, и добавляем в списки с параметрами осей
                {
                    if(_leftAxisParameters.IndexOf(algorithmParameter) == -1 && _topAxisParameters.IndexOf(algorithmParameter) == -1) //если данный параметр еще не был добавлен с списки с осями, добавляем его
                    {
                        if(_topAxisParameters.Count <= _leftAxisParameters.Count)
                        {
                            _topAxisParameters.Add(algorithmParameter);
                        }
                        else
                        {
                            _leftAxisParameters.Add(algorithmParameter);
                        }
                    }
                }
            }
            _leftAxisParameters.Reverse(); //переворачиваем массив чтобы параметры осей находились в конце списка
            _topAxisParameters.Reverse(); //переворачиваем массив чтобы параметры осей находились в конце списка
        }

        private void CreateTestRunsMatrix() //создает двумерный массив с тестовыми прогонами на основе выбранных осей плоскости поиска, на основе данного массива будет строиться график
        {
            int[] leftAxisCountParameterValues = new int[_leftAxisParameters.Count]; //массив, отображающий количество значений параметров
            int leftAxisTotalCountParameterValues = 1; //произведение всех элементов массива leftAxisCountParameterValues
            for (int i = 0; i < _leftAxisParameters.Count; i++)
            {
                int index = 0; //индекс текущего параметра в _testing.Algorithm.AlgorithmParameters
                while (_testing.Algorithm.AlgorithmParameters[index].Id != _leftAxisParameters[i].Id)
                {
                    index++;
                }
                leftAxisCountParameterValues[i] = _testing.AlgorithmParametersAllIntValues[index].Count == 0 ? _testing.AlgorithmParametersAllDoubleValues[index].Count : _testing.AlgorithmParametersAllIntValues[index].Count;
                leftAxisTotalCountParameterValues *= leftAxisCountParameterValues[i];
            }
            leftAxisTotalCountParameterValues = _leftAxisParameters.Count > 0 ? leftAxisTotalCountParameterValues : 1; //если нет параметров для строк, значит устанавливаем в 1, т.к. может быть параметр для столбцов

            int[] topAxisCountParameterValues = new int[_topAxisParameters.Count]; //массив, отображающий количество значений параметров
            int topAxisTotalCountParameterValues = 1; //произведение всех элементов массива topAxisCountParameterValues
            for (int i = 0; i < _topAxisParameters.Count; i++)
            {
                int index = 0; //индекс текущего параметра в _testing.Algorithm.AlgorithmParameters
                while (_testing.Algorithm.AlgorithmParameters[index].Id != _topAxisParameters[i].Id)
                {
                    index++;
                }
                topAxisCountParameterValues[i] = _testing.AlgorithmParametersAllIntValues[index].Count == 0 ? _testing.AlgorithmParametersAllDoubleValues[index].Count : _testing.AlgorithmParametersAllIntValues[index].Count;
                topAxisTotalCountParameterValues *= topAxisCountParameterValues[i];
            }
            topAxisTotalCountParameterValues = _topAxisParameters.Count > 0 ? topAxisTotalCountParameterValues : 0; //если нет параметров для столбцов, значит нет параметров вообще, и строить нечего => устанавливаем количество столбцов в 0
            int[] leftAxisCurrentParameterValues = Enumerable.Repeat(0, _leftAxisParameters.Count).ToArray(); //массив, содержащий индексы значений текущей комбинации параметров
            int[] topAxisCurrentParameterValues = Enumerable.Repeat(0, _topAxisParameters.Count).ToArray(); //массив, содержащий индексы значений текущей комбинации параметров 

            _testRunsMatrix = new TestRun[leftAxisTotalCountParameterValues, topAxisTotalCountParameterValues]; //определяем размер матрицы: количество столбцов на количество строк
            int matrixLineIndex = 0; //индекс строки матрицы
            int matrixColumnIndex = 0; //индекс колонки матрицы
            bool isEndLines = topAxisTotalCountParameterValues > 0 ? false : true; //вышли ли за количество имеющихся строк матрицы (если нет столбцов, значит нет параметров вообще и формировать матрицу не из чего)
            while(isEndLines == false)
            {
                //формируем текущую комбинацию параметров
                List<AlgorithmParameterValue> currentAlgorithmParameterValues = new List<AlgorithmParameterValue>(); //текущая комбинация значений параметров
                for(int i = 0; i < _leftAxisParameters.Count; i++) //проходим по параметрам _leftAxisParameters
                {
                    int index = 0; //индекс текущего параметра в _testing.Algorithm.AlgorithmParameters
                    while (_testing.Algorithm.AlgorithmParameters[index].Id != _leftAxisParameters[i].Id)
                    {
                        index++;
                    }
                    int valueIndex = leftAxisCurrentParameterValues[i]; //индекс значения параметра
                    currentAlgorithmParameterValues.Add(new AlgorithmParameterValue { AlgorithmParameter = _leftAxisParameters[i], IntValue = _leftAxisParameters[i].ParameterValueType.Id == 1 ? _testing.AlgorithmParametersAllIntValues[index][valueIndex] : 0, DoubleValue = _leftAxisParameters[i].ParameterValueType.Id == 1 ? 0 : _testing.AlgorithmParametersAllDoubleValues[index][valueIndex] }); //записали значение текущего параметра
                }
                for(int i = 0; i < _topAxisParameters.Count; i++) //проходим по параметрам _leftAxisParameters
                {
                    int index = 0; //индекс текущего параметра в _testing.Algorithm.AlgorithmParameters
                    while (_testing.Algorithm.AlgorithmParameters[index].Id != _topAxisParameters[i].Id)
                    {
                        index++;
                    }
                    int valueIndex = topAxisCurrentParameterValues[i]; //индекс значения параметра
                    currentAlgorithmParameterValues.Add(new AlgorithmParameterValue { AlgorithmParameter = _topAxisParameters[i], IntValue = _topAxisParameters[i].ParameterValueType.Id == 1 ? _testing.AlgorithmParametersAllIntValues[index][valueIndex] : 0, DoubleValue = _topAxisParameters[i].ParameterValueType.Id == 1 ? 0 : _testing.AlgorithmParametersAllDoubleValues[index][valueIndex] }); //записали значение текущего параметра
                }

                //ищем тестовый прогон с текущей комбинацией значений параметров
                bool isAllEqual = true; //совпадают ли все значения параметров текущего тестового прогона с текущей комбинацией значений параметров

                int testRunIndex = -1; //индекс тестового прогона
                do
                {
                    testRunIndex++; //увеличиваем индекс здесь, чтобы когда тестовый прогон будет найден, после выхода из цикла индекс сохранился на найденном тестовом прогоне
                    isAllEqual = true;
                    for (int i = 0; i < _testBatch.OptimizationTestRuns[testRunIndex].AlgorithmParameterValues.Count; i++)
                    {
                        if(_testBatch.OptimizationTestRuns[testRunIndex].AlgorithmParameterValues[i].AlgorithmParameter.ParameterValueType.Id == 1) //параметр типа int
                        {
                            isAllEqual = _testBatch.OptimizationTestRuns[testRunIndex].AlgorithmParameterValues[i].IntValue == currentAlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == _testBatch.OptimizationTestRuns[testRunIndex].AlgorithmParameterValues[i].AlgorithmParameter.Id).First().IntValue ? isAllEqual : false; //если параметры не равны, устанавливаем isAllEqual в false
                        }
                        else //параметр типа double
                        {
                            isAllEqual = _testBatch.OptimizationTestRuns[testRunIndex].AlgorithmParameterValues[i].DoubleValue == currentAlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == _testBatch.OptimizationTestRuns[testRunIndex].AlgorithmParameterValues[i].AlgorithmParameter.Id).First().DoubleValue ? isAllEqual : false; //если параметры не равны, устанавливаем isAllEqual в false
                        }
                    }
                }
                while (isAllEqual == false);
                _testRunsMatrix[matrixLineIndex, matrixColumnIndex] = _testBatch.OptimizationTestRuns[testRunIndex]; //записываем тестовый прогон в матрицу

                //переход на следующую колонку матрицы
                matrixColumnIndex++;
                topAxisCurrentParameterValues[topAxisCurrentParameterValues.Length - 1]++; //увеличиваем индекс значения последнего параметра в комбинации
                for (int i = topAxisCurrentParameterValues.Length - 2; i >= 0; i--)
                {
                    if(topAxisCurrentParameterValues[i + 1] > topAxisCountParameterValues[i + 1] - 1) //если индекс следующего в комбинации значений параметра превышает допустимый, обнуляем индекс следующего и увеличиваем на 1 индекс текущего
                    {
                        topAxisCurrentParameterValues[i + 1] = 0;
                        topAxisCurrentParameterValues[i]++;
                    }
                }

                //переход на следующую строку матрицы
                if(topAxisCurrentParameterValues[0] > topAxisCountParameterValues[0] - 1) //если индекс первого значения параметра, отражающего номер колонки первышает допустимый, значит нужно обнулить индексы колонок и перейти на следующую строку
                {
                    //обнуляем индексы колонки
                    matrixColumnIndex = 0;
                    for (int i = 0; i < topAxisCurrentParameterValues.Length; i++)
                    {
                        topAxisCurrentParameterValues[i] = 0;
                    }
                    //переходим на следующую строку матрицы
                    matrixLineIndex++;
                    leftAxisCurrentParameterValues[leftAxisCurrentParameterValues.Length - 1]++; //увеличиваем индекс значения последнего параметра, отражающего индекс строки в комбинации
                    for (int i = leftAxisCurrentParameterValues.Length - 2; i >= 0; i--)
                    {
                        if (leftAxisCurrentParameterValues[i + 1] > leftAxisCountParameterValues[i + 1] - 1) //если индекс следующего в комбинации значений параметра превышает допустимый, обнуляем индекс следующего и увеличиваем на 1 индекс текущего
                        {
                            leftAxisCurrentParameterValues[i + 1] = 0;
                            leftAxisCurrentParameterValues[i]++;
                        }
                    }

                    //если индекс первого параметра в комбинации первышает допустимый, значит мы вышли за пределы матрицы, выходим из цикла
                    if(leftAxisCurrentParameterValues[0] > leftAxisCountParameterValues[0] - 1)
                    {
                        isEndLines = true;
                    }
                }
            }
        }

        private void BuildScaleValues() //строит шкалы значений
        {
            ScaleValuesRightPoints.Clear();
            ScaleValuesLeftPoints.Clear();
            /*DiffuseMaterial secondLineDiffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(226, 239, 218)));
            DiffuseMaterial firstLineDiffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(198, 224, 180)));*/
            DiffuseMaterial lineDiffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(0, 0, 0)));
            DiffuseMaterial planeDiffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(72, 255, 255, 255)));
            Model3DGroup model3DGroupLeft = new Model3DGroup();
            for(int i = 1; i < _countScaleValues + 2; i++)
            {
                GeometryModel3D geometryModel3DLines = new GeometryModel3D();
                MeshGeometry3D meshGeometry3DLines = new MeshGeometry3D();
                Point3DCollection positionsCollectionLines = new Point3DCollection();
                Int32Collection triangleIndicesCollectionLines = new Int32Collection();
                GeometryModel3D geometryModel3DPlanes = new GeometryModel3D();
                MeshGeometry3D meshGeometry3DPlanes = new MeshGeometry3D();
                Point3DCollection positionsCollectionPlanes = new Point3DCollection();
                Int32Collection triangleIndicesCollectionPlanes = new Int32Collection();

                double lineYBottom = -_cubeSideSize / 2 + _cubeSideSize / _countScaleValues * (i - 1) - _scaleValueLineWidth / 2; //нижняя координата Y для линии
                double planeYTop = -_cubeSideSize / 2 + _cubeSideSize / _countScaleValues * i - _scaleValueLineWidth / 2; //верхняя координата Y для плоскости фона
                if (i == _countScaleValues + 1)
                {
                    planeYTop = _cubeSideSize / 2 + _offsetScaleValues * _cubeSideSize;
                }

                //нижняя горизонтальная линия
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, lineYBottom, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom, _cubeSideSize / 2));
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);

                //левая вертикальная линия
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 - _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 - _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);

                //правая вертикальная линия
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2, lineYBottom, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2, lineYBottom, _cubeSideSize / 2));
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);

                //левая плоскость
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 - _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);

                //правая плоскость
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);

                /*double yBottom = -_cubeSideSize / 2 + _cubeSideSize / 5 * (i - 1);
                double yTop = -_cubeSideSize / 2 + _cubeSideSize / 5 * i;
                if (i == _countScaleValues + 1)
                {
                    yTop = _cubeSideSize / 2 + _offsetScaleValues * _cubeSideSize;
                }
                positionsCollection.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, yTop, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(_cubeSideSize / 2, yBottom, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, yBottom, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, yTop, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(_cubeSideSize / 2, yTop, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(_cubeSideSize / 2, yBottom, _cubeSideSize / 2));
                triangleIndicesCollection.Add(0);
                triangleIndicesCollection.Add(1);
                triangleIndicesCollection.Add(2);
                triangleIndicesCollection.Add(3);
                triangleIndicesCollection.Add(4);
                triangleIndicesCollection.Add(5);*/
                meshGeometry3DLines.Positions = positionsCollectionLines;
                meshGeometry3DLines.TriangleIndices = triangleIndicesCollectionLines;
                geometryModel3DLines.Geometry = meshGeometry3DLines;
                geometryModel3DLines.Material = lineDiffuseMaterial;
                model3DGroupLeft.Children.Add(geometryModel3DLines);
                meshGeometry3DPlanes.Positions = positionsCollectionPlanes;
                meshGeometry3DPlanes.TriangleIndices = triangleIndicesCollectionPlanes;
                geometryModel3DPlanes.Geometry = meshGeometry3DPlanes;
                geometryModel3DPlanes.Material = planeDiffuseMaterial;
                model3DGroupLeft.Children.Add(geometryModel3DPlanes);
                ScaleValuesRightPoints.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, lineYBottom + _scaleValueLineWidth / 2, _cubeSideSize / 2));
            }
            ScaleValuesRightModel3D = model3DGroupLeft;

            Model3DGroup model3DGroupRight = new Model3DGroup();
            for(int i = 1; i < _countScaleValues + 2; i++)
            {
                GeometryModel3D geometryModel3DLines = new GeometryModel3D();
                MeshGeometry3D meshGeometry3DLines = new MeshGeometry3D();
                Point3DCollection positionsCollectionLines = new Point3DCollection();
                Int32Collection triangleIndicesCollectionLines = new Int32Collection();
                GeometryModel3D geometryModel3DPlanes = new GeometryModel3D();
                MeshGeometry3D meshGeometry3DPlanes = new MeshGeometry3D();
                Point3DCollection positionsCollectionPlanes = new Point3DCollection();
                Int32Collection triangleIndicesCollectionPlanes = new Int32Collection();

                double lineYBottom = -_cubeSideSize / 2 + _cubeSideSize / _countScaleValues * (i - 1) - _scaleValueLineWidth / 2; //нижняя координата Y для линии
                double planeYTop = -_cubeSideSize / 2 + _cubeSideSize / _countScaleValues * i - _scaleValueLineWidth / 2; //верхняя координата Y для плоскости фона
                if (i == _countScaleValues + 1)
                {
                    planeYTop = _cubeSideSize / 2 + _offsetScaleValues * _cubeSideSize;
                }

                //нижняя горизонтальная линия
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 + _cubeSideSize * _offsetScaleValues, lineYBottom, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 + _cubeSideSize * _offsetScaleValues, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 + _cubeSideSize * _offsetScaleValues, lineYBottom, _cubeSideSize / 2));
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);

                //левая вертикальная линия
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2, lineYBottom, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom, _cubeSideSize / 2));
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);

                //правая вертикальная линия
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 + _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionLines.Add(new Point3D(_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);
                triangleIndicesCollectionLines.Add(triangleIndicesCollectionLines.Count);

                //левая плоскость
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(-_cubeSideSize / 2 + _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 - _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);

                //правая плоскость
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 + _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 + _cubeSideSize * _offsetScaleValues, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 + _scaleValueLineWidth / 2, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 + _scaleValueLineWidth / 2, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 + _cubeSideSize * _offsetScaleValues, planeYTop, _cubeSideSize / 2));
                positionsCollectionPlanes.Add(new Point3D(_cubeSideSize / 2 + _cubeSideSize * _offsetScaleValues, lineYBottom + _scaleValueLineWidth, _cubeSideSize / 2));
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);
                triangleIndicesCollectionPlanes.Add(triangleIndicesCollectionPlanes.Count);

                /*double yBottom = -_cubeSideSize / 2 + _cubeSideSize / 5 * (i - 1);
                double yTop = -_cubeSideSize / 2 + _cubeSideSize / 5 * i;
                if (i == _countScaleValues + 1)
                {
                    yTop = _cubeSideSize / 2 + _offsetScaleValues * _cubeSideSize;
                }
                positionsCollection.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, yTop, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(_cubeSideSize / 2, yBottom, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, yBottom, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(-_cubeSideSize / 2 - _cubeSideSize * _offsetScaleValues, yTop, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(_cubeSideSize / 2, yTop, _cubeSideSize / 2));
                positionsCollection.Add(new Point3D(_cubeSideSize / 2, yBottom, _cubeSideSize / 2));
                triangleIndicesCollection.Add(0);
                triangleIndicesCollection.Add(1);
                triangleIndicesCollection.Add(2);
                triangleIndicesCollection.Add(3);
                triangleIndicesCollection.Add(4);
                triangleIndicesCollection.Add(5);*/
                meshGeometry3DLines.Positions = positionsCollectionLines;
                meshGeometry3DLines.TriangleIndices = triangleIndicesCollectionLines;
                geometryModel3DLines.Geometry = meshGeometry3DLines;
                geometryModel3DLines.Material = lineDiffuseMaterial;
                model3DGroupRight.Children.Add(geometryModel3DLines);
                meshGeometry3DPlanes.Positions = positionsCollectionPlanes;
                meshGeometry3DPlanes.TriangleIndices = triangleIndicesCollectionPlanes;
                geometryModel3DPlanes.Geometry = meshGeometry3DPlanes;
                geometryModel3DPlanes.Material = planeDiffuseMaterial;
                model3DGroupRight.Children.Add(geometryModel3DPlanes);
                ScaleValuesLeftPoints.Add(new Point3D(_cubeSideSize / 2 + _cubeSideSize * _offsetScaleValues, lineYBottom + _scaleValueLineWidth / 2, _cubeSideSize / 2));
            }
            ScaleValuesLeftModel3D = model3DGroupRight;

            UpdateScaleValuesRotation(); //обновляем угол вращения шкал значений, чтобы они были напротив камеры
        }
        private Point Convert3DPointTo2D(Point3D point3D) //переводит трехмерные координаты точки в двумерные координаты на вьюпорте
        {
            double x2D = -point3D.Z * Math.Sin(_cameraArcRotate) + point3D.X * Math.Cos(_cameraArcRotate); //-z*sin(a)+x*cos(a)
            double y2D = -(point3D.Z * Math.Cos(_cameraArcRotate) + point3D.X * Math.Sin(_cameraArcRotate)) * Math.Sin(_cameraInArcRotate) + point3D.Y * Math.Cos(_cameraInArcRotate); //-(z*cos(a)+x*sin(a))*sin(b)+y*Cos(b)
            double widthOffset = CameraWidth / 2; //смещение, которое покажет значение не в системе координат где левый край находится на -CameraWidth/2, а где левый край находится на 0 и любая точка находится от 0 до CameraWidth
            x2D = x2D + widthOffset;
            x2D = Viewport3DWidth * (x2D / CameraWidth); //умножаем ширину вьюпорта на ту часть, которую занимает координата от ширины вьюпорта, и получаем координату в пикселях
            double cameraHeight = Viewport3DHeight / Viewport3DWidth * CameraWidth; //получаем величину в высоту, которую покрывает камера вьюпорта
            double heightOffset = cameraHeight / 2;
            y2D = y2D + heightOffset;
            y2D = Viewport3DHeight * (1 - (y2D / cameraHeight));
            return new Point(x2D, y2D);
        }

        private void Draw2D() //отрисовывает 2D информацию
        {
            if (isLoadingTestResultComplete)
            {
                CanvasOn3D.Children.Clear();
                //определяем количество знаков после запятой, до которых нужно округлять значение
                double permissibleError = 0.01; //допустимая погрешность, значение будет округляться не больше чем на данную часть от диапазона значений
                double range = _max - _min;
                double permissibleErrorRange = range * permissibleError;
                int digits = 0; //количество знаков после запятой, до которого нужно округлять значения шкалы значений
                while (permissibleErrorRange * Math.Pow(10, digits) < 1)
                {
                    digits++;
                }

                double textHeight = 9;
                double marginByScaleValues = 5; //отступ влево от левой шкалы и вправо от правой
                double symbolMinusWidth = 5; //ширина сивола -
                double symbolCommaWidth = 3; //ширина символа ,
                double symbolDigitWidth = 6; //ширина символа цифры

                //отрисовываем значения шкал значений
                for (int i = 0; i < ScaleValuesLeftPoints.Count; i++)
                {
                    double value = _min + range * i / (ScaleValuesLeftPoints.Count - 1.0);
                    string text = Math.Round(value, digits).ToString();
                    //определяем ширину теста
                    double textWidth = 0;
                    foreach(char c in text)
                    {
                        if(c == '-')
                        {
                            textWidth += symbolMinusWidth;
                        }
                        if(c == '.')
                        {
                            textWidth += symbolCommaWidth;
                        }
                        if ("1234567890".Contains(c))
                        {
                            textWidth += symbolDigitWidth;
                        }
                    }

                    TextBlock textBlockLeft = new TextBlock();
                    textBlockLeft.Text = Math.Round(value, digits).ToString();
                    Transform3DGroup transform3DGroupLeft = new Transform3DGroup();
                    transform3DGroupLeft.Children.Add(ScaleValuesLeftModel3D.Transform);
                    Point3D point3DLeftTransformed = transform3DGroupLeft.Transform(ScaleValuesLeftPoints[i]); //поворачиваем координату в соответствии с поворотом шкалы занчений
                    Point coordinate2DLeft = Convert3DPointTo2D(point3DLeftTransformed);
                    textBlockLeft.Margin = new Thickness(coordinate2DLeft.X - textWidth - marginByScaleValues, coordinate2DLeft.Y - textHeight, 0, 0);
                    CanvasOn3D.Children.Add(textBlockLeft);

                    TextBlock textBlockRight = new TextBlock();
                    textBlockRight.Text = Math.Round(value, digits).ToString();
                    Transform3DGroup transform3DGroupRight = new Transform3DGroup();
                    transform3DGroupRight.Children.Add(ScaleValuesRightModel3D.Transform);
                    Point3D point3DRightTransformed = transform3DGroupRight.Transform(ScaleValuesRightPoints[i]); //поворачиваем координату в соответствии с поворотом шкалы занчений
                    Point coordinate2DRight = Convert3DPointTo2D(point3DRightTransformed);
                    textBlockRight.Margin = new Thickness(coordinate2DRight.X + marginByScaleValues, coordinate2DRight.Y - textHeight, 0, 0);
                    CanvasOn3D.Children.Add(textBlockRight);
                }
            }
        }

        private void BuildSurfaces() //строит поверхности выбранных критериев оценки
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            foreach (EvaluationCriteriaPageThreeDimensionChart evaluationCriteriaPageThreeDimensionChart in EvaluationCriteriasPageThreeDimensionChart.Where(j => j.IsChecked))
            {
                LinearGradientBrush linearGradientBrush = new LinearGradientBrush(); //кисть с градиентной заливкой
                double offsetRed = 0; //координата красного цвета
                double offsetGreen = 1; //координата зеленого цвета
                if (evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.IsHaveBestAndWorstValue) //если указаны лучшее и худшее значения, устанавливаем координату цвета в данные значения
                {
                    offsetGreen = (evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.BestValue - _min) / (_max - _min);
                    offsetRed = (evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.WorstValue - _min) / (_max - _min);
                }
                if(evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.IsBestPositive == false) //если лучшим считается минимальное значение, меняем цвета, чтобы минимум был зеленым, а максимум - красным
                {
                    double green = offsetGreen;
                    offsetGreen = offsetRed;
                    offsetRed = green;
                }
                linearGradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromRgb(255, 0, 0), Offset = offsetRed });
                linearGradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromRgb(0, 240, 0), Offset = offsetGreen });
                double range = _max - _min; //диапазон значений
                GeometryModel3D geometryModel3D = new GeometryModel3D();
                MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
                Point3DCollection positionsCollection = new Point3DCollection();
                PointCollection textureCoordinates = new PointCollection();
                Int32Collection triangleIndicesCollection = new Int32Collection();
                int lines = _testRunsMatrix.GetLength(0);
                int columns = _testRunsMatrix.GetLength(1);
                for (int x = 1; x < lines; x++)
                {
                    for(int y = 1; y < columns; y++)
                    {
                        double point1Value = _testRunsMatrix[x - 1, y - 1].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria.Id == evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.Id).First().DoubleValue;
                        double point2Value = _testRunsMatrix[x - 1, y].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria.Id == evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.Id).First().DoubleValue;
                        double point3Value = _testRunsMatrix[x, y - 1].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria.Id == evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.Id).First().DoubleValue;
                        double point4Value = _testRunsMatrix[x, y].EvaluationCriteriaValues.Where(j => j.EvaluationCriteria.Id == evaluationCriteriaPageThreeDimensionChart.EvaluationCriteria.Id).First().DoubleValue;

                        Point3D point1 = new Point3D((x - 1) / (double)(lines - 1) * _cubeSideSize - _cubeSideSize / 2, (point1Value - _min) / range * _cubeSideSize - _cubeSideSize / 2, (y - 1) / (double)(columns - 1) * _cubeSideSize - _cubeSideSize / 2);
                        Point3D point2 = new Point3D((x - 1) / (double)(lines - 1) * _cubeSideSize - _cubeSideSize / 2, (point2Value - _min) / range * _cubeSideSize - _cubeSideSize / 2, (y) / (double)(columns - 1) * _cubeSideSize - _cubeSideSize / 2);
                        Point3D point3 = new Point3D((x) / (double)(lines - 1) * _cubeSideSize - _cubeSideSize / 2, (point3Value - _min) / range * _cubeSideSize - _cubeSideSize / 2, (y - 1) / (double)(columns - 1) * _cubeSideSize - _cubeSideSize / 2);
                        Point3D point4 = new Point3D((x) / (double)(lines - 1) * _cubeSideSize - _cubeSideSize / 2, (point4Value - _min) / range * _cubeSideSize - _cubeSideSize / 2, (y) / (double)(columns - 1) * _cubeSideSize - _cubeSideSize / 2);
                        positionsCollection.Add(point1);
                        positionsCollection.Add(point2);
                        positionsCollection.Add(point3);
                        positionsCollection.Add(point2);
                        positionsCollection.Add(point4);
                        positionsCollection.Add(point3);
                        textureCoordinates.Add(new Point((point1Value - _min) / range, (point1Value - _min) / range));
                        textureCoordinates.Add(new Point((point2Value - _min) / range, (point2Value - _min) / range));
                        textureCoordinates.Add(new Point((point3Value - _min) / range, (point3Value - _min) / range));
                        textureCoordinates.Add(new Point((point2Value - _min) / range, (point2Value - _min) / range));
                        textureCoordinates.Add(new Point((point4Value - _min) / range, (point4Value - _min) / range));
                        textureCoordinates.Add(new Point((point3Value - _min) / range, (point3Value - _min) / range));
                        /*textureCoordinates.Add(new Point((point1Value - _min) / range, 0));
                        textureCoordinates.Add(new Point((point2Value - _min) / range, 0));
                        textureCoordinates.Add(new Point((point3Value - _min) / range, 0));
                        textureCoordinates.Add(new Point((point2Value - _min) / range, 0));
                        textureCoordinates.Add(new Point((point4Value - _min) / range, 0));
                        textureCoordinates.Add(new Point((point3Value - _min) / range, 0));*/
                        triangleIndicesCollection.Add(triangleIndicesCollection.Count);
                        triangleIndicesCollection.Add(triangleIndicesCollection.Count);
                        triangleIndicesCollection.Add(triangleIndicesCollection.Count);
                        triangleIndicesCollection.Add(triangleIndicesCollection.Count);
                        triangleIndicesCollection.Add(triangleIndicesCollection.Count);
                        triangleIndicesCollection.Add(triangleIndicesCollection.Count);
                    }
                }
                meshGeometry3D.Positions = positionsCollection;
                meshGeometry3D.TextureCoordinates = textureCoordinates;
                meshGeometry3D.TriangleIndices = triangleIndicesCollection;
                geometryModel3D.Geometry = meshGeometry3D;
                geometryModel3D.Material = new DiffuseMaterial(linearGradientBrush);
                geometryModel3D.BackMaterial = new DiffuseMaterial(linearGradientBrush);
                model3DGroup.Children.Add(geometryModel3D);
            }
            SurfacesModel3D = model3DGroup;
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
                if(EvaluationCriteriasPageThreeDimensionChart.Where(j=>j.IsChecked).Any()) //если выбран хоть один критерий оценки, строим поверхности
                {
                    if (isLoadingTestResultComplete)
                    {
                        DeterminingMinAndMaxValuesInEvaluationCriterias(); //определяем минимальное и максимальное значения у выбранных критериев оценки
                        BuildSurfaces(); //строим поверхности выбранных критериев оценки
                        Draw2D(); //отрисовываем 2D информацию
                    }
                }
                else //иначе выбираем тот что был снят чтобы всегда был выбран хотя бы один
                {
                    evaluationCriteriaPageThreeDimensionChart.IsChecked = true;
                }
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
            int index = 1;
            bool isFind = false;
            while(isFind == false)
            {
                if(EvaluationCriteriasPageThreeDimensionChart[index].EvaluationCriteria.Id == _testing.TopModelCriteria.EvaluationCriteria.Id)
                {
                    EvaluationCriteriasPageThreeDimensionChart[index].IsChecked = true; //выбираем критерий оценки топ-модели
                    isFind = true;
                }
                index++;
            }
            for (int i = 1; i < EvaluationCriteriasPageThreeDimensionChart.Count; i++)
            {
                if (EvaluationCriteriasPageThreeDimensionChart[i].EvaluationCriteria.Id != _testing.TopModelCriteria.EvaluationCriteria.Id) //если это не критерий оценки для оределения топ-модели, сбрасываем его
                {
                    if (EvaluationCriteriasPageThreeDimensionChart[i].IsChecked)
                    {
                        EvaluationCriteriasPageThreeDimensionChart[i].IsChecked = false;
                    }
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
                    if(AxesSearchPlanePageThreeDimensionChart.Count > 2)
                    {
                        AxesSearchPlanePageThreeDimensionChart[1].AlgorithmParameters = GetAlgorithmParameters();
                        AxesSearchPlanePageThreeDimensionChart[2].AlgorithmParameters = GetAlgorithmParameters();

                        AxesSearchPlanePageThreeDimensionChart[1].SelectedAlgorithmParameter = AxesSearchPlanePageThreeDimensionChart[1].AlgorithmParameters.Where(j => j.Id == _testBatch.AxesTopModelSearchPlane[0].AlgorithmParameter.Id).First();
                        AxesSearchPlanePageThreeDimensionChart[2].SelectedAlgorithmParameter = AxesSearchPlanePageThreeDimensionChart[2].AlgorithmParameters.Where(j => j.Id == _testBatch.AxesTopModelSearchPlane[1].AlgorithmParameter.Id).First();
                    }
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
                DeterminingAlgorithmParamtersAxes(); //определяем параметры алгоритма для левой и верхней оси матрицы тестовых прогонов
                CreateTestRunsMatrix(); //создаем двумерный массив с тестовыми прогонами на основе выбранных осей плоскости поиска
                BuildSurfaces(); //строим поверхности выбранных критериев оценки
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
                isLoadingTestResultComplete = false; //устанавливаем чтобы поверхности не строились при выборе критерия оценки пока не создана матрица тестовых прогонов
                _testing = ViewModelPageTestingResult.getInstance().TestingResult;
                _testBatch = ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox.TestBatch;
                ResetEvaluationCriteriasPageThreeDimensionChart(); //сбрасываем выбранные критерии оценки на тот что используется для определения топ-модели
                LevelsPageThreeDimensionChart.Clear();
                LevelsPageThreeDimensionChart.Add(LevelPageThreeDimensionChart.CreateButtonAddLevel(LevelPageThreeDimensionChart_PropertyChanged)); //создаем кнопку добавить для меню уровней
                CreateAxesSearchPlanePageThreeDimensionChart(); //создаем элементы для меню выбора осей плоскости поиска топ-модели

                DeterminingMinAndMaxValuesInEvaluationCriterias(); //определяем минимальное и максимальное значения у выбранных критериев оценки
                DeterminingAlgorithmParamtersAxes(); //определяем параметры алгоритма для левой и верхней оси матрицы тестовых прогонов
                CreateTestRunsMatrix(); //создаем двумерный массив с тестовыми прогонами на основе выбранных осей плоскости поиска
                BuildScaleValues(); //строим шкалы значений
                BuildSurfaces(); //строим поверхности выбранных критериев оценки
                isLoadingTestResultComplete = true;
                Draw2D(); //отрисовываем 2D информацию
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
        public ICommand ResetCamera_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ResetCameraPosition();
                }, (obj) => true);
            }
        }
        public ICommand ShowInfo_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    double x = -0.5;
                    double y = -0.5;
                    double z = 0.5;
                    double x2D = -z * Math.Sin(_cameraArcRotate) + x * Math.Cos(_cameraArcRotate);//-z*sin(a)+x*cos(a)  _cameraArcRotate _cameraInArcRotate
                    double y2D = -(z * Math.Cos(_cameraArcRotate) + x * Math.Sin(_cameraArcRotate)) * Math.Sin(_cameraInArcRotate) + y * Math.Cos(_cameraInArcRotate); //-(z*cos(a)+x*sin(a))*sin(b)+y*Cos(b)
                    double widthOffset = CameraWidth / 2; //смещение, которое покажет значение не в системе координат где левый край находится на -CameraWidth/2, а где левый край находится на 0 и любая точка находится от 0 до CameraWidth
                    x2D = x2D + widthOffset;
                    x2D = Viewport3DWidth * (x2D / CameraWidth); //умножаем ширину вьюпорта на ту часть, которую занимает координата от ширины вьюпорта, и получаем координату в пикселях
                    double cameraHeight = Viewport3DHeight / Viewport3DWidth * CameraWidth; //получаем величину в высоту, которую покрывает камера вьюпорта
                    double heightOffset = cameraHeight / 2;
                    y2D = y2D + heightOffset;
                    y2D = Viewport3DHeight * (1 - (y2D / cameraHeight));
                    /*double x = 0;
                    double y = 0;
                    double z = 1;
                    double x2D = -z * Math.Sin(1.571) + x * Math.Cos(1.571);
                    double y2D = -(z * Math.Cos(1.571) + x * Math.Sin(1.571)) * Math.Sin(1.571) + y * Math.Cos(1.571);*/
                    MessageBox.Show("Point3D(-0.5,-0.5,0.5) == x2D=" + x2D.ToString() + " y2D=" + y2D.ToString());
                }, (obj) => true);
            }
        }
    }
}
