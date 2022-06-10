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
            _viewModelPageTradeChart = ViewModelPageTradeChart.getInstance();
            DataContext = _viewModelPageTradeChart;
            _viewModelPageTradeChart.СanvasTradeChart = canvasTradeChart;
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity)); //без этого actualHeight и actualWidth будут нулями
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height)); //без этого actualHeight и actualWidth будут нулями
        }

        private ViewModelPageTradeChart _viewModelPageTradeChart;

        private void canvasTradeChart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModelPageTradeChart.MouseDown(e.GetPosition(sender as IInputElement));
        }

        private void canvasTradeChart_MouseMove(object sender, MouseEventArgs e)
        {
            _viewModelPageTradeChart.MouseMove(e.GetPosition(sender as IInputElement));
        }

        private void canvasTradeChart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _viewModelPageTradeChart.MouseUp();
        }
    }
}
