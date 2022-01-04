using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            _dataSources = _modelData.DataSources;
            _mainCommunicationChannel = MainCommunicationChannel.getInstance();
        }

        private Database _database;
        private ModelData _modelData;
        private MainCommunicationChannel _mainCommunicationChannel;

        private ObservableCollection<DataSource> _dataSources = new ObservableCollection<DataSource>();
        
        //создание объекта из данных которые набрал пользователь и добавление в DataSources
        public void CreateDataSourceInsertUpdate(string name, Instrument instrument, Currency currency, double? cost, Comissiontype comissiontype, double comission, double priceStep, double costPriceStep, List<DataSourceFile> dataSourceFiles, int id = -1) //метод проверяет присланные данные на корректность и вызывает метод добавления записи в бд или обновления существующей записи если был прислан id
        {
            _mainCommunicationChannel.DataSourceAddingProgress.Clear();

            List<Interval> intervalsInFiles = new List<Interval>(); //список с интервалами файлов, чтобы проверить, имеют ли все файлы одинаковые интервалы
            List<DateTime> datesInFiles = new List<DateTime>(); //список с первыми датами файлов, чтобы проверить, нет ли дублирующихся дат в разных файлах
            //удостоверяемся в том что все файлы имеют допустимый формат данных
            string headerFormat = "<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>";
            string headerErrorFiles = "";
            string unknownIntervalFiles = ""; //файлы с неизвестными интервалами
            bool isAllFilesCorrect = true; //правильный ли формат файлов
            foreach (DataSourceFile item in dataSourceFiles)
            {
                FileStream fileStream = new FileStream(item.Path, FileMode.Open, FileAccess.Read);
                StreamReader streamReader = new StreamReader(fileStream);
                bool isHeaderCorrect = true; //правильный ли заголовок файла
                string header = streamReader.ReadLine();
                if(header != headerFormat)
                {
                    isHeaderCorrect = false;
                    headerErrorFiles = headerErrorFiles.Length == 0 ? item.Path.Split('\\').Last<string>() : headerErrorFiles + ", " + item.Path.Split('\\').Last<string>();
                }

                if (isHeaderCorrect)
                {
                    //считываем первые 3000 строк файла для определения интервала

                    //считываем первую строку и определяем время
                    string line = streamReader.ReadLine();
                    string[] lineArr = line.Split(',');
                    string dateTimeFormated = lineArr[2].Insert(6, "-").Insert(4, "-") + " " + lineArr[3].Insert(4, ":").Insert(2, ":");
                    DateTime lastTime = DateTime.Parse(dateTimeFormated);
                    datesInFiles.Add(lastTime); //записываем первую дату файла
                    //считываем вторую строку и определяем время
                    line = streamReader.ReadLine();
                    lineArr = line.Split(',');
                    dateTimeFormated = lineArr[2].Insert(6, "-").Insert(4, "-") + " " + lineArr[3].Insert(4, ":").Insert(2, ":");
                    DateTime currentTime = DateTime.Parse(dateTimeFormated);
                    //определяем интервал
                    double interval = (currentTime - lastTime).TotalMinutes;
                    //считываем следующую строку
                    line = streamReader.ReadLine();
                    int l = 0;
                    while (l < 3000 && line != null)
                    {
                        lineArr = line.Split(',');
                        dateTimeFormated = lineArr[2].Insert(6, "-").Insert(4, "-") + " " + lineArr[3].Insert(4, ":").Insert(2, ":");
                        currentTime = DateTime.Parse(dateTimeFormated);
                        if ((currentTime - lastTime).TotalMinutes < interval)
                        {
                            interval = (currentTime - lastTime).TotalMinutes;
                        }

                        lastTime = currentTime;
                        line = streamReader.ReadLine();
                        l++;
                    }

                    //определяем интервал
                    switch ((int)interval)
                    {
                        case 1:
                            intervalsInFiles.Add(_modelData.Intervals[0]);
                            break;
                        case 5:
                            intervalsInFiles.Add(_modelData.Intervals[1]);
                            break;
                        case 10:
                            intervalsInFiles.Add(_modelData.Intervals[2]);
                            break;
                        case 15:
                            intervalsInFiles.Add(_modelData.Intervals[3]);
                            break;
                        case 30:
                            intervalsInFiles.Add(_modelData.Intervals[4]);
                            break;
                        case 60:
                            intervalsInFiles.Add(_modelData.Intervals[5]);
                            break;
                        case 1440:
                            intervalsInFiles.Add(_modelData.Intervals[6]);
                            break;
                        default:
                            intervalsInFiles.Add(new Interval { Id = -1, Name = "not found" });
                            unknownIntervalFiles = unknownIntervalFiles.Length == 0 ? item.Path.Split('\\').Last<string>() : unknownIntervalFiles + ", " + item.Path.Split('\\').Last<string>();
                            break;
                    }
                }
                else
                {
                    isAllFilesCorrect = false;
                }
            }
            if (isAllFilesCorrect == false)
            {
                _mainCommunicationChannel.AddMainMessage("Файлы с котировками: " + headerErrorFiles + " имеют неверный формат данных. Допустимый формат: \"" + headerFormat + "\".");
            }
            //определяем что нет не найденного интервала
            bool isNotFoundInterval = false;
            foreach (Interval item in intervalsInFiles)
            {
                if (item.Id == -1)
                {
                    isNotFoundInterval = true;
                }
            }
            if (isNotFoundInterval)
            {
                _mainCommunicationChannel.AddMainMessage("Не удается распознать временной интервал файлов с котировками: " + unknownIntervalFiles + ". Убедитесь что используются поддерживаемые временные интервалы.");
                isAllFilesCorrect = false;
            }
            //если все интервалы определены, определяем, нет ли отличающихся интервалов
            string unequalIntervalFiles = ""; //файлы с интервалами, отличающимися от первого
            bool isEqualIntervals = true;
            if(isNotFoundInterval == false)
            {
                for(int k = 1; k < intervalsInFiles.Count; k++)
                {
                    if (intervalsInFiles[k] != intervalsInFiles[0])
                    {
                        isEqualIntervals = false;
                        unequalIntervalFiles = unequalIntervalFiles.Length == 0 ? dataSourceFiles[k].Path.Split('\\').Last<string>() : unequalIntervalFiles + ", " + dataSourceFiles[k].Path.Split('\\').Last<string>();
                    }
                }
            }
            if (isEqualIntervals == false)
            {
                _mainCommunicationChannel.AddMainMessage("Файлы с котировками: " + unequalIntervalFiles + " имеют разные интервалы времени в свечках в сравнении с: " + dataSourceFiles[0].Path.Split('\\').Last<string>() + ".");
                isAllFilesCorrect = false;
            }

            //проверяем, нет ли файлов с одинаковыми первыми датами
            string ununiqueFiles = "";
            if (isAllFilesCorrect) //поставил проверку, т.к. в datesInFiles может не быть дат
            {
                bool isFirstDateUnique = true;
                for (int i = 0; i < datesInFiles.Count; i++) //проходим по датам и сравниваем дату со всеми датами, кроме неё самой, если есть такая же значит она не уникальна
                {
                    DateTime currentDate = datesInFiles[i];
                    for (int k = 0; k < datesInFiles.Count; k++)
                    {
                        DateTime checkDate = datesInFiles[k];
                        if(i != k)
                        {
                            if(currentDate == checkDate)
                            {
                                isFirstDateUnique = false;
                                ununiqueFiles = ununiqueFiles.Length == 0 ? dataSourceFiles[i].Path.Split('\\').Last<string>() + " и " + dataSourceFiles[k].Path.Split('\\').Last<string>() : ununiqueFiles + ", " + dataSourceFiles[i].Path.Split('\\').Last<string>() + " и " + dataSourceFiles[k].Path.Split('\\').Last<string>();
                            }
                        }
                    }
                }
                if(isFirstDateUnique == false)
                {
                    _mainCommunicationChannel.AddMainMessage("Файлы с котировками: " + ununiqueFiles + " имеют одинаковые даты.");
                    isAllFilesCorrect = false;
                }
            }
            
            //если все корректно, сортируем файлы по датам, создаем периоды с временем работы биржы для файлов, и записываем в БД
            if (isAllFilesCorrect)
            {
                //сортируем файлы
                List<DateTime> firstDates = new List<DateTime>();
                for(int i = 0; i < datesInFiles.Count; i++) //создаем список с первыми датами файлов
                {
                    firstDates.Add(datesInFiles[i]);
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

                /*
                var s = new Stopwatch();
                s.Start();
                for (int i = 0; i < 100000; i++)
                {
                    dateTimeFormated = date.Insert(8, " ").Insert(6, "-").Insert(4, "-") + time.Insert(4, ":").Insert(2, ":");
                }
                s.Stop();
                string duration = s.Elapsed.TotalMilliseconds.ToString();
                */

                //определяем время работы биржи для файлов, распараллеливая это на несколько ядер
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int processorCount = Environment.ProcessorCount; //количество создаваемых потоков
                if(dataSourceFiles.Count < processorCount) //если файлов меньше чем число доступных потоков, устанавливаем количество потоков на количество файлов, т.к. WaitAll ругается если поток в tasks null
                {
                    processorCount = dataSourceFiles.Count;
                }
                if(processorCount < 1)
                {
                    processorCount = 1;
                }
                var tasks = new Task[processorCount]; //задачи
                for(int i = 0; i < dataSourceFiles.Count; i++)
                {
                    if(i < processorCount)
                    {
                        DataSourceFile dataSourceFile = dataSourceFiles[i];
                        tasks[i] = Task.Run(() => DefiningFileWorkingPeriods(dataSourceFile));
                    }
                    else
                    {
                        int indexCompleted = Task.WaitAny(tasks);
                        _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                        _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { TasksCount = dataSourceFiles.Count, CompletedTasksCount = i + 1 - processorCount, ElapsedTime = stopwatch.Elapsed, IsFinish = false });
                        DataSourceFile dataSourceFile = dataSourceFiles[i];
                        tasks[indexCompleted] = Task.Run(() => DefiningFileWorkingPeriods(dataSourceFile));
                    }
                }
                Task.WaitAll(tasks);
                stopwatch.Stop();
                _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { TasksCount  = dataSourceFiles.Count, CompletedTasksCount = dataSourceFiles.Count, ElapsedTime = stopwatch.Elapsed, IsFinish = false });

                bool isAddCost = true; //нужно ли добавлять стоимость в запись (у акции нет стоимости)
                if(instrument.Id == 1)
                {
                    isAddCost = false;
                }

                if (id == -1)
                {
                    _database.InsertDataSource(name, instrument, intervalsInFiles[0], currency, cost, comissiontype, comission, priceStep, costPriceStep, isAddCost);
                    _modelData.ReadDataSources();
                    //вставляем записи dataSourceFiles для данного источника данных
                    int newDataSourceId = _modelData.DataSources[_modelData.DataSources.Count - 1].Id;
                    foreach (DataSourceFile dataSourceFile in dataSourceFiles)
                    {
                        dataSourceFile.IdDataSource = newDataSourceId;
                        _database.InsertDataSourceFile(dataSourceFile);
                        _modelData.ReadDataSourceFiles();
                        //вставляем записи DataSourceFileWorkingPeriods для только что вставленного DataSourceFile
                        int newDataSourceFileId = _modelData.DataSourceFiles[_modelData.DataSourceFiles.Count - 1].Id;
                        foreach(DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod in dataSourceFile.DataSourceFileWorkingPeriods)
                        {
                            dataSourceFileWorkingPeriod.IdDataSourceFile = newDataSourceFileId;
                            _database.InsertDataSourceFileWorkingPeriod(dataSourceFileWorkingPeriod);
                        }
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
                    //добавляем
                    foreach (DataSourceFile dataSourceFile1 in addDataSourceFiles)
                    {
                        _database.InsertDataSourceFile(dataSourceFile1);
                    }
                    //удаляем
                    foreach (int idDeleteDataSourceFile in deleteDataSourceFiles)
                    {
                        _database.DeleteDataSourceFile(idDeleteDataSourceFile);
                    }
                    _modelData.ReadDataSourceFiles();

                    //обновляю dataSourceFileWorkingPeriods
                    for(int k = 0; k < dataSourceFiles.Count; k++)
                    {
                        DataSourceFile oldDataSourceFile = _modelData.DataSourceFiles[k];
                        
                        List<DataSourceFileWorkingPeriod> updateDataSourceFileWorkingPeriods = new List<DataSourceFileWorkingPeriod>(); //список с id записи которую нужно обновить и данные на которые нужно обновить
                        List<DataSourceFileWorkingPeriod> addDataSourceFileWorkingPeriods = new List<DataSourceFileWorkingPeriod>(); //список с записями которые нужно добавить
                        List<int> deleteDataSourceFileWorkingPeriods = new List<int>(); //список с id записей которые нужно удалить

                        int maxLengthDataSourceFileWorkingPeriod = dataSourceFiles[k].DataSourceFileWorkingPeriods.Count;
                        if (oldDataSourceFile.DataSourceFileWorkingPeriods.Count > maxLengthDataSourceFileWorkingPeriod)
                        {
                            maxLengthDataSourceFileWorkingPeriod = oldDataSourceFile.DataSourceFileWorkingPeriods.Count;
                        }
                        //проходим по всем элементам старого и нового списка, и определяем какие обновить, какие добавить, а какие удалить
                        for (int i = 0; i < maxLengthDataSourceFileWorkingPeriod; i++)
                        {
                            if (dataSourceFiles[k].DataSourceFileWorkingPeriods.Count > i && oldDataSourceFile.DataSourceFileWorkingPeriods.Count > i)
                            {
                                if (DateTime.Compare(dataSourceFiles[k].DataSourceFileWorkingPeriods[i].StartPeriod, oldDataSourceFile.DataSourceFileWorkingPeriods[i].StartPeriod) != 0 || DateTime.Compare(dataSourceFiles[k].DataSourceFileWorkingPeriods[i].TradingStartTime, oldDataSourceFile.DataSourceFileWorkingPeriods[i].TradingStartTime) != 0 || DateTime.Compare(dataSourceFiles[k].DataSourceFileWorkingPeriods[i].TradingEndTime, oldDataSourceFile.DataSourceFileWorkingPeriods[i].TradingEndTime) != 0)
                                {
                                    dataSourceFiles[k].DataSourceFileWorkingPeriods[i].Id = oldDataSourceFile.DataSourceFileWorkingPeriods[i].Id;
                                    dataSourceFiles[k].DataSourceFileWorkingPeriods[i].IdDataSourceFile = oldDataSourceFile.DataSourceFileWorkingPeriods[i].IdDataSourceFile;
                                    updateDataSourceFileWorkingPeriods.Add(dataSourceFiles[k].DataSourceFileWorkingPeriods[i]);
                                }
                            }
                            else if (dataSourceFiles[k].DataSourceFileWorkingPeriods.Count > i && oldDataSourceFile.DataSourceFileWorkingPeriods.Count <= i)
                            {
                                dataSourceFiles[k].DataSourceFileWorkingPeriods[i].IdDataSourceFile = oldDataSourceFile.Id;
                                addDataSourceFileWorkingPeriods.Add(dataSourceFiles[k].DataSourceFileWorkingPeriods[i]);
                            }
                            else if (dataSourceFiles[k].DataSourceFileWorkingPeriods.Count <= i && oldDataSourceFile.DataSourceFileWorkingPeriods.Count > i)
                            {
                                deleteDataSourceFileWorkingPeriods.Add(oldDataSourceFile.DataSourceFileWorkingPeriods[i].Id);
                            }
                        }
                        //обновляем
                        foreach (DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod in updateDataSourceFileWorkingPeriods)
                        {
                            _database.UpdateDataSourceFileWorkingPeriod(dataSourceFileWorkingPeriod);
                        }
                        //добавляем
                        foreach (DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod1 in addDataSourceFileWorkingPeriods)
                        {
                            _database.InsertDataSourceFileWorkingPeriod(dataSourceFileWorkingPeriod1);
                        }
                        //удаляем
                        foreach (int idDeleteDataSourceFileWorkingPeriod in deleteDataSourceFileWorkingPeriods)
                        {
                            _database.DeleteDataSourceFileWorkingPeriod(idDeleteDataSourceFileWorkingPeriod);
                        }
                    }

                    //обновляю DataSource
                    _database.UpdateDataSource(name, instrument, intervalsInFiles[0], currency, cost, comissiontype, comission, priceStep, costPriceStep, isAddCost, id);
                }
                
                _modelData.ReadDataSources();

                _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { TasksCount = dataSourceFiles.Count, CompletedTasksCount = dataSourceFiles.Count, ElapsedTime = stopwatch.Elapsed, IsFinish = true });
            }
        }

        private void DefiningFileWorkingPeriods(DataSourceFile dataSourceFile)
        {
            //проходим по датам и для каждого дня формируем время начала и окончания. Далее сравниваем время начала и окончания существующего объекта с прошлым днем, и если отличаются создаем новый объект периода

            List<DataSourceFileWorkingPeriod> dataSourceFileWorkingPeriods = new List<DataSourceFileWorkingPeriod>();
            DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod = new DataSourceFileWorkingPeriod();

            FileStream fileStream = new FileStream(dataSourceFile.Path, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);
            string line = streamReader.ReadLine(); //пропускаем шапку файла
            //считываем первую строку и записываем в dataSourceFileWorkingPeriod чтобы не ставить условие на первое считывание в цикле
            line = streamReader.ReadLine();
            string[] lineArr = line.Split(',');
            string dateFormated = lineArr[2].Insert(6, "-").Insert(4, "-");
            string timeFormated = lineArr[3].Insert(4, ":").Insert(2, ":");
            dataSourceFileWorkingPeriod.StartPeriod = DateTime.Parse(dateFormated);
            dataSourceFileWorkingPeriod.TradingStartTime = DateTime.Parse(timeFormated);

            DateTime lastDate; //прошлая дата, чтобы определять что день сменился
            DateTime lastOpenTime = new DateTime(); //время открытия прошлого дня
            DateTime lastCloseTime = new DateTime(); //время закрытия прошлого дня
            lastDate = dataSourceFileWorkingPeriod.StartPeriod;
            lastOpenTime = dataSourceFileWorkingPeriod.TradingStartTime;
            line = streamReader.ReadLine();
            while (line != null)
            {
                lineArr = line.Split(',');
                dateFormated = lineArr[2].Insert(6, "-").Insert(4, "-");
                timeFormated = lineArr[3].Insert(4, ":").Insert(2, ":");
                DateTime currentDate = DateTime.Parse(dateFormated);
                if (dataSourceFileWorkingPeriod.TradingEndTime == null) //при формировании первого объекта, если день сменился, записываем время окончания, а так же дату и время открытия нового дня
                {
                    if((lastDate.Date - dataSourceFileWorkingPeriod.StartPeriod.Date).TotalDays >= 1)
                    {
                        dataSourceFileWorkingPeriod.TradingEndTime = lastCloseTime; //записали время окончания первого дня
                        dataSourceFileWorkingPeriods.Add(dataSourceFileWorkingPeriod);
                    }
                }
                else if((currentDate.Date - lastDate.Date).TotalDays >= 1) //определяем что прошлый день сформировался (т.к. наступил новый)
                {
                    //сравниваем прошлый день и объект dataSourceFileWorkingPeriod
                    if(lastOpenTime.TimeOfDay != dataSourceFileWorkingPeriod.TradingStartTime.TimeOfDay || lastCloseTime.TimeOfDay != dataSourceFileWorkingPeriod.TradingEndTime.TimeOfDay)
                    {
                        //если не совпадают, формируем новый объект dataSourceFileWorkingPeriod
                        dataSourceFileWorkingPeriod = new DataSourceFileWorkingPeriod { StartPeriod = lastDate, TradingStartTime = lastOpenTime, TradingEndTime = lastCloseTime };
                        dataSourceFileWorkingPeriods.Add(dataSourceFileWorkingPeriod);
                    }
                    //начинаем запись нового дня
                    lastDate = currentDate;
                    lastOpenTime = DateTime.Parse(timeFormated);
                    lastCloseTime = lastOpenTime;
                }
                else
                {
                    lastCloseTime = DateTime.Parse(timeFormated);
                }

                line = streamReader.ReadLine();
            }
            streamReader.Close();
            fileStream.Close();
            //т.к. после последней записи последнего дня цикл заканчивается, проверяем этот день на соответствие текущему dataSourceFileWorkingPeriod
            //сравниваем прошлый день и объект dataSourceFileWorkingPeriod
            if (lastOpenTime.TimeOfDay != dataSourceFileWorkingPeriod.TradingStartTime.TimeOfDay || lastCloseTime.TimeOfDay != dataSourceFileWorkingPeriod.TradingEndTime.TimeOfDay)
            {
                //если не совпадают, формируем новый объект dataSourceFileWorkingPeriod
                dataSourceFileWorkingPeriod = new DataSourceFileWorkingPeriod { StartPeriod = lastDate, TradingStartTime = lastOpenTime, TradingEndTime = lastCloseTime };
                dataSourceFileWorkingPeriods.Add(dataSourceFileWorkingPeriod);
            }

            dataSourceFile.DataSourceFileWorkingPeriods = dataSourceFileWorkingPeriods;
        }

        public void DeleteDataSource(int id)
        {
            _database.DeleteDataSource(id);
            _modelData.ReadDataSources();
            _modelData.NotifyDataSourcesSubscribers();
        }
    }
}
