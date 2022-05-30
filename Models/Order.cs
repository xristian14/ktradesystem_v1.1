using ktradesystem.Models.Datatables;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace ktradesystem.Models
{
    [Serializable]
    public class Order
    {
        public int Number { get; set; } //номер заявки
        public int IdDataSource { get; set; }
        [NonSerialized]
        public DataSource DataSource;
        public TypeOrder TypeOrder { get; set; }
        public bool Direction { get; set; } //true - купить, false - продать
        public double Price { get; set; }
        public decimal Count { get; set; } //текущее количество, изменяется при совершении сделок по данной заявке
        public decimal StartCount { get; set; } //начальное количество
        public DateTime DateTimeSubmit { get; set; }
        public DateTime DateTimeRemove { get; set; }
        public int LinkedOrderNumber { get; set; } //номер связанной заявки
        [NonSerialized]
        public Order LinkedOrder; //связанная заявка (тейк-профит для стоп-заявки или стоп-лосс для лимитной заявки на тейк профит)
    }
}
