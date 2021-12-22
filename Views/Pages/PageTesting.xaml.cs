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

namespace ktradesystem.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageTesting.xaml
    /// </summary>
    public partial class PageTesting : Page
    {
        public PageTesting()
        {
            InitializeComponent();
            ViewModelPageTesting viewModelPageTesting = ViewModelPageTesting.getInstance();
            DataContext = viewModelPageTesting;

            viewModelPageTesting.IndicatorScriptTextBox = indiactorScriptText; //передаю в viewModel текстовое поле со скриптом индикатора, чтобы отслеживать положение курсора
        }
    }
}
