using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Collections.ObjectModel;
using System.Data;
using ktradesystem.Models.Datatables;
using ktradesystem.CommunicationChannel;
using System.Diagnostics;

namespace ktradesystem.Models
{
    class ModelDataSource : ModelBase
    {
        private static ModelDataSource _instance;

        public static ModelDataSource getInstance()
        {
            if (_instance == null)
            {
                _instance = new ModelDataSource();
            }
            return _instance;
        }

        private ModelDataSource()
        {
            _database = Database.getInstance();
            _modelData = ModelData.getInstance();
            _mainCommunicationChannel = MainCommunicationChannel.getInstance();
        }

        private Database _database;
        private ModelData _modelData;
        private MainCommunicationChannel _mainCommunicationChannel;

        private CancellationTokenSource _cancellationTokenSourceDataSource; //токен отмены операции добавления источника данных
        public void CancellationTokenSourceDataSourceCancel()
        {
            if(_cancellationTokenSourceDataSource != null)
            {
                _cancellationTokenSourceDataSource.Cancel();
            }
        }

        //создание объекта из данных которые набрал пользователь и добавление в DataSources
        public void CreateDataSourceInsertUpdate(string name, MarginType marginType, Currency currency, double marginCost, decimal minLotCount, double minLotMarginPartCost, Comissiontype comissiontype, double comission, double priceStep, double costPriceStep, int pointsSlippage, List<DataSourceFile> dataSourceFiles, int id = -1) //метод проверяет присланные данные на корректность и вызывает метод добавления записи в бд или обновления существующей записи если был прислан id
        {
            _cancellationTokenSourceDataSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSourceDataSource.Token;

            DispatcherInvoke((Action)(() => { _mainCommunicationChannel.DataSourceAddingProgress.Clear(); }));

            List<DateTime> startFilesDateTime = new List<DateTime>(); //даты начала файлов
            string unknownIntervalFiles = ""; //файлы с неизвестными интервалами
            bool isAllFilesCorrect = true; //правильный ли формат файлов
            foreach (DataSourceFile item in dataSourceFiles)
            {
                startFilesDateTime.Add(DefineStartFileDateTime(item.Path));
            }

            //проверяем, нет ли файлов с одинаковыми первыми датами
            string ununiqueFiles = "";
            if (isAllFilesCorrect) //поставил проверку, т.к. в datesInFiles может не быть дат
            {
                bool isFirstDateUnique = true;
                for (int i = 0; i < startFilesDateTime.Count; i++) //проходим по датам и сравниваем дату со всеми датами, кроме неё самой, если есть такая же значит она не уникальна
                {
                    DateTime currentDate = startFilesDateTime[i];
                    for (int k = 0; k < startFilesDateTime.Count; k++)
                    {
                        DateTime checkDate = startFilesDateTime[k];
                        if (i != k)
                        {
                            if (currentDate == checkDate)
                            {
                                isFirstDateUnique = false;
                                ununiqueFiles = ununiqueFiles.Length == 0 ? dataSourceFiles[i].Path.Split('\\').Last<string>() + " и " + dataSourceFiles[k].Path.Split('\\').Last<string>() : ununiqueFiles + ", " + dataSourceFiles[i].Path.Split('\\').Last<string>() + " и " + dataSourceFiles[k].Path.Split('\\').Last<string>();
                            }
                        }
                    }
                }
                if (isFirstDateUnique == false)
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Файлы с котировками: " + ununiqueFiles + " имеют одинаковые даты."); }));
                    isAllFilesCorrect = false;
                }
            }
            
            
            //если все корректно, сортируем файлы по датам, создаем периоды с временем работы биржы и интервалы для файлов, и записываем в БД
            if (isAllFilesCorrect)
            {
                //сортируем файлы
                List<DateTime> firstDates = new List<DateTime>();
                for(int i = 0; i < startFilesDateTime.Count; i++) //создаем список с первыми датами файлов
                {
                    firstDates.Add(startFilesDateTime[i]);
                }
                DateTime saveDate; //первая дата файла для сохранения после удаления из элемента списка
                DataSourceFile saveFile; //элемент файла для сохранения после удаления из элемента списка
                for (int i = 0; i < firstDates.Count; i++)
                {
                    for (int k = 0; k < firstDates.Count - 1; k++)
                    {
                        if (DateTime.Compare(firstDates[k], firstDates[k + 1]) > 0)
                        {
                            saveDate = firstDates[k];
                            firstDates[k] = firstDates[k + 1];
                            firstDates[k + 1] = saveDate;

                            saveFile = dataSourceFiles[k];
                            dataSourceFiles[k] = dataSourceFiles[k + 1];
                            dataSourceFiles[k + 1] = saveFile;
                        }
                    }
                }

                //определяем время работы биржи для файлов и интервалы
                List<Interval> intervalsInFiles = new List<Interval>(); //список с интервалами файлов, чтобы проверить, имеют ли все файлы одинаковые интервалы
                List<DateTime> endFilesDateTime = new List<DateTime>(); //даты окончания файлов
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string progressHeader = id == -1 ? "Добавление источника данных" : "Редактирование источника данных";
                int taskCount = id == -1 ? dataSourceFiles.Count * 2 : dataSourceFiles.Count * 2 + 3;//количество задач (для прогресса выполнения)
                for(int i = 0; i < dataSourceFiles.Count; i++)
                {
                    //определяем интервалы для всех файлов
                    intervalsInFiles.Add(DefineInterval(dataSourceFiles[i].Path));
                    endFilesDateTime.Add(DefineEndFileDateTime(dataSourceFiles[i].Path));
                    
                    if (cancellationToken.IsCancellationRequested) //если был запрос на отмену операции, прекращем функцию
                    {
                        AddDataSourceEnding();
                        return;
                    }

                    DispatcherInvoke((Action)(() => {
                        _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                        _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = progressHeader, TasksCount = taskCount, CompletedTasksCount = i + 1, ElapsedTime = stopwatch.Elapsed, CancelPossibility = true, IsFinish = false });
                    }));
                }

