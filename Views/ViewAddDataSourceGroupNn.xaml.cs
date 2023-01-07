using ktradesystem.ViewModels;
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

namespace ktradesystem.Views
{
    /// <summary>
    /// Логика взаимодействия для ViewAddDataSourceGroupNn.xaml
    /// </summary>
    public partial class ViewAddDataSourceGroupNn : Window
    {
        public ViewAddDataSourceGroupNn()
        {
            InitializeComponent();

            ViewModelPageTestingNN viewModelPageTestingNN = ViewModelPageTestingNN.getInstance();
            DataContext = viewModelPageTestingNN;

            if (viewModelPageTestingNN.CloseAdditionalWindowAction == null)
            {
                viewModelPageTestingNN.CloseAdditionalWindowAction = new Action(this.Close); //устанавливаем на action viewmodel метод закрытия окна(для кнопки отмена)
            }
            Closing += viewModelPageTestingNN.AdditionalWindow_Closing;
        }
    }
}
