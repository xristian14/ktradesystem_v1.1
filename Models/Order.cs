using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ktradesystem.Models.Datatables;

namespace ktradesystem.Models
{
    public class Order
    {
        public Order(int idTypeOrder, bool direction, DataSourceForCalculate dataSourceForCalculate, double price, double count)
        {
            _modelData = ModelData.getInstance();

            TypeOrder = _modelData.TypeOrders.Where(i => i.Id == idTypeOrder).First();
            Direction = direction;
            DataSource = dataSourceForCalculate.DataSource;
            Price = price;
            Count = count;
        }
        private ModelData _modelData;
        public int Number { get; set; }
        public DataSource DataSource { get; set; }
        public TypeOrder TypeOrder { get; set; }
        public bool Direction { get; set; } //true - купить, false - продать
        public double Price { get; set; }
        public double Count { get; set; }
        public DateTime DateTimeSubmit { get; set; }
        public DateTime DateTimeRemove { get; set; }
        public Order LinkedOrder { get; set; } //связанная заявка (тейк-профит для стоп-заявки или стоп-лосс для лимитной заявки на тейк профит)
    }
}
