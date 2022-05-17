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
        }

        ViewModelPageTheeDimensionChart _viewModelPageTheeDimensionChart;

        private void canvasOn3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModelPageTheeDimensionChart.MouseDown(e.GetPosition(sender as IInputElement));
        }

        private void canvasOn3D_MouseMove(object sender, MouseEventArgs e)
        {
            _viewModelPageTheeDimensionChart.MouseMove(e.GetPosition(sender as IInputElement));
        }

        private void canvasOn3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _viewModelPageTheeDimensionChart.MouseUp();
        }

        private void viewport3D_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModelPageTheeDimensionChart.Viewport3DWidth = viewport3D.ActualWidth;
            _viewModelPageTheeDimensionChart.Viewport3DHeight = viewport3D.ActualHeight;
        }
    }
}
