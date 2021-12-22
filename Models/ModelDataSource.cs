using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using System.Data;
using ktradesystem.Models.Datatables;

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
            _communicationChannel = CommunicationChannel.getInstance();
        }

        private Database _database;
        private ModelData _modelData;
        private CommunicationChannel _communicationChannel;

        private ObservableCollection<DataSource> _dataSources = new ObservableCollection<DataSource>();
        
        //создание объекта из данных которые набрал пользователь и добавление в DataSources
        public void CreateDataSourceInsertUpdate(string name, Instrument instrument, Currency currency, double? cost, Comissiontype comissiontype, double comission, double priceStep, double costPriceStep, string files, int id = -1) //метод проверяет присланные данные на корректность и вызывает метод добавления записи в бд или обновления существующей записи если был прислан id
        {
            List<Interval> intervalsInFiles = new List<Interval>(); //список с интервалами файлов, чтобы проверить, имеют ли все файлы одинаковые интервалы
            List<DateTime[]> datesInFiles = new List<DateTime[]>(); //список с первыми тремя датами файлов, чтобы определить интервал, а так же проверить, нет ли дублирующихся дат в разных файлах
            //удостоверяемся в том что все файлы имеют допустимый формат данных
            bool isAllFilesCorrect = true; //правильный и формат файлов
            //формирует массив с файлами из строки
            List<string> filesList = new List<string>();
            string[] filesArr = files.Split('|');
            foreach (string file in filesArr)
            {
                filesList.Add(file);
            }
            foreach (string item in filesList)
            {
                FileStream fileStream = new FileStream(item, FileMode.Open, FileAccess.Read);
                StreamReader streamReader = new StreamReader(fileStream);
                bool isFileCorrect = true; //правильный ли формат данного файла
                string header = streamReader.ReadLine();
                if(header != "<TICKER>,<PER>,<DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>")
                {
                    isFileCorrect = false;
                }
                DateTime[] dateTimes = new DateTime[3];
                //проверяем первые 3 строки на возможность считать дату
                for(int i = 0; i < 3; i++)
                {
                    string line = streamReader.ReadLine();
                    string[] lineArr = line.Split(',');

                    int year = 0;
                    int month = 0;
                    int day = 0;
                    int hour = 0;
                    int minute = 0;
                    int second = 0;

                    if (lineArr.Length == 9) //проверяем количество элементов, чтобы не обратиться к несуществующему элементу
                    {
                        if (lineArr[2].Length == 8 && lineArr[3].Length == 6) //проверяем на количество символов, чтобы не обратиться к несуществующему индексу
                        {
                            if(int.TryParse(lineArr[2].Substring(0, 4), out int intRes)) {  year = intRes; }
                            else { isFileCorrect = false; }
                            if (int.TryParse(lineArr[2].Substring(4, 2), out intRes)) { month = intRes; }
                            else { isFileCorrect = false; }
                            if (int.TryParse(lineArr[2].Substring(6, 2), out intRes)) { day = intRes; }
                            else { isFileCorrect = false; }
                            if (int.TryParse(lineArr[3].Substring(0, 2), out intRes)) { hour = intRes; }
                            else { isFileCorrect = false; }
                            if (int.TryParse(lineArr[3].Substring(2, 2), out intRes)) { minute = intRes; }
                            else { isFileCorrect = false; }
                            if (int.TryParse(lineArr[3].Substring(4, 2), out intRes)) { second = intRes; }
                            else { isFileCorrect = false; }
                        }
                        else { isFileCorrect = false; }
                    }
                    else { isFileCorrect = false; }
                    if (isFileCorrect)
                    {
                        string stringDate = year.ToString() + "/" + month.ToString() + "/" + day.ToString() + " " + hour.ToString() + ":" + minute.ToString() + ":" + second.ToString();
                        if(DateTime.TryParse(stringDate, out DateTime dateTimeRes))
                        {
                            dateTimes[i] = dateTimeRes;
                            
                        }
                        else
                        {
                            isFileCorrect = false;
                        }
                    }
                }
                datesInFiles.Add(dateTimes);
                streamReader.Close();
                fileStream.Close();

                if (isFileCorrect) //если не было ошибок, определяем интервал для данного файла
                {
                    //определяем первые 2 интервала
                    TimeSpan timeSpan1 = dateTimes[2].Subtract(dateTimes[1]);
                    TimeSpan timeSpan2 = dateTimes[1].Subtract(dateTimes[0]);
                    //находим меньший из них, чтобы исключить ситуацию где сделки были с промежутками во времени и разница дат не соответствует интервалу
                    if(TimeSpan.Compare(timeSpan1, timeSpan2) != -1)
                    {
                        timeSpan1 = timeSpan2;
                    }
                    switch (timeSpan1.TotalMinutes)
                    {
                        case (double)1:
                            intervalsInFiles.Add(_modelData.Intervals[0]);
                            break;
                        case (double)5:
                            intervalsInFiles.Add(_modelData.Intervals[1]);
                            break;
                        case (double)10:
                            intervalsInFiles.Add(_modelData.Intervals[2]);
                            break;
                        case (double)15:
                            intervalsInFiles.Add(_modelData.Intervals[3]);
                            break;
                        case (double)30:
                            intervalsInFiles.Add(_modelData.Intervals[4]);
                            break;
                        case (double)60:
                            intervalsInFiles.Add(_modelData.Intervals[5]);
                            break;
                        case (double)1440:
                            intervalsInFiles.Add(_modelData.Intervals[6]);
                            break;
                        default:
                            intervalsInFiles.Add(new Interval { Id = -1, Name = "not found" });
                            break;
                    }
                }
                else
                {
                    _communicationChannel.AddMainMessage("Файл " + item + " имеет неверный формат данных.");
                    isAllFilesCorrect = false;
                }
            }
            //определяем что все файлы имеют одинаковый интервал и не имеют не найденного интервала
            bool isEqualIntervals = true;
            bool isNotFoundInterval = false;
            foreach(Interval item in intervalsInFiles)
            {
                if(item != intervalsInFiles[0])
                {
                    isEqualIntervals = false;
                }
                if(item.Id == -1)
                {
                    isNotFoundInterval = true;
                }
            }
            if(isEqualIntervals == false)
            {
                _communicationChannel.AddMainMessage("Файлы с котировками имеют разные интервалы времени в свечках.");
                isAllFilesCorrect = false;
            }
            if (isNotFoundInterval)
            {
                _communicationChannel.AddMainMessage("Не удается распознать временной интервал файлов с котировками. Убедитесь что используются поддерживаемые временные интервалы.");
                isAllFilesCorrect = false;
            }
            //проверяем, нет ли файлов с одинаковыми первыми датами
            if (isAllFilesCorrect) //поставил проверку, т.к. в datesInFiles может не быть дат
            {
                bool isFirstDateUnique = true;
                for (int i = 0; i < datesInFiles.Count; i++) //проходим по датам и сравниваем дату со всеми датами, кроме неё самой, если есть такая же значит она не уникальна
                {
                    DateTime currentDate = datesInFiles[i][0];
                    for (int k = 0; k < datesInFiles.Count; k++)
                    {
                        DateTime checkDate = datesInFiles[k][0];
                        if(i != k)
                        {
                            if(currentDate == checkDate)
                            {
                                isFirstDateUnique = false;
                            }
                        }
                    }
                }
                if(isFirstDateUnique == false)
                {
                    _communicationChannel.AddMainMessage("Файлы с котировками имеют одинаковые даты.");
                    isAllFilesCorrect = false;
                }
            }
            
            //если все корректно, сортируем файлы по датам, создаем объект, добавляем в список DataSources и записываем в БД
            if (isAllFilesCorrect)
            {
                //сортируем файлы
                List<DateTime> firstDates = new List<DateTime>();
                for(int i = 0; i < datesInFiles.Count; i++) //создаем список с первыми датами файлов
                {
                    firstDates.Add(datesInFiles[i][0]);
                }
                DateTime saveDate; //первая дата файла для сохранения после удаления из элемента списка
                string saveFile; //путь к файлу для сохранения после удаления из элемента списка
                for (int i = 0; i < firstDates.Count; i++)
                {
                    for (int k = 0; k < firstDates.Count - 1; k++)
                    {
                        if (DateTime.Compare(firstDates[k], firstDates[k + 1]) > 0)
                        {
                            saveDate = firstDates[k];
                            firstDates[k] = firstDates[k + 1];
                            firstDates[k + 1] = saveDate;

                            saveFile = filesList[k];
                            filesList[k] = filesList[k + 1];
                            filesList[k + 1] = saveFile;
                        }
                    }
                }
                string filesSorted = "";
                foreach(string itemFilesList in filesList)
                {
                    filesSorted += itemFilesList + "|";
                }
                filesSorted = filesSorted.Remove(filesSorted.Length - 1);
                bool isAddCost = true; //нужно ли добавлять стоимость в запись (у акции нет стоимости)
                if(instrument.Id == 1)
                {
                    isAddCost = false;
                }

                if (id == -1)
                {
                    _database.InsertDataSource(name, instrument, intervalsInFiles[0], currency, cost, comissiontype, comission, priceStep, costPriceStep, filesSorted, isAddCost);
                }
                else
                {
                    _database.UpdateDataSource(name, instrument, intervalsInFiles[0], currency, cost, comissiontype, comission, priceStep, costPriceStep, filesSorted, isAddCost, id);
                }
                
                _modelData.ReadDataSources();
            }
        }

        public void DeleteDataSource(int id)
        {
            _database.DeleteDataSource(id);
            _modelData.ReadDataSources();
        }
    }
}
