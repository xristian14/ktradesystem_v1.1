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
    /// Логика взаимодействия для PageProfitChart.xaml
    /// </summary>
    public partial class PageProfitChart : Page
    {
        public PageProfitChart()
        {
            InitializeComponent();
            _viewModelPageProfitChart = new ViewModelPageProfitChart();
            DataContext = _viewModelPageProfitChart;
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); //без этого actualHeight и actualWidth будут нулями
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height)); //без этого actualHeight и actualWidth будут нулями
            _viewModelPageProfitChart.СanvasProfitChartWidth = canvasProfitChart.ActualWidth;
            _viewModelPageProfitChart.СanvasProfitChartHeight = canvasProfitChart.ActualHeight;
        }
        private ViewModelPageProfitChart _viewModelPageProfitChart;
    }
}
