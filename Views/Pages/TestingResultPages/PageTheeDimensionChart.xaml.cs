using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ktradesystem.ViewModels;

namespace ktradesystem.Views.Pages.TestingResultPages
{
    /// <summary>
    /// Логика взаимодействия для PageTheeDimensionChart.xaml
    /// </summary>
    public partial class PageTheeDimensionChart : Page
    {
        public PageTheeDimensionChart()
        {
            InitializeComponent();
            _viewModelPageTheeDimensionChart = new ViewModelPageTheeDimensionChart();
            DataContext = _viewModelPageTheeDimensionChart;
            _viewModelPageTheeDimensionChart.Viewport3D = viewport3D;
            _viewModelPageTheeDimensionChart.CanvasOn3D = canvasOn3D;
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); //без этого actualHeight и actualWidth будут нулями
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height)); //без этого actualHeight и actualWidth будут нулями
        }

        ViewModelPageTheeDimensionChart _viewModelPageTheeDimensionChart;

        private void canvasOn3DForMouseEvents_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModelPageTheeDimensionChart.MouseDown(e.GetPosition(sender as IInputElement));
        }

        private void canvasOn3DForMouseEvents_MouseMove(object sender, MouseEventArgs e)
        {
            _viewModelPageTheeDimensionChart.MouseMove(e.GetPosition(sender as IInputElement));
        }

        private void canvasOn3DForMouseEvents_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _viewModelPageTheeDimensionChart.MouseUp();
        }
    }
}
