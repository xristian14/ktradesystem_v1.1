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
using System.Collections.ObjectModel;
using System.Diagnostics;

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
            _modelData = ModelData.getInstance();
            _modelSimulation = ModelSimulation.getInstance();

            ReadAndCheckTestingResults(); //считываем результаты тестирования
        }

        private MainCommunicationChannel _mainCommunicationChannel;
        private ModelData _modelData;
        private ModelSimulation _modelSimulation;
        private ModelTesting _modelTesting;
        private ModelTesting ModelTesting
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

        private ObservableCollection<TestingResultHeader> _testingHistory = new ObservableCollection<TestingResultHeader>(); //история результатов тестирования
        private ObservableCollection<TestingResultHeader> _testingHistoryForSubscribers = new ObservableCollection<TestingResultHeader>(); //история результатов тестирования для подписчиков (т.к. при подписке на основной будет ошибка изменения UI компонентов вне основного потока UI)
        public ObservableCollection<TestingResultHeader> TestingHistoryForSubscribers
        {
            get { return _testingHistoryForSubscribers; }
            private set
            {
                _testingHistoryForSubscribers = value;
                OnPropertyChanged();
            }
        }
        public void NotifyTestingHistorySubscribers() //выполняет обновление TestingHistoryForSubscribers, вследствии чего UI обновится на новые данные
        {
            TestingHistoryForSubscribers.Clear();
            foreach (TestingResultHeader testing in _testingHistory)
            {
                TestingHistoryForSubscribers.Add(testing);
            }
        }

        private ObservableCollection<TestingResultHeader> _testingSaves = new ObservableCollection<TestingResultHeader>(); //история результатов тестирования
        private ObservableCollection<TestingResultHeader> _testingSavesForSubscribers = new ObservableCollection<TestingResultHeader>(); //история результатов тестирования для подписчиков (т.к. при подписке на основной будет ошибка изменения UI компонентов вне основного потока UI)
        public ObservableCollection<TestingResultHeader> TestingSavesForSubscribers
        {
            get { return _testingSavesForSubscribers; }
            private set
            {
                _testingSavesForSubscribers = value;
                OnPropertyChanged();
            }
        }
        public void NotifyTestingSavesSubscribers() //выполняет обновление _testingSavesForSubscribers, вследствии чего UI обновится на новые данные
        {
            TestingSavesForSubscribers.Clear();
            foreach (TestingResultHeader testing in _testingSaves)
            {
                TestingSavesForSubscribers.Add(testing);
            }
        }

        private void CheckFileStructure() //проверяет наличие папок файлов приложения
        {
            string currentDirectory = Directory.GetCurrentDirectory(); //путь к папке с приложением
            if(Directory.Exists(currentDirectory + "\\applicationFiles") == false)
            {
                Directory.CreateDirectory(currentDirectory + "\\applicationFiles");
            }
            if(Directory.Exists(currentDirectory + "\\applicationFiles\\testingResults") == false)
            {
                Directory.CreateDirectory(currentDirectory + "\\applicationFiles\\testingResults");
            }
            if(Directory.Exists(currentDirectory + "\\applicationFiles\\testingResults\\history") == false)
            {
                Directory.CreateDirectory(currentDirectory + "\\applicationFiles\\testingResults\\history");
            }
            if(Directory.Exists(currentDirectory + "\\applicationFiles\\testingResults\\saves") == false)
            {
                Directory.CreateDirectory(currentDirectory + "\\applicationFiles\\testingResults\\saves");
            }
        }

        public void WriteTestingResult(Testing testing) //записывает результат тестирования в папку с историей результатов тестирования
        {
            CheckFileStructure(); //проверяем существование нужных папок
            string historyPath = Directory.GetCurrentDirectory() + "\\applicationFiles\\testingResults\\history"; //путь к папке с историей результатов тестирования
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
            testing.TestingResultName = timeStr + space + testing.Algorithm.Name; //название результата тестирования
            Directory.CreateDirectory(testingDirectoryPath); //создаем папку с текущим тестированием
            TestingResultHeader testingResultHeader = new TestingResultHeader { TestingResultName = testing.TestingResultName, DateTimeSimulationEnding = testing.DateTimeSimulationEnding }; //создаем файл с заголовком тестирования, который будет считываться в список результатов тестирования
            string jsonTestingHeader = JsonSerializer.Serialize(testingResultHeader); //сериализуем
            File.WriteAllText(testingDirectoryPath + "\\testingHeader.json", jsonTestingHeader); //записываем в файл
            string jsonTesting = JsonSerializer.Serialize(testing); //сериализуем объект тестирования
            File.WriteAllText(testingDirectoryPath + "\\testing.json", jsonTesting); //записываем в файл

            //формируем AlgorithmIndicatorCatalogElements для DataSourcesCandles
            for (int i = 0; i < testing.DataSourcesCandles.Length; i++)
            {
                //определяем каталоги индикаторов алгоритмов
                testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs = new List<AlgorithmIndicatorCatalog>();
                //проходим по всем индикаторам алгоритма
                foreach (AlgorithmIndicator algorithmIndicator in testing.Algorithm.AlgorithmIndicators)
                {
                    AlgorithmIndicatorCatalog algorithmIndicatorCatalog = new AlgorithmIndicatorCatalog { AlgorithmIndicator = algorithmIndicator, AlgorithmIndicatorFolderName = algorithmIndicator.Indicator.Name + "_" + algorithmIndicator.Ending + "_values", AlgorithmIndicatorCatalogElements = new List<AlgorithmIndicatorCatalogElement>() };

                    //получаем список параметров алгоритмов, используемых в индикаторе алгоритма
                    List<AlgorithmParameter> algorithmParameters = new List<AlgorithmParameter>();
                    foreach (IndicatorParameterRange indicatorParameterRange in algorithmIndicator.IndicatorParameterRanges)
                    {
                        if (algorithmParameters.Contains(indicatorParameterRange.AlgorithmParameter) == false)
                        {
                            algorithmParameters.Add(indicatorParameterRange.AlgorithmParameter);
                        }
                    }

                    if (algorithmParameters.Count == 0) //если нет параметров, значит только 1 вариант значений индикатора для источника данных
                    {
                        algorithmIndicatorCatalog.AlgorithmIndicatorCatalogElements.Add(new AlgorithmIndicatorCatalogElement { AlgorithmParameterValues = new List<AlgorithmParameterValue>(), FileName = "withoutParameters.json" });
                    }
                    else
                    {
                        //формируем список со всеми комбинациями параметров алгоритма данного индикатора алгоритма
                        List<int[][]> algorithmParameterCombinations = new List<int[][]>(); //список с комбинациями (массивами с комбинацией параметров алгоритма и значения): 0-й элемент - индекс параметра алгоритма во всех параметрах, 1-й - индекс значения параметра во всех значениях параметров алгоритма
                        //algorithmParameterCombinations[0] - первая комбинация параметров алгоритма
                        //algorithmParameterCombinations[0][0] - первый параметр комбинации
                        //algorithmParameterCombinations[0][0][0] - индекс параметра алгоритма первого элемента первой комбинации
                        //algorithmParameterCombinations[0][0][1] - индекс значения параметра алгоритма первого элемента первой комбинации

                        //заполняем комбинации всеми вариантами первого параметра
                        int indexFirstParameter = testing.Algorithm.AlgorithmParameters.IndexOf(algorithmParameters[0]); //индекс первого параметра алгоритма индикатора алгоритма
                        int countParameterValues1 = testing.Algorithm.AlgorithmParameters[indexFirstParameter].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[indexFirstParameter].Count : testing.AlgorithmParametersAllDoubleValues[indexFirstParameter].Count; //количество значений текущего параметра алгоритма
                        for (int k = 0; k < countParameterValues1; k++)
                        {
                            int[][] arr = new int[1][];
                            arr[0] = new int[2] { indexFirstParameter, k }; //записываем индекс параметра алгоритма и индекс значения параметра
                            algorithmParameterCombinations.Add(arr);
                        }

                        //формируем комбинации со всеми параметрами кроме первого
                        for (int k = 1; k < algorithmParameters.Count; k++)
                        {
                            int indexAlgorithmParameter = testing.Algorithm.AlgorithmParameters.IndexOf(algorithmParameters[k]); //индекс текущего параметра алгоритма
                            List<int[][]> newAlgorithmParameterCombinations = new List<int[][]>(); //новые комбинации. Для всех элементов старых комбинаций будут созданы комбинации с текущим параметром и старые комбинации обновятся на новые
                            for (int u = 0; u < algorithmParameterCombinations.Count; u++)
                            {
                                int countParameterValues2 = testing.Algorithm.AlgorithmParameters[indexAlgorithmParameter].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[indexAlgorithmParameter].Count : testing.AlgorithmParametersAllDoubleValues[indexAlgorithmParameter].Count; //количество значений текущего параметра алгоритма
                                for (int y = 0; y < countParameterValues2; y++)
                                {
                                    int[][] arr = new int[algorithmParameterCombinations[u].Length + 1][]; //увеличиваем количество элементов в комбинации на 1
                                    //заполняем комбинацию старыми элементами
                                    for (int x = 0; x < algorithmParameterCombinations[u].Length; x++)
                                    {
                                        arr[x] = algorithmParameterCombinations[u][x];
                                    }
                                    //записываем новый параметр
                                    arr[arr.Length - 1] = new int[2] { indexAlgorithmParameter, y }; //записываем индекс параметра алгоритма и индекс значения параметра
                                    newAlgorithmParameterCombinations.Add(arr);
                                }
                            }
                            algorithmParameterCombinations = newAlgorithmParameterCombinations;
                        }

                        //для каждой комбинации параметров формируем элемент каталога индикатора алгоритма
                        for (int k = 0; k < algorithmParameterCombinations.Count; k++)
                        {
                            string fileName = "";
                            foreach (int[] value in algorithmParameterCombinations[k])
                            {
                                string parameterValue = testing.Algorithm.AlgorithmParameters[value[0]].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[value[0]][value[1]].ToString() : testing.AlgorithmParametersAllDoubleValues[value[0]][value[1]].ToString();
                                fileName += fileName.Length == 0 ? "" : " "; //если это не первые символы названия, отделяем их пробелом от предыдущих
                                fileName += testing.Algorithm.AlgorithmParameters[value[0]].Name + "=" + parameterValue;
                            }
                            fileName += ".json";
                            AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement = new AlgorithmIndicatorCatalogElement { AlgorithmParameterValues = new List<AlgorithmParameterValue>(), FileName = fileName };
                            for (int u = 0; u < algorithmParameterCombinations[k].Length; u++)
                            {
                                int intValue = testing.Algorithm.AlgorithmParameters[algorithmParameterCombinations[k][u][0]].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[algorithmParameterCombinations[k][u][0]][algorithmParameterCombinations[k][u][1]] : 0;
                                double doubleValue = testing.Algorithm.AlgorithmParameters[algorithmParameterCombinations[k][u][0]].ParameterValueType.Id == 1 ? 0 : testing.AlgorithmParametersAllDoubleValues[algorithmParameterCombinations[k][u][0]][algorithmParameterCombinations[k][u][1]];
                                algorithmIndicatorCatalogElement.AlgorithmParameterValues.Add(new AlgorithmParameterValue { AlgorithmParameter = testing.Algorithm.AlgorithmParameters[algorithmParameterCombinations[k][u][0]], IntValue = intValue, DoubleValue = doubleValue });
                            }
                            algorithmIndicatorCatalog.AlgorithmIndicatorCatalogElements.Add(algorithmIndicatorCatalogElement);
                        }
                    }
                    testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs.Add(algorithmIndicatorCatalog); //записываем каталог для индикатора алгоритма
                }
            }

            //определяем количество DataSourcesCandles и AlgorithmIndicatorValues которые необходимо записать в файлы
            int countWrites = 0;
            for (int i = 0; i < testing.DataSourcesCandles.Length; i++)
            {
                countWrites++;
                foreach (AlgorithmIndicatorCatalog algorithmIndicatorCatalog1 in testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs)
                {
                    foreach (AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement in algorithmIndicatorCatalog1.AlgorithmIndicatorCatalogElements)
                    {
                        countWrites++;
                    }
                }
            }
            int countWrite = 0; //количество записанных файлов
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            DispatcherInvoke((Action)(() => {
                _mainCommunicationChannel.TestingProgress.Clear();
                _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/3:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsSuccessSimulation=true, IsFinish = false });
            }));

            //записываем DataSourcesCandles
            Directory.CreateDirectory(testingDirectoryPath + "\\dataSourcesCandles");
            for (int i = 0; i < testing.DataSourcesCandles.Length; i++)
            {
                string dataSourcesCandlesPath = testingDirectoryPath + "\\dataSourcesCandles\\" + testing.DataSourcesCandles[i].DataSource.Id.ToString(); //путь к папке с текущим DataSourceCandles
                Directory.CreateDirectory(dataSourcesCandlesPath); //создаем папку с текущим DataSourceCandles
                string jsonDataSourceCandles = JsonSerializer.Serialize(testing.DataSourcesCandles[i]); //сериализуем
                File.WriteAllText(dataSourcesCandlesPath + "\\dataSourceCandles.json", jsonDataSourceCandles); //записываем в файл
                countWrite++;
                DispatcherInvoke((Action)(() => {
                    _mainCommunicationChannel.TestingProgress.Clear();
                    _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/3:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsSuccessSimulation = true, IsFinish = false });
                }));

                //вычисляем и записываем значения всех индикаторов алгоритмов со всеми комбинациями параметров
                foreach (AlgorithmIndicatorCatalog algorithmIndicatorCatalog1 in testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs)
                {
                    string algorithmIndicatorPath = dataSourcesCandlesPath + "\\" + algorithmIndicatorCatalog1.AlgorithmIndicatorFolderName; //путь к папке с значениями данного индикатора алгоритма
                    Directory.CreateDirectory(algorithmIndicatorPath); //создаем папку для значений индикатора алгоритма
                    foreach (AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement in algorithmIndicatorCatalog1.AlgorithmIndicatorCatalogElements)
                    {
                        //вычисляем значения индикатора алгоритма
                        AlgorithmIndicatorValues algorithmIndicatorValues = _modelSimulation.AlgorithmIndicatorCalculate(testing, testing.DataSourcesCandles[i], algorithmIndicatorCatalog1.AlgorithmIndicator, algorithmIndicatorCatalogElement.AlgorithmParameterValues);
                        string jsonAlgorithmIndicatorValues = JsonSerializer.Serialize(algorithmIndicatorValues); //сериализуем
                        File.WriteAllText(algorithmIndicatorPath + "\\" + algorithmIndicatorCatalogElement.FileName, jsonAlgorithmIndicatorValues); //записываем в файл
                        countWrite++;
                        DispatcherInvoke((Action)(() => {
                            _mainCommunicationChannel.TestingProgress.Clear();
                            _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/3:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsSuccessSimulation = true, IsFinish = false });
                        }));
                    }
                }
            }
            stopwatch.Stop();

            //считываем результаты тестирования, проверяя просроченные и удаляя их
            ReadAndCheckTestingResults();

            DispatcherInvoke((Action)(() => {
                _mainCommunicationChannel.TestingProgress.Clear();
                _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/3:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsSuccessSimulation = true, IsFinish = true });
            }));
            ModelTesting.StopwatchTesting.Stop();
        }

        public void ReadTestingResults() //считывает результаты тестирования
        {
            CheckFileStructure(); //проверяем существование нужных папок
            _testingHistory.Clear();
            _testingSaves.Clear();
            //считываем историю результатов тестирования и сохраненные результаты тестирования
            //дважды проходим по циклу считывания. В первый раз считываем историю, во второй раз - сохраненные результаты тестирования
            int iteration = 0;
            do
            {
                iteration++;
                string testingResultsPath = iteration == 1 ? Directory.GetCurrentDirectory() + "\\applicationFiles\\testingResults\\history" : Directory.GetCurrentDirectory() + "\\applicationFiles\\testingResults\\saves"; //путь к папке с результатами тестирования
                string[] testingResultsDirectories = Directory.GetDirectories(testingResultsPath); //получаем массив с каталогами результатов тестирования
                //проходим по каждому каталогу и пытаемся считать тестирование из него
                for (int i = 0; i < testingResultsDirectories.Length; i++)
                {
                    bool isBroken = false; //испорчено ли данное тестирование, нельзя ли его считать
                    TestingResultHeader testingHeader = new TestingResultHeader();
                    if (File.Exists(testingResultsDirectories[i] + "\\testingHeader.json")) //если файл с сохраненным тестированием существует
                    {
                        //пробуем считать и десериализовать тестирование
                        try
                        {
                            string jsonTestingHeader = File.ReadAllText(testingResultsDirectories[i] + "\\testingHeader.json");
                            testingHeader = JsonSerializer.Deserialize<TestingResultHeader>(jsonTestingHeader);
                        }
                        catch
                        {
                            isBroken = true;
                        }
                    }
                    else //файла не существует
                    {
                        isBroken = true;
                    }

                    if (isBroken == false) //если тестирование успешно считано и десериализовано, добавляем в историю
                    {
                        if(iteration == 1) //если считываем историю записываем в историю
                        {
                            _testingHistory.Add(testingHeader);
                        }
                        else //иначе записываем в сохраненные
                        {
                            _testingSaves.Add(testingHeader);
                        }
                    }
                    else //иначе удаляем директорию с данным результатом тестирования и сообщаем об этом пользователю
                    {
                        bool isDeleteException = false;
                        try
                        {
                            Directory.Delete(testingResultsDirectories[i], true);
                        }
                        catch
                        {
                            isDeleteException = true;
                        }
                        if (isDeleteException == false) //если директория успешно удалена
                        {
                            DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Результат тестирования: " + testingResultsDirectories[i] + " - поврежден, и был удален."); }));
                        }
                        else //если возникло исключение
                        {
                            DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Результат тестирования: " + testingResultsDirectories[i] + " - поврежден, и при попытке удалить его возникло исключение."); }));
                        }
                    }
                }
            }
            while (iteration < 2);
        }

        public bool CheckAndDeleteExpiredTestingResults() //проверяет наличие результатов тестирования в истории у которых вышел срок хранения, и удаляет их. Если было удаление возвращает true, иначе - false
        {
            bool isWasDelete = false; //было ли удаление
            TimeSpan timeSpanLife = TimeSpan.FromHours(_modelData.Settings.Where(j => j.Id == 5).First().DoubleValue); //время хранения результатов тестирования
            DateTime dateTimeNow = DateTime.Now; //текущая дата и время
            foreach(TestingResultHeader testingHeader in _testingHistory)
            {
                if(DateTime.Compare(testingHeader.DateTimeSimulationEnding.Add(timeSpanLife), dateTimeNow) < 0) //если дата звершения симуляции тестирования + срок хранения меньше текущей, значит срок вышел и нужно удалить данный результат тестирования
                {
                    //пытаемся удалить данный результат тестирования
                    bool isDeleteException = false;
                    try
                    {
                        Directory.Delete(Directory.GetCurrentDirectory() + "\\applicationFiles\\testingResults\\history\\" + testingHeader.TestingResultName, true);
                    }
                    catch
                    {
                        isDeleteException = true;
                    }
                    if (isDeleteException == true) //если возникло исключение
                    {
                        DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("При попытке удалить результат тестирования " + testingHeader.TestingResultName + " у которого истек срок хранения, возникло исключение."); }));
                    }
                    isWasDelete = true;
                }
            }
            return isWasDelete;
        }

        public void ReadAndCheckTestingResults() //считывает результаты тестирования, удаляет просроченные, и если были удалены, считывает заного
        {
            //считываем результаты тестирования
            ReadTestingResults();
            //удаляем результаты тестирования у которых истек срок хранения
            bool isDelete = CheckAndDeleteExpiredTestingResults();
            //если были удалены результаты, считываем еще раз результаты тестирования
            if (isDelete)
            {
                ReadTestingResults();
            }
            //обновляем списки с результатами тестирования на которые подписана модель представления
            DispatcherInvoke((Action)(() => {
                NotifyTestingHistorySubscribers();
                NotifyTestingSavesSubscribers();
            }));
        }
    }
}
