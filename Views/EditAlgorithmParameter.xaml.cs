﻿using System;
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
    /// Логика взаимодействия для EditAlgorithmParameter.xaml
    /// </summary>
    public partial class EditAlgorithmParameter : Window
    {
        public EditAlgorithmParameter()
        {
            InitializeComponent();

            ViewModelPageTesting viewModelPageTesting = ViewModelPageTesting.getInstance();
            DataContext = viewModelPageTesting;

            if (viewModelPageTesting.CloseAddDataSourceTemplateAction == null)
            {
                viewModelPageTesting.CloseAddDataSourceTemplateAction = new Action(this.Close); //устанавливаем на action viewmodel метод закрытия окна(для кнопки отмена)
            }
            Closing += viewModelPageTesting.AddDataSourceTemplate_Closing;
        }
    }
}
