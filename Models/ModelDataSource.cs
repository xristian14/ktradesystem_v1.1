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
        public void CreateDataSourceInsertUpdate(string name, MarginType marginType, Currency currency, double marginCost, decimal minLotCount, double minLotMarginPrcentCost, Comissiontype comissiontype, double comission, double priceStep, double costPriceStep, List<DataSourceFile> dataSourceFiles, int id = -1) //метод проверяет присланные данные на корректность и вызывает метод добавления записи в бд или обновления существующей записи если был прислан id
        {
            _cancellationTokenSourceDataSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSourceDataSource.Token;

            DispatcherInvoke((Action)(() => { _mainCommunicationChannel.DataSourceAddingProgress.Clear(); }));

            List<Interval> intervalsInFiles = new List<Interval>(); //список с интервалами файлов, чтобы проверить, имеют ли все файлы одинаковые интервалы
            List<DateTime> datesInFiles = new List<DateTime>(); //список с первыми датами файлов, чтобы проверить, нет ли дублирующихся дат в разных файлах
            //удостоверяемся в том что все файлы имеют допустимый формат данных
            string headerFormat = "<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>";
            string headerErrorFiles = "";
            string unknownIntervalFiles = ""; //файлы с неизвестными интервалами
            bool isAllFilesCorrect = true; //правильный ли формат файлов
            foreach (DataSourceFile item in dataSourceFiles)
            {
                //запроняем список с интервалами пустыми интервалами, для передачи в функцию обхекта Interval
                intervalsInFiles.Add(new Interval());

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
                    //считываем первую дату и время файла для проверки файлов на уникальность
                    string line = streamReader.ReadLine();
                    string[] lineArr = line.Split(',');
                    string dateTimeFormated = lineArr[2].Insert(6, "-").Insert(4, "-") + " " + lineArr[3].Insert(4, ":").Insert(2, ":");
                    datesInFiles.Add(DateTime.Parse(dateTimeFormated)); //записываем первую дату файла
                }
                else
                {
                    isAllFilesCorrect = false;
                }
                streamReader.Close();
                fileStream.Close();
            }
            if (isAllFilesCorrect == false)
            {
                DispatcherInvoke((Action)(() => { _mainCommunicationChannel.AddMainMessage("Файлы с котировками: " + headerErrorFiles + " имеют неверный формат данных. Допустимый формат: \"" + headerFormat + "\"."); }));
                
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

                //определяем время работы биржи для файлов и интервалы
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string progressHeader = id == -1 ? "Добавление источника данных" : "Редактирование источника данных";
                int taskCount = id == -1 ? dataSourceFiles.Count * 2 : dataSourceFiles.Count * 2 + 3;//количество задач (для прогресса выполнения)
                for(int i = 0; i < dataSourceFiles.Count; i++)
                {
                    DefiningFileWorkingPeriods(dataSourceFiles[i], intervalsInFiles[i]);

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

                //определяем первую и последнюю даты
                DateTime startDate = dataSourceFiles[0].DataSourceFileWorkingPeriods[0].StartPeriod.Date;
                DateTime endDate = dataSourceFiles.Last().DataSourceFileWorkingPeriods.Last().EndDateTime.Date;

                //если интервалы корректны, выполняем добавление или обновление записей в БД
                if (isAllFilesCorrect)
                {
                    if (id == -1)
                    {
                        _database.InsertDataSource(name, marginType, intervalsInFiles[0], currency, marginCost, minLotCount, minLotMarginPrcentCost, comissiontype, comission, priceStep, costPriceStep, startDate, endDate);
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

                            //вставляем записи DataSourceFileWorkingPeriods для только что вставленного DataSourceFile
                            int newDataSourceFileId = newDataSource.DataSourceFiles.Last().Id;
                            foreach (DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod in dataSourceFile.DataSourceFileWorkingPeriods)
                            {
                                dataSourceFileWorkingPeriod.IdDataSourceFile = newDataSourceFileId;
                                _database.InsertDataSourceFileWorkingPeriod(dataSourceFileWorkingPeriod);
                            }

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

                        //обновляю dataSourceFileWorkingPeriods
                        for (int k = 0; k < dataSourceFiles.Count; k++)
                        {
                            List<DataSourceFileWorkingPeriod> updateDataSourceFileWorkingPeriods = new List<DataSourceFileWorkingPeriod>(); //список с id записи которую нужно обновить и данные на которые нужно обновить
                            List<DataSourceFileWorkingPeriod> addDataSourceFileWorkingPeriods = new List<DataSourceFileWorkingPeriod>(); //список с записями которые нужно добавить
                            List<int> deleteDataSourceFileWorkingPeriods = new List<int>(); //список с id записей которые нужно удалить

                            
                            if(oldDataSource.DataSourceFiles.Count > k) //если в старой версии источника данных есть файл по этому индексу
                            {
                                DataSourceFile oldDataSourceFile = oldDataSource.DataSourceFiles[k];

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
                            }
                            else //если текущий файл отсутствует в старых файлах, считаем что все DataSourceFileWorkingPeriod данного файла нужно добавить
                            {
                                for(int i = 0; i < dataSourceFiles[k].DataSourceFileWorkingPeriods.Count; i++)
                                {
                                    dataSourceFiles[k].DataSourceFileWorkingPeriods[i].IdDataSourceFile = dataSourceFiles[k].Id;
                                    addDataSourceFileWorkingPeriods.Add(dataSourceFiles[k].DataSourceFileWorkingPeriods[i]);
                                }
                            }

                            //обновляем
                            foreach (DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod in updateDataSourceFileWorkingPeriods)
                            {
                                _database.UpdateDataSourceFileWorkingPeriod(dataSourceFileWorkingPeriod);
                            }
                            DispatcherInvoke((Action)(() => {
                                _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                                _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = progressHeader, TasksCount = taskCount, CompletedTasksCount = dataSourceFiles.Count + 3 + k + 1, ElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
                            }));
                            //добавляем
                            foreach (DataSourceFileWorkingPeriod dataSourceFileWorkingPeriod1 in addDataSourceFileWorkingPeriods)
                            {
                                _database.InsertDataSourceFileWorkingPeriod(dataSourceFileWorkingPeriod1);
                            }
                            DispatcherInvoke((Action)(() => {
                                _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                                _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = progressHeader, TasksCount = taskCount, CompletedTasksCount = dataSourceFiles.Count + 3 + k + 1, ElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
                            }));
                            //удаляем
                            foreach (int idDeleteDataSourceFileWorkingPeriod in deleteDataSourceFileWorkingPeriods)
                            {
                                _database.DeleteDataSourceFileWorkingPeriod(idDeleteDataSourceFileWorkingPeriod);
                            }
                            DispatcherInvoke((Action)(() => {
                                _mainCommunicationChannel.DataSourceAddingProgress.Clear();
                                _mainCommunicationChannel.DataSourceAddingProgress.Add(new DataSourceAddingProgress { Header = progressHeader, TasksCount = taskCount, CompletedTasksCount = dataSourceFiles.Count + 3 + k + 1, ElapsedTime = stopwatch.Elapsed, CancelPossibility = false, IsFinish = false });
                            }));
                        }

                        //обновляю DataSource
                        _database.UpdateDataSource(name, marginType, intervalsInFiles[0], currency, marginCost, minLotCount, minLotMarginPrcentCost, comissiontype, comission, priceStep, costPriceStep, startDate, endDate, id);
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

        private void DefiningFileWorkingPeriods(DataSourceFile dataSourceFile, Interval interval)
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
            string dateTimeFormated = dateFormated + " " + lineArr[3].Insert(4, ":").Insert(2, ":");
            dataSourceFileWorkingPeriod.StartPeriod = DateTime.Parse(dateFormated);
            dataSourceFileWorkingPeriod.TradingStartTime = DateTime.Parse(dateTimeFormated);

            double intervalMinutes = 1500; //интервал в минутах

            DateTime lastDate = dataSourceFileWorkingPeriod.StartPeriod; //прошлая дата, чтобы определять что день сменился
            DateTime lastOpenTime = dataSourceFileWorkingPeriod.TradingStartTime; //время открытия прошлого дня
            DateTime lastCloseTime = dataSourceFileWorkingPeriod.TradingStartTime; //время закрытия прошлого дня (а так же дата и время прошлой строки)
            line = streamReader.ReadLine();
            while (line != null)
            {
                lineArr = line.Split(',');
                dateFormated = lineArr[2].Insert(6, "-").Insert(4, "-");
                dateTimeFormated = dateFormated + " " + lineArr[3].Insert(4, ":").Insert(2, ":");
                DateTime currentDate = DateTime.Parse(dateFormated);

                //определяем интервал
                DateTime currentDateTime = DateTime.Parse(dateTimeFormated);
                if ((currentDateTime - lastCloseTime).TotalMinutes < intervalMinutes)
                {
                    intervalMinutes = (currentDateTime - lastCloseTime).TotalMinutes;
                }

                if (dataSourceFileWorkingPeriod.TradingEndTime == null) //при формировании первого объекта, если день сменился, записываем время окончания, а так же дату и время открытия нового дня
                {
                    if ((lastDate.Date - dataSourceFileWorkingPeriod.StartPeriod.Date).TotalDays >= 1)
                    {
                        dataSourceFileWorkingPeriod.TradingEndTime = lastCloseTime; //записали время окончания первого дня
                        dataSourceFileWorkingPeriods.Add(dataSourceFileWorkingPeriod);
                    }
                }
                else if ((currentDate.Date - lastDate.Date).TotalDays >= 1) //определяем что прошлый день сформировался (т.к. наступил новый)
                {
                    //сравниваем прошлый день и объект dataSourceFileWorkingPeriod
                    if (lastOpenTime.TimeOfDay != dataSourceFileWorkingPeriod.TradingStartTime.TimeOfDay || lastCloseTime.TimeOfDay != dataSourceFileWorkingPeriod.TradingEndTime.TimeOfDay)
                    {
                        //если не совпадают, формируем новый объект dataSourceFileWorkingPeriod
                        dataSourceFileWorkingPeriod = new DataSourceFileWorkingPeriod { StartPeriod = lastDate, TradingStartTime = lastOpenTime, TradingEndTime = lastCloseTime };
                        dataSourceFileWorkingPeriods.Add(dataSourceFileWorkingPeriod);
                    }
                    //начинаем запись нового дня
                    lastDate = currentDate;
                    lastOpenTime = DateTime.Parse(dateTimeFormated);
                    lastCloseTime = lastOpenTime;
                }
                else
                {
                    lastCloseTime = DateTime.Parse(dateTimeFormated);
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

            dataSourceFileWorkingPeriods.Last().EndDateTime = lastCloseTime; //сохраняю в последний элемент dataSourceFileWorkingPeriods, последнюю дату данного файла, для определения последней даты источника данных

            dataSourceFile.DataSourceFileWorkingPeriods = dataSourceFileWorkingPeriods;

            //определяем интервал
            switch ((int)intervalMinutes)
            {
                case 1:
                    interval.Id = _modelData.Intervals[0].Id;
                    break;
                case 5:
                    interval.Id = _modelData.Intervals[1].Id;
                    break;
                case 10:
                    interval.Id = _modelData.Intervals[2].Id;
                    break;
                case 15:
                    interval.Id = _modelData.Intervals[3].Id;
                    break;
                case 30:
                    interval.Id = _modelData.Intervals[4].Id;
                    break;
                case 60:
                    interval.Id = _modelData.Intervals[5].Id;
                    break;
                case 1440:
                    interval.Id = _modelData.Intervals[6].Id;
                    break;
                default:
                    interval.Id = -1;
                    break;
            }
        }

        public void DeleteDataSource(int id)
        {
            _database.DeleteDataSource(id);
            _modelData.ReadDataSources();
            _modelData.NotifyDataSourcesSubscribers();
        }
    }
}