                if (cancellationToken.IsCancellationRequested) //если был запрос на отмену операции, прекращем функцию
                {
                    AddDataSourceEnding();
                    return;
                }

                DispatcherInvoke((Action)(() => {
                    _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                    _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = progressHeader, TasksCount = taskCount, CompletedTasksCount = dataSourceFiles.Count, ElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
                }));
                

                //определяем что нет не найденного интервала
                bool isNotFoundInterval = false;
                for(int h = 0; h < intervalsInFiles.Count; h++)
                {
                    if (intervalsInFiles[h].Id == -1)
                    {
                        isNotFoundInterval = true;
                        unknownIntervalFiles = unknownIntervalFiles.Length == 0 ? dataSourceFiles[h].Path.Split('\\').Last<string>() : unknownIntervalFiles + ", " + dataSourceFiles[h].Path.Split('\\').Last<string>();
                    }
                }
                if (isNotFoundInterval)
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Не удается распознать временной интервал файлов с котировками: " + unknownIntervalFiles + ". Убедитесь что используются поддерживаемые временные интервалы."); }));
                    
                    isAllFilesCorrect = false;
                }

                //если все интервалы определены, определяем, нет ли отличающихся интервалов
                string unequalIntervalFiles = ""; //файлы с интервалами, отличающимися от первого
                bool isEqualIntervals = true;
                if (isNotFoundInterval == false)
                {
                    for (int k = 1; k < intervalsInFiles.Count; k++)
                    {
                        if (intervalsInFiles[k].Id != intervalsInFiles[0].Id)
                        {
                            isEqualIntervals = false;
                            unequalIntervalFiles = unequalIntervalFiles.Length == 0 ? dataSourceFiles[k].Path.Split('\\').Last<string>() : unequalIntervalFiles + ", " + dataSourceFiles[k].Path.Split('\\').Last<string>();
                        }
                    }
                }
                if (isEqualIntervals == false)
                {
                    DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Файлы с котировками: " + unequalIntervalFiles + " имеют разные интервалы времени в свечках в сравнении с: " + dataSourceFiles[0].Path.Split('\\').Last<string>() + "."); }));
                    
                    isAllFilesCorrect = false;
                }

                //определяем первую и последнюю даты источника данных
                DateTime startDate = startFilesDateTime[0];
                DateTime endDate = endFilesDateTime.Last();

                //если интервалы корректны, выполняем добавление или обновление записей в БД
                if (isAllFilesCorrect)
                {
                    if (id == -1)
                    {
                        _database.InsertDataSource(name, marginType, intervalsInFiles[0], currency, marginCost, minLotCount, minLotMarginPartCost, comissiontype, comission, priceStep, costPriceStep, pointsSlippage, startDate, endDate);
                        _modelData.ReadDataSources();

                        int a = 1;

                        //вставляем записи dataSourceFiles для данного источника данных
                        int newDataSourceId = _modelData.DataSources.Last().Id;
                        foreach (DataSourceFile dataSourceFile in dataSourceFiles)
                        {
                            dataSourceFile.IdDataSource = newDataSourceId;
                            _database.InsertDataSourceFile(dataSourceFile);
                            //_modelData.ReadDataSources();
                            DataSource newDataSource = _modelData.SelectDataSourceById(newDataSourceId); //считываем только только что добавленный источник данных, т.к. нет необходимости считывать все

                            DispatcherInvoke((Action)(() => {
                                _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                                _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = progressHeader, TasksCount = taskCount, CompletedTasksCount = dataSourceFiles.Count + a, ElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
                            }));
                            
                            a++;
                        }
                    }
                    else
                    {
                        //обновлю dataSourceFiles, после считаю заного все записи из бд, и обновлю dataSourceFileWorkingPeriods, далее обновлю DataSource
                        DataSource oldDataSource = new DataSource();
                        foreach (DataSource dataSource in _modelData.DataSources)
                        {
                            if (dataSource.Id == id)
                            {
                                oldDataSource = dataSource;
                            }
                        }

                        List<DataSourceFile> updateDataSourceFiles = new List<DataSourceFile>(); //список с id записи которую нужно обновить и данные на которые нужно обновить
                        List<DataSourceFile> addDataSourceFiles = new List<DataSourceFile>(); //список с записями которые нужно добавить
                        List<int> deleteDataSourceFiles = new List<int>(); //список с id записей которые нужно удалить

                        int maxLengthDataSourceFile = dataSourceFiles.Count;
                        if (oldDataSource.DataSourceFiles.Count > maxLengthDataSourceFile)
                        {
                            maxLengthDataSourceFile = oldDataSource.DataSourceFiles.Count;
                        }
                        //проходим по всем элементам старого и нового списка, и определяем какие обновить, какие добавить, а какие удалить
                        for (int i = 0; i < maxLengthDataSourceFile; i++)
                        {
                            if (dataSourceFiles.Count > i && oldDataSource.DataSourceFiles.Count > i)
                            {
                                if (dataSourceFiles[i].Path != oldDataSource.DataSourceFiles[i].Path)
                                {
                                    dataSourceFiles[i].Id = oldDataSource.DataSourceFiles[i].Id;
                                    dataSourceFiles[i].IdDataSource = oldDataSource.DataSourceFiles[i].IdDataSource;
                                    updateDataSourceFiles.Add(dataSourceFiles[i]);
                                }
                            }
                            else if (dataSourceFiles.Count > i && oldDataSource.DataSourceFiles.Count <= i)
                            {
                                dataSourceFiles[i].IdDataSource = oldDataSource.Id;
                                addDataSourceFiles.Add(dataSourceFiles[i]);
                            }
                            else if (dataSourceFiles.Count <= i && oldDataSource.DataSourceFiles.Count > i)
                            {
                                deleteDataSourceFiles.Add(oldDataSource.DataSourceFiles[i].Id);
                            }
                        }
                        //обновляем
                        foreach (DataSourceFile dataSourceFile in updateDataSourceFiles)
                        {
                            _database.UpdateDataSourceFile(dataSourceFile);
                        }
                        DispatcherInvoke((Action)(() => {
                            _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                            _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = progressHeader, TasksCount = taskCount, CompletedTasksCount = dataSourceFiles.Count + 1, ElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
                        }));
                        //добавляем
                        foreach (DataSourceFile dataSourceFile1 in addDataSourceFiles)
                        {
                            _database.InsertDataSourceFile(dataSourceFile1);
                        }
                        DispatcherInvoke((Action)(() => {
                            _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                            _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = progressHeader, TasksCount = taskCount, CompletedTasksCount = dataSourceFiles.Count + 2, ElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
                        }));
                        //удаляем
                        foreach (int idDeleteDataSourceFile in deleteDataSourceFiles)
                        {
                            _database.DeleteDataSourceFile(idDeleteDataSourceFile);
                        }
                        DispatcherInvoke((Action)(() => {
                            _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                            _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = progressHeader, TasksCount = taskCount, CompletedTasksCount = dataSourceFiles.Count + 3, ElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
                        }));

                        //помещаю в dataSourceFiles их id из БД
                        DataSource currentDataSource = _modelData.SelectDataSourceById(oldDataSource.Id);
                        for(int r = 0; r < currentDataSource.DataSourceFiles.Count; r++)
                        {
                            dataSourceFiles[r].Id = currentDataSource.DataSourceFiles[r].Id;
                        }

                        //обновляю DataSource
                        _database.UpdateDataSource(name, marginType, intervalsInFiles[0], currency, marginCost, minLotCount, minLotMarginPartCost, comissiontype, comission, priceStep, costPriceStep, pointsSlippage, startDate, endDate, id);
                    };

                    _modelData.ReadDataSources();
                }
            }
            AddDataSourceEnding();
        }

        private void AddDataSourceEnding()
        {
            DispatcherInvoke((Action)(() => {
                _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = "Редактирование исчтоника данных", TasksCount = 1, CompletedTasksCount = 1, ElapsedTime = TimeSpan.FromSeconds(1), CancelPossibility = false, IsFinish = true });
            }));
            
        }

        private Interval DefineInterval(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);

            string line = streamReader.ReadLine(); //пропускаем шапку файла
            line = streamReader.ReadLine();
            int intervalSeconds = -1;
            int lastTimestampSeconds = -1;
            int i = 0;
            while (line != null && i < 100)
            {
                string[] lineArr = line.Split(',');
                int timestampSeconds = int.Parse(lineArr[0].Remove(lineArr[0].Length - 3));
                if(lastTimestampSeconds > 0)
                {
                    if(intervalSeconds > 0)
                    {
                        if(timestampSeconds - lastTimestampSeconds < intervalSeconds)
                        {
                            intervalSeconds = timestampSeconds - lastTimestampSeconds;
                        }
                    }
                    else
                    {
                        intervalSeconds = timestampSeconds - lastTimestampSeconds;
                    }
                }
                lastTimestampSeconds = timestampSeconds;
                line = streamReader.ReadLine();
                i++;
            }

            streamReader.Close();
            fileStream.Close();

            Interval interval = new Interval();

            //определяем интервал
            switch (intervalSeconds)
            {
                case 60:
                    interval.Id = _modelData.Intervals[0].Id;
                    break;
                case 300:
                    interval.Id = _modelData.Intervals[1].Id;
                    break;
                case 600:
                    interval.Id = _modelData.Intervals[2].Id;
                    break;
                case 900:
                    interval.Id = _modelData.Intervals[3].Id;
                    break;
                case 1800:
                    interval.Id = _modelData.Intervals[4].Id;
                    break;
                case 3600:
                    interval.Id = _modelData.Intervals[5].Id;
                    break;
                case 86400:
                    interval.Id = _modelData.Intervals[6].Id;
                    break;
                default:
                    interval.Id = -1;
                    break;
            }

            return interval;
        }

        private DateTime DefineEndFileDateTime(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);

            string line = streamReader.ReadLine(); //пропускаем шапку файла
            line = streamReader.ReadLine();
            int timestampSeconds = -1;
            while (line != null)
            {
                string[] lineArr = line.Split(',');
                timestampSeconds = int.Parse(lineArr[0].Remove(lineArr[0].Length - 3));
                line = streamReader.ReadLine();
            }

            streamReader.Close();
            fileStream.Close();

            return ModelFunctions.UnixTimestampToUtc(timestampSeconds);
        }

        private DateTime DefineStartFileDateTime(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);

            string line = streamReader.ReadLine(); //пропускаем шапку файла
            line = streamReader.ReadLine();
            string[] lineArr = line.Split(',');
            int timestampSeconds = int.Parse(lineArr[0].Remove(lineArr[0].Length - 3));

            streamReader.Close();
            fileStream.Close();

            return ModelFunctions.UnixTimestampToUtc(timestampSeconds);
        }

        public void DeleteDataSource(int id)
        {
            _database.DeleteDataSource(id);
            _modelData.ReadDataSources();
            _modelData.NotifyDataSourcesSubscribers();
        }
    }
}
