using ktradesystem.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ktradesystem.ViewModels
{
    class ViewModelPageTestingNN : ViewModelBase
    {
        private static ViewModelPageTestingNN _instance;
        public static ViewModelPageTestingNN getInstance()
        {
            if (_instance == null)
            {
                _instance = new ViewModelPageTestingNN();
            }
            return _instance;
        }

        private ViewmodelData _viewmodelData;
        public ViewmodelData viewmodelData
        {
            get
            {
                if (_viewmodelData == null)
                {
                    _viewmodelData = ViewmodelData.getInstance();
                }
                return _viewmodelData;
            }
        }

        public void AdditionalWindow_Closing(object sender, CancelEventArgs e)
        {
            viewmodelData.IsPagesAndMainMenuButtonsEnabled = true;
            CloseAdditionalWindowAction = null; //сбрасываем Action, чтобы при инициализации нового окна в него поместился метод его закрытия
        }

        public Action CloseAdditionalWindowAction { get; set; }

        private ObservableCollection<DataSourceTemplateNnView> _dataSourceTemplatesNnView = new ObservableCollection<DataSourceTemplateNnView>();
        public ObservableCollection<DataSourceTemplateNnView> DataSourceTemplatesNnView
        {
            get { return _dataSourceTemplatesNnView; }
            private set
            {
                _dataSourceTemplatesNnView = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _minScalerDataSourceTemplates = new ObservableCollection<string>();
        public ObservableCollection<string> MinScalerDataSourceTemplates //список с доступными для выбора шаблонами источников данных в minScaler
        {
            get { return _minScalerDataSourceTemplates; }
            private set
            {
                _minScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private string _selectedMinScalerDataSourceTemplates;
        public string SelectedMinScalerDataSourceTemplates
        {
            get { return _selectedMinScalerDataSourceTemplates; }
            set
            {
                _selectedMinScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _maxScalerDataSourceTemplates = new ObservableCollection<string>();
        public ObservableCollection<string> MaxScalerDataSourceTemplates //список с доступными для выбора шаблонами источников данных в maxScaler
        {
            get { return _maxScalerDataSourceTemplates; }
            private set
            {
                _maxScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private string _selectedMaxScalerDataSourceTemplates;
        public string SelectedMaxScalerDataSourceTemplates
        {
            get { return _selectedMaxScalerDataSourceTemplates; }
            set
            {
                _selectedMaxScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _listSelectedMinScalerDataSourceTemplates = new ObservableCollection<string>();
        public ObservableCollection<string> ListSelectedMinScalerDataSourceTemplates //список с выбранными шаблонами источников данных в minScaler
        {
            get { return _listSelectedMinScalerDataSourceTemplates; }
            private set
            {
                _listSelectedMinScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private string _selectedListSelectedMinScalerDataSourceTemplates;
        public string SelectedListSelectedMinScalerDataSourceTemplates
        {
            get { return _selectedListSelectedMinScalerDataSourceTemplates; }
            set
            {
                _selectedListSelectedMinScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<string> _listSelectedMaxScalerDataSourceTemplates = new ObservableCollection<string>();
        public ObservableCollection<string> ListSelectedMaxScalerDataSourceTemplates //список с выбранными шаблонами источников данных в maxScaler
        {
            get { return _listSelectedMaxScalerDataSourceTemplates; }
            private set
            {
                _listSelectedMaxScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        private string _selectedListSelectedMaxScalerDataSourceTemplates;
        public string SelectedListSelectedMaxScalerDataSourceTemplates
        {
            get { return _selectedListSelectedMaxScalerDataSourceTemplates; }
            set
            {
                _selectedListSelectedMaxScalerDataSourceTemplates = value;
                OnPropertyChanged();
            }
        }
        const string ZERO_SCALER = "Zero";
        private void UpdateMinMaxScalerDataSourceTemplates()
        {
            ListSelectedMinScalerDataSourceTemplates.Clear();
            ListSelectedMaxScalerDataSourceTemplates.Clear();
            SelectedListSelectedMinScalerDataSourceTemplates = "";
            SelectedListSelectedMaxScalerDataSourceTemplates = "";
            MinScalerDataSourceTemplates.Clear();
            MaxScalerDataSourceTemplates.Clear();
            MinScalerDataSourceTemplates.Add(ZERO_SCALER);
            for(int i = 0; i < DataSourceTemplatesNnView.Count; i++)
            {
                MinScalerDataSourceTemplates.Add(DataSourceTemplatesNnView[i].Name);
                MaxScalerDataSourceTemplates.Add(DataSourceTemplatesNnView[i].Name);
            }
            SelectedMinScalerDataSourceTemplates = MinScalerDataSourceTemplates[0];
            SelectedMaxScalerDataSourceTemplates = MaxScalerDataSourceTemplates.Count > 0 ? MaxScalerDataSourceTemplates[0] : "";
        }
        public ICommand AddMinScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (!ListSelectedMinScalerDataSourceTemplates.Contains(SelectedMinScalerDataSourceTemplates))
                    {
                        ListSelectedMinScalerDataSourceTemplates.Add(SelectedMinScalerDataSourceTemplates);
                    }
                }, (obj) => MinScalerDataSourceTemplates.Contains(SelectedMinScalerDataSourceTemplates));
            }
        }
        public ICommand DeleteMinScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ListSelectedMinScalerDataSourceTemplates.Remove(SelectedListSelectedMinScalerDataSourceTemplates);
                }, (obj) => ListSelectedMinScalerDataSourceTemplates.Contains(SelectedListSelectedMinScalerDataSourceTemplates));
            }
        }
        public ICommand AddMaxScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (!ListSelectedMaxScalerDataSourceTemplates.Contains(SelectedMaxScalerDataSourceTemplates))
                    {
                        ListSelectedMaxScalerDataSourceTemplates.Add(SelectedMaxScalerDataSourceTemplates);
                    }
                }, (obj) => MaxScalerDataSourceTemplates.Contains(SelectedMaxScalerDataSourceTemplates));
            }
        }
        public ICommand DeleteMaxScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ListSelectedMaxScalerDataSourceTemplates.Remove(SelectedListSelectedMaxScalerDataSourceTemplates);
                }, (obj) => ListSelectedMaxScalerDataSourceTemplates.Contains(SelectedListSelectedMaxScalerDataSourceTemplates));
            }
        }

        /*private bool IsFieldsAddAlgorithmParameterCorrect()
        {
            bool result = true;
            TooltipAddAddAlgorithmParameter.Clear(); //очищаем подсказку кнопки добавить

            string name = AlgorithmParameterName != null ? AlgorithmParameterName.Replace(" ", "") : "";

            //проверка на пустое значение
            if (name == "" || AlgorithmParameterMinValue == "" || AlgorithmParameterMaxValue == "" || AlgorithmParameterstep == "")
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Не заполнены все поля.");
            }

            //проверка на допустимые символы
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool isNotFind = false;
            for (int i = 0; i < name.Length; i++)
            {
                if (letters.IndexOf(name[i]) == -1)
                {
                    isNotFind = true;
                }

            }
            if (isNotFind)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Допустимо использование только английского алфавита.");
            }

            //проверка на уникальность названия
            bool isUnique = true;
            foreach (AlgorithmParameterView item in AlgorithmParametersView)
            {
                if (name == item.Name) //проверяем имя на уникальность среди всех записей
                {
                    isUnique = false;
                }
            }
            if (isUnique == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Данное название уже используется.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(AlgorithmParameterMinValue, out double res) == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Минимальное значение должно быть числом.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(AlgorithmParameterMaxValue, out res) == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Максимальное значение должно быть числом.");
            }

            //проверка на возможность конвертации в число с плавающей точкой
            if (double.TryParse(AlgorithmParameterstep, out res) == false)
            {
                result = false;
                TooltipAddAddAlgorithmParameter.Add("Шаг должен быть числом.");
            }

            //проверка на возможность достигнуть максимума с минимума с шагом
            if (double.TryParse(AlgorithmParameterMinValue, out double min) && double.TryParse(AlgorithmParameterMaxValue, out double max) && double.TryParse(AlgorithmParameterstep, out double step))
            {
                if ((max > min && step > 0) == false)
                {
                    result = false;
                    TooltipAddAddAlgorithmParameter.Add("Максимум должен быть больше минимума, а шаг должен быть положительным.");
                }
            }

            return result;
        }*/

        public ICommand AddScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    UpdateMinMaxScalerDataSourceTemplates();
                    viewmodelData.IsPagesAndMainMenuButtonsEnabled = false;
                    ViewAddEditScaler viewAddEditScaler = new ViewAddEditScaler();
                    viewAddEditScaler.Show();
                }, (obj) => true);
            }
        }
        public ICommand SaveScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CloseAdditionalWindowAction?.Invoke();
                }, (obj) => true/*IsFieldsAddAlgorithmParameterCorrect()*/);
            }
        }
        public ICommand CancelScaler_Click
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CloseAdditionalWindowAction?.Invoke();
                }, (obj) => true);
            }
        }
    }
}
