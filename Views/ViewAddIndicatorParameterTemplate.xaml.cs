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
using System.Windows.Shapes;
using ktradesystem.ViewModels;

namespace ktradesystem.Views
{
    /// <summary>
    /// Логика взаимодействия для ViewAddIndicatorParameterTemplate.xaml
    /// </summary>
    public partial class ViewAddIndicatorParameterTemplate : Window
    {
        public ViewAddIndicatorParameterTemplate()
        {
            InitializeComponent();

            ViewModelPageTesting viewModelPageTesting = ViewModelPageTesting.getInstance();
            DataContext = viewModelPageTesting;
            
            if (viewModelPageTesting.CloseAddIndicatorParameterTemplateAction == null)
            {
                viewModelPageTesting.CloseAddIndicatorParameterTemplateAction = new Action(this.Close); //устанавливаем на action viewmodel метод закрытия окна(для кнопки отмена)
            }
            Closing += viewModelPageTesting.AddIndicatorParameterTemplate_Closing;
        }
    }
}
