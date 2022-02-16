using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    [Serializable]
    public class DataSourceGroup
    {
        public List<DataSourceAccordance> DataSourceAccordances { get; set; }
        public DataSourceGroup Clone()
        {
            DataSourceGroup dataSourceGroup = new DataSourceGroup { DataSourceAccordances = new List<DataSourceAccordance>() };
            foreach(DataSourceAccordance dataSourceAccordance in DataSourceAccordances)
            {
                DataSourceTemplate dataSourceTemplate = dataSourceAccordance.DataSourceTemplate.Clone();
                DataSource dataSource = new DataSource { Id = dataSourceAccordance.DataSource.Id, Name = dataSourceAccordance.DataSource.Name, Instrument = dataSourceAccordance.DataSource.Instrument, Interval = dataSourceAccordance.DataSource.Interval, Currency = dataSourceAccordance.DataSource.Currency, Cost = dataSourceAccordance.DataSource.Cost, Comissiontype = dataSourceAccordance.DataSource.Comissiontype, Comission = dataSourceAccordance.DataSource.Comission, PriceStep = dataSourceAccordance.DataSource.PriceStep, CostPriceStep = dataSourceAccordance.DataSource.CostPriceStep, StartDate = dataSourceAccordance.DataSource.StartDate, EndDate = dataSourceAccordance.DataSource.EndDate };
                dataSourceGroup.DataSourceAccordances.Add(new DataSourceAccordance { DataSourceTemplate = dataSourceTemplate, DataSource = dataSource });
            }
            return dataSourceGroup;
        }
    }
}
