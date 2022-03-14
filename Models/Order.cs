using ktradesystem.Models.Datatables;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace ktradesystem.Models
{
    public class Order
    {
        public int Number { get; set; } //номер заявки
        public int IdDataSource { get; set; }
        [JsonIgnore]
        public DataSource DataSource { get; set; }
        public TypeOrder TypeOrder { get; set; }
        public bool Direction { get; set; } //true - купить, false - продать
        public double Price { get; set; }
        public decimal Count { get; set; } //текущее количество, изменяется при совершении сделок по данной заявке
        public decimal StartCount { get; set; } //начальное количество
        public DateTime DateTimeSubmit { get; set; }
        public DateTime DateTimeRemove { get; set; }
        public int LinkedOrderNumber { get; set; } //номер связанной заявки
        [JsonIgnore]
        public Order LinkedOrder { get; set; } //связанная заявка (тейк-профит для стоп-заявки или стоп-лосс для лимитной заявки на тейк профит)
    }
}
