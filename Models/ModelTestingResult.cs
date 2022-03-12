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
            _modelSimulation = ModelSimulation.getInstance();
        }

        private MainCommunicationChannel _mainCommunicationChannel;
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
            /*for(int i = 0; i < testing.DataSourcesCandles.Length; i++)
            {
                //определяем каталоги индикаторов алгоритмов
                testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs = new List<AlgorithmIndicatorCatalog>();
                //проходим по всем индикаторам алгоритма
                foreach(AlgorithmIndicator algorithmIndicator in testing.Algorithm.AlgorithmIndicators)
                {
                    AlgorithmIndicatorCatalog algorithmIndicatorCatalog = new AlgorithmIndicatorCatalog { AlgorithmIndicator = algorithmIndicator, AlgorithmIndicatorFolderName = algorithmIndicator.Indicator.Name + "_" + algorithmIndicator.Ending + "_values", AlgorithmIndicatorCatalogElements = new List<AlgorithmIndicatorCatalogElement>() };

                    //получаем список параметров алгоритмов, используемых в индикаторе алгоритма
                    List<AlgorithmParameter> algorithmParameters = new List<AlgorithmParameter>();
                    foreach (IndicatorParameterRange indicatorParameterRange in algorithmIndicator.IndicatorParameterRanges)
                    {
                        if(algorithmParameters.Contains(indicatorParameterRange.AlgorithmParameter) == false)
                        {
                            algorithmParameters.Add(indicatorParameterRange.AlgorithmParameter);
                        }
                    }

                    if(algorithmParameters.Count == 0) //если нет параметров, значит только 1 вариант значений индикатора для источника данных
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
                        for( int k = 1; k < algorithmParameters.Count; k++)
                        {
                            int indexAlgorithmParameter = testing.Algorithm.AlgorithmParameters.IndexOf(algorithmParameters[k]); //индекс текущего параметра алгоритма
                            List<int[][]> newAlgorithmParameterCombinations = new List<int[][]>(); //новые комбинации. Для всех элементов старых комбинаций будут созданы комбинации с текущим параметром и старые комбинации обновятся на новые
                            for(int u = 0; u < algorithmParameterCombinations.Count; u++)
                            {
                                int countParameterValues2 = testing.Algorithm.AlgorithmParameters[indexAlgorithmParameter].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[indexAlgorithmParameter].Count : testing.AlgorithmParametersAllDoubleValues[indexAlgorithmParameter].Count; //количество значений текущего параметра алгоритма
                                for (int y = 0; y < countParameterValues2; y++)
                                {
                                    int[][] arr = new int[algorithmParameterCombinations[u].Length + 1][]; //увеличиваем количество элементов в комбинации на 1
                                    //заполняем комбинацию старыми элементами
                                    for(int x = 0; x < algorithmParameterCombinations[u].Length; x++)
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
                        for(int k = 0; k < algorithmParameterCombinations.Count; k++)
                        {
                            string fileName = "";
                            foreach(int[] value in algorithmParameterCombinations[k])
                            {
                                string parameterValue = testing.Algorithm.AlgorithmParameters[value[0]].ParameterValueType.Id == 1 ? testing.AlgorithmParametersAllIntValues[value[0]][value[1]].ToString() : testing.AlgorithmParametersAllDoubleValues[value[0]][value[1]].ToString();
                                fileName += fileName.Length == 0 ? "" : " "; //если это не первые символы названия, отделяем их пробелом от предыдущих
                                fileName += testing.Algorithm.AlgorithmParameters[value[0]].Name + "=" + parameterValue;
                            }
                            fileName += ".json";
                            AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement = new AlgorithmIndicatorCatalogElement { AlgorithmParameterValues = new List<AlgorithmParameterValue>(), FileName = fileName };
                            for(int u = 0; u < algorithmParameterCombinations[k].Length; u++)
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

                string dataSourcesCandlesPath = testingDirectoryPath + "\\dataSourcesCandles\\" + testing.DataSourcesCandles[i].DataSource.Id.ToString(); //путь к папке с текущим DataSourceCandles
                Directory.CreateDirectory(dataSourcesCandlesPath); //создаем папку с текущим DataSourceCandles
                string jsonDataSourceCandles = JsonSerializer.Serialize(testing.DataSourcesCandles[i]); //сериализуем
                File.WriteAllText(dataSourcesCandlesPath + "\\dataSourceCandles.json", jsonDataSourceCandles); //записываем в файл

                //вычисляем и записываем значения всех индикаторов алгоритмов со всеми комбинациями параметров
                foreach(AlgorithmIndicatorCatalog algorithmIndicatorCatalog1 in testing.DataSourcesCandles[i].AlgorithmIndicatorCatalogs)
                {
                    string algorithmIndicatorPath = dataSourcesCandlesPath + "\\" + algorithmIndicatorCatalog1.AlgorithmIndicatorFolderName; //путь к папке с значениями данного индикатора алгоритма
                    Directory.CreateDirectory(algorithmIndicatorPath); //создаем папку для значений индикатора алгоритма
                    foreach (AlgorithmIndicatorCatalogElement algorithmIndicatorCatalogElement in algorithmIndicatorCatalog1.AlgorithmIndicatorCatalogElements)
                    {
                        //вычисляем значения индикатора алгоритма
                        AlgorithmIndicatorValues algorithmIndicatorValues = _modelSimulation.AlgorithmIndicatorCalculate(testing, testing.DataSourcesCandles[i], algorithmIndicatorCatalog1.AlgorithmIndicator, algorithmIndicatorCatalogElement.AlgorithmParameterValues);
                        string jsonAlgorithmIndicatorValues = JsonSerializer.Serialize(algorithmIndicatorValues); //сериализуем
                        File.WriteAllText(algorithmIndicatorPath + "\\" + algorithmIndicatorCatalogElement.FileName, jsonAlgorithmIndicatorValues); //записываем в файл
                    }
                }
            }*/


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
                _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/3:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
            }));

            //записываем DataSourcesCandles
            for (int i = 0; i < testing.DataSourcesCandles.Length; i++)
            {
                string dataSourcesCandlesPath = testingDirectoryPath + "\\dataSourcesCandles\\" + testing.DataSourcesCandles[i].DataSource.Id.ToString(); //путь к папке с текущим DataSourceCandles
                Directory.CreateDirectory(dataSourcesCandlesPath); //создаем папку с текущим DataSourceCandles
                string jsonDataSourceCandles = JsonSerializer.Serialize(testing.DataSourcesCandles[i]); //сериализуем
                File.WriteAllText(dataSourcesCandlesPath + "\\dataSourceCandles.json", jsonDataSourceCandles); //записываем в файл
                countWrite++;
                DispatcherInvoke((Action)(() => {
                    _mainCommunicationChannel.TestingProgress.Clear();
                    _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/3:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
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
                            _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/3:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
                        }));
                    }
                }
            }

            stopwatch.Stop();
            DispatcherInvoke((Action)(() => {
                _mainCommunicationChannel.TestingProgress.Clear();
                _mainCommunicationChannel.TestingProgress.Add(new TestingProgress { StepDescription = "Шаг 3/3:  Запись результатов", StepTasksCount = countWrites, CompletedStepTasksCount = countWrite, TotalElapsedTime = ModelTesting.StopwatchTesting.Elapsed, StepElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = true });
            }));
            ModelTesting.StopwatchTesting.Stop();
        }
    }
}
