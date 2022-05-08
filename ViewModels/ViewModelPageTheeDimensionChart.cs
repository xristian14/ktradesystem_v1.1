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
            CreateEvaluationCriteriasPageThreeDimensionChart(); //создаем критерии оценки для меню выбора критериев оценки
            CreateLevelsPageThreeDimensionChart(); //создаем кнопку добавить для меню уровней
        }

        private TestBatch _testBatch; //тестовая связка, на основании которой будет строиться график
        private double _sizeChartSide = 1; //размер стороны куба, в который вписывается график
        private int _countScaleValues = 7; //количество отрезков на шкале значений по оси критерия оценки. К этому значению будет стремиться количество отрезков
        private double _scaleValuesPlaneOffset = 0.002; //величина смещения плоскости от размера стороны _sizeChartSide. Плоскость будет смещена на _sizeChartSide*_scaleValuesPlaneOffset относительно линий на шкале значений
        private double _averageOpacity = 1; //средняя прозрачность для поверхностей графика, плоскостей уровней и выделения плоскостей поиска

        private float _cameraDistance; //расстояние камеры до центра вращения

        private Model3DGroup _chartScaleValuesFront;
        public Model3DGroup ChartScaleValuesFront //шкала со значениями спереди
        {
            get { return _chartScaleValuesFront; }
            private set
            {
                _chartScaleValuesFront = value;
                OnPropertyChanged();
            }
        }
        private Model3DGroup _chartScaleValuesLeft;
        public Model3DGroup ChartScaleValuesLeft //шкала со значениями слева
        {
            get { return _chartScaleValuesLeft; }
            private set
            {
                _chartScaleValuesLeft = value;
                OnPropertyChanged();
            }
        }
        private Model3DGroup _chartScaleValuesBack;
        public Model3DGroup ChartScaleValuesBack //шкала со значениями сзади
        {
            get { return _chartScaleValuesBack; }
            private set
            {
                _chartScaleValuesBack = value;
                OnPropertyChanged();
            }
        }
        private Model3DGroup _chartScaleValuesRight;
        public Model3DGroup ChartScaleValuesRight //шкала со значениями справа
        {
            get { return _chartScaleValuesRight; }
            private set
            {
                _chartScaleValuesRight = value;
                OnPropertyChanged();
            }
        }
        private Model3DGroup _chartScaleValuesTop;
        public Model3DGroup ChartScaleValuesTop //шкала со значениями сверху
        {
            get { return _chartScaleValuesTop; }
            private set
            {
                _chartScaleValuesTop = value;
                OnPropertyChanged();
            }
        }
        private Model3DGroup _chartScaleValuesBottom;
        public Model3DGroup ChartScaleValuesBottom //шкала со значениями снизу
        {
            get { return _chartScaleValuesBottom; }
            private set
            {
                _chartScaleValuesBottom = value;
                OnPropertyChanged();
            }
        }

        private Model3DGroup _chartSurfaces;
        public Model3DGroup ChartSurfaces //поверхности графика
        {
            get { return _chartSurfaces; }
            private set
            {
                _chartSurfaces = value;
                OnPropertyChanged();
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

        public void CreateChartScalesValues() //создает шкалы значений
        {
            if(_testBatch != null)
            {
                //определяем значения на шкале по оси критерия оценки

                //создаем переднюю шкалу со значениями
                ChartScaleValuesFront = new Model3DGroup();
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
                }
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
        private void CreateEvaluationCriteriasPageThreeDimensionChart() //создает критерии оценки для представления. Добавляет только те, которые имеют числовое значение
        {
            EvaluationCriteriasPageThreeDimensionChart.Clear();
            EvaluationCriteriasPageThreeDimensionChart.Add(new EvaluationCriteriaPageThreeDimensionChart { ButtonResetVisibility = Visibility.Visible, CheckBoxVisibility=Visibility.Collapsed }); //добавляем кнопку сбросить критерии оценки

            //добавляем критерии оценки
            foreach (EvaluationCriteria evaluationCriteria in ViewModelPageTesting.getInstance().EvaluationCriterias)
            {
                if (evaluationCriteria.IsDoubleValue)
                {
                    EvaluationCriteriasPageThreeDimensionChart.Add(new EvaluationCriteriaPageThreeDimensionChart { EvaluationCriteria = evaluationCriteria, IsChecked = false, ButtonResetVisibility = Visibility.Collapsed, CheckBoxVisibility = Visibility.Visible });
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
        public void CreateLevelsPageThreeDimensionChart() //создает уровни. В данном случае только один - кнопку добавить уровень.
        {
            LevelsPageThreeDimensionChart.Clear();
            LevelsPageThreeDimensionChart.Add(LevelPageThreeDimensionChart.CreateButtonAddLevel(LevelPageThreeDimensionChart_PropertyChanged));
        }

        public void LevelPageThreeDimensionChart_PropertyChanged(LevelPageThreeDimensionChart levelPageThreeDimensionChart, string propertyName, string propertyValue)
        {
            if(propertyName == "IsButtonAddLevelChecked") //если была переключена кнопка: Добавить уровень
            {
                if(propertyValue == "True") //если кнопка в состоянии true, добавляем уровень
                {
                    LevelsPageThreeDimensionChart.Add(LevelPageThreeDimensionChart.CreateLevel(LevelPageThreeDimensionChart_PropertyChanged, -50, 50, 0));
                    levelPageThreeDimensionChart.IsButtonAddLevelChecked = false;
                }
            }
            if(propertyName == "IsDeleteChecked") //если была переключена кнопка: Удалить, удаляем уровень
            {
                LevelsPageThreeDimensionChart.Remove(levelPageThreeDimensionChart);
            }
        }









        public void UpdatePage() //обновляет страницу
        {
            if(ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox != null)
            {
                _testBatch = ViewModelPageTestingResult.getInstance().SelectedTestBatchTestingResultCombobox.TestBatch;
            }
        }
        public ICommand ResetCamera_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CreateEvaluationCriteriasPageThreeDimensionChart();
                }, (obj) => true);
            }
        }
        public ICommand ResetEvaluationCriterias_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CreateEvaluationCriteriasPageThreeDimensionChart();
                }, (obj) => true);
            }
        }
    }
}
