using System.Collections.Generic;
using System.Linq;
namespace Canducci.QueryableExpressions.Orders.Models
{
    public sealed class OrderBuilder
    {
        private readonly List<Order> _orders = new List<Order>();        
        public OrderBuilder Add(string propertyName)
        {
            _orders.Add(new Order(propertyName, OrderDirection.Ascending));
            return this;
        }
        
        public OrderBuilder AddDescending(string propertyName)
        {
            _orders.Add(new Order(propertyName, OrderDirection.Descending));
            return this;
        }
        
        public void Clear()
        {
            _orders.Clear();
        }

        public IEnumerable<Order> Build()
        {
            return _orders.AsEnumerable();
        }

        public static OrderBuilder Create()
        {
            return new OrderBuilder();
        }
    }
}