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
    /// Логика взаимодействия для ViewAddDataSource.xaml
    /// </summary>
    public partial class ViewAddDataSource : Window
    {
        public ViewAddDataSource()
        {
            InitializeComponent();
            ViewModelPageDataSource viewModelPageDataSource = ViewModelPageDataSource.getInstance();
            DataContext = viewModelPageDataSource;

            if(viewModelPageDataSource.CloseAddDataSourceAction == null)
            {
                viewModelPageDataSource.CloseAddDataSourceAction = new Action(this.Close); //устанавливаем на action viewmodel метод закрытия окна(для кнопки отмена)
            }
            Closing += viewModelPageDataSource.AddDataSource_Closing;
        }
    }
}
