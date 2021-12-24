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

        public void UpdateParameterTemplate(ParameterTemplate parameterTemplate)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDADE ParameterTemplates SET name = :name, description = :description WHERE id = :id";
            command.Parameters.AddWithValue("name", parameterTemplate.Name);
            command.Parameters.AddWithValue("description", parameterTemplate.Description);
            command.Parameters.AddWithValue("id", parameterTemplate.Id);

            command.ExecuteNonQuery();
        }

        public void InsertParameterTemplate(ParameterTemplate parameterTemplate)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO ParameterTemplates (name, description, idIndicator) VALUES (:name, :description, :idIndicator)";
            command.Parameters.AddWithValue("name", parameterTemplate.Name);
            command.Parameters.AddWithValue("description", parameterTemplate.Description);
            command.Parameters.AddWithValue("idIndicator", parameterTemplate.IdIndicator);

            command.ExecuteNonQuery();
        }

        public void DeleteParameterTemplate(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM ParameterTemplates WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void UpdateIndicator(Indicator indicator)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDADE Indicators SET name = :name, description = :description, script = :script WHERE id = :id";
            command.Parameters.AddWithValue("name", indicator.Name);
            command.Parameters.AddWithValue("description", indicator.Description);
            command.Parameters.AddWithValue("description", indicator.Script);
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
    }
}
