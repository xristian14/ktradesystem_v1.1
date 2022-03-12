using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;
using System.IO;
using ktradesystem.CommunicationChannel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ktradesystem.Models
{
    class ModelTestingResult : ModelBase
    {
        private static ModelTestingResult _instance;

        public static ModelTestingResult getInstance()
        {
            if (_instance == null)
            {
                _instance = new ModelTestingResult();
            }
            return _instance;
        }
        private ModelTestingResult()
        {
            _mainCommunicationChannel = MainCommunicationChannel.getInstance();
            _modelSimulation = ModelSimulation.getInstance();
        }

        private MainCommunicationChannel _mainCommunicationChannel;
        private ModelSimulation _modelSimulation;
        private ModelTesting _modelTesting;
        public ModelTesting ModelTesting
        {
            get
            {
                if (_modelTesting == null)
                {
                    _modelTesting = ModelTesting.getInstance(); //реализовано таким образом, т.к. объекты ссылаюстя друг на друга и идет бесконечный цикл инициализации
                }
                return _modelTesting;
            }
        }

        private void CheckFileStructure() //проверяет наличие папок файлов приложения
        {
            string currentDirectory = Directory.GetCurrentDirectory(); //путь к папке с приложением
            if(Directory.Exists(currentDirectory + "\\applicationFiles") == false)
            {
                Directory.CreateDirectory(currentDirectory + "\\applicationFiles");
            }
            if(Directory.Exists(currentDirectory + "\\applicationFiles\\testResults") == false)
            {
                Directory.CreateDirectory(currentDirectory + "\\applicationFiles\\testResults");
            }
            if(Directory.Exists(currentDirectory + "\\applicationFiles\\testResults\\history") == false)
            {
                Directory.CreateDirectory(currentDirectory + "\\applicationFiles\\testResults\\history");
            }
            if(Directory.Exists(currentDirectory + "\\applicationFiles\\testResults\\saves") == false)
            {
                Directory.CreateDirectory(currentDirectory + "\\applicationFiles\\testResults\\saves");
            }
        }

        public void WriteTestingResult(Testing testing) //записывает результат тестирования в папку с историей результатов тестирования
        {
            CheckFileStructure(); //проверяем существование нужных папок
            string historyPath = Directory.GetCurrentDirectory() + "\\applicationFiles\\testResults\\history"; //путь к папке с историей результатов тестирования
            DateTime dateTime = testing.DateTimeSimulationEnding; //получаем дату и время завершения выполнения симуляции тестирования
            string day = dateTime.Day.ToString().Length == 2 ? dateTime.Day.ToString() : "0" + dateTime.Day.ToString();
            string month = dateTime.Month.ToString().Length == 2 ? dateTime.Month.ToString() : "0" + dateTime.Month.ToString();
            string hour = dateTime.Hour.ToString().Length == 2 ? dateTime.Hour.ToString() : "0" + dateTime.Hour.ToString();
            string minute = dateTime.Minute.ToString().Length == 2 ? dateTime.Minute.ToString() : "0" + dateTime.Minute.ToString();
            string second = dateTime.Second.ToString().Length == 2 ? dateTime.Second.ToString() : "0" + dateTime.Second.ToString();
            string timeStr = day + "." + month + "." + dateTime.Year.ToString() + "  " + hour + "ч " + minute + "м " + second + "с";
            string space = "  ";
            while(Directory.Exists(historyPath + "\\" + timeStr + space + testing.Algorithm.Name)) //пока имя папки не будет уникально, прибавляем пробел между датой и временем и названием алгоритма
            {
                space += " ";
            }
            string testingDirectoryPath = historyPath + "\\" + timeStr + space + testing.Algorithm.Name; //путь к папке с текущим тестированием
            Directory.CreateDirectory(testingDirectoryPath); //создаем папку с текущим тестированием
            string jsonTesting = JsonSerializer.Serialize(testing); //сериализуем объект тестирования
            File.WriteAllText(testingDirectoryPath + "\\testing.json", jsonTesting); //записываем в файл

            //записываем DataSourcesCandles
            Directory.CreateDirectory(testingDirectoryPath + "\\dataSourcesCandles");
            for(int i = 0; i < testing.DataSourcesCandles.Length; i++)
            {
                //определяем каталоги индикаторов алгоритмов
                testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs = new List<AlgorithmIndicatorCatalog>();
                //проходим по всем индикаторам алгоритма
                foreach(AlgorithmIndicator algorithmIndicator in testing.Algorithm.AlgorithmIndicators)
                {
                    AlgorithmIndicatorCatalog algorithmIndicatorCatalog = new AlgorithmIndicatorCatalog { AlgorithmIndicator = algorithmIndicator, AlgorithmIndicatorCatalogElements = new List<AlgorithmIndicatorCatalogElement>() };

                }

                string dataSourcesCandlesPath = testingDirectoryPath + "\\dataSourcesCandles\\" + testing.DataSourcesCandles[i].DataSource.Id.ToString(); //путь к папке с текущим DataSourceCandles
                Directory.CreateDirectory(dataSourcesCandlesPath); //создаем папку с текущим DataSourceCandles
                string jsonDataSourceCandles = JsonSerializer.Serialize(testing.DataSourcesCandles[i]); //сериализуем
                File.WriteAllText(dataSourcesCandlesPath + "\\dataSourceCandles.json", jsonDataSourceCandles); //записываем в файл

                //вычисляем и записываем значения всех индикаторов алгоритмов со всеми комбинациями параметров

            }
        }
    }
}
