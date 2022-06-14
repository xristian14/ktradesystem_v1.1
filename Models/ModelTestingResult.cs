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
using System.Runtime.Serialization.Formatters.Binary;

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

        private string _historyRealivePath = "\\applicationFiles\\testingResults\\history"; //относительный путь к истории результатов
        private string _savesRealivePath = "\\applicationFiles\\testingResults\\saves"; //относительный путь к сохраненным результатам

        private ObservableCollection<TestingHeader> _testingHistory = new ObservableCollection<TestingHeader>(); //история результатов тестирования
        private ObservableCollection<TestingHeader> _testingHistoryForSubscribers = new ObservableCollection<TestingHeader>(); //история результатов тестирования для подписчиков (т.к. при подписке на основной будет ошибка изменения UI компонентов вне основного потока UI)
        public ObservableCollection<TestingHeader> TestingHistoryForSubscribers
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
            foreach (TestingHeader testing in _testingHistory)
            {
                TestingHistoryForSubscribers.Add(testing);
            }
        }

        private ObservableCollection<TestingHeader> _testingSaves = new ObservableCollection<TestingHeader>(); //история результатов тестирования
        private ObservableCollection<TestingHeader> _testingSavesForSubscribers = new ObservableCollection<TestingHeader>(); //история результатов тестирования для подписчиков (т.к. при подписке на основной будет ошибка изменения UI компонентов вне основного потока UI)
        public ObservableCollection<TestingHeader> TestingSavesForSubscribers
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
            foreach (TestingHeader testing in _testingSaves)
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
            if(Directory.Exists(currentDirectory + _historyRealivePath) == false)
            {
                Directory.CreateDirectory(currentDirectory + _historyRealivePath);
            }
            if(Directory.Exists(currentDirectory + _savesRealivePath) == false)
            {
                Directory.CreateDirectory(currentDirectory + _savesRealivePath);
            }
        }

        public void WriteTestingResult(Testing testing) //записывает результат тестирования в папку с историей результатов тестирования
        {
            CheckFileStructure(); //проверяем существование нужных папок
            string historyPath = Directory.GetCurrentDirectory() + _historyRealivePath; //путь к папке с историей результатов тестирования
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
            testing.TestingName = timeStr + space + testing.Algorithm.Name; //название результата тестирования
            Directory.CreateDirectory(testingDirectoryPath); //создаем папку с текущим тестированием
            TestingHeader testingHeader = new TestingHeader { IsHistory = true, TestingName = testing.TestingName, DateTimeSimulationEnding = testing.DateTimeSimulationEnding }; //создаем файл с заголовком тестирования, который будет считываться в список результатов тестирования
            string jsonTestingHeader = JsonSerializer.Serialize(testingHeader); //сериализуем
            File.WriteAllText(testingDirectoryPath + "\\testingHeader.json", jsonTestingHeader); //записываем в файл

            Stopwatch stopwatch1 = new Stopwatch();
            stopwatch1.Start();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = new FileStream(testingDirectoryPath + "\\testing.dat", FileMode.Create))
            {
                binaryFormatter.Serialize(fileStream, testing); //сериализуем объект тестирования в файл
            }
            
            //записываем все testRun
            foreach (TestBatch testBatch in testing.TestBatches)
            {
                string testBatchFolder = testingDirectoryPath + "\\testBatches\\" + testBatch.Number.ToString();
                Directory.CreateDirectory(testBatchFolder); //создаем папку с тестовыми прогонами текущего testBatch
                //проходим по всем оптимизационным тестовым прогонам
                foreach(TestRun testRun in testBatch.OptimizationTestRuns)
                {
                    using (FileStream fileStream = new FileStream(testBatchFolder + "\\" + testRun.Number.ToString() + ".dat", FileMode.Create))
                    {
                        binaryFormatter.Serialize(fileStream, testRun); //сериализуем тестовый прогон в файл
                    }
                }
                if (testing.IsForwardTesting && testBatch.IsTopModelWasFind)
                {
                    //сериализуем форвардный тест
                    using (FileStream fileStream = new FileStream(testBatchFolder + "\\forwardTestRun.dat", FileMode.Create))
                    {
                        binaryFormatter.Serialize(fileStream, testBatch.ForwardTestRun); //сериализуем тестовый прогон в файл
                    }
                    if (testing.IsForwardDepositTrading)
                    {
                        //сериализуем форвардный тест с торговлей депозитом
                        using (FileStream fileStream = new FileStream(testBatchFolder + "\\forwardTestRunDepositTrading.dat", FileMode.Create))
                        {
                            binaryFormatter.Serialize(fileStream, testBatch.ForwardTestRunDepositTrading); //сериализуем тестовый прогон в файл
                        }
                    }
                }
            }
            stopwatch1.Stop();
            //DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Время записи результата тестирования: " + testingHeader.TestingName + " заняло " + stopwatch1.ElapsedMilliseconds + " милисекунд"); }));
            /*
            string jsonTesting = JsonSerializer.Serialize(testing); //сериализуем объект тестирования
            File.WriteAllText(testingDirectoryPath + "\\testing.json", jsonTesting); //записываем в файл
            */

            //определяем количество DataSourcesCandles и AlgorithmIndicatorValues которые необходимо записать в файлы
            int countWrites = 0;
            for (int i = 0; i < testing.DataSourcesCandles.Count; i++)
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
                _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 4/4:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsSuccessSimulation=true, IsFinish = false });
            }));

            //записываем DataSourcesCandles
            Directory.CreateDirectory(testingDirectoryPath + "\\dataSourcesCandles");
            for (int i = 0; i < testing.DataSourcesCandles.Count; i++)
            {
                string dataSourcesCandlesPath = testingDirectoryPath + "\\dataSourcesCandles\\" + testing.DataSourcesCandles[i].DataSource.Id.ToString(); //путь к папке с текущим DataSourceCandles
                Directory.CreateDirectory(dataSourcesCandlesPath); //создаем папку с текущим DataSourceCandles
                using (FileStream fileStream = new FileStream(dataSourcesCandlesPath + "\\dataSourceCandles.dat", FileMode.Create))
                {
                    binaryFormatter.Serialize(fileStream, testing.DataSourcesCandles[i]); //сериализуем
                }
                /*
                string jsonDataSourceCandles = JsonSerializer.Serialize(testing.DataSourcesCandles[i]); //сериализуем
                File.WriteAllText(dataSourcesCandlesPath + "\\dataSourceCandles.dat", jsonDataSourceCandles); //записываем в файл
                */
                countWrite++;
                DispatcherInvoke((Action)(() => {
                    _mainCommunicationChannel.TestingProgress.Clear();
                    _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 4/4:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsSuccessSimulation = true, IsFinish = false });
                }));

                //записываем значения всех индикаторов алгоритмов со всеми комбинациями параметров
                foreach (AlgorithmIndicatorCatalog algorithmIndicatorCatalog1 in testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs)
                {
                    string algorithmIndicatorPath = dataSourcesCandlesPath + "\\" + algorithmIndicatorCatalog1.AlgorithmIndicatorFolderName; //путь к папке с значениями данного индикатора алгоритма
                    Directory.CreateDirectory(algorithmIndicatorPath); //создаем папку для значений индикатора алгоритма
                    foreach (AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement in algorithmIndicatorCatalog1.AlgorithmIndicatorCatalogElements)
                    {
                        using (FileStream fileStream = new FileStream(algorithmIndicatorPath + "\\" + algorithmIndicatorCatalogElement.FileName, FileMode.Create))
                        {
                            binaryFormatter.Serialize(fileStream, algorithmIndicatorCatalogElement.AlgorithmIndicatorValues); //сериализуем в файл
                        }

                        /*string jsonAlgorithmIndicatorValues = JsonSerializer.Serialize(algorithmIndicatorValues); //сериализуем
                        File.WriteAllText(algorithmIndicatorPath + "\\" + algorithmIndicatorCatalogElement.FileName, jsonAlgorithmIndicatorValues); //записываем в файл
                        */
                        countWrite++;
                        DispatcherInvoke((Action)(() => {
                            _mainCommunicationChannel.TestingProgress.Clear();
                            _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 4/4:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsSuccessSimulation = true, IsFinish = false });
                        }));
                    }
                }
            }
            stopwatch.Stop();
            ModelTesting.StopwatchTesting.Stop();

            //считываем результаты тестирования, проверяя просроченные и удаляя их
            ReadAndCheckTestingResults();

            DispatcherInvoke((Action)(() => {
                _mainCommunicationChannel.TestingProgress.Clear();
                _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 4/4:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsSuccessSimulation = true, IsFinish = true });
            }));
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
                string testingResultsPath = iteration == 1 ? Directory.GetCurrentDirectory() + _historyRealivePath : Directory.GetCurrentDirectory() + _savesRealivePath; //путь к папке с результатами тестирования
                string[] testingResultsDirectories = Directory.GetDirectories(testingResultsPath); //получаем массив с каталогами результатов тестирования
                //проходим по каждому каталогу и пытаемся считать тестирование из него
                for (int i = 0; i < testingResultsDirectories.Length; i++)
                {
                    bool isBroken = false; //испорчено ли данное тестирование, нельзя ли его считать
                    TestingHeader testingHeader = new TestingHeader();
                    if (File.Exists(testingResultsDirectories[i] + "\\testingHeader.json")) //если файл с сохраненным тестированием существует
                    {
                        //пробуем считать и десериализовать тестирование
                        try
                        {
                            string jsonTestingHeader = File.ReadAllText(testingResultsDirectories[i] + "\\testingHeader.json");
                            testingHeader = JsonSerializer.Deserialize<TestingHeader>(jsonTestingHeader);
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
                        DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Заголовок результата тестирования: " + testingResultsDirectories[i] + " - поврежден, не удалось считать его."); }));
                        //DeleteTestingResultDirectory(testingResultsDirectories[i], true, "Заголовок результата тестирования: " + testingResultsDirectories[i] + " - поврежден, и результат тестирования был удален.", "Заголовок результата тестирования: " + testingResultsDirectories[i] + " - поврежден, и при попытке удалить результат тестирования возникло исключение.");
                    }
                }
            }
            while (iteration < 2);
        }

        public List<DataSourceCandles> ReadDataSourceCandles(Testing testing, bool isHistory, DataSourceGroup dataSourceGroup) //считывает свечки для полученных источников данных
        {
            List<DataSourceCandles> dataSourcesCandles = new List<DataSourceCandles>(); //new DataSourceCandles[dataSourceGroup.DataSourceAccordances.Count];
            string dataSourceCandlesPath = isHistory ? Directory.GetCurrentDirectory() + _historyRealivePath : Directory.GetCurrentDirectory() + _savesRealivePath; //путь к папке со свечками. Если isHistory == true значит в папке с историей, иначе - в папке с сохраненными
            dataSourceCandlesPath += "\\" + testing.TestingName + "\\dataSourcesCandles";
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            for (int i = 0; i < dataSourceGroup.DataSourceAccordances.Count; i++)
            {
                bool isBroken = false; //прозошли ли ошибки при попытки считать свечки
                string dataSourceCandlesPathFolder = dataSourceCandlesPath + "\\" + dataSourceGroup.DataSourceAccordances[i].DataSource.Id;
                if (Directory.Exists(dataSourceCandlesPathFolder) == true) //если существует папка со свечками для данного источника данных
                {
                    if(File.Exists(dataSourceCandlesPathFolder + "\\dataSourceCandles.dat") == true) //если существует файл со свечками
                    {
                        //пробуем считать и десериализовать тестирование
                        try
                        {
                            using (FileStream fileStream = new FileStream(dataSourceCandlesPathFolder + "\\dataSourceCandles.dat", FileMode.Open))
                            {
                                DataSourceCandles dataSourceCandles = (DataSourceCandles)binaryFormatter.Deserialize(fileStream); //десериализуем объект
                                dataSourcesCandles.Add(dataSourceCandles);
                            }
                            
                            /*string jsonDataSourceCandles = File.ReadAllText(dataSourceCandlesPathFolder + "\\dataSourceCandles.json");
                            dataSourceCandles[i] = JsonSerializer.Deserialize<DataSourceCandles>(jsonDataSourceCandles);*/
                        }
                        catch
                        {
                            isBroken = true;
                        }
                    }
                    else//файла со свечками не существует
                    {
                        isBroken = true;
                    }
                }
                else //папки не существует
                {
                    isBroken = true;
                }

                if (isBroken) //если не удалось считать файл, уведомляем об этом пользователя
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Файл со свечками: " + dataSourceCandlesPathFolder + "\\dataSourceCandles.dat не удалось считать."); }));
                }
            }

            return dataSourcesCandles;
        }

        public void ReadIndicatorValues(Testing testing, bool isHistory, TestRun testRun) //считывает значения индикаторов алгоритма для источников данных в testing.DataSourcesCandles и для параметров testRun
        {
            string dataSourceCandlesPath = isHistory ? Directory.GetCurrentDirectory() + _historyRealivePath : Directory.GetCurrentDirectory() + _savesRealivePath; //путь к папке со свечками. Если isHistory == true значит в папке с историей, иначе - в папке с сохраненными
            dataSourceCandlesPath += "\\" + testing.TestingName + "\\dataSourcesCandles";

            foreach(DataSourceCandles dataSourceCandles in testing.DataSourcesCandles) //проходим по всем testing.DataSourcesCandles (источникам данных) для которых имеются свечки и значения индикаторов
            {
                AlgorithmIndicatorValues[] algorithmIndicatorValues = new AlgorithmIndicatorValues[testing.Algorithm.AlgorithmIndicators.Count];
                for (int i = 0; i < dataSourceCandles.AlgorithmIndicatorCatalogs.Length; i++) //проходим по всем индикаторам для данного источника данных
                {
                    bool isBroken = false; //прозошли ли ошибки при попытки считать значения индикатора
                    //находим название файла со значениями текущего индикатора для указанных в testRun параметрах алгоритма
                    bool isFind = false; //найдено ли название файла
                    int indexCatalogElement = -1;
                    while(isFind == false)
                    {
                        indexCatalogElement++;
                        //проходим по всем параметрам текущего элемента каталога, и если все его значения параметров совпадают со значениями этих параметров у testRun, отмечаем что нашли элемент каталога с параметрами как у testRun
                        bool isAllEqual = true; //все ли значения параметров совпадают
                        foreach(AlgorithmParameterValue algorithmParameterValue in dataSourceCandles.AlgorithmIndicatorCatalogs[i].AlgorithmIndicatorCatalogElements[indexCatalogElement].AlgorithmParameterValues)
                        {
                            AlgorithmParameterValue algorithmParameterValueTestRun = testRun.AlgorithmParameterValues.Where(j => j.AlgorithmParameter.Id == algorithmParameterValue.AlgorithmParameter.Id).First(); //значение параметра алгоритма у testRun
                            if(algorithmParameterValue.AlgorithmParameter.ParameterValueType.Id == 1) //параметр типа int
                            {
                                if(algorithmParameterValue.IntValue != algorithmParameterValueTestRun.IntValue) //если не равны
                                {
                                    isAllEqual = false;
                                }
                            }
                            else //параметр типа double
                            {
                                if (algorithmParameterValue.DoubleValue != algorithmParameterValueTestRun.DoubleValue) //если не равны
                                {
                                    isAllEqual = false;
                                }
                            }
                        }

                        if (isAllEqual)
                        {
                            isFind = true;
                        }
                    }

                    //проверяем наличие файла со значениями индикатора
                    string dataSourceCandlesPathFolder = dataSourceCandlesPath + "\\" + dataSourceCandles.DataSource.Id; //папка с текущим источником данных
                    string dataSourceCandlesCurrentIndicatorPathFolder = dataSourceCandlesPathFolder + "\\" + dataSourceCandles.AlgorithmIndicatorCatalogs[i].AlgorithmIndicatorFolderName; //папка с текущим индикатором
                    string filePath = dataSourceCandlesCurrentIndicatorPathFolder + "\\" + dataSourceCandles.AlgorithmIndicatorCatalogs[i].AlgorithmIndicatorCatalogElements[indexCatalogElement].FileName;
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    if (Directory.Exists(dataSourceCandlesCurrentIndicatorPathFolder) == true) //если существует папка с текущим индикатором для данного источника данных
                    {
                        if (File.Exists(filePath) == true) //если существует файл с текущим индикатором
                        {
                            //пробуем считать и десериализовать
                            try
                            {
                                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                                {
                                    algorithmIndicatorValues[i] = (AlgorithmIndicatorValues)binaryFormatter.Deserialize(fileStream); //десериализуем объект
                                }
                                /*string jsonAlgorithmIndicatorValues = File.ReadAllText(filePath);
                                algorithmIndicatorValues[i] = JsonSerializer.Deserialize<AlgorithmIndicatorValues>(jsonAlgorithmIndicatorValues);*/
                            }
                            catch
                            {
                                isBroken = true;
                            }
                        }
                        else//файла со свечками не существует
                        {
                            isBroken = true;
                        }
                    }
                    else //папки не существует
                    {
                        isBroken = true;
                    }

                    if (isBroken) //если не удалось считать файл, уведомляем об этом пользователя
                    {
                        DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Файл со значениями индикатора: " + filePath + " не удалось считать."); }));
                    }
                }
                dataSourceCandles.AlgorithmIndicatorsValues = algorithmIndicatorValues;
            }
        }

        public bool CheckAndDeleteExpiredTestingResults() //проверяет наличие результатов тестирования в истории у которых вышел срок хранения, и удаляет их. Если было удаление возвращает true, иначе - false
        {
            bool isWasDelete = false; //было ли удаление
            TimeSpan timeSpanLife = TimeSpan.FromHours(_modelData.Settings.Where(j => j.Id == 5).First().DoubleValue); //время хранения результатов тестирования
            DateTime dateTimeNow = DateTime.Now; //текущая дата и время
            foreach(TestingHeader testingHeader in _testingHistory)
            {
                if(DateTime.Compare(testingHeader.DateTimeSimulationEnding.Add(timeSpanLife), dateTimeNow) < 0) //если дата звершения симуляции тестирования + срок хранения меньше текущей, значит срок вышел и нужно удалить данный результат тестирования
                {
                    //удаляем данный результат тестирования
                    string testingDirectory = Directory.GetCurrentDirectory(); //папка с тестированием
                    testingDirectory += testingHeader.IsHistory ? _historyRealivePath : _savesRealivePath;
                    testingDirectory += "\\" + testingHeader.TestingName;
                    DeleteTestingResultDirectory(testingDirectory, false, "", "У результата тестирования: " + testingHeader.TestingName + " - истек срок хранения, и при попытке его удалить возникло исключение.");
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

        public bool DeleteTestingResultDirectory(string directoryPath, bool isSuccessDeleteMessage, string successMessage, string exceptionMessage) //удаляет результат тестирования, и возвращает true в случае успешного удаления, и false в случае возникновения исключения
        {
            bool isDeleteException = false;
            try
            {
                Directory.Delete(directoryPath, true);
            }
            catch
            {
                isDeleteException = true;
            }
            if (isDeleteException == false) //если успешно удалено
            {
                if (isSuccessDeleteMessage)
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage(successMessage); }));
                }
            }
            else //если возникло исключение
            {
                DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage(exceptionMessage); }));
            }
            return !isDeleteException;
        }

        public Testing LoadTesting(TestingHeader testingHeader) //считывает и возвращает testing в случае успешного считывания, и null в случае ошибки
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string testingDirectory = Directory.GetCurrentDirectory(); //папка с тестированием
            testingDirectory += testingHeader.IsHistory ? _historyRealivePath : _savesRealivePath;
            testingDirectory += "\\" + testingHeader.TestingName;
            Testing testing = null;
            bool isException = false; //было ли исключение при считывании
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (FileStream fileStream = new FileStream(testingDirectory + "\\testing.dat", FileMode.Open))
                {
                    testing = (Testing)binaryFormatter.Deserialize(fileStream); //десериализуем объект
                }
                //считываем все testRun
                foreach (TestBatch testBatch in testing.TestBatches)
                {
                    testBatch.OptimizationTestRuns = new List<TestRun>();
                    string testBatchFolder = testingDirectory + "\\testBatches\\" + testBatch.Number.ToString();
                    //считываем все оптимизационные тестовые прогоны
                    int testRunNumber = 1;
                    while(File.Exists(testBatchFolder + "\\" + testRunNumber.ToString() + ".dat"))
                    {
                        using (FileStream fileStream = new FileStream(testBatchFolder + "\\" + testRunNumber.ToString() + ".dat", FileMode.Open))
                        {
                            testBatch.OptimizationTestRuns.Add((TestRun)binaryFormatter.Deserialize(fileStream)); //десериализуем объект
                        }
                        testRunNumber++;
                    }
                    if(testBatch.OptimizationTestRuns.Where(a => a.Number == testBatch.TopModelTestRunNumber).Any())
                    {
                        testBatch.TopModelTestRun = testBatch.OptimizationTestRuns.Where(a => a.Number == testBatch.TopModelTestRunNumber).First();
                    }
                    if(testing.IsForwardTesting && testBatch.IsTopModelWasFind)
                    {
                        //считываем форвардный тест
                        using (FileStream fileStream = new FileStream(testBatchFolder + "\\forwardTestRun.dat", FileMode.Open))
                        {
                            testBatch.ForwardTestRun = (TestRun)binaryFormatter.Deserialize(fileStream); //десериализуем объект
                        }
                        if (testing.IsForwardDepositTrading)
                        {
                            //считываем форвардный тест с торговлей депозитом
                            using (FileStream fileStream = new FileStream(testBatchFolder + "\\forwardTestRunDepositTrading.dat", FileMode.Open))
                            {
                                testBatch.ForwardTestRunDepositTrading = (TestRun)binaryFormatter.Deserialize(fileStream); //десериализуем объект
                            }
                        }
                    }
                }
            }
            catch
            {
                isException = true;
            }
            stopwatch.Stop();
            if (isException == false) //если считывание прошло успешно
            {
                //DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Время считывания результата тестирования: " + testingHeader.TestingName + " заняло " + stopwatch.ElapsedMilliseconds + " милисекунд"); }));
                return testing;
            }
            else //если было исключение при считывании, удаляем результат тестирования
            {
                DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Результат тестирования: " + testingHeader.TestingName + " - поврежден, не удалось считать его."); }));
                //DeleteTestingResultDirectory(testingDirectory, true, "Результат тестирования: " + testingHeader.TestingName + " - поврежден, и был удален.", "Результат тестирования: " + testingHeader.TestingName + " - поврежден, и при попытке его удалить возникло исключение.");
                ReadAndCheckTestingResults(); //обновляем списки с результатами тестирования
                return null;
            }
        }
    }
}
