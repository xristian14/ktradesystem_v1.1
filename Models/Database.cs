using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections.ObjectModel;
using System.Data;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    class Database : ModelBase //реализует подключение к БД, запросы
    {
        private static Database _instance;

        public static Database getInstance()
        {
            if (_instance == null)
            {
                _instance = new Database();
            }
            return _instance;
        }

        private Database()
        {
            Connect("kts.db");
        }

        private SQLiteConnection _connection;

        private void Connect(string fileName)
        {
            try
            {
                _connection = new SQLiteConnection("Data Source=" + fileName + ";Version=3; FailMissing=True");
                _connection.Open();
                _dbStatus.Add(true); //первый элемент true означает что соединение установлено

                QueryInsertUpdate("PRAGMA foreign_keys = ON;"); //включаем поддержку внешних ключей (необходимо делать это для каждого подключения)
            }
            catch (SQLiteException ex)
            {
                _dbStatus.Add(false); //первый элемент false означает что соединение не было установлено
            }
        }

        private ObservableCollection<bool> _dbStatus = new ObservableCollection<bool>(); //статус работы базы данных

        public ObservableCollection<bool> DbStatus
        {
            get { return _dbStatus; }
            set
            {
                _dbStatus = value;
                OnPropertyChanged();
            }
        }

        public void QueryInsertUpdate(string query) //выполняет запросы insert и update
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public DataTable QuerySelect(string query) //выполняет запрос select
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = query;
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);
            return data;
        }

        public void InsertDataSource(string name, Instrument instrument, Interval interval, Currency currency, double? cost, Comissiontype comissiontype, double comission, double priceStep, double costPriceStep, string files, bool isAddCost) //isAddCost - добавлять ли стоимость в запись, при false это поле не будет задано в запросе
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "";
            if (isAddCost)
            {
                query = "INSERT INTO Datasources (name, idInstrument, idInterval, idCurrency, cost, idComissiontype, comission, priceStep, costPriceStep, files) VALUES (:name, :idInstrument, :idInterval, :idCurrency, :cost, :idComissiontype, :comission, :priceStep, :costPriceStep, :files)";
            }
            else
            {
                query = "INSERT INTO Datasources (name, idInstrument, idInterval, idCurrency, idComissiontype, comission, priceStep, costPriceStep, files) VALUES (:name, :idInstrument, :idInterval, :idCurrency, :idComissiontype, :comission, :priceStep, :costPriceStep, :files)";
            }
            command.CommandText = query;
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("idInstrument", instrument.Id);
            command.Parameters.AddWithValue("idInterval", interval.Id);
            command.Parameters.AddWithValue("idCurrency", currency.Id);
            if (isAddCost)
            {
                command.Parameters.AddWithValue("cost", cost);
            }
            command.Parameters.AddWithValue("idComissiontype", comissiontype.Id);
            command.Parameters.AddWithValue("comission", comission);
            command.Parameters.AddWithValue("priceStep", priceStep);
            command.Parameters.AddWithValue("costPriceStep", costPriceStep);
            command.Parameters.AddWithValue("files", files);

            command.ExecuteNonQuery();
        }

        public void UpdateDataSource(string name, Instrument instrument, Interval interval, Currency currency, double? cost, Comissiontype comissiontype, double comission, double priceStep, double costPriceStep, string files, bool isAddCost, int id) //isAddCost - добавлять ли стоимость в запись, при false это поле не будет задано в запросе
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "";
            if (isAddCost)
            {
                query = "UPDATE Datasources SET name = :name, idInstrument = :idInstrument, idInterval = :idInterval, idCurrency = :idCurrency, cost = :cost, idComissiontype = :idComissiontype, comission = :comission, priceStep = :priceStep, costPriceStep = :costPriceStep, files = :files WHERE id = :id";
            }
            else
            {
                query = "UPDATE Datasources SET name = :name, idInstrument = :idInstrument, idInterval = :idInterval, idCurrency = :idCurrency, idComissiontype = :idComissiontype, comission = :comission, priceStep = :priceStep, costPriceStep = :costPriceStep, files = :files WHERE id = :id";
            }
            command.CommandText = query;
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("idInstrument", instrument.Id);
            command.Parameters.AddWithValue("idInterval", interval.Id);
            command.Parameters.AddWithValue("idCurrency", currency.Id);
            if (isAddCost)
            {
                command.Parameters.AddWithValue("cost", cost);
            }
            command.Parameters.AddWithValue("idComissiontype", comissiontype.Id);
            command.Parameters.AddWithValue("comission", comission);
            command.Parameters.AddWithValue("priceStep", priceStep);
            command.Parameters.AddWithValue("costPriceStep", costPriceStep);
            command.Parameters.AddWithValue("files", files);
            command.Parameters.AddWithValue("id", id);

            command.ExecuteNonQuery();
        }

        public void DeleteDataSource(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM Datasources WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void UpdateIndicatorParameterTemplate(IndicatorParameterTemplate parameterTemplate)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDATE IndicatorParameterTemplates SET name = :name, description = :description WHERE id = :id";
            command.Parameters.AddWithValue("name", parameterTemplate.Name);
            command.Parameters.AddWithValue("description", parameterTemplate.Description);
            command.Parameters.AddWithValue("id", parameterTemplate.Id);

            command.ExecuteNonQuery();
        }

        public void InsertIndicatorParameterTemplate(IndicatorParameterTemplate parameterTemplate)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO IndicatorParameterTemplates (name, description, idIndicator) VALUES (:name, :description, :idIndicator)";
            command.Parameters.AddWithValue("name", parameterTemplate.Name);
            command.Parameters.AddWithValue("description", parameterTemplate.Description);
            command.Parameters.AddWithValue("idIndicator", parameterTemplate.IdIndicator);

            command.ExecuteNonQuery();

            //нужно выполнить проверку на то, есть ли индикатор данного параметра в алгоритмах, и если есть, то создать для них IndicatorParameterRange, ссылающийся на этот IndicatorParameterTemplate, для того чтобы запрос с LEFT JOIN и UNION был правильным (если будет parameterTemplate но не будет parameterRange, то будет добавлена строка с некорректными данными в полях в запросе IndicatorParameterRanges), а так же для того чтобы список со значениями параметров индикаторов соответствовал списку шаблонов параметров данного индикатора
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        public void DeleteIndicatorParameterTemplate(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM IndicatorParameterTemplates WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void UpdateIndicator(Indicator indicator)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDATE Indicators SET name = :name, description = :description, script = :script WHERE id = :id";
            command.Parameters.AddWithValue("name", indicator.Name);
            command.Parameters.AddWithValue("description", indicator.Description);
            command.Parameters.AddWithValue("script", indicator.Script);
            command.Parameters.AddWithValue("id", indicator.Id);

            command.ExecuteNonQuery();
        }

        public void InsertIndicator(Indicator indicator)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO Indicators (name, description, script, isStandart) VALUES (:name, :description, :script, :isStandart)";
            command.Parameters.AddWithValue("name", indicator.Name);
            command.Parameters.AddWithValue("description", indicator.Description);
            command.Parameters.AddWithValue("script", indicator.Script);
            command.Parameters.AddWithValue("isStandart", 0);

            command.ExecuteNonQuery();
        }

        public void DeleteIndicator(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM Indicators WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void InsertAlgorithm(Algorithm algorithm)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO Algorithms (name, description, script) VALUES (:name, :description, :script)";
            command.Parameters.AddWithValue("name", algorithm.Name);
            command.Parameters.AddWithValue("description", algorithm.Description);
            command.Parameters.AddWithValue("script", algorithm.Script);

            command.ExecuteNonQuery();
        }

        public void InsertDataSourceTemplate(DataSourceTemplate dataSourceTemplate)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO DataSourceTemplates (name, description, idAlgorithm) VALUES (:name, :description, :idAlgorithm)";
            command.Parameters.AddWithValue("name", dataSourceTemplate.Name);
            command.Parameters.AddWithValue("description", dataSourceTemplate.Description);
            command.Parameters.AddWithValue("idAlgorithm", dataSourceTemplate.IdAlgorithm);

            command.ExecuteNonQuery();
        }

        public void InsertIndicatorParameterRange(IndicatorParameterRange indicatorParameterRange)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO IndicatorParameterRanges (minValue, maxValue, step, isStepPercent, idAlgorithm, idIndicatorParameterTemplate) VALUES (:minValue, :maxValue, :step, :isStepPercent, :idAlgorithm, :idIndicatorParameterTemplate)";
            command.Parameters.AddWithValue("minValue", indicatorParameterRange.MinValue);
            command.Parameters.AddWithValue("maxValue", indicatorParameterRange.MaxValue);
            command.Parameters.AddWithValue("step", indicatorParameterRange.Step);
            command.Parameters.AddWithValue("isStepPercent", indicatorParameterRange.IsStepPercent);
            command.Parameters.AddWithValue("idAlgorithm", indicatorParameterRange.IdAlgorithm);
            command.Parameters.AddWithValue("idIndicatorParameterTemplate", indicatorParameterRange.IdIndicatorParameterTemplate);

            command.ExecuteNonQuery();
        }

        public void InsertAlgorithmParameter(AlgorithmParameter algorithmParameter)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO AlgorithmParameters (name, description, minValue, maxValue, step, isStepPercent, idAlgorithm) VALUES (:name, :description, :minValue, :maxValue, :step, :isStepPercent, :idAlgorithm)";
            command.Parameters.AddWithValue("name", algorithmParameter.Name);
            command.Parameters.AddWithValue("description", algorithmParameter.Description);
            command.Parameters.AddWithValue("minValue", algorithmParameter.MinValue);
            command.Parameters.AddWithValue("maxValue", algorithmParameter.MaxValue);
            command.Parameters.AddWithValue("step", algorithmParameter.Step);
            command.Parameters.AddWithValue("isStepPercent", algorithmParameter.IsStepPercent);
            command.Parameters.AddWithValue("idAlgorithm", algorithmParameter.IdAlgorithm);

            command.ExecuteNonQuery();
        }
    }
}
