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
    /// Логика взаимодействия для PageTradeChart.xaml
    /// </summary>
    public partial class PageTradeChart : Page
    {
        public PageTradeChart()
        {
            InitializeComponent();
            _viewModelPageTradeChart = new ViewModelPageTradeChart();
            DataContext = _viewModelPageTradeChart;
        }

        private ViewModelPageTradeChart _viewModelPageTradeChart;

        private void canvasChart_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModelPageTradeChart.СanvasTradeChartWidth = canvasChart.ActualWidth;
            _viewModelPageTradeChart.СanvasTradeChartHeight = canvasChart.ActualHeight;
        }
    }
}
