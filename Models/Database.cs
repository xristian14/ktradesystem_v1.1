﻿using System;
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

        public DataTable SelectDataSourceFiles(int idDataSource)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "SELECT * FROM DataSourceFiles WHERE idDataSource = :idDataSource";
            command.Parameters.AddWithValue("idDataSource", idDataSource);
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);
            return data;
        }

        public DataTable SelectDataSourceFromId(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "SELECT * FROM DataSources WHERE id = :id";
            command.Parameters.AddWithValue("id", id);
            DataTable data = new DataTable();
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            adapter.Fill(data);
            return data;
        }

        public void InsertDataSource(string name, MarginType marginType, Interval interval, Currency currency, double marginCost, decimal minLotCount, double minLotMarginPartCost, Comissiontype comissiontype, double comission, double priceStep, double costPriceStep, int pointsSlippage, DateTime startDate, DateTime endDate)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "INSERT INTO Datasources (name, idMarginType, idInterval, idCurrency, marginCost, minLotCount, minLotMarginPartCost, idComissiontype, comission, priceStep, costPriceStep, pointsSlippage, startDate, endDate) VALUES (:name, :idMarginType, :idInterval, :idCurrency, :marginCost, :minLotCount, :minLotMarginPartCost, :idComissiontype, :comission, :priceStep, :costPriceStep, :pointsSlippage, :startDate, :endDate)";
            command.CommandText = query;
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("idMarginType", marginType.Id);
            command.Parameters.AddWithValue("idInterval", interval.Id);
            command.Parameters.AddWithValue("idCurrency", currency.Id);
            command.Parameters.AddWithValue("marginCost", marginCost);
            command.Parameters.AddWithValue("minLotCount", minLotCount);
            command.Parameters.AddWithValue("minLotMarginPartCost", minLotMarginPartCost);
            command.Parameters.AddWithValue("idComissiontype", comissiontype.Id);
            command.Parameters.AddWithValue("comission", comission);
            command.Parameters.AddWithValue("priceStep", priceStep);
            command.Parameters.AddWithValue("costPriceStep", costPriceStep);
            command.Parameters.AddWithValue("pointsSlippage", pointsSlippage);
            command.Parameters.AddWithValue("startDate", startDate);
            command.Parameters.AddWithValue("endDate", endDate);

            command.ExecuteNonQuery();
        }

        public void UpdateDataSource(string name, MarginType marginType, Interval interval, Currency currency, double marginCost, decimal minLotCount, double minLotMarginPartCost, Comissiontype comissiontype, double comission, double priceStep, double costPriceStep, int pointsSlippage, DateTime startDate, DateTime endDate, int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "UPDATE Datasources SET name = :name, idMarginType = :idMarginType, idInterval = :idInterval, idCurrency = :idCurrency, marginCost = :marginCost, minLotCount = :minLotCount, minLotMarginPartCost = :minLotMarginPartCost, idComissiontype = :idComissiontype, comission = :comission, priceStep = :priceStep, costPriceStep = :costPriceStep, pointsSlippage = :pointsSlippage, startDate = :startDate, endDate = :endDate WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("idMarginType", marginType.Id);
            command.Parameters.AddWithValue("idInterval", interval.Id);
            command.Parameters.AddWithValue("idCurrency", currency.Id);
            command.Parameters.AddWithValue("marginCost", marginCost);
            command.Parameters.AddWithValue("minLotCount", minLotCount);
            command.Parameters.AddWithValue("minLotMarginPartCost", minLotMarginPartCost);
            command.Parameters.AddWithValue("idComissiontype", comissiontype.Id);
            command.Parameters.AddWithValue("comission", comission);
            command.Parameters.AddWithValue("priceStep", priceStep);
            command.Parameters.AddWithValue("costPriceStep", costPriceStep);
            command.Parameters.AddWithValue("pointsSlippage", pointsSlippage);
            command.Parameters.AddWithValue("startDate", startDate);
            command.Parameters.AddWithValue("endDate", endDate);
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

        public void InsertDataSourceFile(DataSourceFile dataSourceFile)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO DataSourceFiles (path, idDataSource) VALUES (:path, :idDataSource)";
            command.Parameters.AddWithValue("path", dataSourceFile.Path);
            command.Parameters.AddWithValue("idDataSource", dataSourceFile.IdDataSource);

            command.ExecuteNonQuery();
        }

        public void UpdateIndicatorParameterTemplate(IndicatorParameterTemplate parameterTemplate)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDATE IndicatorParameterTemplates SET name = :name, description = :description, idParameterValueType = :idParameterValueType WHERE id = :id";
            command.Parameters.AddWithValue("name", parameterTemplate.Name);
            command.Parameters.AddWithValue("description", parameterTemplate.Description);
            command.Parameters.AddWithValue("idParameterValueType", parameterTemplate.ParameterValueType.Id);
            command.Parameters.AddWithValue("id", parameterTemplate.Id);

            command.ExecuteNonQuery();
        }

        public void InsertIndicatorParameterTemplate(IndicatorParameterTemplate parameterTemplate)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO IndicatorParameterTemplates (name, description, idIndicator, idParameterValueType) VALUES (:name, :description, :idIndicator, :idParameterValueType)";
            command.Parameters.AddWithValue("name", parameterTemplate.Name);
            command.Parameters.AddWithValue("description", parameterTemplate.Description);
            command.Parameters.AddWithValue("idIndicator", parameterTemplate.IdIndicator);
            command.Parameters.AddWithValue("idParameterValueType", parameterTemplate.ParameterValueType.Id);

            command.ExecuteNonQuery();
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

        public void InsertAlgorithmIndicator(AlgorithmIndicator algorithmIndicator)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO AlgorithmIndicators (idAlgorithm, idIndicator, ending) VALUES (:idAlgorithm, :idIndicator, :ending)";
            command.Parameters.AddWithValue("idAlgorithm", algorithmIndicator.IdAlgorithm);
            command.Parameters.AddWithValue("idIndicator", algorithmIndicator.Indicator.Id);
            command.Parameters.AddWithValue("ending", algorithmIndicator.Ending);

            command.ExecuteNonQuery();
        }

        public void InsertIndicatorParameterRange(IndicatorParameterRange indicatorParameterRange)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO IndicatorParameterRanges (idIndicatorParameterTemplate, idAlgorithmParameter, idAlgorithmIndicator) VALUES (:idIndicatorParameterTemplate, :idAlgorithmParameter, :idAlgorithmIndicator)";
            command.Parameters.AddWithValue("idIndicatorParameterTemplate", indicatorParameterRange.IndicatorParameterTemplate.Id);
            command.Parameters.AddWithValue("idAlgorithmParameter", indicatorParameterRange.AlgorithmParameter.Id);
            command.Parameters.AddWithValue("idAlgorithmIndicator", indicatorParameterRange.AlgorithmIndicator.Id);

            command.ExecuteNonQuery();
        }

        public void InsertAlgorithmParameter(AlgorithmParameter algorithmParameter)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "INSERT INTO AlgorithmParameters (name, description, minValue, maxValue, step, isStepPercent, idAlgorithm, idParameterValueType) VALUES (:name, :description, :minValue, :maxValue, :step, :isStepPercent, :idAlgorithm, :idParameterValueType)";
            command.Parameters.AddWithValue("name", algorithmParameter.Name);
            command.Parameters.AddWithValue("description", algorithmParameter.Description);
            command.Parameters.AddWithValue("minValue", algorithmParameter.MinValue);
            command.Parameters.AddWithValue("maxValue", algorithmParameter.MaxValue);
            command.Parameters.AddWithValue("step", algorithmParameter.Step);
            command.Parameters.AddWithValue("isStepPercent", algorithmParameter.IsStepPercent);
            command.Parameters.AddWithValue("idAlgorithm", algorithmParameter.IdAlgorithm);
            command.Parameters.AddWithValue("idParameterValueType", algorithmParameter.ParameterValueType.Id);

            command.ExecuteNonQuery();
        }

        public void UpdateDataSourceTemplate(DataSourceTemplate dataSourceTemplate)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDATE DataSourceTemplates SET name = :name, description = :description, idAlgorithm = :idAlgorithm WHERE id = :id";
            command.Parameters.AddWithValue("name", dataSourceTemplate.Name);
            command.Parameters.AddWithValue("description", dataSourceTemplate.Description);
            command.Parameters.AddWithValue("idAlgorithm", dataSourceTemplate.IdAlgorithm);
            command.Parameters.AddWithValue("id", dataSourceTemplate.Id);

            command.ExecuteNonQuery();
        }

        public void DeleteDataSourceTemplate(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM DataSourceTemplates WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void UpdateAlgorithmIndicator(AlgorithmIndicator algorithmIndicator)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDATE AlgorithmIndicators SET idIndicator = :idIndicator, ending = :ending WHERE id = :id";
            command.Parameters.AddWithValue("idIndicator", algorithmIndicator.Indicator.Id);
            command.Parameters.AddWithValue("ending", algorithmIndicator.Ending);
            command.Parameters.AddWithValue("id", algorithmIndicator.Id);

            command.ExecuteNonQuery();
        }

        public void DeleteAlgorithmIndicator(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM AlgorithmIndicators WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void UpdateIndicatorParameterRange(IndicatorParameterRange indicatorParameterRange)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDATE IndicatorParameterRanges SET idIndicatorParameterTemplate = :idIndicatorParameterTemplate, idAlgorithmParameter = :idAlgorithmParameter WHERE id = :id";
            command.Parameters.AddWithValue("idIndicatorParameterTemplate", indicatorParameterRange.IndicatorParameterTemplate.Id);
            command.Parameters.AddWithValue("idAlgorithmParameter", indicatorParameterRange.AlgorithmParameter.Id);
            command.Parameters.AddWithValue("id", indicatorParameterRange.Id);

            command.ExecuteNonQuery();
        }

        public void DeleteIndicatorParameterRange(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM IndicatorParameterRanges WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void UpdateAlgorithmParameter(AlgorithmParameter algorithmParameter)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDATE AlgorithmParameters SET name = :name, description = :description, minValue = :minValue, maxValue = :maxValue, step = :step, isStepPercent = :isStepPercent, idParameterValueType = :idParameterValueType WHERE id = :id";
            command.Parameters.AddWithValue("name", algorithmParameter.Name);
            command.Parameters.AddWithValue("description", algorithmParameter.Description);
            command.Parameters.AddWithValue("minValue", algorithmParameter.MinValue);
            command.Parameters.AddWithValue("maxValue", algorithmParameter.MaxValue);
            command.Parameters.AddWithValue("step", algorithmParameter.Step);
            command.Parameters.AddWithValue("isStepPercent", algorithmParameter.IsStepPercent);
            command.Parameters.AddWithValue("idParameterValueType", algorithmParameter.ParameterValueType.Id);
            command.Parameters.AddWithValue("id", algorithmParameter.Id);

            command.ExecuteNonQuery();
        }

        public void DeleteAlgorithmParameter(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM AlgorithmParameters WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void UpdateAlgorithm(Algorithm algorithm)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDATE Algorithms SET name = :name, description = :description, script = :script WHERE id = :id";
            command.Parameters.AddWithValue("name", algorithm.Name);
            command.Parameters.AddWithValue("description", algorithm.Description);
            command.Parameters.AddWithValue("script", algorithm.Script);
            command.Parameters.AddWithValue("id", algorithm.Id);

            command.ExecuteNonQuery();
        }

        public void DeleteAlgorithm(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM Algorithms WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }

        public void UpdateDataSourceFile(DataSourceFile dataSourceFile)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            command.CommandText = "UPDATE DataSourceFiles SET path = :path, idDataSource = :idDataSource WHERE id = :id";
            command.Parameters.AddWithValue("path", dataSourceFile.Path);
            command.Parameters.AddWithValue("idDataSource", dataSourceFile.IdDataSource);
            command.Parameters.AddWithValue("id", dataSourceFile.Id);

            command.ExecuteNonQuery();
        }

        public void DeleteDataSourceFile(int id)
        {
            SQLiteCommand command = new SQLiteCommand(_connection);
            string query = "DELETE FROM DataSourceFiles WHERE id = :id";
            command.CommandText = query;
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
        }
    }
}
