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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ktradesystem.ViewModels;

namespace ktradesystem.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageTestingResult.xaml
    /// </summary>
    public partial class PageTestingResult : Page
    {
        public PageTestingResult()
        {
            InitializeComponent();

            ViewModelPageTestingResult viewModelPageTestingResult = ViewModelPageTestingResult.getInstance();
            DataContext = viewModelPageTestingResult;
        }
    }
}
