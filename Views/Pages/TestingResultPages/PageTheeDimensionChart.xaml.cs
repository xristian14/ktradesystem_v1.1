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
            ViewModelPageTheeDimensionChart viewModelPageTheeDimensionChart = new ViewModelPageTheeDimensionChart();
            DataContext = viewModelPageTheeDimensionChart;
        }
    }
}
