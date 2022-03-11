﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;
using ktradesystem.Views;
using ktradesystem.Models;
using ktradesystem.Models.Datatables;
using System.IO;

using System.Reflection;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTestingResult : ViewModelBase
    {
        private static ViewModelPageTestingResult _instance;

        private ViewModelPageTestingResult()
        {

        }

        public static ViewModelPageTestingResult getInstance()
        {
            if (_instance == null)
            {
                _instance = new ViewModelPageTestingResult();
            }
            return _instance;
        }

        private ObservableCollection<string> _resultTestingMenu = new ObservableCollection<string> { "Последние", "Сохраненные" };
        public ObservableCollection<string> ResultTestingMenu
        {
            get { return _resultTestingMenu; }
            private set
            {
                _resultTestingMenu = value;
                OnPropertyChanged();
            }
        }

        private string _selectedResultTestingMenu;
        public string SelectedResultTestingMenu
        {
            get { return _selectedResultTestingMenu; }
            set
            {
                _selectedResultTestingMenu = value;
                OnPropertyChanged();
            }
        }

        public void WriteTestingView(Testing testing) //создает объект тестирования для представления, создает папку для данного тестирования, сериализует созданный объект и сохраняет в файл, вычисляет значения индикаторов для каждого тестового прогона, сериализует их и сохраняет в файлы
        {

        }

        private string _testText;
        public string TestText
        {
            get { return _testText; }
            private set
            {
                _testText = value;
                OnPropertyChanged();
            }
        }
        public ICommand ButtonTest_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {

                }, (obj) => true);
            }
        }
    }
}
