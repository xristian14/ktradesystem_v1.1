﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTestingNavigation : ViewModelBase
    {
        private static ViewModelPageTestingNavigation _instance;

        private ViewModelPageTestingNavigation()
        {
            _testing = new Views.Pages.PageTesting();
            _testingResult = new Views.Pages.PageTestingResult();
            CurrentPage = _testing;
        }

        public static ViewModelPageTestingNavigation getInstance()
        {
            if (_instance == null)
            {
                _instance = new ViewModelPageTestingNavigation();
            }
            return _instance;
        }

        private Page _testing;
        private Page _testingResult;

        private Page _currentPage;
        public Page CurrentPage
        {
            get { return _currentPage; }
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigationCreateTesting_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CurrentPage = _testing;
                }, (obj) => true);
            }
        }

        public ICommand NavigationResultTesting_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CurrentPage = _testingResult;
                }, (obj) => true);
            }
        }
    }
}
